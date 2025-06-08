using System.Data.Entity.Utilities;
using System.Runtime.Serialization;
using System.Xml;

namespace System.Data.Entity.Core.Objects;

public class ProxyDataContractResolver : DataContractResolver
{
	private readonly XsdDataContractExporter _exporter = new XsdDataContractExporter();

	public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
	{
		Check.NotEmpty(typeName, "typeName");
		Check.NotEmpty(typeNamespace, "typeNamespace");
		Check.NotNull(declaredType, "declaredType");
		Check.NotNull<DataContractResolver>(knownTypeResolver, "knownTypeResolver");
		return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, (DataContractResolver)null);
	}

	public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		Check.NotNull(type, "type");
		Check.NotNull(declaredType, "declaredType");
		Check.NotNull<DataContractResolver>(knownTypeResolver, "knownTypeResolver");
		Type objectType = ObjectContext.GetObjectType(type);
		if (objectType != type)
		{
			XmlQualifiedName schemaTypeName = _exporter.GetSchemaTypeName(objectType);
			XmlDictionary val = new XmlDictionary(2);
			typeName = new XmlDictionaryString((IXmlDictionary)(object)val, schemaTypeName.Name, 0);
			typeNamespace = new XmlDictionaryString((IXmlDictionary)(object)val, schemaTypeName.Namespace, 1);
			return true;
		}
		return knownTypeResolver.TryResolveType(type, declaredType, (DataContractResolver)null, ref typeName, ref typeNamespace);
	}
}
