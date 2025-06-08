using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public struct SyntaxID
{
	public const int Length = 20;

	public Guid InterfaceUUID;

	public uint InterfaceVersion;

	public SyntaxID(Guid interfaceUUID, uint interfaceVersion)
	{
		InterfaceUUID = interfaceUUID;
		InterfaceVersion = interfaceVersion;
	}

	public SyntaxID(byte[] buffer, int offset)
	{
		InterfaceUUID = LittleEndianConverter.ToGuid(buffer, offset);
		InterfaceVersion = LittleEndianConverter.ToUInt32(buffer, offset + 16);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteGuid(buffer, offset, InterfaceUUID);
		LittleEndianWriter.WriteUInt32(buffer, offset + 16, InterfaceVersion);
	}

	public override bool Equals(object obj)
	{
		if (obj is SyntaxID)
		{
			if (InterfaceUUID.Equals(((SyntaxID)obj).InterfaceUUID))
			{
				return InterfaceVersion.Equals(((SyntaxID)obj).InterfaceVersion);
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return InterfaceUUID.GetHashCode() * InterfaceVersion.GetHashCode();
	}
}
