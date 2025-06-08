using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public struct ResultElement
{
	public const int Length = 24;

	public NegotiationResult Result;

	public RejectionReason Reason;

	public SyntaxID TransferSyntax;

	public ResultElement(byte[] buffer, int offset)
	{
		Result = (NegotiationResult)LittleEndianConverter.ToUInt16(buffer, offset);
		Reason = (RejectionReason)LittleEndianConverter.ToUInt16(buffer, offset + 2);
		TransferSyntax = new SyntaxID(buffer, offset + 4);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, (ushort)Result);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, (ushort)Reason);
		TransferSyntax.WriteBytes(buffer, offset + 4);
	}
}
