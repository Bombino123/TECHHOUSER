using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Server.Helper
{
	// Token: 0x0200006E RID: 110
	public class DiscordWebhook
	{
		// Token: 0x06000257 RID: 599 RVA: 0x00023024 File Offset: 0x00021224
		public static string Send(string mssgBody, string userName, string webhook)
		{
			HttpWebResponse httpWebResponse = DiscordWebhook.FormUpload.MultipartFormDataPost(webhook, DiscordWebhook.defaultUserAgent, new Dictionary<string, object>
			{
				{
					"username",
					userName
				},
				{
					"content",
					mssgBody
				},
				{
					"avatar_url",
					DiscordWebhook.defaultAvatar
				}
			});
			string result = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
			httpWebResponse.Close();
			return result;
		}

		// Token: 0x04000171 RID: 369
		private static string defaultUserAgent = "";

		// Token: 0x04000172 RID: 370
		private static string defaultAvatar = "";

		// Token: 0x0200021A RID: 538
		public static class FormUpload
		{
			// Token: 0x060008D5 RID: 2261 RVA: 0x00062618 File Offset: 0x00060818
			public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters)
			{
				string text = string.Format("----------{0:N}", Guid.NewGuid());
				string contentType = "multipart/form-data; boundary=" + text;
				byte[] multipartFormData = DiscordWebhook.FormUpload.GetMultipartFormData(postParameters, text);
				return DiscordWebhook.FormUpload.PostForm(postUrl, userAgent, contentType, multipartFormData);
			}

			// Token: 0x060008D6 RID: 2262 RVA: 0x00062658 File Offset: 0x00060858
			private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData)
			{
				HttpWebRequest httpWebRequest = WebRequest.Create(postUrl) as HttpWebRequest;
				if (httpWebRequest == null)
				{
					throw new NullReferenceException("request is not a http request");
				}
				httpWebRequest.Method = "POST";
				httpWebRequest.ContentType = contentType;
				httpWebRequest.UserAgent = userAgent;
				httpWebRequest.CookieContainer = new CookieContainer();
				httpWebRequest.ContentLength = (long)formData.Length;
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(formData, 0, formData.Length);
					requestStream.Close();
				}
				return httpWebRequest.GetResponse() as HttpWebResponse;
			}

			// Token: 0x060008D7 RID: 2263 RVA: 0x000626EC File Offset: 0x000608EC
			private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
			{
				Stream stream = new MemoryStream();
				bool flag = false;
				foreach (KeyValuePair<string, object> keyValuePair in postParameters)
				{
					if (flag)
					{
						stream.Write(DiscordWebhook.FormUpload.encoding.GetBytes("\r\n"), 0, DiscordWebhook.FormUpload.encoding.GetByteCount("\r\n"));
					}
					flag = true;
					if (keyValuePair.Value is DiscordWebhook.FormUpload.FileParameter)
					{
						DiscordWebhook.FormUpload.FileParameter fileParameter = (DiscordWebhook.FormUpload.FileParameter)keyValuePair.Value;
						string s = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n", new object[]
						{
							boundary,
							keyValuePair.Key,
							fileParameter.FileName ?? keyValuePair.Key,
							fileParameter.ContentType ?? "application/octet-stream"
						});
						stream.Write(DiscordWebhook.FormUpload.encoding.GetBytes(s), 0, DiscordWebhook.FormUpload.encoding.GetByteCount(s));
						stream.Write(fileParameter.File, 0, fileParameter.File.Length);
					}
					else
					{
						string s2 = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, keyValuePair.Key, keyValuePair.Value);
						stream.Write(DiscordWebhook.FormUpload.encoding.GetBytes(s2), 0, DiscordWebhook.FormUpload.encoding.GetByteCount(s2));
					}
				}
				string s3 = "\r\n--" + boundary + "--\r\n";
				stream.Write(DiscordWebhook.FormUpload.encoding.GetBytes(s3), 0, DiscordWebhook.FormUpload.encoding.GetByteCount(s3));
				stream.Position = 0L;
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				stream.Close();
				return array;
			}

			// Token: 0x04000810 RID: 2064
			private static readonly Encoding encoding = Encoding.UTF8;

			// Token: 0x0200025A RID: 602
			public class FileParameter
			{
				// Token: 0x1700007F RID: 127
				// (get) Token: 0x06000971 RID: 2417 RVA: 0x00063AF8 File Offset: 0x00061CF8
				// (set) Token: 0x06000972 RID: 2418 RVA: 0x00063B00 File Offset: 0x00061D00
				public byte[] File { get; set; }

				// Token: 0x17000080 RID: 128
				// (get) Token: 0x06000973 RID: 2419 RVA: 0x00063B09 File Offset: 0x00061D09
				// (set) Token: 0x06000974 RID: 2420 RVA: 0x00063B11 File Offset: 0x00061D11
				public string FileName { get; set; }

				// Token: 0x17000081 RID: 129
				// (get) Token: 0x06000975 RID: 2421 RVA: 0x00063B1A File Offset: 0x00061D1A
				// (set) Token: 0x06000976 RID: 2422 RVA: 0x00063B22 File Offset: 0x00061D22
				public string ContentType { get; set; }

				// Token: 0x06000977 RID: 2423 RVA: 0x00063B2B File Offset: 0x00061D2B
				public FileParameter(byte[] file) : this(file, null)
				{
				}

				// Token: 0x06000978 RID: 2424 RVA: 0x00063B35 File Offset: 0x00061D35
				public FileParameter(byte[] file, string filename) : this(file, filename, null)
				{
				}

				// Token: 0x06000979 RID: 2425 RVA: 0x00063B40 File Offset: 0x00061D40
				public FileParameter(byte[] file, string filename, string contenttype)
				{
					this.File = file;
					this.FileName = filename;
					this.ContentType = contenttype;
				}
			}
		}
	}
}
