using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;
using Toolbelt.Drawing;

namespace Server.Helper
{
	// Token: 0x02000071 RID: 113
	internal class Methods
	{
		// Token: 0x06000262 RID: 610 RVA: 0x000236B4 File Offset: 0x000218B4
		public static byte[] getIcon(string hash, object[] list)
		{
			for (int i = 1; i < list.Length; i += 2)
			{
				if ((string)list[i] == hash)
				{
					return (byte[])list[i - 1];
				}
			}
			return null;
		}

		// Token: 0x06000263 RID: 611 RVA: 0x000236EC File Offset: 0x000218EC
		public static string Shuffle(string str)
		{
			char[] array = str.ToCharArray();
			Random random = new Random();
			int i = array.Length;
			while (i > 1)
			{
				i--;
				int num = random.Next(i + 1);
				char c = array[num];
				array[num] = array[i];
				array[i] = c;
			}
			return new string(array);
		}

		// Token: 0x06000264 RID: 612 RVA: 0x00023734 File Offset: 0x00021934
		public static string GetPublicIpAsync()
		{
			try
			{
				using (WebClient webClient = new WebClient())
				{
					return webClient.DownloadString("http://icanhazip.com").Replace("\n", "");
				}
			}
			catch
			{
			}
			return "127.0.0.1";
		}

		// Token: 0x06000265 RID: 613 RVA: 0x00023794 File Offset: 0x00021994
		public static string GetIcon(string path)
		{
			try
			{
				string text = Path.GetTempFileName() + ".ico";
				using (FileStream fileStream = new FileStream(text, FileMode.Create))
				{
					IconExtractor.Extract1stIconTo(path, fileStream);
				}
				return text;
			}
			catch
			{
			}
			return "";
		}

		// Token: 0x06000266 RID: 614 RVA: 0x000237F8 File Offset: 0x000219F8
		public static string GetChecksum(string file)
		{
			string result;
			using (FileStream fileStream = File.OpenRead(file))
			{
				result = BitConverter.ToString(new SHA256Managed().ComputeHash(fileStream)).Replace("-", string.Empty);
			}
			return result;
		}

		// Token: 0x06000267 RID: 615 RVA: 0x0002384C File Offset: 0x00021A4C
		public static void AppendLogs(string client, string message, Color color)
		{
			DataGridViewRow Item = new DataGridViewRow();
			Item.DefaultCellStyle = new DataGridViewCellStyle
			{
				Alignment = DataGridViewContentAlignment.MiddleLeft,
				ForeColor = color,
				SelectionForeColor = Color.White,
				Font = new Font("Segoe UI", 11f, FontStyle.Regular, GraphicsUnit.Pixel),
				WrapMode = DataGridViewTriState.False
			};
			Item.Cells.Add(new DataGridViewTextBoxCell
			{
				Value = client
			});
			Item.Cells.Add(new DataGridViewTextBoxCell
			{
				Value = DateTime.Now.ToString("HH:mm:ss")
			});
			Item.Cells.Add(new DataGridViewTextBoxCell
			{
				Value = message
			});
			Program.form.GridLogs.Invoke(new MethodInvoker(delegate()
			{
				Program.form.GridLogs.Rows.Insert(0, Item);
			}));
		}

		// Token: 0x06000268 RID: 616 RVA: 0x00023938 File Offset: 0x00021B38
		public static string BytesToString(long byteCount)
		{
			string[] array = new string[]
			{
				"B",
				"KB",
				"MB",
				"GB",
				"TB",
				"PB",
				"EB"
			};
			if (byteCount == 0L)
			{
				return "0" + array[0];
			}
			long num = Math.Abs(byteCount);
			int num2 = Convert.ToInt32(Math.Floor(Math.Log((double)num, 1024.0)));
			double num3 = Math.Round((double)num / Math.Pow(1024.0, (double)num2), 1);
			return ((double)Math.Sign(byteCount) * num3).ToString() + " " + array[num2];
		}

		// Token: 0x06000269 RID: 617 RVA: 0x000239F0 File Offset: 0x00021BF0
		public static Bitmap ByteArrayToBitmap(byte[] byteArray)
		{
			Bitmap result;
			using (MemoryStream memoryStream = new MemoryStream(byteArray))
			{
				result = new Bitmap(memoryStream);
			}
			return result;
		}
	}
}
