using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public class NegotiateMessage
{
	public string Signature;

	public MessageTypeName MessageType;

	public NegotiateFlags NegotiateFlags;

	public string DomainName;

	public string Workstation;

	public NTLMVersion Version;

	public NegotiateMessage()
	{
		Signature = "NTLMSSP\0";
		MessageType = MessageTypeName.Negotiate;
		DomainName = string.Empty;
		Workstation = string.Empty;
	}

	public NegotiateMessage(byte[] buffer)
	{
		Signature = ByteReader.ReadAnsiString(buffer, 0, 8);
		MessageType = (MessageTypeName)LittleEndianConverter.ToUInt32(buffer, 8);
		NegotiateFlags = (NegotiateFlags)LittleEndianConverter.ToUInt32(buffer, 12);
		DomainName = AuthenticationMessageUtils.ReadAnsiStringBufferPointer(buffer, 16);
		Workstation = AuthenticationMessageUtils.ReadAnsiStringBufferPointer(buffer, 24);
		if ((NegotiateFlags & NegotiateFlags.Version) != 0)
		{
			Version = new NTLMVersion(buffer, 32);
		}
	}

	public byte[] GetBytes()
	{
		if ((NegotiateFlags & NegotiateFlags.DomainNameSupplied) == 0)
		{
			DomainName = string.Empty;
		}
		if ((NegotiateFlags & NegotiateFlags.WorkstationNameSupplied) == 0)
		{
			Workstation = string.Empty;
		}
		int num = 32;
		if ((NegotiateFlags & NegotiateFlags.Version) != 0)
		{
			num += 8;
		}
		int num2 = DomainName.Length * 2 + Workstation.Length * 2;
		byte[] array = new byte[num + num2];
		ByteWriter.WriteAnsiString(array, 0, "NTLMSSP\0", 8);
		LittleEndianWriter.WriteUInt32(array, 8, (uint)MessageType);
		LittleEndianWriter.WriteUInt32(array, 12, (uint)NegotiateFlags);
		if ((NegotiateFlags & NegotiateFlags.Version) != 0)
		{
			Version.WriteBytes(array, 32);
		}
		int offset = num;
		AuthenticationMessageUtils.WriteBufferPointer(array, 16, (ushort)(DomainName.Length * 2), (uint)offset);
		ByteWriter.WriteUTF16String(array, ref offset, DomainName);
		AuthenticationMessageUtils.WriteBufferPointer(array, 24, (ushort)(Workstation.Length * 2), (uint)offset);
		ByteWriter.WriteUTF16String(array, ref offset, Workstation);
		return array;
	}
}
