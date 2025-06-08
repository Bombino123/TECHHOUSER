using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Common.Utils;

internal static class CommandHelper
{
	internal static void ConsumeReader(DbDataReader reader)
	{
		if (reader != null && !reader.IsClosed)
		{
			while (reader.NextResult())
			{
			}
		}
	}

	internal static async Task ConsumeReaderAsync(DbDataReader reader, CancellationToken cancellationToken)
	{
		if (reader != null && !reader.IsClosed)
		{
			cancellationToken.ThrowIfCancellationRequested();
			while (await reader.NextResultAsync(cancellationToken).WithCurrentCulture())
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
		}
	}

	internal static void ParseFunctionImportCommandText(string commandText, string defaultContainerName, out string containerName, out string functionImportName)
	{
		string[] array = commandText.Split(new char[1] { '.' });
		containerName = null;
		functionImportName = null;
		if (2 == array.Length)
		{
			containerName = array[0].Trim();
			functionImportName = array[1].Trim();
		}
		else if (1 == array.Length && defaultContainerName != null)
		{
			containerName = defaultContainerName;
			functionImportName = array[0].Trim();
		}
		if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(functionImportName))
		{
			throw new InvalidOperationException(Strings.EntityClient_InvalidStoredProcedureCommandText);
		}
	}

	internal static void SetStoreProviderCommandState(EntityCommand entityCommand, EntityTransaction entityTransaction, DbCommand storeProviderCommand)
	{
		storeProviderCommand.CommandTimeout = entityCommand.CommandTimeout;
		storeProviderCommand.Connection = entityCommand.Connection.StoreConnection;
		storeProviderCommand.Transaction = entityTransaction?.StoreTransaction;
		storeProviderCommand.UpdatedRowSource = entityCommand.UpdatedRowSource;
	}

	internal static void SetEntityParameterValues(EntityCommand entityCommand, DbCommand storeProviderCommand, EntityConnection connection)
	{
		foreach (DbParameter parameter in storeProviderCommand.Parameters)
		{
			ParameterDirection direction = parameter.Direction;
			if ((direction & ParameterDirection.Output) == 0)
			{
				continue;
			}
			int num = entityCommand.Parameters.IndexOf(parameter.ParameterName);
			if (0 <= num)
			{
				EntityParameter entityParameter = entityCommand.Parameters[num];
				object obj = parameter.Value;
				TypeUsage typeUsage = entityParameter.GetTypeUsage();
				if (Helper.IsSpatialType(typeUsage))
				{
					obj = GetSpatialValueFromProviderValue(obj, (PrimitiveType)typeUsage.EdmType, connection);
				}
				entityParameter.Value = obj;
			}
		}
	}

	private static object GetSpatialValueFromProviderValue(object spatialValue, PrimitiveType parameterType, EntityConnection connection)
	{
		DbSpatialServices spatialServices = DbProviderServices.GetSpatialServices(DbConfiguration.DependencyResolver, connection);
		if (Helper.IsGeographicType(parameterType))
		{
			return spatialServices.GeographyFromProviderValue(spatialValue);
		}
		return spatialServices.GeometryFromProviderValue(spatialValue);
	}

	internal static EdmFunction FindFunctionImport(MetadataWorkspace workspace, string containerName, string functionImportName)
	{
		if (!workspace.TryGetEntityContainer(containerName, DataSpace.CSpace, out var entityContainer))
		{
			throw new InvalidOperationException(Strings.EntityClient_UnableToFindFunctionImportContainer(containerName));
		}
		EdmFunction edmFunction = null;
		foreach (EdmFunction functionImport in entityContainer.FunctionImports)
		{
			if (functionImport.Name == functionImportName)
			{
				edmFunction = functionImport;
				break;
			}
		}
		if (edmFunction == null)
		{
			throw new InvalidOperationException(Strings.EntityClient_UnableToFindFunctionImport(containerName, functionImportName));
		}
		if (edmFunction.IsComposableAttribute)
		{
			throw new InvalidOperationException(Strings.EntityClient_FunctionImportMustBeNonComposable(containerName + "." + functionImportName));
		}
		return edmFunction;
	}
}
