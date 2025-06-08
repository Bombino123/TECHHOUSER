using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;

internal class QueryRewriter
{
	private readonly MemberPath _extentPath;

	private readonly MemberDomainMap _domainMap;

	private readonly ConfigViewGenerator _config;

	private readonly CqlIdentifiers _identifiers;

	private readonly ViewgenContext _context;

	private readonly RewritingProcessor<Tile<FragmentQuery>> _qp;

	private readonly List<MemberPath> _keyAttributes;

	private readonly List<FragmentQuery> _fragmentQueries = new List<FragmentQuery>();

	private readonly List<Tile<FragmentQuery>> _views = new List<Tile<FragmentQuery>>();

	private readonly FragmentQuery _domainQuery;

	private readonly EdmType _generatedType;

	private readonly HashSet<FragmentQuery> _usedViews = new HashSet<FragmentQuery>();

	private List<LeftCellWrapper> _usedCells = new List<LeftCellWrapper>();

	private BoolExpression _topLevelWhereClause;

	private CellTreeNode _basicView;

	private Dictionary<MemberPath, CaseStatement> _caseStatements = new Dictionary<MemberPath, CaseStatement>();

	private readonly ErrorLog _errorLog = new ErrorLog();

	private readonly ViewGenMode _typesGenerationMode;

	private static readonly Tile<FragmentQuery> _trueViewSurrogate = CreateTile(FragmentQuery.Create(BoolExpression.True));

	internal ViewgenContext ViewgenContext => _context;

	internal Dictionary<MemberPath, CaseStatement> CaseStatements => _caseStatements;

	internal BoolExpression TopLevelWhereClause => _topLevelWhereClause;

	internal CellTreeNode BasicView => _basicView.MakeCopy();

	internal List<LeftCellWrapper> UsedCells => _usedCells;

	private IEnumerable<FragmentQuery> FragmentQueries => _fragmentQueries;

	internal QueryRewriter(EdmType generatedType, ViewgenContext context, ViewGenMode typesGenerationMode)
	{
		_typesGenerationMode = typesGenerationMode;
		_context = context;
		_generatedType = generatedType;
		_domainMap = context.MemberMaps.LeftDomainMap;
		_config = context.Config;
		_identifiers = context.CqlIdentifiers;
		_qp = new RewritingProcessor<Tile<FragmentQuery>>(new DefaultTileProcessor<FragmentQuery>(context.LeftFragmentQP));
		_extentPath = new MemberPath(context.Extent);
		_keyAttributes = new List<MemberPath>(MemberPath.GetKeyMembers(context.Extent, _domainMap));
		foreach (LeftCellWrapper item2 in _context.AllWrappersForExtent)
		{
			FragmentQuery fragmentQuery = item2.FragmentQuery;
			Tile<FragmentQuery> item = CreateTile(fragmentQuery);
			_fragmentQueries.Add(fragmentQuery);
			_views.Add(item);
		}
		AdjustMemberDomainsForUpdateViews();
		_domainQuery = GetDomainQuery(FragmentQueries, generatedType);
		_usedViews = new HashSet<FragmentQuery>();
	}

	internal void GenerateViewComponents()
	{
		EnsureExtentIsFullyMapped(_usedViews);
		GenerateCaseStatements(_domainMap.ConditionMembers(_extentPath.Extent), _usedViews);
		AddTrivialCaseStatementsForConditionMembers();
		if (_usedViews.Count == 0 || _errorLog.Count > 0)
		{
			ExceptionHelpers.ThrowMappingException(_errorLog, _config);
		}
		_topLevelWhereClause = GetTopLevelWhereClause(_usedViews);
		_ = _context.ViewTarget;
		_usedCells = RemapFromVariables();
		BasicViewGenerator basicViewGenerator = new BasicViewGenerator(_context.MemberMaps.ProjectedSlotMap, _usedCells, _domainQuery, _context, _domainMap, _errorLog, _config);
		_basicView = basicViewGenerator.CreateViewExpression();
		if (_context.LeftFragmentQP.IsContainedIn(_basicView.LeftFragmentQuery, _domainQuery))
		{
			_topLevelWhereClause = BoolExpression.True;
		}
		if (_errorLog.Count > 0)
		{
			ExceptionHelpers.ThrowMappingException(_errorLog, _config);
		}
	}

	private IEnumerable<Constant> GetDomain(MemberPath currentPath)
	{
		if (_context.ViewTarget == ViewTarget.QueryView && MemberPath.EqualityComparer.Equals(currentPath, _extentPath))
		{
			IEnumerable<EdmType> types = ((_typesGenerationMode != ViewGenMode.OfTypeOnlyViews) ? MetadataHelper.GetTypeAndSubtypesOf(_generatedType, _context.EdmItemCollection, includeAbstractTypes: false) : new HashSet<EdmType> { _generatedType });
			return GetTypeConstants(types);
		}
		return _domainMap.GetDomain(currentPath);
	}

