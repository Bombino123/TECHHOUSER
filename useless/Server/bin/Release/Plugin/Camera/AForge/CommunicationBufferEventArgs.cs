using System;

namespace AForge;

public class CommunicationBufferEventArgs : EventArgs
{
	private readonly byte[] message;

	private readonly int index;

	private readonly int length;

	public int MessageLength => length;

	public CommunicationBufferEventArgs(byte[] message)
	{
		this.message = message;
		index = 0;
		length = message.Length;
	}

	public CommunicationBufferEventArgs(byte[] buffer, int index, int length)
	{
		message = buffer;
		this.index = index;
		this.length = length;
	}

	public byte[] GetMessage()
	{
		byte[] array = new byte[length];
		Array.Copy(message, index, array, 0, length);
		return array;
	}

	public string GetMessageString()
	{
		return BitConverter.ToString(message, index, length);
	}
}
