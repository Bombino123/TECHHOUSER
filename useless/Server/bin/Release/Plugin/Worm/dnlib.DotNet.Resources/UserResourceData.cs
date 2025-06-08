using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using dnlib.IO;

namespace dnlib.DotNet.Resources;

[ComVisible(true)]
public abstract class UserResourceData : IResourceData, IFileSection
{
	private readonly UserResourceType type;

	public string TypeName => type.Name;

	public ResourceTypeCode Code => type.Code;

	public FileOffset StartOffset { get; set; }

	public FileOffset EndOffset { get; set; }

	public UserResourceData(UserResourceType type)
	{
		this.type = type;
	}

	public abstract void WriteData(ResourceBinaryWriter writer, IFormatter formatter);
}