	private void AdjustMemberDomainsForUpdateViews()
	{
		if (_context.ViewTarget != ViewTarget.UpdateView)
		{
			return;
		}
		foreach (MemberPath currentPath in new List<MemberPath>(_domainMap.ConditionMembers(_extentPath.Extent)))
		{
			Constant constant = _domainMap.GetDomain(currentPath).FirstOrDefault((Constant domainValue) => IsDefaultValue(domainValue, currentPath));
			if (constant != null)
			{
				RemoveUnusedValueFromStoreDomain(constant, currentPath);
			}
			Constant constant2 = _domainMap.GetDomain(currentPath).FirstOrDefault((Constant domainValue) => domainValue is NegatedConstant);
			if (constant2 != null)
			{
				RemoveUnusedValueFromStoreDomain(constant2, currentPath);
			}
		}
	}

	private void RemoveUnusedValueFromStoreDomain(Constant domainValue, MemberPath currentPath)
	{
		BoolExpression whereClause = CreateMemberCondition(currentPath, domainValue);
		HashSet<FragmentQuery> outputUsedViews = new HashSet<FragmentQuery>();
		bool flag = false;
		if (FindRewritingAndUsedViews(_keyAttributes, whereClause, outputUsedViews, out var rewriting))
		{
			flag = !TileToCellTree(rewriting, _context).IsEmptyRightFragmentQuery;
		}
		if (flag)
		{
			return;
		}
		Set<Constant> set = new Set<Constant>(_domainMap.GetDomain(currentPath), Constant.EqualityComparer);
		set.Remove(domainValue);
		_domainMap.UpdateConditionMemberDomain(currentPath, set);
		foreach (FragmentQuery fragmentQuery in _fragmentQueries)
		{
			fragmentQuery.Condition.FixDomainMap(_domainMap);
		}
	}

	internal FragmentQuery GetDomainQuery(IEnumerable<FragmentQuery> fragmentQueries, EdmType generatedType)
	{
		BoolExpression boolExpression = null;
		if (_context.ViewTarget == ViewTarget.QueryView)
		{
			if (generatedType == null)
			{
				boolExpression = BoolExpression.True;
			}
			else
			{
				IEnumerable<EdmType> types = ((_typesGenerationMode != ViewGenMode.OfTypeOnlyViews) ? MetadataHelper.GetTypeAndSubtypesOf(generatedType, _context.EdmItemCollection, includeAbstractTypes: false) : new HashSet<EdmType> { _generatedType });
				Domain domain = new Domain(GetTypeConstants(types), _domainMap.GetDomain(_extentPath));
				boolExpression = BoolExpression.CreateLiteral(new TypeRestriction(new MemberProjectedSlot(_extentPath), domain), _domainMap);
			}
			return FragmentQuery.Create(_keyAttributes, boolExpression);
		}
		BoolExpression whereClause = BoolExpression.CreateOr(fragmentQueries.Select((FragmentQuery fragmentQuery) => fragmentQuery.Condition).ToArray());
		return FragmentQuery.Create(_keyAttributes, whereClause);
	}

	private bool AddRewritingToCaseStatement(Tile<FragmentQuery> rewriting, CaseStatement caseStatement, MemberPath currentPath, Constant domainValue)
	{
		BoolExpression @true = BoolExpression.True;
		bool flag = _qp.IsContainedIn(CreateTile(_domainQuery), rewriting);
		if (_qp.IsDisjointFrom(CreateTile(_domainQuery), rewriting))
		{
			return false;
		}
		ProjectedSlot value = ((!domainValue.HasNotNull()) ? ((ProjectedSlot)new ConstantProjectedSlot(domainValue)) : ((ProjectedSlot)new MemberProjectedSlot(currentPath)));
		@true = (flag ? BoolExpression.True : TileToBoolExpr(rewriting));
		caseStatement.AddWhenThen(@true, value);
		return flag;
	}

