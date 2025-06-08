using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public sealed class ModificationFunctionMapping : MappingItem
{
	private FunctionParameter _rowsAffectedParameter;

	private readonly EdmFunction _function;

	private readonly ReadOnlyCollection<ModificationFunctionParameterBinding> _parameterBindings;

	private readonly ReadOnlyCollection<AssociationSetEnd> _collocatedAssociationSetEnds;

	private readonly ReadOnlyCollection<ModificationFunctionResultBinding> _resultBindings;

	public FunctionParameter RowsAffectedParameter
	{
		get
		{
			return _rowsAffectedParameter;
		}
		internal set
		{
			_rowsAffectedParameter = value;
		}
	}

	internal string RowsAffectedParameterName
	{
		get
		{
			if (RowsAffectedParameter == null)
			{
				return null;
			}
			return RowsAffectedParameter.Name;
		}
	}

	public EdmFunction Function => _function;

	public ReadOnlyCollection<ModificationFunctionParameterBinding> ParameterBindings => _parameterBindings;

	internal ReadOnlyCollection<AssociationSetEnd> CollocatedAssociationSetEnds => _collocatedAssociationSetEnds;

	public ReadOnlyCollection<ModificationFunctionResultBinding> ResultBindings => _resultBindings;

	public ModificationFunctionMapping(EntitySetBase entitySet, EntityTypeBase entityType, EdmFunction function, IEnumerable<ModificationFunctionParameterBinding> parameterBindings, FunctionParameter rowsAffectedParameter, IEnumerable<ModificationFunctionResultBinding> resultBindings)
	{
		Check.NotNull(entitySet, "entitySet");
		Check.NotNull(function, "function");
		Check.NotNull(parameterBindings, "parameterBindings");
		_function = function;
		_rowsAffectedParameter = rowsAffectedParameter;
		_parameterBindings = new ReadOnlyCollection<ModificationFunctionParameterBinding>(parameterBindings.ToList());
		if (resultBindings != null)
		{
			List<ModificationFunctionResultBinding> list = resultBindings.ToList();
			if (0 < list.Count)
			{
				_resultBindings = new ReadOnlyCollection<ModificationFunctionResultBinding>(list);
			}
		}
		_collocatedAssociationSetEnds = new ReadOnlyCollection<AssociationSetEnd>(GetReferencedAssociationSetEnds(entitySet as EntitySet, entityType as EntityType, parameterBindings).ToList());
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "Func{{{0}}}: Prm={{{1}}}, Result={{{2}}}", new object[3]
		{
			Function,
			StringUtil.ToCommaSeparatedStringSorted(ParameterBindings),
			StringUtil.ToCommaSeparatedStringSorted(ResultBindings)
		});
	}

	internal override void SetReadOnly()
	{
		MappingItem.SetReadOnly(_parameterBindings);
		MappingItem.SetReadOnly(_resultBindings);
		base.SetReadOnly();
	}

	private static IEnumerable<AssociationSetEnd> GetReferencedAssociationSetEnds(EntitySet entitySet, EntityType entityType, IEnumerable<ModificationFunctionParameterBinding> parameterBindings)
	{
		HashSet<AssociationSetEnd> hashSet = new HashSet<AssociationSetEnd>();
		if (entitySet != null && entityType != null)
		{
			foreach (ModificationFunctionParameterBinding parameterBinding in parameterBindings)
			{
				AssociationSetEnd associationSetEnd = parameterBinding.MemberPath.AssociationSetEnd;
				if (associationSetEnd != null)
				{
					hashSet.Add(associationSetEnd);
				}
			}
			foreach (AssociationSet associationSet in entitySet.AssociationSets)
			{
				ReadOnlyMetadataCollection<ReferentialConstraint> referentialConstraints = associationSet.ElementType.ReferentialConstraints;
				if (referentialConstraints == null)
				{
					continue;
				}
				foreach (ReferentialConstraint item in referentialConstraints)
				{
					if (associationSet.AssociationSetEnds[item.ToRole.Name].EntitySet == entitySet && item.ToRole.GetEntityType().IsAssignableFrom(entityType))
					{
						hashSet.Add(associationSet.AssociationSetEnds[item.FromRole.Name]);
					}
				}
			}
		}
		return hashSet;
	}
}
