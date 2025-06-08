using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class FunctionUpdateCommand : UpdateCommand
{
	private readonly ReadOnlyCollection<IEntityStateEntry> _stateEntries;

	private readonly DbCommand _dbCommand;

	private List<KeyValuePair<int, DbParameter>> _inputIdentifiers;

	private Dictionary<int, string> _outputIdentifiers;

	private DbParameter _rowsAffectedParameter;

	protected virtual List<KeyValuePair<string, PropagatorResult>> ResultColumns { get; set; }

	internal override IEnumerable<int> InputIdentifiers
	{
		get
		{
			if (_inputIdentifiers == null)
			{
				yield break;
			}
			foreach (KeyValuePair<int, DbParameter> inputIdentifier in _inputIdentifiers)
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

	internal override UpdateCommandKind Kind => UpdateCommandKind.Function;

	internal FunctionUpdateCommand(ModificationFunctionMapping functionMapping, UpdateTranslator translator, ReadOnlyCollection<IEntityStateEntry> stateEntries, ExtractedStateEntry stateEntry)
		: this(translator, stateEntries, stateEntry, translator.GenerateCommandDefinition(functionMapping).CreateCommand())
	{
	}

	protected FunctionUpdateCommand(UpdateTranslator translator, ReadOnlyCollection<IEntityStateEntry> stateEntries, ExtractedStateEntry stateEntry, DbCommand dbCommand)
		: base(translator, stateEntry.Original, stateEntry.Current)
	{
		_stateEntries = stateEntries;
		_dbCommand = new InterceptableDbCommand(dbCommand, translator.InterceptionContext);
	}

	internal override IList<IEntityStateEntry> GetStateEntries(UpdateTranslator translator)
	{
		return _stateEntries;
	}

	internal void SetParameterValue(PropagatorResult result, ModificationFunctionParameterBinding parameterBinding, UpdateTranslator translator)
	{
		DbParameter dbParameter = _dbCommand.Parameters[parameterBinding.Parameter.Name];
		TypeUsage typeUsage = parameterBinding.Parameter.TypeUsage;
		object principalValue = translator.KeyManager.GetPrincipalValue(result);
		translator.SetParameterValue(dbParameter, typeUsage, principalValue);
		int identifier = result.Identifier;
		if (-1 == identifier)
		{
			return;
		}
		if (_inputIdentifiers == null)
		{
			_inputIdentifiers = new List<KeyValuePair<int, DbParameter>>(2);
		}
		foreach (int principal in translator.KeyManager.GetPrincipals(identifier))
		{
			_inputIdentifiers.Add(new KeyValuePair<int, DbParameter>(principal, dbParameter));
		}
	}

	internal void RegisterRowsAffectedParameter(FunctionParameter rowsAffectedParameter)
	{
		if (rowsAffectedParameter != null)
		{
			_rowsAffectedParameter = _dbCommand.Parameters[rowsAffectedParameter.Name];
		}
	}

	internal void AddResultColumn(UpdateTranslator translator, string columnName, PropagatorResult result)
	{
		if (ResultColumns == null)
		{
			ResultColumns = new List<KeyValuePair<string, PropagatorResult>>(2);
		}
		ResultColumns.Add(new KeyValuePair<string, PropagatorResult>(columnName, result));
		int identifier = result.Identifier;
		if (-1 != identifier)
		{
			if (translator.KeyManager.HasPrincipals(identifier))
			{
				throw new InvalidOperationException(Strings.Update_GeneratedDependent(columnName));
			}
			AddOutputIdentifier(columnName, identifier);
		}
	}

	private void AddOutputIdentifier(string columnName, int identifier)
	{
		if (_outputIdentifiers == null)
		{
			_outputIdentifiers = new Dictionary<int, string>(2);
		}
		_outputIdentifiers[identifier] = columnName;
	}

	internal virtual void SetInputIdentifiers(Dictionary<int, object> identifierValues)
	{
		if (_inputIdentifiers == null)
		{
			return;
		}
		foreach (KeyValuePair<int, DbParameter> inputIdentifier in _inputIdentifiers)
		{
			if (identifierValues.TryGetValue(inputIdentifier.Key, out var value))
			{
				inputIdentifier.Value.Value = value;
			}
		}
	}

	internal override long Execute(Dictionary<int, object> identifierValues, List<KeyValuePair<PropagatorResult, object>> generatedValues)
	{
		EntityConnection connection = base.Translator.Connection;
		_dbCommand.Transaction = ((connection.CurrentTransaction == null) ? null : connection.CurrentTransaction.StoreTransaction);
		_dbCommand.Connection = connection.StoreConnection;
		if (base.Translator.CommandTimeout.HasValue)
		{
			_dbCommand.CommandTimeout = base.Translator.CommandTimeout.Value;
		}
		SetInputIdentifiers(identifierValues);
		long num;
		if (ResultColumns != null)
		{
			num = 0L;
			IBaseList<EdmMember> allStructuralMembers = TypeHelpers.GetAllStructuralMembers(base.CurrentValues.StructuralType);
			DbDataReader reader = _dbCommand.ExecuteReader(CommandBehavior.SequentialAccess);
			try
			{
				if (reader.Read())
				{
					num++;
					foreach (KeyValuePair<int, PropagatorResult> item in from r in ResultColumns
						select new KeyValuePair<int, PropagatorResult>(GetColumnOrdinal(base.Translator, reader, r.Key), r.Value) into r
						orderby r.Key
						select r)
					{
						int key = item.Key;
						if (key == -1)
						{
							break;
						}
						TypeUsage typeUsage = allStructuralMembers[item.Value.RecordOrdinal].TypeUsage;
						object value = ((!Helper.IsSpatialType(typeUsage) || reader.IsDBNull(key)) ? reader.GetValue(key) : SpatialHelpers.GetSpatialValue(base.Translator.MetadataWorkspace, reader, typeUsage, key));
						PropagatorResult value2 = item.Value;
						generatedValues.Add(new KeyValuePair<PropagatorResult, object>(value2, value));
						int identifier = value2.Identifier;
						if (-1 != identifier)
						{
							identifierValues.Add(identifier, value);
						}
					}
				}
				CommandHelper.ConsumeReader(reader);
			}
			finally
			{
				if (reader != null)
				{
					((IDisposable)reader).Dispose();
				}
			}
		}
		else
		{
			num = _dbCommand.ExecuteNonQuery();
		}
		return GetRowsAffected(num, base.Translator);
	}

	internal override async Task<long> ExecuteAsync(Dictionary<int, object> identifierValues, List<KeyValuePair<PropagatorResult, object>> generatedValues, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		EntityConnection connection = base.Translator.Connection;
		_dbCommand.Transaction = ((connection.CurrentTransaction == null) ? null : connection.CurrentTransaction.StoreTransaction);
		_dbCommand.Connection = connection.StoreConnection;
		if (base.Translator.CommandTimeout.HasValue)
		{
			_dbCommand.CommandTimeout = base.Translator.CommandTimeout.Value;
		}
		SetInputIdentifiers(identifierValues);
		long rowsAffected;
		if (ResultColumns != null)
		{
			rowsAffected = 0L;
			IBaseList<EdmMember> members = TypeHelpers.GetAllStructuralMembers(base.CurrentValues.StructuralType);
			DbDataReader reader = await _dbCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken).WithCurrentCulture();
			try
			{
				if (await reader.ReadAsync(cancellationToken).WithCurrentCulture())
				{
					rowsAffected++;
					foreach (KeyValuePair<int, PropagatorResult> resultColumn in from r in ResultColumns
						select new KeyValuePair<int, PropagatorResult>(GetColumnOrdinal(base.Translator, reader, r.Key), r.Value) into r
						orderby r.Key
						select r)
					{
						int columnOrdinal = resultColumn.Key;
						TypeUsage columnType = members[resultColumn.Value.RecordOrdinal].TypeUsage;
						bool flag = Helper.IsSpatialType(columnType);
						if (flag)
						{
							flag = !(await reader.IsDBNullAsync(columnOrdinal, cancellationToken).WithCurrentCulture());
						}
						object value = ((!flag) ? (await reader.GetFieldValueAsync<object>(columnOrdinal, cancellationToken).WithCurrentCulture()) : (await SpatialHelpers.GetSpatialValueAsync(base.Translator.MetadataWorkspace, reader, columnType, columnOrdinal, cancellationToken).WithCurrentCulture()));
						PropagatorResult value2 = resultColumn.Value;
						generatedValues.Add(new KeyValuePair<PropagatorResult, object>(value2, value));
						int identifier = value2.Identifier;
						if (-1 != identifier)
						{
							identifierValues.Add(identifier, value);
						}
					}
				}
				await CommandHelper.ConsumeReaderAsync(reader, cancellationToken).WithCurrentCulture();
			}
			finally
			{
				if (reader != null)
				{
					((IDisposable)reader).Dispose();
				}
			}
		}
		else
		{
			rowsAffected = await _dbCommand.ExecuteNonQueryAsync(cancellationToken).WithCurrentCulture();
		}
		return GetRowsAffected(rowsAffected, base.Translator);
	}

	protected virtual long GetRowsAffected(long rowsAffected, UpdateTranslator translator)
	{
		if (_rowsAffectedParameter != null)
		{
			if (DBNull.Value.Equals(_rowsAffectedParameter.Value))
			{
				rowsAffected = 0L;
			}
			else
			{
				try
				{
					rowsAffected = Convert.ToInt64(_rowsAffectedParameter.Value, CultureInfo.InvariantCulture);
				}
				catch (Exception ex)
				{
					if (ex.RequiresContext())
					{
						throw new UpdateException(Strings.Update_UnableToConvertRowsAffectedParameter(_rowsAffectedParameter.ParameterName, typeof(long).FullName), ex, GetStateEntries(translator).Cast<ObjectStateEntry>().Distinct());
					}
					throw;
				}
			}
		}
		return rowsAffected;
	}

	private int GetColumnOrdinal(UpdateTranslator translator, DbDataReader reader, string columnName)
	{
		try
		{
			return reader.GetOrdinal(columnName);
		}
		catch (IndexOutOfRangeException)
		{
			throw new UpdateException(Strings.Update_MissingResultColumn(columnName), null, GetStateEntries(translator).Cast<ObjectStateEntry>().Distinct());
		}
	}

	private static ModificationOperator GetModificationOperator(EntityState state)
	{
		if (state <= EntityState.Added)
		{
			switch (state)
			{
			case EntityState.Unchanged:
				break;
			case EntityState.Added:
				return ModificationOperator.Insert;
			default:
				return ModificationOperator.Update;
			}
		}
		else
		{
			if (state == EntityState.Deleted)
			{
				return ModificationOperator.Delete;
			}
			_ = 16;
		}
		return ModificationOperator.Update;
	}

	internal override int CompareToType(UpdateCommand otherCommand)
	{
		FunctionUpdateCommand functionUpdateCommand = (FunctionUpdateCommand)otherCommand;
		IEntityStateEntry entityStateEntry = _stateEntries[0];
		IEntityStateEntry entityStateEntry2 = functionUpdateCommand._stateEntries[0];
		int num = GetModificationOperator(entityStateEntry.State) - GetModificationOperator(entityStateEntry2.State);
		if (num != 0)
		{
			return num;
		}
		num = StringComparer.Ordinal.Compare(entityStateEntry.EntitySet.Name, entityStateEntry2.EntitySet.Name);
		if (num != 0)
		{
			return num;
		}
		num = StringComparer.Ordinal.Compare(entityStateEntry.EntitySet.EntityContainer.Name, entityStateEntry2.EntitySet.EntityContainer.Name);
		if (num != 0)
		{
			return num;
		}
		int num2 = ((_inputIdentifiers != null) ? _inputIdentifiers.Count : 0);
		int num3 = ((functionUpdateCommand._inputIdentifiers != null) ? functionUpdateCommand._inputIdentifiers.Count : 0);
		num = num2 - num3;
		if (num != 0)
		{
			return num;
		}
		for (int i = 0; i < num2; i++)
		{
			DbParameter value = _inputIdentifiers[i].Value;
			DbParameter value2 = functionUpdateCommand._inputIdentifiers[i].Value;
			num = ByValueComparer.Default.Compare(value.Value, value2.Value);
			if (num != 0)
			{
				return num;
			}
		}
		for (int j = 0; j < num2; j++)
		{
			int key = _inputIdentifiers[j].Key;
			int key2 = functionUpdateCommand._inputIdentifiers[j].Key;
			num = key - key2;
			if (num != 0)
			{
				return num;
			}
		}
		return num;
	}
}
