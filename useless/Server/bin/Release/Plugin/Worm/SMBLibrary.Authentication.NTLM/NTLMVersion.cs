using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public class NTLMVersion
{
	public const int Length = 8;

	public const byte NTLMSSP_REVISION_W2K3 = 15;

	public byte ProductMajorVersion;

	public byte ProductMinorVersion;

	public ushort ProductBuild;

	public byte NTLMRevisionCurrent;

	public static NTLMVersion WindowsXP => new NTLMVersion(5, 1, 2600, 15);

	public static NTLMVersion Server2003 => new NTLMVersion(5, 2, 3790, 15);

	public NTLMVersion(byte majorVersion, byte minorVersion, ushort build, byte ntlmRevisionCurrent)
	{
		ProductMajorVersion = majorVersion;
		ProductMinorVersion = minorVersion;
		ProductBuild = build;
		NTLMRevisionCurrent = ntlmRevisionCurrent;
	}

	public NTLMVersion(byte[] buffer, int offset)
	{
		ProductMajorVersion = ByteReader.ReadByte(buffer, offset);
		ProductMinorVersion = ByteReader.ReadByte(buffer, offset + 1);
		ProductBuild = LittleEndianConverter.ToUInt16(buffer, offset + 2);
		NTLMRevisionCurrent = ByteReader.ReadByte(buffer, offset + 7);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		ByteWriter.WriteByte(buffer, offset, ProductMajorVersion);
		ByteWriter.WriteByte(buffer, offset + 1, ProductMinorVersion);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, ProductBuild);
		ByteWriter.WriteByte(buffer, offset + 7, NTLMRevisionCurrent);
	}

	public override string ToString()
	{
		return $"{ProductMajorVersion}.{ProductMinorVersion}.{ProductBuild}";
	}
}
