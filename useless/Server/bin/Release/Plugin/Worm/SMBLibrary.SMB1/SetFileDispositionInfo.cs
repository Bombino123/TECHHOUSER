using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SetFileDispositionInfo : SetInformation
{
	public const int Length = 1;

	public bool DeletePending;

	public override SetInformationLevel InformationLevel => SetInformationLevel.SMB_SET_FILE_DISPOSITION_INFO;

	public SetFileDispositionInfo()
	{
	}

	public SetFileDispositionInfo(byte[] buffer)
		: this(buffer, 0)
	{
	}

	public SetFileDispositionInfo(byte[] buffer, int offset)
	{
		DeletePending = ByteReader.ReadByte(buffer, ref offset) > 0;
	}

	public override byte[] GetBytes()
	{
		byte[] array = new byte[1];
		ByteWriter.WriteByte(array, 0, Convert.ToByte(DeletePending));
		return array;
	}
}
