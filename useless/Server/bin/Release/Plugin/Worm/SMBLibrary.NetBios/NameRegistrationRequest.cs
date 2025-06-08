using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class NameRegistrationRequest
{
	public const int DataLength = 6;

	public NameServicePacketHeader Header;

	public QuestionSection Question;

	public ResourceRecord Resource;

	public NameFlags NameFlags;

	public byte[] Address;

	public NameRegistrationRequest()
	{
		Header = new NameServicePacketHeader();
		Header.OpCode = NameServiceOperation.RegistrationRequest;
		Header.QDCount = 1;
		Header.ARCount = 1;
		Header.Flags = OperationFlags.Broadcast | OperationFlags.RecursionDesired;
		Question = new QuestionSection();
		Resource = new ResourceRecord(NameRecordType.NB);
		Address = new byte[4];
	}

	public NameRegistrationRequest(string machineName, NetBiosSuffix suffix, IPAddress address)
		: this()
	{
		Question.Name = NetBiosUtils.GetMSNetBiosName(machineName, suffix);
		Address = address.GetAddressBytes();
	}

	public byte[] GetBytes()
	{
		Resource.Data = GetData();
		MemoryStream memoryStream = new MemoryStream();
		Header.WriteBytes(memoryStream);
		Question.WriteBytes(memoryStream);
		Resource.WriteBytes(memoryStream, 12);
		return memoryStream.ToArray();
	}

	private byte[] GetData()
	{
		byte[] array = new byte[6];
		BigEndianWriter.WriteUInt16(array, 0, (ushort)NameFlags);
		ByteWriter.WriteBytes(array, 2, Address, 4);
		return array;
	}
}
