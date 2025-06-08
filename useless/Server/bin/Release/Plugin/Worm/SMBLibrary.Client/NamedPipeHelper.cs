using System;
using System.Runtime.InteropServices;
using SMBLibrary.RPC;
using SMBLibrary.Services;

namespace SMBLibrary.Client;

[ComVisible(true)]
public class NamedPipeHelper
{
	public static NTStatus BindPipe(INTFileStore namedPipeShare, string pipeName, Guid interfaceGuid, uint interfaceVersion, out object pipeHandle, out int maxTransmitFragmentSize)
	{
		maxTransmitFragmentSize = 0;
		NTStatus nTStatus = namedPipeShare.CreateFile(out pipeHandle, out var _, pipeName, (AccessMask)3u, (FileAttributes)0u, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, (CreateOptions)0u, null);
		if (nTStatus != 0)
		{
			return nTStatus;
		}
		BindPDU obj = new BindPDU
		{
			Flags = (PacketFlags.FirstFragment | PacketFlags.LastFragment),
			DataRepresentation = 
			{
				CharacterFormat = CharacterFormat.ASCII,
				ByteOrder = ByteOrder.LittleEndian,
				FloatingPointRepresentation = FloatingPointRepresentation.IEEE
			},
			MaxTransmitFragmentSize = 5680,
			MaxReceiveFragmentSize = 5680
		};
		ContextElement item = new ContextElement
		{
			AbstractSyntax = new SyntaxID(interfaceGuid, interfaceVersion),
			TransferSyntaxList = 
			{
				new SyntaxID(RemoteServiceHelper.NDRTransferSyntaxIdentifier, 2u)
			}
		};
		obj.ContextList.Add(item);
		byte[] bytes = obj.GetBytes();
		nTStatus = namedPipeShare.DeviceIOControl(pipeHandle, 1163287u, bytes, out var output, 4096);
		if (nTStatus != 0)
		{
			return nTStatus;
		}
		if (!(RPCPDU.GetPDU(output, 0) is BindAckPDU bindAckPDU))
		{
			return NTStatus.STATUS_NOT_SUPPORTED;
		}
		maxTransmitFragmentSize = bindAckPDU.MaxTransmitFragmentSize;
		return NTStatus.STATUS_SUCCESS;
	}
}
