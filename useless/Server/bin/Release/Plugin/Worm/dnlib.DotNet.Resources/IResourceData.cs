using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using dnlib.IO;

namespace dnlib.DotNet.Resources;

[ComVisible(true)]
public interface IResourceData : IFileSection
{
	ResourceTypeCode Code { get; }

	new FileOffset StartOffset { get; set; }

	new FileOffset EndOffset { get; set; }

	void WriteData(ResourceBinaryWriter writer, IFormatter formatter);
}
