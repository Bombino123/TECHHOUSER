namespace System.Data.Entity.Infrastructure;

public interface IMetadataAnnotationSerializer
{
	string Serialize(string name, object value);

	object Deserialize(string name, string value);
}
