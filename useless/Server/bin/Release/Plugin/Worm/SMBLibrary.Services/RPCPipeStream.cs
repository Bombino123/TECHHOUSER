using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class RPCPipeStream : Stream
{
	private RemoteService m_service;

	private List<MemoryStream> m_outputStreams;

	private int? m_maxTransmitFragmentSize;

	public override bool CanSeek => false;

	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public int MessageLength
	{
		get
		{
			if (m_outputStreams.Count > 0)
			{
				return (int)m_outputStreams[0].Length;
			}
			return 0;
		}
	}

	public RPCPipeStream(RemoteService service)
	{
		m_service = service;
		m_outputStreams = new List<MemoryStream>();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (m_outputStreams.Count > 0)
		{
			int result = m_outputStreams[0].Read(buffer, offset, count);
			if (m_outputStreams[0].Position == m_outputStreams[0].Length)
			{
				m_outputStreams.RemoveAt(0);
			}
			return result;
		}
		return 0;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		RPCPDU pDU = RPCPDU.GetPDU(buffer, offset);
		ProcessRPCRequest(pDU);
	}

	private void ProcessRPCRequest(RPCPDU rpcRequest)
	{
		if (rpcRequest is BindPDU)
		{
			BindAckPDU rPCBindResponse = RemoteServiceHelper.GetRPCBindResponse((BindPDU)rpcRequest, m_service);
			m_maxTransmitFragmentSize = rPCBindResponse.MaxTransmitFragmentSize;
			Append(rPCBindResponse.GetBytes());
			return;
		}
		if (m_maxTransmitFragmentSize.HasValue && rpcRequest is RequestPDU)
		{
			foreach (RPCPDU item in RemoteServiceHelper.GetRPCResponse((RequestPDU)rpcRequest, m_service, m_maxTransmitFragmentSize.Value))
			{
				Append(item.GetBytes());
			}
			return;
		}
		FaultPDU faultPDU = new FaultPDU();
		faultPDU.Flags = PacketFlags.FirstFragment | PacketFlags.LastFragment;
		faultPDU.DataRepresentation = new DataRepresentationFormat(CharacterFormat.ASCII, ByteOrder.LittleEndian, FloatingPointRepresentation.IEEE);
		faultPDU.CallID = 0u;
		faultPDU.AllocationHint = 32u;
		faultPDU.Status = FaultStatus.ProtocolError;
		Append(faultPDU.GetBytes());
	}

	private void Append(byte[] buffer)
	{
		MemoryStream item = new MemoryStream(buffer);
		m_outputStreams.Add(item);
	}

	public override void Flush()
	{
	}

	public override void Close()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}
}
