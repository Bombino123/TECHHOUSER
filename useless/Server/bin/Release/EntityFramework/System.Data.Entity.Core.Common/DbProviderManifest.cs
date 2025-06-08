using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Xml;

namespace System.Data.Entity.Core.Common;

public abstract class DbProviderManifest
{
	public const string StoreSchemaDefinition = "StoreSchemaDefinition";

	public const string StoreSchemaMapping = "StoreSchemaMapping";

	public const string ConceptualSchemaDefinition = "ConceptualSchemaDefinition";

	public const string StoreSchemaDefinitionVersion3 = "StoreSchemaDefinitionVersion3";

	public const string StoreSchemaMappingVersion3 = "StoreSchemaMappingVersion3";

	public const string ConceptualSchemaDefinitionVersion3 = "ConceptualSchemaDefinitionVersion3";

	public const string MaxLengthFacetName = "MaxLength";

	public const string UnicodeFacetName = "Unicode";

	public const string FixedLengthFacetName = "FixedLength";

	public const string PrecisionFacetName = "Precision";

	public const string ScaleFacetName = "Scale";

	public const string NullableFacetName = "Nullable";

	public const string DefaultValueFacetName = "DefaultValue";

	public const string CollationFacetName = "Collation";

	public const string SridFacetName = "SRID";

	public const string IsStrictFacetName = "IsStrict";

	public abstract string NamespaceName { get; }

	public abstract ReadOnlyCollection<PrimitiveType> GetStoreTypes();

	public abstract ReadOnlyCollection<EdmFunction> GetStoreFunctions();

	public abstract ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType edmType);

	public abstract TypeUsage GetEdmType(TypeUsage storeType);

	public abstract TypeUsage GetStoreType(TypeUsage edmType);

	protected abstract XmlReader GetDbInformation(string informationType);

	public XmlReader GetInformation(string informationType)
	{
		XmlReader xmlReader = null;
		try
		{
			xmlReader = GetDbInformation(informationType);
		}
		catch (Exception ex)
		{
			if (ex.IsCatchableExceptionType())
			{
				throw new ProviderIncompatibleException(Strings.EntityClient_FailedToGetInformation(informationType), ex);
			}
			throw;
		}
		if (xmlReader == null)
		{
			if (informationType == "ConceptualSchemaDefinitionVersion3" || informationType == "ConceptualSchemaDefinition")
			{
				return DbProviderServices.GetConceptualSchemaDefinition(informationType);
			}
			throw new ProviderIncompatibleException(Strings.ProviderReturnedNullForGetDbInformation(informationType));
		}
		return xmlReader;
	}

	public virtual bool SupportsEscapingLikeArgument(out char escapeCharacter)
	{
		escapeCharacter = '\0';
		return false;
	}

	public virtual bool SupportsParameterOptimizationInSchemaQueries()
	{
		return false;
	}

	public virtual string EscapeLikeArgument(string argument)
	{
		Check.NotNull(argument, "argument");
		throw new ProviderIncompatibleException(Strings.ProviderShouldOverrideEscapeLikeArgument);
	}

	public virtual bool SupportsInExpression()
	{
		return false;
	}

	public virtual bool SupportsIntersectAndUnionAllFlattening()
	{
		return false;
	}
}
