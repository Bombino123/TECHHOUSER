using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SMBLibrary.RPC;
using Utilities;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class RemoteServiceHelper
{
	public static readonly Guid NDRTransferSyntaxIdentifier = new Guid("8A885D04-1CEB-11C9-9FE8-08002B104860");

	public const int NDRTransferSyntaxVersion = 2;

	public static readonly Guid BindTimeFeatureIdentifier3 = new Guid("6CB71C2C-9812-4540-0300-000000000000");

	public const int BindTimeFeatureIdentifierVersion = 1;

	private static uint m_associationGroupID = 1u;

	public static BindAckPDU GetRPCBindResponse(BindPDU bindPDU, RemoteService service)
	{
		BindAckPDU bindAckPDU = new BindAckPDU();
		bindAckPDU.Flags = PacketFlags.FirstFragment | PacketFlags.LastFragment;
		bindAckPDU.DataRepresentation = bindPDU.DataRepresentation;
		bindAckPDU.CallID = bindPDU.CallID;
		if (bindPDU.AssociationGroupID == 0)
		{
			bindAckPDU.AssociationGroupID = m_associationGroupID;
			m_associationGroupID++;
			if (m_associationGroupID == 0)
			{
				m_associationGroupID++;
			}
		}
		else
		{
			bindAckPDU.AssociationGroupID = bindPDU.AssociationGroupID;
		}
		bindAckPDU.SecondaryAddress = "\\PIPE\\" + service.PipeName;
		bindAckPDU.MaxTransmitFragmentSize = bindPDU.MaxReceiveFragmentSize;
		bindAckPDU.MaxReceiveFragmentSize = bindPDU.MaxTransmitFragmentSize;
		foreach (ContextElement context in bindPDU.ContextList)
		{
			ResultElement item = default(ResultElement);
			if (context.AbstractSyntax.InterfaceUUID.Equals(service.InterfaceGuid))
			{
				int num = IndexOfSupportedTransferSyntax(context.TransferSyntaxList);
				if (num >= 0)
				{
					item.Result = NegotiationResult.Acceptance;
					item.TransferSyntax = context.TransferSyntaxList[num];
				}
				else if (context.TransferSyntaxList.Contains(new SyntaxID(BindTimeFeatureIdentifier3, 1u)))
				{
					item.Result = NegotiationResult.NegotiateAck;
					item.Reason = RejectionReason.AbstractSyntaxNotSupported;
				}
				else
				{
					item.Result = NegotiationResult.ProviderRejection;
					item.Reason = RejectionReason.ProposedTransferSyntaxesNotSupported;
				}
			}
			else
			{
				item.Result = NegotiationResult.ProviderRejection;
				item.Reason = RejectionReason.AbstractSyntaxNotSupported;
			}
			bindAckPDU.ResultList.Add(item);
		}
		return bindAckPDU;
	}

	private static int IndexOfSupportedTransferSyntax(List<SyntaxID> syntaxList)
	{
		List<SyntaxID> list = new List<SyntaxID>();
		list.Add(new SyntaxID(NDRTransferSyntaxIdentifier, 1u));
		list.Add(new SyntaxID(NDRTransferSyntaxIdentifier, 2u));
		for (int i = 0; i < syntaxList.Count; i++)
		{
			if (list.Contains(syntaxList[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static List<RPCPDU> GetRPCResponse(RequestPDU requestPDU, RemoteService service, int maxTransmitFragmentSize)
	{
		List<RPCPDU> list = new List<RPCPDU>();
		byte[] responseBytes;
		try
		{
			responseBytes = service.GetResponseBytes(requestPDU.OpNum, requestPDU.Data);
		}
		catch (UnsupportedOpNumException)
		{
			FaultPDU faultPDU = new FaultPDU();
			faultPDU.Flags = PacketFlags.FirstFragment | PacketFlags.LastFragment | PacketFlags.DidNotExecute;
			faultPDU.DataRepresentation = requestPDU.DataRepresentation;
			faultPDU.CallID = requestPDU.CallID;
			faultPDU.AllocationHint = 32u;
			faultPDU.Status = FaultStatus.OpRangeError;
			list.Add(faultPDU);
			return list;
		}
		int num = 0;
		int val = maxTransmitFragmentSize - 16 - 8;
		do
		{
			ResponsePDU responsePDU = new ResponsePDU();
			int num2 = Math.Min(responseBytes.Length - num, val);
			responsePDU.DataRepresentation = requestPDU.DataRepresentation;
			responsePDU.CallID = requestPDU.CallID;
			responsePDU.AllocationHint = (uint)(responseBytes.Length - num);
			responsePDU.Data = ByteReader.ReadBytes(responseBytes, num, num2);
			if (num == 0)
			{
				responsePDU.Flags |= PacketFlags.FirstFragment;
			}
			if (num + num2 == responseBytes.Length)
			{
				responsePDU.Flags |= PacketFlags.LastFragment;
			}
			list.Add(responsePDU);
			num += num2;
		}
		while (num < responseBytes.Length);
		return list;
	}
}
