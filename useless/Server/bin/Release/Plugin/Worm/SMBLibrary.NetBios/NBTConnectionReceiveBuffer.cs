using System;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public class NBTConnectionReceiveBuffer : IDisposable
{
	private byte[] m_buffer;

	private int m_readOffset;

	private int m_bytesInBuffer;

	private int? m_packetLength;

	public byte[] Buffer => m_buffer;

	public int WriteOffset => m_readOffset + m_bytesInBuffer;

	public int BytesInBuffer => m_bytesInBuffer;

	public int AvailableLength => m_buffer.Length - (m_readOffset + m_bytesInBuffer);

	public NBTConnectionReceiveBuffer()
		: this(131075)
	{
	}

	public NBTConnectionReceiveBuffer(int bufferLength)
	{
		if (bufferLength < 131075)
		{
			throw new ArgumentException("bufferLength must be large enough to hold the largest possible NBT packet");
		}
		m_buffer = new byte[bufferLength];
	}

	public void IncreaseBufferSize(int bufferLength)
	{
		byte[] array = new byte[bufferLength];
		if (m_bytesInBuffer > 0)
		{
			Array.Copy(m_buffer, m_readOffset, array, 0, m_bytesInBuffer);
			m_readOffset = 0;
		}
		m_buffer = array;
	}

	public void SetNumberOfBytesReceived(int numberOfBytesReceived)
	{
		m_bytesInBuffer += numberOfBytesReceived;
	}

	public bool HasCompletePacket()
	{
		if (m_bytesInBuffer >= 4)
		{
			if (!m_packetLength.HasValue)
			{
				m_packetLength = SessionPacket.GetSessionPacketLength(m_buffer, m_readOffset);
			}
			return m_bytesInBuffer >= m_packetLength.Value;
		}
		return false;
	}

	public SessionPacket DequeuePacket()
	{
		SessionPacket sessionPacket;
		try
		{
			sessionPacket = SessionPacket.GetSessionPacket(m_buffer, m_readOffset);
		}
		catch (IndexOutOfRangeException innerException)
		{
			throw new InvalidDataException("Invalid NetBIOS session packet", innerException);
		}
		RemovePacketBytes();
		return sessionPacket;
	}

	public byte[] DequeuePacketBytes()
	{
		byte[] result = ByteReader.ReadBytes(m_buffer, m_readOffset, m_packetLength.Value);
		RemovePacketBytes();
		return result;
	}

	private void RemovePacketBytes()
	{
		m_bytesInBuffer -= m_packetLength.Value;
		if (m_bytesInBuffer == 0)
		{
			m_readOffset = 0;
			m_packetLength = null;
			return;
		}
		m_readOffset += m_packetLength.Value;
		m_packetLength = null;
		if (!HasCompletePacket())
		{
			Array.Copy(m_buffer, m_readOffset, m_buffer, 0, m_bytesInBuffer);
			m_readOffset = 0;
		}
	}

	public void Dispose()
	{
		m_buffer = null;
	}
}
