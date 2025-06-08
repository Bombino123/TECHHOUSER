using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public abstract class ACE
{
	public abstract int Length { get; }

	public abstract void WriteBytes(byte[] buffer, ref int offset);

	public static ACE GetAce(byte[] buffer, int offset)
	{
		return (AceType)ByteReader.ReadByte(buffer, offset) switch
		{
			AceType.ACCESS_ALLOWED_ACE_TYPE => new AccessAllowedACE(buffer, offset), 
			AceType.ACCESS_DENIED_ACE_TYPE => new AccessDeniedACE(buffer, offset), 
			_ => throw new NotImplementedException(), 
		};
	}
}
