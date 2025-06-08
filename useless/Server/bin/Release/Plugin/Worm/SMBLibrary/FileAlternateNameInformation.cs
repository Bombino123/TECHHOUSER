using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public class FileAlternateNameInformation : FileNameInformation
{
	public override FileInformationClass FileInformationClass => FileInformationClass.FileAlternateNameInformation;

	public FileAlternateNameInformation()
	{
	}

	public FileAlternateNameInformation(byte[] buffer, int offset)
		: base(buffer, offset)
	{
	}
}
