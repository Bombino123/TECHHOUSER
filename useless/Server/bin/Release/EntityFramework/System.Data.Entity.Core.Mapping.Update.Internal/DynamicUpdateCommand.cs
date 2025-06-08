using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class DynamicUpdateCommand : UpdateCommand
{
	private readonly ModificationOperator _operator;

	private readonly TableChangeProcessor _processor;

	private readonly List<KeyValuePair<int, DbSetClause>> _inputIdentifiers;

	private readonly Dictionary<int, string> _outputIdentifiers;

	private readonly DbModificationCommandTree _modificationCommandTree;

	internal ModificationOperator Operator => _operator;

	internal override EntitySet Table => _processor.Table;

	internal override IEnumerable<int> InputIdentifiers
	{
		get
		{
			if (_inputIdentifiers == null)
			{
				yield break;
			}
			foreach (KeyValuePair<int, DbSetClause> inputIdentifier in _inputIdentifiers)
			{
				yield return inputIdentifier.Key;
			}
		}
	}

	internal override IEnumerable<int> OutputIdentifiers
	{
		get
		{
			if (_outputIdentifiers == null)
			{
				return Enumerable.Empty<int>();
			}
			return _outputIdentifiers.Keys;
		}
	}

	internal override UpdateCommandKind Kind => UpdateCommandKind.Dynamic;

	internal DynamicUpdateCommand(TableChangeProcessor processor, UpdateTranslator translator, ModificationOperator modificationOperator, PropagatorResult originalValues, PropagatorResult currentValues, DbModificationCommandTree tree, Dictionary<int, string> outputIdentifiers)
		: base(translator, originalValues, currentValues)
	{
		_processor = processor;
		_operator = modificationOperator;
		_modificationCommandTree = tree;
		_outputIdentifiers = outputIdentifiers;
		if (ModificationOperator.Insert != modificationOperator && modificationOperator != 0)
		{
			return;
		}
		_inputIdentifiers = new List<KeyValuePair<int, DbSetClause>>(2);
		foreach (KeyValuePair<EdmMember, PropagatorResult> item in Helper.PairEnumerations(TypeHelpers.GetAllStructuralMembers(base.CurrentValues.StructuralType), base.CurrentValues.GetMemberValues()))
		{
			int identifier = item.Value.Identifier;
			if (-1 == identifier || !TryGetSetterExpression(tree, item.Key, modificationOperator, out var setter))
			{
				continue;
			}
			foreach (int principal in translator.KeyManager.GetPrincipals(identifier))
			{
				_inputIdentifiers.Add(new KeyValuePair<int, DbSetClause>(principal, setter));
			}
		}
	}

	private static bool TryGetSetterExpression(DbModificationCommandTree tree, EdmMember member, ModificationOperator op, out DbSetClause setter)
	{
		IEnumerable<DbModificationClause> enumerable = ((ModificationOperator.Insert != op) ? ((DbUpdateCommandTree)tree).SetClauses : ((DbInsertCommandTree)tree).SetClauses);
		foreach (DbSetClause item in enumerable)
		{
			if (((DbPropertyExpression)item.Property).Property.EdmEquals(member))
			{
				setter = item;
				return true;
			}
		}
		setter = null;
		return false;
	}

	internal override long Execute(Dictionary<int, object> identifierValues, List<KeyValuePair<PropagatorResult, object>> generatedValues)
	{
		using DbCommand dbCommand = CreateCommand(identifierValues);
		EntityConnection connection = base.Translator.Connection;
		dbCommand.Transaction = ((connection.CurrentTransaction == null) ? null : connection.CurrentTransaction.StoreTransaction);
		dbCommand.Connection = connection.StoreConnection;
		if (base.Translator.CommandTimeout.HasValue)
		{
			dbCommand.CommandTimeout = base.Translator.CommandTimeout.Value;
		}
		int num;
		if (_modificationCommandTree.HasReader)
		{
			num = 0;
			using DbDataReader dbDataReader = dbCommand.ExecuteReader(CommandBehavior.SequentialAccess);
			if (dbDataReader.Read())
			{
				num++;
				IBaseList<EdmMember> allStructuralMembers = TypeHelpers.GetAllStructuralMembers(base.CurrentValues.StructuralType);
				for (int i = 0; i < dbDataReader.FieldCount; i++)
				{
					string name = dbDataReader.GetName(i);
					EdmMember edmMember = allStructuralMembers[name];
					object value = ((!Helper.IsSpatialType(edmMember.TypeUsage) || dbDataReader.IsDBNull(i)) ? dbDataReader.GetValue(i) : SpatialHelpers.GetSpatialValue(base.Translator.MetadataWorkspace, dbDataReader, edmMember.TypeUsage, i));
					int ordinal = allStructuralMembers.IndexOf(edmMember);
					PropagatorResult memberValue = base.CurrentValues.GetMemberValue(ordinal);
					generatedValues.Add(new KeyValuePair<PropagatorResult, object>(memberValue, value));
					int identifier = memberValue.Identifier;
					if (-1 != identifier)
					{
						identifierValues.Add(identifier, value);
					}
				}
			}
			CommandHelper.ConsumeReader(dbDataReader);
		}
		else
		{
			num = dbCommand.ExecuteNonQuery();
		}
		return num;
	}

	internal override async Task<long> ExecuteAsync(Dictionary<int, object> identifierValues, List<KeyValuePair<PropagatorResult, object>> generatedValues, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using DbCommand command = CreateCommand(identifierValues);
		EntityConnection connection = base.Translator.Connection;
		command.Transaction = ((connection.CurrentTransaction == null) ? null : connection.CurrentTransaction.StoreTransaction);
		command.Connection = connection.StoreConnection;
		if (base.Translator.CommandTimeout.HasValue)
		{
			command.CommandTimeout = base.Translator.CommandTimeout.Value;
		}
		int rowsAffected;
		if (_modificationCommandTree.HasReader)
		{
			rowsAffected = 0;
			using DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken).WithCurrentCulture();
			if (await reader.ReadAsync(cancellationToken).WithCurrentCulture())
			{
				rowsAffected++;
				IBaseList<EdmMember> members = TypeHelpers.GetAllStructuralMembers(base.CurrentValues.StructuralType);
				for (int ordinal = 0; ordinal < reader.FieldCount; ordinal++)
				{
					string name = reader.GetName(ordinal);
					EdmMember member = members[name];
					bool flag = Helper.IsSpatialType(member.TypeUsage);
					if (flag)
					{
						flag = !(await reader.IsDBNullAsync(ordinal, cancellationToken).WithCurrentCulture());
					}
					object value = ((!flag) ? (await reader.GetFieldValueAsync<object>(ordinal, cancellationToken).WithCurrentCulture()) : (await SpatialHelpers.GetSpatialValueAsync(base.Translator.MetadataWorkspace, reader, member.TypeUsage, ordinal, cancellationToken).WithCurrentCulture()));
					int ordinal2 = members.IndexOf(member);
					PropagatorResult memberValue = base.CurrentValues.GetMemberValue(ordinal2);
					generatedValues.Add(new KeyValuePair<PropagatorResult, object>(memberValue, value));
					int identifier = memberValue.Identifier;
					if (-1 != identifier)
					{
						identifierValues.Add(identifier, value);
					}
				}
			}
			await CommandHelper.ConsumeReaderAsync(reader, cancellationToken).WithCurrentCulture();
		}
		else
		{
			rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).WithCurrentCulture();
		}
		return rowsAffected;
	}

	protected virtual DbCommand CreateCommand(Dictionary<int, object> identifierValues)
	{
		DbModificationCommandTree dbModificationCommandTree = _modificationCommandTree;
		if (_inputIdentifiers != null)
		{
			Dictionary<DbSetClause, DbSetClause> dictionary = new Dictionary<DbSetClause, DbSetClause>();
			for (int i = 0; i < _inputIdentifiers.Count; i++)
			{
				KeyValuePair<int, DbSetClause> keyValuePair = _inputIdentifiers[i];
				if (identifierValues.TryGetValue(keyValuePair.Key, out var value))
				{
					DbSetClause value2 = new DbSetClause(keyValuePair.Value.Property, DbExpressionBuilder.Constant(value));
					dictionary[keyValuePair.Value] = value2;
					_inputIdentifiers[i] = new KeyValuePair<int, DbSetClause>(keyValuePair.Key, value2);
				}
			}
			dbModificationCommandTree = RebuildCommandTree(dbModificationCommandTree, dictionary);
		}
		return base.Translator.CreateCommand(dbModificationCommandTree);
	}

	private static DbModificationCommandTree RebuildCommandTree(DbModificationCommandTree originalTree, Dictionary<DbSetClause, DbSetClause> clauseMappings)
	{
		if (clauseMappings.Count == 0)
		{
			return originalTree;
		}
		if (originalTree.CommandTreeKind == DbCommandTreeKind.Insert)
		{
			DbInsertCommandTree dbInsertCommandTree = (DbInsertCommandTree)originalTree;
			return new DbInsertCommandTree(dbInsertCommandTree.MetadataWorkspace, dbInsertCommandTree.DataSpace, dbInsertCommandTree.Target, new ReadOnlyCollection<DbModificationClause>(ReplaceClauses(dbInsertCommandTree.SetClauses, clauseMappings)), dbInsertCommandTree.Returning);
		}
		DbUpdateCommandTree dbUpdateCommandTree = (DbUpdateCommandTree)originalTree;
		return new DbUpdateCommandTree(dbUpdateCommandTree.MetadataWorkspace, dbUpdateCommandTree.DataSpace, dbUpdateCommandTree.Target, dbUpdateCommandTree.Predicate, new ReadOnlyCollection<DbModificationClause>(ReplaceClauses(dbUpdateCommandTree.SetClauses, clauseMappings)), dbUpdateCommandTree.Returning);
	}

	private static List<DbModificationClause> ReplaceClauses(IList<DbModificationClause> originalClauses, Dictionary<DbSetClause, DbSetClause> mappings)
	{
		List<DbModificationClause> list = new List<DbModificationClause>(originalClauses.Count);
		for (int i = 0; i < originalClauses.Count; i++)
		{
			if (mappings.TryGetValue((DbSetClause)originalClauses[i], out var value))
			{
				list.Add(value);
			}
			else
			{
				list.Add(originalClauses[i]);
			}
		}
		return list;
	}

	internal override IList<IEntityStateEntry> GetStateEntries(UpdateTranslator translator)
	{
		List<IEntityStateEntry> list = new List<IEntityStateEntry>(2);
		if (base.OriginalValues != null)
		{
			foreach (IEntityStateEntry allStateEntry in SourceInterpreter.GetAllStateEntries(base.OriginalValues, translator, Table))
			{
				list.Add(allStateEntry);
			}
		}
		if (base.CurrentValues != null)
		{
			foreach (IEntityStateEntry allStateEntry2 in SourceInterpreter.GetAllStateEntries(base.CurrentValues, translator, Table))
			{
				list.Add(allStateEntry2);
			}
		}
		return list;
	}

	internal override int CompareToType(UpdateCommand otherCommand)
	{
		DynamicUpdateCommand dynamicUpdateCommand = (DynamicUpdateCommand)otherCommand;
		int num = Operator - dynamicUpdateCommand.Operator;
		if (num != 0)
		{
			return num;
		}
		num = StringComparer.Ordinal.Compare(_processor.Table.Name, dynamicUpdateCommand._processor.Table.Name);
		if (num != 0)
		{
			return num;
		}
		num = StringComparer.Ordinal.Compare(_processor.Table.EntityContainer.Name, dynamicUpdateCommand._processor.Table.EntityContainer.Name);
		if (num != 0)
		{
			return num;
		}
		PropagatorResult propagatorResult = ((Operator == ModificationOperator.Delete) ? base.OriginalValues : base.CurrentValues);
		PropagatorResult propagatorResult2 = ((dynamicUpdateCommand.Operator == ModificationOperator.Delete) ? dynamicUpdateCommand.OriginalValues : dynamicUpdateCommand.CurrentValues);
		for (int i = 0; i < _processor.KeyOrdinals.Length; i++)
		{
			int ordinal = _processor.KeyOrdinals[i];
			object simpleValue = propagatorResult.GetMemberValue(ordinal).GetSimpleValue();
			object simpleValue2 = propagatorResult2.GetMemberValue(ordinal).GetSimpleValue();
			num = ByValueComparer.Default.Compare(simpleValue, simpleValue2);
			if (num != 0)
			{
				return num;
			}
		}
		for (int j = 0; j < _processor.KeyOrdinals.Length; j++)
		{
			int ordinal2 = _processor.KeyOrdinals[j];
			int identifier = propagatorResult.GetMemberValue(ordinal2).Identifier;
			int identifier2 = propagatorResult2.GetMemberValue(ordinal2).Identifier;
			num = identifier - identifier2;
			if (num != 0)
			{
				return num;
			}
		}
		return num;
	}
}
