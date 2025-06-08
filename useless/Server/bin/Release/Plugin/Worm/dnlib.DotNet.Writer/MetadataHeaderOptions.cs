using System.Runtime.InteropServices;
using dnlib.DotNet.MD;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class MetadataHeaderOptions
{
	public const string DEFAULT_VERSION_STRING = "v2.0.50727";

	public const uint DEFAULT_SIGNATURE = 1112167234u;

	public uint? Signature;

	public ushort? MajorVersion;

	public ushort? MinorVersion;

	public uint? Reserved1;

	public string VersionString;

	public StorageFlags? StorageFlags;

	public byte? Reserved2;

	public static MetadataHeaderOptions CreatePortablePdbV1_0()
	{
		return new MetadataHeaderOptions
		{
			Signature = 1112167234u,
			MajorVersion = (ushort)1,
			MinorVersion = (ushort)1,
			Reserved1 = 0u,
			VersionString = "PDB v1.0",
			StorageFlags = dnlib.DotNet.MD.StorageFlags.Normal,
			Reserved2 = 0
		};
	}
}
