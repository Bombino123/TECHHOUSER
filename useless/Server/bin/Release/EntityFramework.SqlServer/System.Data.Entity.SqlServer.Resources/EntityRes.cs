using System.CodeDom.Compiler;
using System.Data.Entity.SqlServer.Utilities;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace System.Data.Entity.SqlServer.Resources;

[GeneratedCode("Resources.SqlServer.tt", "1.0.0.0")]
internal sealed class EntityRes
{
	internal const string ArgumentIsNullOrWhitespace = "ArgumentIsNullOrWhitespace";

	internal const string SqlProvider_GeographyValueNotSqlCompatible = "SqlProvider_GeographyValueNotSqlCompatible";

	internal const string SqlProvider_GeometryValueNotSqlCompatible = "SqlProvider_GeometryValueNotSqlCompatible";

	internal const string ProviderReturnedNullForGetDbInformation = "ProviderReturnedNullForGetDbInformation";

	internal const string ProviderDoesNotSupportType = "ProviderDoesNotSupportType";

	internal const string NoStoreTypeForEdmType = "NoStoreTypeForEdmType";

	internal const string Mapping_Provider_WrongManifestType = "Mapping_Provider_WrongManifestType";

	internal const string ADP_InternalProviderError = "ADP_InternalProviderError";

	internal const string UnableToDetermineStoreVersion = "UnableToDetermineStoreVersion";

	internal const string SqlProvider_NeedSqlDataReader = "SqlProvider_NeedSqlDataReader";

	internal const string SqlProvider_Sql2008RequiredForSpatial = "SqlProvider_Sql2008RequiredForSpatial";

	internal const string SqlProvider_SqlTypesAssemblyNotFound = "SqlProvider_SqlTypesAssemblyNotFound";

	internal const string SqlProvider_IncompleteCreateDatabase = "SqlProvider_IncompleteCreateDatabase";

	internal const string SqlProvider_IncompleteCreateDatabaseAggregate = "SqlProvider_IncompleteCreateDatabaseAggregate";

	internal const string SqlProvider_DdlGeneration_MissingInitialCatalog = "SqlProvider_DdlGeneration_MissingInitialCatalog";

	internal const string SqlProvider_DdlGeneration_CannotDeleteDatabaseNoInitialCatalog = "SqlProvider_DdlGeneration_CannotDeleteDatabaseNoInitialCatalog";

	internal const string SqlProvider_DdlGeneration_CannotTellIfDatabaseExists = "SqlProvider_DdlGeneration_CannotTellIfDatabaseExists";

	internal const string SqlProvider_CredentialsMissingForMasterConnection = "SqlProvider_CredentialsMissingForMasterConnection";

	internal const string SqlProvider_InvalidGeographyColumn = "SqlProvider_InvalidGeographyColumn";

	internal const string SqlProvider_InvalidGeometryColumn = "SqlProvider_InvalidGeometryColumn";

	internal const string Mapping_Provider_WrongConnectionType = "Mapping_Provider_WrongConnectionType";

	internal const string Update_NotSupportedServerGenKey = "Update_NotSupportedServerGenKey";

	internal const string Update_NotSupportedIdentityType = "Update_NotSupportedIdentityType";

	internal const string Update_SqlEntitySetWithoutDmlFunctions = "Update_SqlEntitySetWithoutDmlFunctions";

	internal const string Cqt_General_UnsupportedExpression = "Cqt_General_UnsupportedExpression";

	internal const string SqlGen_ApplyNotSupportedOnSql8 = "SqlGen_ApplyNotSupportedOnSql8";

	internal const string SqlGen_NiladicFunctionsCannotHaveParameters = "SqlGen_NiladicFunctionsCannotHaveParameters";

	internal const string SqlGen_InvalidDatePartArgumentExpression = "SqlGen_InvalidDatePartArgumentExpression";

	internal const string SqlGen_InvalidDatePartArgumentValue = "SqlGen_InvalidDatePartArgumentValue";

	internal const string SqlGen_TypedNaNNotSupported = "SqlGen_TypedNaNNotSupported";

	internal const string SqlGen_TypedPositiveInfinityNotSupported = "SqlGen_TypedPositiveInfinityNotSupported";

	internal const string SqlGen_TypedNegativeInfinityNotSupported = "SqlGen_TypedNegativeInfinityNotSupported";

	internal const string SqlGen_PrimitiveTypeNotSupportedPriorSql10 = "SqlGen_PrimitiveTypeNotSupportedPriorSql10";

	internal const string SqlGen_CanonicalFunctionNotSupportedPriorSql10 = "SqlGen_CanonicalFunctionNotSupportedPriorSql10";

	internal const string SqlGen_ParameterForLimitNotSupportedOnSql8 = "SqlGen_ParameterForLimitNotSupportedOnSql8";

	internal const string SqlGen_ParameterForSkipNotSupportedOnSql8 = "SqlGen_ParameterForSkipNotSupportedOnSql8";

	internal const string Spatial_WellKnownGeographyValueNotValid = "Spatial_WellKnownGeographyValueNotValid";

	internal const string Spatial_WellKnownGeometryValueNotValid = "Spatial_WellKnownGeometryValueNotValid";

	internal const string SqlSpatialServices_ProviderValueNotSqlType = "SqlSpatialServices_ProviderValueNotSqlType";

	internal const string SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoSrid = "SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoSrid";

	internal const string SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoWkbOrWkt = "SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoWkbOrWkt";

	internal const string SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoSrid = "SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoSrid";

	internal const string SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoWkbOrWkt = "SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoWkbOrWkt";

	internal const string TransientExceptionDetected = "TransientExceptionDetected";

	internal const string ELinq_DbFunctionDirectCall = "ELinq_DbFunctionDirectCall";

	internal const string AutomaticMigration = "AutomaticMigration";

	internal const string InvalidDatabaseName = "InvalidDatabaseName";

	internal const string SqlServerMigrationSqlGenerator_UnknownOperation = "SqlServerMigrationSqlGenerator_UnknownOperation";

	private static EntityRes loader;

	private readonly ResourceManager resources;

	private static CultureInfo Culture => null;

	public static ResourceManager Resources => GetLoader().resources;

	private EntityRes()
	{
		resources = new ResourceManager("System.Data.Entity.SqlServer.Properties.Resources.SqlServer", typeof(SqlProviderServices).Assembly());
	}

	private static EntityRes GetLoader()
	{
		if (loader == null)
		{
			EntityRes value = new EntityRes();
			Interlocked.CompareExchange(ref loader, value, null);
		}
		return loader;
	}

	public static string GetString(string name, params object[] args)
	{
		EntityRes entityRes = GetLoader();
		if (entityRes == null)
		{
			return null;
		}
		string @string = entityRes.resources.GetString(name, Culture);
		if (args != null && args.Length != 0)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] is string { Length: >1024 } text)
				{
					args[i] = text.Substring(0, 1021) + "...";
				}
			}
			return string.Format(CultureInfo.CurrentCulture, @string, args);
		}
		return @string;
	}

	public static string GetString(string name)
	{
		return GetLoader()?.resources.GetString(name, Culture);
	}

	public static string GetString(string name, out bool usedFallback)
	{
		usedFallback = false;
		return GetString(name);
	}

	public static object GetObject(string name)
	{
		return GetLoader()?.resources.GetObject(name, Culture);
	}
}
