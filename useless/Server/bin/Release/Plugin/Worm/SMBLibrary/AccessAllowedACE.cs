using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class AccessAllowedACE : ACE
{
	public const int FixedLength = 8;

	public AceHeader Header;

	public AccessMask Mask;

	public SID Sid;

	public override int Length => 8 + Sid.Length;

	public AccessAllowedACE()
	{
		Header = new AceHeader();
		Header.AceType = AceType.ACCESS_ALLOWED_ACE_TYPE;
	}

	public AccessAllowedACE(byte[] buffer, int offset)
	{
		Header = new AceHeader(buffer, offset);
		Mask = (AccessMask)LittleEndianConverter.ToUInt32(buffer, offset + 4);
		Sid = new SID(buffer, offset + 8);
	}

	public override void WriteBytes(byte[] buffer, ref int offset)
	{
		Header.AceSize = (ushort)Length;
		Header.WriteBytes(buffer, ref offset);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, (uint)Mask);
		Sid.WriteBytes(buffer, ref offset);
	}
}
