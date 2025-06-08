using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileDispositionInformation : FileInformation
{
	public const int FixedLength = 1;

	public bool DeletePending;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileDispositionInformation;

	public override int Length => 1;

	public FileDispositionInformation()
	{
	}

	public FileDispositionInformation(byte[] buffer, int offset)
	{
		DeletePending = Convert.ToBoolean(ByteReader.ReadByte(buffer, offset));
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		ByteWriter.WriteByte(buffer, offset, Convert.ToByte(DeletePending));
	}
}
