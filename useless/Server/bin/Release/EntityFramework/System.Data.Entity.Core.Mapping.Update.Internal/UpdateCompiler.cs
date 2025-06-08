using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal sealed class UpdateCompiler
{
	internal readonly UpdateTranslator m_translator;

	private const string s_targetVarName = "target";

	internal UpdateCompiler(UpdateTranslator translator)
	{
		m_translator = translator;
	}

	internal UpdateCommand BuildDeleteCommand(PropagatorResult oldRow, TableChangeProcessor processor)
	{
		bool rowMustBeTouched = true;
		DbExpressionBinding target = GetTarget(processor);
		DbExpression predicate = BuildPredicate(target, oldRow, null, processor, ref rowMustBeTouched);
		DbDeleteCommandTree tree = new DbDeleteCommandTree(m_translator.MetadataWorkspace, DataSpace.SSpace, target, predicate);
		return new DynamicUpdateCommand(processor, m_translator, ModificationOperator.Delete, oldRow, null, tree, null);
	}

	internal UpdateCommand BuildUpdateCommand(PropagatorResult oldRow, PropagatorResult newRow, TableChangeProcessor processor)
	{
		bool rowMustBeTouched = false;
		DbExpressionBinding target = GetTarget(processor);
		List<DbModificationClause> list = new List<DbModificationClause>();
		Dictionary<int, string> outputIdentifiers;
		DbExpression returning;
		foreach (DbModificationClause item in BuildSetClauses(target, newRow, oldRow, processor, insertMode: false, out outputIdentifiers, out returning, ref rowMustBeTouched))
		{
			list.Add(item);
		}
		DbExpression predicate = BuildPredicate(target, oldRow, newRow, processor, ref rowMustBeTouched);
		if (list.Count == 0)
		{
			if (rowMustBeTouched)
			{
				List<IEntityStateEntry> list2 = new List<IEntityStateEntry>();
				list2.AddRange(SourceInterpreter.GetAllStateEntries(oldRow, m_translator, processor.Table));
				list2.AddRange(SourceInterpreter.GetAllStateEntries(newRow, m_translator, processor.Table));
				if (list2.All((IEntityStateEntry it) => it.State == EntityState.Unchanged))
				{
					rowMustBeTouched = false;
				}
			}
			if (!rowMustBeTouched)
			{
				return null;
			}
		}
		DbUpdateCommandTree tree = new DbUpdateCommandTree(m_translator.MetadataWorkspace, DataSpace.SSpace, target, predicate, new ReadOnlyCollection<DbModificationClause>(list), returning);
		return new DynamicUpdateCommand(processor, m_translator, ModificationOperator.Update, oldRow, newRow, tree, outputIdentifiers);
	}

	internal UpdateCommand BuildInsertCommand(PropagatorResult newRow, TableChangeProcessor processor)
	{
		DbExpressionBinding target = GetTarget(processor);
		bool rowMustBeTouched = true;
		List<DbModificationClause> list = new List<DbModificationClause>();
		Dictionary<int, string> outputIdentifiers;
		DbExpression returning;
		foreach (DbModificationClause item in BuildSetClauses(target, newRow, null, processor, insertMode: true, out outputIdentifiers, out returning, ref rowMustBeTouched))
		{
			list.Add(item);
		}
		DbInsertCommandTree tree = new DbInsertCommandTree(m_translator.MetadataWorkspace, DataSpace.SSpace, target, new ReadOnlyCollection<DbModificationClause>(list), returning);
		return new DynamicUpdateCommand(processor, m_translator, ModificationOperator.Insert, null, newRow, tree, outputIdentifiers);
	}

	private IEnumerable<DbModificationClause> BuildSetClauses(DbExpressionBinding target, PropagatorResult row, PropagatorResult originalRow, TableChangeProcessor processor, bool insertMode, out Dictionary<int, string> outputIdentifiers, out DbExpression returning, ref bool rowMustBeTouched)
	{
		Dictionary<EdmProperty, PropagatorResult> dictionary = new Dictionary<EdmProperty, PropagatorResult>();
		List<KeyValuePair<string, DbExpression>> list = new List<KeyValuePair<string, DbExpression>>();
		outputIdentifiers = new Dictionary<int, string>();
		PropagatorFlags propagatorFlags = ((!insertMode) ? (PropagatorFlags.Preserve | PropagatorFlags.Unknown) : PropagatorFlags.NoFlags);
		for (int i = 0; i < processor.Table.ElementType.Properties.Count; i++)
		{
			EdmProperty edmProperty = processor.Table.ElementType.Properties[i];
			PropagatorResult propagatorResult = row.GetMemberValue(i);
			if (-1 != propagatorResult.Identifier)
			{
				propagatorResult = propagatorResult.ReplicateResultWithNewValue(m_translator.KeyManager.GetPrincipalValue(propagatorResult));
			}
			bool flag = false;
			bool flag2 = false;
			for (int j = 0; j < processor.KeyOrdinals.Length; j++)
			{
				if (processor.KeyOrdinals[j] == i)
				{
					flag2 = true;
					break;
				}
			}
			PropagatorFlags propagatorFlags2 = PropagatorFlags.NoFlags;
			if (!insertMode && flag2)
			{
				flag = true;
			}
			else
			{
				propagatorFlags2 |= propagatorResult.PropagatorFlags;
			}
			StoreGeneratedPattern storeGeneratedPattern = MetadataHelper.GetStoreGeneratedPattern(edmProperty);
			bool flag3 = storeGeneratedPattern == StoreGeneratedPattern.Computed || (insertMode && storeGeneratedPattern == StoreGeneratedPattern.Identity);
			if (flag3)
			{
				DbPropertyExpression value = target.Variable.Property(edmProperty);
				list.Add(new KeyValuePair<string, DbExpression>(edmProperty.Name, value));
				int identifier = propagatorResult.Identifier;
				if (-1 != identifier)
				{
					if (m_translator.KeyManager.HasPrincipals(identifier))
					{
						throw new InvalidOperationException(Strings.Update_GeneratedDependent(edmProperty.Name));
					}
					outputIdentifiers.Add(identifier, edmProperty.Name);
					if (storeGeneratedPattern != StoreGeneratedPattern.Identity && processor.IsKeyProperty(i))
					{
						throw new NotSupportedException(Strings.Update_NotSupportedComputedKeyColumn("StoreGeneratedPattern", "Computed", "Identity", edmProperty.Name, edmProperty.DeclaringType.FullName));
					}
				}
			}
			if ((propagatorFlags2 & propagatorFlags) != 0)
			{
				flag = true;
			}
			else if (flag3)
			{
				flag = true;
				rowMustBeTouched = true;
			}
			if (!flag && !insertMode && storeGeneratedPattern == StoreGeneratedPattern.Identity)
			{
				PropagatorResult memberValue = originalRow.GetMemberValue(i);
				if (!ByValueEqualityComparer.Default.Equals(memberValue.GetSimpleValue(), propagatorResult.GetSimpleValue()))
				{
					throw new InvalidOperationException(Strings.Update_ModifyingIdentityColumn("Identity", edmProperty.Name, edmProperty.DeclaringType.FullName));
				}
				flag = true;
			}
			if (!flag)
			{
				dictionary.Add(edmProperty, propagatorResult);
			}
		}
		if (0 < list.Count)
		{
			returning = DbExpressionBuilder.NewRow(list);
		}
		else
		{
			returning = null;
		}
		List<DbModificationClause> list2 = new List<DbModificationClause>(dictionary.Count);
		foreach (KeyValuePair<EdmProperty, PropagatorResult> item in dictionary)
		{
			list2.Add(new DbSetClause(GeneratePropertyExpression(target, item.Key), GenerateValueExpression(item.Key, item.Value)));
		}
		return list2;
	}

	private DbExpression BuildPredicate(DbExpressionBinding target, PropagatorResult referenceRow, PropagatorResult current, TableChangeProcessor processor, ref bool rowMustBeTouched)
	{
		Dictionary<EdmProperty, PropagatorResult> dictionary = new Dictionary<EdmProperty, PropagatorResult>();
		int num = 0;
		foreach (EdmProperty property in processor.Table.ElementType.Properties)
		{
			PropagatorResult memberValue = referenceRow.GetMemberValue(num);
			PropagatorResult input = current?.GetMemberValue(num);
			if (!rowMustBeTouched && (HasFlag(memberValue, PropagatorFlags.ConcurrencyValue) || HasFlag(input, PropagatorFlags.ConcurrencyValue)))
			{
				rowMustBeTouched = true;
			}
			if (!dictionary.ContainsKey(property) && (HasFlag(memberValue, PropagatorFlags.ConcurrencyValue | PropagatorFlags.Key) || HasFlag(input, PropagatorFlags.ConcurrencyValue | PropagatorFlags.Key)))
			{
				dictionary.Add(property, memberValue);
			}
			num++;
		}
		DbExpression dbExpression = null;
		foreach (KeyValuePair<EdmProperty, PropagatorResult> item in dictionary)
		{
			DbExpression dbExpression2 = GenerateEqualityExpression(target, item.Key, item.Value);
			dbExpression = ((dbExpression != null) ? dbExpression.And(dbExpression2) : dbExpression2);
		}
		return dbExpression;
	}

	private DbExpression GenerateEqualityExpression(DbExpressionBinding target, EdmProperty property, PropagatorResult value)
	{
		DbExpression dbExpression = GeneratePropertyExpression(target, property);
		DbExpression dbExpression2 = GenerateValueExpression(property, value);
		if (dbExpression2.ExpressionKind == DbExpressionKind.Null)
		{
			return dbExpression.IsNull();
		}
		return dbExpression.Equal(dbExpression2);
	}

	private static DbExpression GeneratePropertyExpression(DbExpressionBinding target, EdmProperty property)
	{
		return target.Variable.Property(property);
	}

	private DbExpression GenerateValueExpression(EdmProperty property, PropagatorResult value)
	{
		if (value.IsNull)
		{
			return Helper.GetModelTypeUsage(property).Null();
		}
		object obj = m_translator.KeyManager.GetPrincipalValue(value);
		if (Convert.IsDBNull(obj))
		{
			return Helper.GetModelTypeUsage(property).Null();
		}
		TypeUsage modelTypeUsage = Helper.GetModelTypeUsage(property);
		Type type = obj.GetType();
		if (type.IsEnum())
		{
			obj = Convert.ChangeType(obj, type.GetEnumUnderlyingType(), CultureInfo.InvariantCulture);
		}
		Type clrEquivalentType = ((PrimitiveType)modelTypeUsage.EdmType).ClrEquivalentType;
		if (type != clrEquivalentType)
		{
			obj = Convert.ChangeType(obj, clrEquivalentType, CultureInfo.InvariantCulture);
		}
		return modelTypeUsage.Constant(obj);
	}

	private static bool HasFlag(PropagatorResult input, PropagatorFlags flags)
	{
		if (input == null)
		{
			return false;
		}
		return (flags & input.PropagatorFlags) != 0;
	}

	private static DbExpressionBinding GetTarget(TableChangeProcessor processor)
	{
		return processor.Table.Scan().BindAs("target");
	}
}
