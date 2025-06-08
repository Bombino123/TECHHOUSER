using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Internal;
using System.Data.Entity.ModelConfiguration.Edm.Serialization;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Xml;
using System.Xml.Linq;

namespace System.Data.Entity.Infrastructure;

public static class EdmxWriter
{
	public static void WriteEdmx(DbContext context, XmlWriter writer)
	{
		Check.NotNull(context, "context");
		Check.NotNull(writer, "writer");
		InternalContext internalContext = context.InternalContext;
		if (internalContext is EagerInternalContext)
		{
			throw Error.EdmxWriter_EdmxFromObjectContextNotSupported();
		}
		DbModel modelBeingInitialized = internalContext.ModelBeingInitialized;
		if (modelBeingInitialized != null)
		{
			WriteEdmx(modelBeingInitialized, writer);
			return;
		}
		DbCompiledModel codeFirstModel = internalContext.CodeFirstModel;
		if (codeFirstModel == null)
		{
			throw Error.EdmxWriter_EdmxFromModelFirstNotSupported();
		}
		DbModelStore service = DbConfiguration.DependencyResolver.GetService<DbModelStore>();
		if (service != null)
		{
			XDocument val = service.TryGetEdmx(context.GetType());
			if (val != null)
			{
				((XNode)val).WriteTo(writer);
				return;
			}
		}
		DbModelBuilder dbModelBuilder = (codeFirstModel.CachedModelBuilder ?? throw Error.EdmxWriter_EdmxFromRawCompiledModelNotSupported()).Clone();
		WriteEdmx((internalContext.ModelProviderInfo == null) ? dbModelBuilder.Build(internalContext.Connection) : dbModelBuilder.Build(internalContext.ModelProviderInfo), writer);
	}

	public static void WriteEdmx(DbModel model, XmlWriter writer)
	{
		Check.NotNull(model, "model");
		Check.NotNull(writer, "writer");
		new EdmxSerializer().Serialize(model.DatabaseMapping, writer);
	}
}
