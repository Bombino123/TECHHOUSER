using System.Collections.Generic;
using System.Runtime.InteropServices;
using SMBLibrary.RPC;
using SMBLibrary.Services;
using Utilities;

namespace SMBLibrary.Client;

[ComVisible(true)]
public class ServerServiceHelper
{
	public static List<string> ListShares(INTFileStore namedPipeShare, ShareType? shareType, out NTStatus status)
	{
		return ListShares(namedPipeShare, "*", shareType, out status);
	}

	public static List<string> ListShares(INTFileStore namedPipeShare, string serverName, ShareType? shareType, out NTStatus status)
	{
		status = NamedPipeHelper.BindPipe(namedPipeShare, "srvsvc", ServerService.ServiceInterfaceGuid, 3u, out var pipeHandle, out var maxTransmitFragmentSize);
		if (status != 0)
		{
			return null;
		}
		NetrShareEnumRequest netrShareEnumRequest = new NetrShareEnumRequest();
		netrShareEnumRequest.InfoStruct = new ShareEnum();
		netrShareEnumRequest.InfoStruct.Level = 1u;
		netrShareEnumRequest.InfoStruct.Info = new ShareInfo1Container();
		netrShareEnumRequest.PreferedMaximumLength = uint.MaxValue;
		netrShareEnumRequest.ServerName = "\\\\" + serverName;
		RequestPDU obj = new RequestPDU
		{
			Flags = (PacketFlags.FirstFragment | PacketFlags.LastFragment),
			DataRepresentation = 
			{
				CharacterFormat = CharacterFormat.ASCII,
				ByteOrder = ByteOrder.LittleEndian,
				FloatingPointRepresentation = FloatingPointRepresentation.IEEE
			},
			OpNum = 15,
			Data = netrShareEnumRequest.GetBytes()
		};
		obj.AllocationHint = (uint)obj.Data.Length;
		byte[] bytes = obj.GetBytes();
		int num = maxTransmitFragmentSize;
		status = namedPipeShare.DeviceIOControl(pipeHandle, 1163287u, bytes, out var output, num);
		if (status != 0)
		{
			return null;
		}
		ResponsePDU responsePDU = RPCPDU.GetPDU(output, 0) as ResponsePDU;
		if (responsePDU == null)
		{
			status = NTStatus.STATUS_NOT_SUPPORTED;
			return null;
		}
		byte[] array = responsePDU.Data;
		while ((responsePDU.Flags & PacketFlags.LastFragment) == 0)
		{
			status = namedPipeShare.ReadFile(out output, pipeHandle, 0L, num);
			if (status != 0)
			{
				return null;
			}
			responsePDU = RPCPDU.GetPDU(output, 0) as ResponsePDU;
			if (responsePDU == null)
			{
				status = NTStatus.STATUS_NOT_SUPPORTED;
				return null;
			}
			array = ByteUtils.Concatenate(array, responsePDU.Data);
		}
		namedPipeShare.CloseFile(pipeHandle);
		NetrShareEnumResponse netrShareEnumResponse = new NetrShareEnumResponse(array);
		if (!(netrShareEnumResponse.InfoStruct.Info is ShareInfo1Container { Entries: not null } shareInfo1Container))
		{
			if (netrShareEnumResponse.Result == Win32Error.ERROR_ACCESS_DENIED)
			{
				status = NTStatus.STATUS_ACCESS_DENIED;
			}
			else
			{
				status = NTStatus.STATUS_NOT_SUPPORTED;
			}
			return null;
		}
		List<string> list = new List<string>();
		foreach (ShareInfo1Entry entry in shareInfo1Container.Entries)
		{
			if (!shareType.HasValue || shareType.Value == entry.ShareType.ShareType)
			{
				list.Add(entry.NetName.Value);
			}
		}
		return list;
	}
}
