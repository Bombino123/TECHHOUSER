using System;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public abstract class SessionPacket
{
	public const int HeaderLength = 4;

	public const int MaxSessionPacketLength = 131075;

	public const int MaxDirectTcpPacketLength = 16777215;

	public SessionPacketTypeName Type;

	private int TrailerLength;

	public byte[] Trailer;

	public virtual int Length => 4 + Trailer.Length;

	public SessionPacket()
	{
	}

	public SessionPacket(byte[] buffer, int offset)
	{
		Type = (SessionPacketTypeName)ByteReader.ReadByte(buffer, offset);
		TrailerLength = (ByteReader.ReadByte(buffer, offset + 1) << 16) | BigEndianConverter.ToUInt16(buffer, offset + 2);
		Trailer = ByteReader.ReadBytes(buffer, offset + 4, TrailerLength);
	}

	public virtual byte[] GetBytes()
	{
		TrailerLength = Trailer.Length;
		byte value = Convert.ToByte(TrailerLength >> 16);
		byte[] array = new byte[4 + Trailer.Length];
		ByteWriter.WriteByte(array, 0, (byte)Type);
		ByteWriter.WriteByte(array, 1, value);
		BigEndianWriter.WriteUInt16(array, 2, (ushort)((uint)TrailerLength & 0xFFFFu));
		ByteWriter.WriteBytes(array, 4, Trailer);
		return array;
	}

	public static int GetSessionPacketLength(byte[] buffer, int offset)
	{
		int num = (ByteReader.ReadByte(buffer, offset + 1) << 16) | BigEndianConverter.ToUInt16(buffer, offset + 2);
		return 4 + num;
	}

	public static SessionPacket GetSessionPacket(byte[] buffer, int offset)
	{
		SessionPacketTypeName sessionPacketTypeName = (SessionPacketTypeName)ByteReader.ReadByte(buffer, offset);
		switch (sessionPacketTypeName)
		{
		case SessionPacketTypeName.SessionMessage:
			return new SessionMessagePacket(buffer, offset);
		case SessionPacketTypeName.SessionRequest:
			return new SessionRequestPacket(buffer, offset);
		case SessionPacketTypeName.PositiveSessionResponse:
			return new PositiveSessionResponsePacket(buffer, offset);
		case SessionPacketTypeName.NegativeSessionResponse:
			return new NegativeSessionResponsePacket(buffer, offset);
		case SessionPacketTypeName.RetargetSessionResponse:
			return new SessionRetargetResponsePacket(buffer, offset);
		case SessionPacketTypeName.SessionKeepAlive:
			return new SessionKeepAlivePacket(buffer, offset);
		default:
		{
			byte b = (byte)sessionPacketTypeName;
			throw new InvalidDataException("Invalid NetBIOS session packet type: 0x" + b.ToString("X2"));
		}
		}
	}
}