	private void EnsureConfigurationIsFullyMapped(MemberPath currentPath, BoolExpression currentWhereClause, HashSet<FragmentQuery> outputUsedViews, ErrorLog errorLog)
	{
		foreach (Constant item in GetDomain(currentPath))
		{
			if (item == Constant.Undefined)
			{
				continue;
			}
			BoolExpression boolExpression = CreateMemberCondition(currentPath, item);
			BoolExpression boolExpression2 = BoolExpression.CreateAnd(currentWhereClause, boolExpression);
			if (!FindRewritingAndUsedViews(_keyAttributes, boolExpression2, outputUsedViews, out var rewriting))
			{
				if (!ErrorPatternMatcher.FindMappingErrors(_context, _domainMap, _errorLog))
				{
					StringBuilder stringBuilder = new StringBuilder();
					string p = StringUtil.FormatInvariant("{0}", _extentPath);
					BoolExpression condition = rewriting.Query.Condition;
					condition.ExpensiveSimplify();
					if (condition.RepresentsAllTypeConditions)
					{
						string viewGen_Extent = Strings.ViewGen_Extent;
						stringBuilder.AppendLine(Strings.ViewGen_Cannot_Recover_Types(viewGen_Extent, p));
					}
					else
					{
						string viewGen_Entities = Strings.ViewGen_Entities;
						stringBuilder.AppendLine(Strings.ViewGen_Cannot_Disambiguate_MultiConstant(viewGen_Entities, p));
					}
					RewritingValidator.EntityConfigurationToUserString(condition, stringBuilder);
					ErrorLog.Record record = new ErrorLog.Record(ViewGenErrorCode.AmbiguousMultiConstants, stringBuilder.ToString(), _context.AllWrappersForExtent, string.Empty);
					errorLog.AddEntry(record);
				}
			}
			else
			{
				if (!(item is TypeConstant { EdmType: var edmType }))
				{
					continue;
				}
				List<MemberPath> list = GetNonConditionalScalarMembers(edmType, currentPath, _domainMap).Union(GetNonConditionalComplexMembers(edmType, currentPath, _domainMap)).ToList();
				if (list.Count > 0 && !FindRewritingAndUsedViews(list, boolExpression2, outputUsedViews, out rewriting, out var notCoveredAttributes))
				{
					list = new List<MemberPath>(list.Where((MemberPath a) => !a.IsPartOfKey));
					AddUnrecoverableAttributesError(notCoveredAttributes, boolExpression, errorLog);
					continue;
				}
				foreach (MemberPath conditionalComplexMember in GetConditionalComplexMembers(edmType, currentPath, _domainMap))
				{
					EnsureConfigurationIsFullyMapped(conditionalComplexMember, boolExpression2, outputUsedViews, errorLog);
				}
				foreach (MemberPath conditionalScalarMember in GetConditionalScalarMembers(edmType, currentPath, _domainMap))
				{
					EnsureConfigurationIsFullyMapped(conditionalScalarMember, boolExpression2, outputUsedViews, errorLog);
				}
			}
		}
	}

	private static List<string> GetTypeBasedMemberPathList(IEnumerable<MemberPath> nonConditionalScalarAttributes)
	{
		List<string> list = new List<string>();
		foreach (MemberPath nonConditionalScalarAttribute in nonConditionalScalarAttributes)
		{
			EdmMember leafEdmMember = nonConditionalScalarAttribute.LeafEdmMember;
			list.Add(leafEdmMember.DeclaringType.Name + "." + leafEdmMember);
		}
		return list;
	}

