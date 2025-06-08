using System.IO;
using System.Reflection;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class ClrTypeAnnotationSerializer : IMetadataAnnotationSerializer
{
	public string Serialize(string name, object value)
	{
		return ((Type)value).AssemblyQualifiedName;
	}

	public object Deserialize(string name, string value)
	{
		try
		{
			return Type.GetType(value, throwOnError: false);
		}
		catch (FileLoadException)
		{
		}
		catch (TargetInvocationException)
		{
		}
		catch (BadImageFormatException)
		{
		}
		return null;
	}
}
