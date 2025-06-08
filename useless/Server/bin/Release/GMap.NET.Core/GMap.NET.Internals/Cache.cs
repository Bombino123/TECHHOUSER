#define TRACE
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GMap.NET.CacheProviders;

namespace GMap.NET.Internals;

internal class Cache
{
	public PureImageCache ImageCache;

	public PureImageCache ImageCacheSecond;

	private string _cache;

	private static readonly SHA1CryptoServiceProvider HashProvider = new SHA1CryptoServiceProvider();

	public string CacheLocation
	{
		get
		{
			return _cache;
		}
		set
		{
			_cache = value;
			if (ImageCache is SQLitePureImageCache)
			{
				(ImageCache as SQLitePureImageCache).CacheLocation = value;
			}
			CacheLocator.Delay = true;
		}
	}

	public static Cache Instance { get; } = new Cache();


	private Cache()
	{
		ImageCache = new SQLitePureImageCache();
		string location = CacheLocator.Location;
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		char directorySeparatorChar = Path.DirectorySeparatorChar;
		string text = directorySeparatorChar.ToString();
		directorySeparatorChar = Path.DirectorySeparatorChar;
		string text2 = folderPath + text + "GMap.NET" + directorySeparatorChar;
		if (Directory.Exists(text2))
		{
			try
			{
				if (Directory.Exists(location))
				{
					Directory.Delete(text2, recursive: true);
				}
				else
				{
					Directory.Move(text2, location);
				}
				CacheLocation = location;
				return;
			}
			catch (Exception ex)
			{
				CacheLocation = text2;
				Trace.WriteLine("SQLitePureImageCache, moving data: " + ex.ToString());
				return;
			}
		}
		CacheLocation = location;
	}

	private void ConvertToHash(ref string s)
	{
		s = BitConverter.ToString(HashProvider.ComputeHash(Encoding.Unicode.GetBytes(s)));
	}

	public void SaveContent(string url, CacheType type, string content)
	{
		try
		{
			ConvertToHash(ref url);
			string text = Path.Combine(_cache, type.ToString());
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			string text2 = text + directorySeparatorChar;
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			using StreamWriter streamWriter = new StreamWriter(text2 + url + ".txt", append: false, Encoding.UTF8);
			streamWriter.Write(content);
		}
		catch (Exception)
		{
		}
	}

	public string GetContent(string url, CacheType type, TimeSpan stayInCache)
	{
		string result = null;
		try
		{
			ConvertToHash(ref url);
			string text = Path.Combine(_cache, type.ToString());
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			string path = string.Concat(text + directorySeparatorChar, url, ".txt");
			if (File.Exists(path))
			{
				DateTime lastWriteTime = File.GetLastWriteTime(path);
				if (DateTime.Now - lastWriteTime < stayInCache)
				{
					using StreamReader streamReader = new StreamReader(path, Encoding.UTF8);
					result = streamReader.ReadToEnd();
				}
				else
				{
					File.Delete(path);
				}
			}
		}
		catch (Exception)
		{
			result = null;
		}
		return result;
	}

	public string GetContent(string url, CacheType type)
	{
		return GetContent(url, type, TimeSpan.FromDays(100.0));
	}
}
