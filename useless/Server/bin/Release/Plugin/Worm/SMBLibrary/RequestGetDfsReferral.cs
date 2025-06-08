using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class RequestGetDfsReferral
{
	public ushort MaxReferralLevel;

	public string RequestFileName;

	public RequestGetDfsReferral()
	{
	}

	public RequestGetDfsReferral(byte[] buffer)
	{
		MaxReferralLevel = LittleEndianConverter.ToUInt16(buffer, 0);
		RequestFileName = ByteReader.ReadNullTerminatedUTF16String(buffer, 2);
	}

	public byte[] GetBytes()
	{
		byte[] array = new byte[2 + RequestFileName.Length * 2 + 2];
		LittleEndianWriter.WriteUInt16(array, 0, MaxReferralLevel);
		ByteWriter.WriteUTF16String(array, 2, RequestFileName);
		return array;
	}
}