	private void AddUnrecoverableAttributesError(IEnumerable<MemberPath> attributes, BoolExpression domainAddedWhereClause, ErrorLog errorLog)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string p = StringUtil.FormatInvariant("{0}", _extentPath);
		string viewGen_Extent = Strings.ViewGen_Extent;
		string p2 = StringUtil.ToCommaSeparatedString(GetTypeBasedMemberPathList(attributes));
		stringBuilder.AppendLine(Strings.ViewGen_Cannot_Recover_Attributes(p2, viewGen_Extent, p));
		RewritingValidator.EntityConfigurationToUserString(domainAddedWhereClause, stringBuilder);
		ErrorLog.Record record = new ErrorLog.Record(ViewGenErrorCode.AttributesUnrecoverable, stringBuilder.ToString(), _context.AllWrappersForExtent, string.Empty);
		errorLog.AddEntry(record);
	}

	private void GenerateCaseStatements(IEnumerable<MemberPath> members, HashSet<FragmentQuery> outputUsedViews)
	{
		IEnumerable<LeftCellWrapper> source = _context.AllWrappersForExtent.Where((LeftCellWrapper w) => _usedViews.Contains(w.FragmentQuery));
		ViewgenContext context = _context;
		CellTreeNode[] children = source.Select((LeftCellWrapper wrapper) => new LeafCellTreeNode(_context, wrapper)).ToArray();
		CellTreeNode rightDomainQuery = new OpCellTreeNode(context, CellTreeOpType.Union, children);
		foreach (MemberPath member in members)
		{
			List<Constant> list = GetDomain(member).ToList();
			CaseStatement caseStatement = new CaseStatement(member);
			Tile<FragmentQuery> tile = null;
			bool flag = list.Count != 2 || !list.Contains(Constant.Null, Constant.EqualityComparer) || !list.Contains(Constant.NotNull, Constant.EqualityComparer);
			foreach (Constant item in list)
			{
				if (item == Constant.Undefined && _context.ViewTarget == ViewTarget.QueryView)
				{
					caseStatement.AddWhenThen(BoolExpression.False, new ConstantProjectedSlot(Constant.Undefined));
					continue;
				}
				FragmentQuery fragmentQuery = CreateMemberConditionQuery(member, item);
				if (FindRewritingAndUsedViews(fragmentQuery.Attributes, fragmentQuery.Condition, outputUsedViews, out var rewriting))
				{
					if (_context.ViewTarget == ViewTarget.UpdateView)
					{
						tile = ((tile != null) ? _qp.Union(tile, rewriting) : rewriting);
					}
					if (flag && AddRewritingToCaseStatement(rewriting, caseStatement, member, item))
					{
						break;
					}
				}
				else if (!IsDefaultValue(item, member) && !ErrorPatternMatcher.FindMappingErrors(_context, _domainMap, _errorLog))
				{
					StringBuilder stringBuilder = new StringBuilder();
					string text = StringUtil.FormatInvariant("{0}", _extentPath);
					string p = ((_context.ViewTarget == ViewTarget.QueryView) ? Strings.ViewGen_Entities : Strings.ViewGen_Tuples);
					if (_context.ViewTarget == ViewTarget.QueryView)
					{
						stringBuilder.AppendLine(Strings.Viewgen_CannotGenerateQueryViewUnderNoValidation(text));
					}
					else
					{
						stringBuilder.AppendLine(Strings.ViewGen_Cannot_Disambiguate_MultiConstant(p, text));
					}
					RewritingValidator.EntityConfigurationToUserString(fragmentQuery.Condition, stringBuilder, _context.ViewTarget == ViewTarget.UpdateView);
					ErrorLog.Record record = new ErrorLog.Record(ViewGenErrorCode.AmbiguousMultiConstants, stringBuilder.ToString(), _context.AllWrappersForExtent, string.Empty);
					_errorLog.AddEntry(record);
				}
			}
			if (_errorLog.Count == 0)
			{
				if (_context.ViewTarget == ViewTarget.UpdateView && flag)
				{
					AddElseDefaultToCaseStatement(member, caseStatement, list, rightDomainQuery, tile);
				}
				if (caseStatement.Clauses.Count > 0)
				{
					_caseStatements[member] = caseStatement;
				}
			}
		}
	}

	private void AddElseDefaultToCaseStatement(MemberPath currentPath, CaseStatement caseStatement, List<Constant> domain, CellTreeNode rightDomainQuery, Tile<FragmentQuery> unionCaseRewriting)
	{
		Constant defaultConstant;
		bool flag = Domain.TryGetDefaultValueForMemberPath(currentPath, out defaultConstant);
		if (flag && domain.Contains(defaultConstant))
		{
			return;
		}
		CellTreeNode cellTreeNode = TileToCellTree(unionCaseRewriting, _context);
		FragmentQuery fragmentQuery = _context.RightFragmentQP.Difference(rightDomainQuery.RightFragmentQuery, cellTreeNode.RightFragmentQuery);
		if (_context.RightFragmentQP.IsSatisfiable(fragmentQuery))
		{
			if (flag)
			{
				caseStatement.AddWhenThen(BoolExpression.True, new ConstantProjectedSlot(defaultConstant));
				return;
			}
			fragmentQuery.Condition.ExpensiveSimplify();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(Strings.ViewGen_No_Default_Value_For_Configuration(currentPath.PathToString(false)));
			_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.NoDefaultValue, stringBuilder.ToString(), _context.AllWrappersForExtent, string.Empty));
		}
	}

	private BoolExpression GetTopLevelWhereClause(HashSet<FragmentQuery> outputUsedViews)
	{
		BoolExpression boolExpression = BoolExpression.True;
		if (_context.ViewTarget == ViewTarget.QueryView && !_domainQuery.Condition.IsTrue && FindRewritingAndUsedViews(_keyAttributes, _domainQuery.Condition, outputUsedViews, out var rewriting))
		{
			boolExpression = TileToBoolExpr(rewriting);
			boolExpression.ExpensiveSimplify();
		}
		return boolExpression;
	}

	internal void EnsureExtentIsFullyMapped(HashSet<FragmentQuery> outputUsedViews)
	{
		if (_context.ViewTarget == ViewTarget.QueryView && _config.IsValidationEnabled)
		{
			EnsureConfigurationIsFullyMapped(_extentPath, BoolExpression.True, outputUsedViews, _errorLog);
			if (_errorLog.Count > 0)
			{
				ExceptionHelpers.ThrowMappingException(_errorLog, _config);
			}
			return;
		}
		if (_config.IsValidationEnabled)
		{
			foreach (MemberPath member in _context.MemberMaps.ProjectedSlotMap.Members)
			{
				if (!member.IsScalarType() || member.IsPartOfKey || _domainMap.IsConditionMember(member) || Domain.TryGetDefaultValueForMemberPath(member, out var _))
				{
					continue;
				}
				HashSet<MemberPath> hashSet = new HashSet<MemberPath>(_keyAttributes);
				hashSet.Add(member);
				foreach (LeftCellWrapper item in _context.AllWrappersForExtent)
				{
					FragmentQuery fragmentQuery = item.FragmentQuery;
					FragmentQuery query = new FragmentQuery(fragmentQuery.Description, fragmentQuery.FromVariable, hashSet, fragmentQuery.Condition);
					Tile<FragmentQuery> toAvoid = CreateTile(FragmentQuery.Create(_keyAttributes, BoolExpression.CreateNot(fragmentQuery.Condition)));
					if (!RewriteQuery(CreateTile(query), toAvoid, out var _, out var _, isRelaxed: false))
					{
						Domain.GetDefaultValueForMemberPath(member, new LeftCellWrapper[1] { item }, _config);
					}
				}
			}
		}
		foreach (Tile<FragmentQuery> toFill in _views)
		{
			Tile<FragmentQuery> toAvoid2 = CreateTile(FragmentQuery.Create(_keyAttributes, BoolExpression.CreateNot(toFill.Query.Condition)));
			if (!RewriteQuery(toFill, toAvoid2, out var rewriting2, out var _, isRelaxed: true))
			{
				LeftCellWrapper leftCellWrapper = _context.AllWrappersForExtent.First((LeftCellWrapper lcr) => lcr.FragmentQuery.Equals(toFill.Query));
				ErrorLog.Record record = new ErrorLog.Record(ViewGenErrorCode.ImpossibleCondition, Strings.Viewgen_QV_RewritingNotFound(leftCellWrapper.RightExtent.ToString()), leftCellWrapper.Cells, string.Empty);
				_errorLog.AddEntry(record);
			}
			else
			{
				outputUsedViews.UnionWith(rewriting2.GetNamedQueries());
			}
		}
	}

	private List<LeftCellWrapper> RemapFromVariables()
	{
		List<LeftCellWrapper> list = new List<LeftCellWrapper>();
		int num = 0;
		Dictionary<BoolLiteral, BoolLiteral> dictionary = new Dictionary<BoolLiteral, BoolLiteral>(BoolLiteral.EqualityIdentifierComparer);
		foreach (LeftCellWrapper item in _context.AllWrappersForExtent)
		{
			if (_usedViews.Contains(item.FragmentQuery))
			{
				list.Add(item);
				int cellNumber = item.OnlyInputCell.CellNumber;
				if (num != cellNumber)
				{
					dictionary[new CellIdBoolean(_identifiers, cellNumber)] = new CellIdBoolean(_identifiers, num);
				}
				num++;
			}
		}
		if (dictionary.Count > 0)
		{
			_topLevelWhereClause = _topLevelWhereClause.RemapLiterals(dictionary);
			Dictionary<MemberPath, CaseStatement> dictionary2 = new Dictionary<MemberPath, CaseStatement>();
			foreach (KeyValuePair<MemberPath, CaseStatement> caseStatement2 in _caseStatements)
			{
				CaseStatement caseStatement = new CaseStatement(caseStatement2.Key);
				foreach (CaseStatement.WhenThen clause in caseStatement2.Value.Clauses)
				{
					caseStatement.AddWhenThen(clause.Condition.RemapLiterals(dictionary), clause.Value);
				}
				dictionary2[caseStatement2.Key] = caseStatement;
			}
			_caseStatements = dictionary2;
		}
		return list;
	}

	internal void AddTrivialCaseStatementsForConditionMembers()
	{
		for (int i = 0; i < _context.MemberMaps.ProjectedSlotMap.Count; i++)
		{
			MemberPath memberPath = _context.MemberMaps.ProjectedSlotMap[i];
			if (!memberPath.IsScalarType() && !_caseStatements.ContainsKey(memberPath))
			{
				Constant value = new TypeConstant(memberPath.EdmType);
				CaseStatement caseStatement = new CaseStatement(memberPath);
				caseStatement.AddWhenThen(BoolExpression.True, new ConstantProjectedSlot(value));
				_caseStatements[memberPath] = caseStatement;
			}
		}
	}

	private bool FindRewritingAndUsedViews(IEnumerable<MemberPath> attributes, BoolExpression whereClause, HashSet<FragmentQuery> outputUsedViews, out Tile<FragmentQuery> rewriting)
	{
		IEnumerable<MemberPath> notCoveredAttributes;
		return FindRewritingAndUsedViews(attributes, whereClause, outputUsedViews, out rewriting, out notCoveredAttributes);
	}

	private bool FindRewritingAndUsedViews(IEnumerable<MemberPath> attributes, BoolExpression whereClause, HashSet<FragmentQuery> outputUsedViews, out Tile<FragmentQuery> rewriting, out IEnumerable<MemberPath> notCoveredAttributes)
	{
		if (FindRewriting(attributes, whereClause, out rewriting, out notCoveredAttributes))
		{
			outputUsedViews.UnionWith(rewriting.GetNamedQueries());
			return true;
		}
		return false;
	}

	private bool FindRewriting(IEnumerable<MemberPath> attributes, BoolExpression whereClause, out Tile<FragmentQuery> rewriting, out IEnumerable<MemberPath> notCoveredAttributes)
	{
		Tile<FragmentQuery> toFill = CreateTile(FragmentQuery.Create(attributes, whereClause));
		Tile<FragmentQuery> toAvoid = CreateTile(FragmentQuery.Create(_keyAttributes, BoolExpression.CreateNot(whereClause)));
		bool isRelaxed = _context.ViewTarget == ViewTarget.UpdateView;
		return RewriteQuery(toFill, toAvoid, out rewriting, out notCoveredAttributes, isRelaxed);
	}

	private bool RewriteQuery(Tile<FragmentQuery> toFill, Tile<FragmentQuery> toAvoid, out Tile<FragmentQuery> rewriting, out IEnumerable<MemberPath> notCoveredAttributes, bool isRelaxed)
	{
		notCoveredAttributes = new List<MemberPath>();
		FragmentQuery fragmentQuery = toFill.Query;
		if (_context.TryGetCachedRewriting(fragmentQuery, out rewriting))
		{
			return true;
		}
		IEnumerable<Tile<FragmentQuery>> relevantViews = GetRelevantViews(fragmentQuery);
		FragmentQuery query = fragmentQuery;
		if (!RewriteQueryCached(CreateTile(FragmentQuery.Create(fragmentQuery.Condition)), toAvoid, relevantViews, out rewriting))
		{
			if (!isRelaxed)
			{
				return false;
			}
			fragmentQuery = FragmentQuery.Create(fragmentQuery.Attributes, BoolExpression.CreateAndNot(fragmentQuery.Condition, rewriting.Query.Condition));
			if (_qp.IsEmpty(CreateTile(fragmentQuery)) || !RewriteQueryCached(CreateTile(FragmentQuery.Create(fragmentQuery.Condition)), toAvoid, relevantViews, out rewriting))
			{
				return false;
			}
		}
		if (fragmentQuery.Attributes.Count == 0)
		{
			return true;
		}
		Dictionary<MemberPath, FragmentQuery> dictionary = new Dictionary<MemberPath, FragmentQuery>();
		foreach (MemberPath item in NonKeys(fragmentQuery.Attributes))
		{
			dictionary[item] = fragmentQuery;
		}
		if (dictionary.Count == 0 || CoverAttributes(ref rewriting, dictionary))
		{
			GetUsedViewsAndRemoveTrueSurrogate(ref rewriting);
			_context.SetCachedRewriting(query, rewriting);
			return true;
		}
		if (isRelaxed)
		{
			foreach (MemberPath item2 in NonKeys(fragmentQuery.Attributes))
			{
				if (dictionary.TryGetValue(item2, out var value))
				{
					dictionary[item2] = FragmentQuery.Create(BoolExpression.CreateAndNot(fragmentQuery.Condition, value.Condition));
				}
				else
				{
					dictionary[item2] = fragmentQuery;
				}
			}
			if (CoverAttributes(ref rewriting, dictionary))
			{
				GetUsedViewsAndRemoveTrueSurrogate(ref rewriting);
				_context.SetCachedRewriting(query, rewriting);
				return true;
			}
		}
		notCoveredAttributes = dictionary.Keys;
		return false;
	}

	private bool RewriteQueryCached(Tile<FragmentQuery> toFill, Tile<FragmentQuery> toAvoid, IEnumerable<Tile<FragmentQuery>> views, out Tile<FragmentQuery> rewriting)
	{
		if (!_context.TryGetCachedRewriting(toFill.Query, out rewriting))
		{
			bool num = _qp.RewriteQuery(toFill, toAvoid, views, out rewriting);
			if (num)
			{
				_context.SetCachedRewriting(toFill.Query, rewriting);
			}
			return num;
		}
		return true;
	}

	private bool CoverAttributes(ref Tile<FragmentQuery> rewriting, Dictionary<MemberPath, FragmentQuery> attributeConditions)
	{
		foreach (FragmentQuery item in new HashSet<FragmentQuery>(rewriting.GetNamedQueries()))
		{
			foreach (MemberPath item2 in NonKeys(item.Attributes))
			{
				CoverAttribute(item2, item, attributeConditions);
			}
			if (attributeConditions.Count == 0)
			{
				return true;
			}
		}
		Tile<FragmentQuery> tile = null;
		foreach (FragmentQuery fragmentQuery in _fragmentQueries)
		{
			foreach (MemberPath item3 in NonKeys(fragmentQuery.Attributes))
			{
				if (CoverAttribute(item3, fragmentQuery, attributeConditions))
				{
					tile = ((tile == null) ? CreateTile(fragmentQuery) : _qp.Union(tile, CreateTile(fragmentQuery)));
				}
			}
			if (attributeConditions.Count == 0)
			{
				break;
			}
		}
		if (attributeConditions.Count == 0)
		{
			rewriting = _qp.Join(rewriting, tile);
			return true;
		}
		return false;
	}

	private bool CoverAttribute(MemberPath projectedAttribute, FragmentQuery view, Dictionary<MemberPath, FragmentQuery> attributeConditions)
	{
		if (attributeConditions.TryGetValue(projectedAttribute, out var value))
		{
			value = FragmentQuery.Create(BoolExpression.CreateAndNot(value.Condition, view.Condition));
			if (_qp.IsEmpty(CreateTile(value)))
			{
				attributeConditions.Remove(projectedAttribute);
			}
			else
			{
				attributeConditions[projectedAttribute] = value;
			}
			return true;
		}
		return false;
	}

	private IEnumerable<Tile<FragmentQuery>> GetRelevantViews(FragmentQuery query)
	{
		Set<MemberPath> variables = GetVariables(query);
		Tile<FragmentQuery> tile = null;
		List<Tile<FragmentQuery>> list = new List<Tile<FragmentQuery>>();
		Tile<FragmentQuery> tile2 = null;
		foreach (Tile<FragmentQuery> view in _views)
		{
			if (GetVariables(view.Query).Overlaps(variables))
			{
				tile = ((tile == null) ? view : _qp.Union(tile, view));
				list.Add(view);
			}
			else if (IsTrue(view.Query) && tile2 == null)
			{
				tile2 = view;
			}
		}
		if (tile != null && IsTrue(tile.Query))
		{
			return list;
		}
		if (tile2 == null)
		{
			Tile<FragmentQuery> tile3 = null;
			foreach (FragmentQuery fragmentQuery in _fragmentQueries)
			{
				tile3 = ((tile3 == null) ? CreateTile(fragmentQuery) : _qp.Union(tile3, CreateTile(fragmentQuery)));
				if (IsTrue(tile3.Query))
				{
					tile2 = _trueViewSurrogate;
					break;
				}
			}
		}
		if (tile2 != null)
		{
			list.Add(tile2);
			return list;
		}
		return _views;
	}

	private HashSet<FragmentQuery> GetUsedViewsAndRemoveTrueSurrogate(ref Tile<FragmentQuery> rewriting)
	{
		HashSet<FragmentQuery> hashSet = new HashSet<FragmentQuery>(rewriting.GetNamedQueries());
		if (!hashSet.Contains(_trueViewSurrogate.Query))
		{
			return hashSet;
		}
		hashSet.Remove(_trueViewSurrogate.Query);
		Tile<FragmentQuery> tile = null;
		foreach (FragmentQuery item in hashSet.Concat(_fragmentQueries))
		{
			tile = ((tile == null) ? CreateTile(item) : _qp.Union(tile, CreateTile(item)));
			hashSet.Add(item);
			if (IsTrue(tile.Query))
			{
				rewriting = rewriting.Replace(_trueViewSurrogate, tile);
				return hashSet;
			}
		}
		return hashSet;
	}

	private BoolExpression CreateMemberCondition(MemberPath path, Constant domainValue)
	{
		return FragmentQuery.CreateMemberCondition(path, domainValue, _domainMap);
	}

	private FragmentQuery CreateMemberConditionQuery(MemberPath currentPath, Constant domainValue)
	{
		return CreateMemberConditionQuery(currentPath, domainValue, _keyAttributes, _domainMap);
	}

	internal static FragmentQuery CreateMemberConditionQuery(MemberPath currentPath, Constant domainValue, IEnumerable<MemberPath> keyAttributes, MemberDomainMap domainMap)
	{
		BoolExpression whereClause = FragmentQuery.CreateMemberCondition(currentPath, domainValue, domainMap);
		IEnumerable<MemberPath> attrs = keyAttributes;
		if (domainValue is NegatedConstant)
		{
			attrs = keyAttributes.Concat(new MemberPath[1] { currentPath });
		}
		return FragmentQuery.Create(attrs, whereClause);
	}

	private static TileNamed<FragmentQuery> CreateTile(FragmentQuery query)
	{
		return new TileNamed<FragmentQuery>(query);
	}

	private static IEnumerable<Constant> GetTypeConstants(IEnumerable<EdmType> types)
	{
		foreach (EdmType type in types)
		{
			yield return new TypeConstant(type);
		}
	}

	private static IEnumerable<MemberPath> GetNonConditionalScalarMembers(EdmType edmType, MemberPath currentPath, MemberDomainMap domainMap)
	{
		return currentPath.GetMembers(edmType, true, false, null, domainMap);
	}

	private static IEnumerable<MemberPath> GetConditionalComplexMembers(EdmType edmType, MemberPath currentPath, MemberDomainMap domainMap)
	{
		return currentPath.GetMembers(edmType, false, true, null, domainMap);
	}

	private static IEnumerable<MemberPath> GetNonConditionalComplexMembers(EdmType edmType, MemberPath currentPath, MemberDomainMap domainMap)
	{
		return currentPath.GetMembers(edmType, false, false, null, domainMap);
	}

	private static IEnumerable<MemberPath> GetConditionalScalarMembers(EdmType edmType, MemberPath currentPath, MemberDomainMap domainMap)
	{
		return currentPath.GetMembers(edmType, true, true, null, domainMap);
	}

	private static IEnumerable<MemberPath> NonKeys(IEnumerable<MemberPath> attributes)
	{
		return attributes.Where((MemberPath attr) => !attr.IsPartOfKey);
	}

	internal static CellTreeNode TileToCellTree(Tile<FragmentQuery> tile, ViewgenContext context)
	{
		if (tile.OpKind == TileOpKind.Named)
		{
			FragmentQuery view = ((TileNamed<FragmentQuery>)tile).NamedQuery;
			LeftCellWrapper cellWrapper = context.AllWrappersForExtent.First((LeftCellWrapper w) => w.FragmentQuery == view);
			return new LeafCellTreeNode(context, cellWrapper);
		}
		CellTreeOpType opType;
		switch (tile.OpKind)
		{
		case TileOpKind.Join:
			opType = CellTreeOpType.IJ;
			break;
		case TileOpKind.AntiSemiJoin:
			opType = CellTreeOpType.LASJ;
			break;
		case TileOpKind.Union:
			opType = CellTreeOpType.Union;
			break;
		default:
			return null;
		}
		return new OpCellTreeNode(context, opType, TileToCellTree(tile.Arg1, context), TileToCellTree(tile.Arg2, context));
	}

	private static BoolExpression TileToBoolExpr(Tile<FragmentQuery> tile)
	{
		switch (tile.OpKind)
		{
		case TileOpKind.Named:
		{
			FragmentQuery namedQuery = ((TileNamed<FragmentQuery>)tile).NamedQuery;
			if (namedQuery.Condition.IsAlwaysTrue())
			{
				return BoolExpression.True;
			}
			return namedQuery.FromVariable;
		}
		case TileOpKind.Join:
			return BoolExpression.CreateAnd(TileToBoolExpr(tile.Arg1), TileToBoolExpr(tile.Arg2));
		case TileOpKind.AntiSemiJoin:
			return BoolExpression.CreateAnd(TileToBoolExpr(tile.Arg1), BoolExpression.CreateNot(TileToBoolExpr(tile.Arg2)));
		case TileOpKind.Union:
			return BoolExpression.CreateOr(TileToBoolExpr(tile.Arg1), TileToBoolExpr(tile.Arg2));
		default:
			return null;
		}
	}

	private static bool IsDefaultValue(Constant domainValue, MemberPath path)
	{
		if (domainValue.IsNull() && path.IsNullable)
		{
			return true;
		}
		if (path.DefaultValue != null)
		{
			return (domainValue as ScalarConstant).Value == path.DefaultValue;
		}
		return false;
	}

	private static Set<MemberPath> GetVariables(FragmentQuery query)
	{
		return new Set<MemberPath>(from domainConstraint in query.Condition.VariableConstraints
			where domainConstraint.Variable.Identifier is MemberRestriction && !domainConstraint.Variable.Domain.All((Constant constant) => domainConstraint.Range.Contains(constant))
			select ((MemberRestriction)domainConstraint.Variable.Identifier).RestrictedMemberSlot.MemberPath, MemberPath.EqualityComparer);
	}

	private bool IsTrue(FragmentQuery query)
	{
		return !_context.LeftFragmentQP.IsSatisfiable(FragmentQuery.Create(BoolExpression.CreateNot(query.Condition)));
	}

	[Conditional("DEBUG")]
	private void PrintStatistics(RewritingProcessor<Tile<FragmentQuery>> qp)
	{
		qp.GetStatistics(out var _, out var _, out var _, out var _, out var _);
	}

	[Conditional("DEBUG")]
	internal void TraceVerbose(string msg, params object[] parameters)
	{
		if (_config.IsVerboseTracing)
		{
			Helpers.FormatTraceLine(msg, parameters);
		}
	}
}
