using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Server.Helper
{
	// Token: 0x0200007A RID: 122
	public class RegistrySeeker
	{
		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060002A2 RID: 674 RVA: 0x00024485 File Offset: 0x00022685
		public RegistrySeeker.RegSeekerMatch[] Matches
		{
			get
			{
				List<RegistrySeeker.RegSeekerMatch> matches = this._matches;
				if (matches == null)
				{
					return null;
				}
				return matches.ToArray();
			}
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x00024498 File Offset: 0x00022698
		public RegistrySeeker()
		{
			this._matches = new List<RegistrySeeker.RegSeekerMatch>();
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x000244AC File Offset: 0x000226AC
		public void BeginSeeking(string rootKeyName)
		{
			if (!string.IsNullOrEmpty(rootKeyName))
			{
				using (RegistryKey rootKey = RegistrySeeker.GetRootKey(rootKeyName))
				{
					if (rootKey != null && rootKey.Name != rootKeyName)
					{
						string name = rootKeyName.Substring(rootKey.Name.Length + 1);
						using (RegistryKey registryKey = rootKey.OpenReadonlySubKeySafe(name))
						{
							if (registryKey == null)
							{
								return;
							}
							this.Seek(registryKey);
							return;
						}
					}
					this.Seek(rootKey);
					return;
				}
			}
			this.Seek(null);
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x00024544 File Offset: 0x00022744
		private void Seek(RegistryKey rootKey)
		{
			if (rootKey == null)
			{
				using (List<RegistryKey>.Enumerator enumerator = RegistrySeeker.GetRootKeys().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						RegistryKey registryKey = enumerator.Current;
						this.ProcessKey(registryKey, registryKey.Name);
					}
					return;
				}
			}
			this.Search(rootKey);
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x000245A8 File Offset: 0x000227A8
		private void Search(RegistryKey rootKey)
		{
			foreach (string text in rootKey.GetSubKeyNames())
			{
				this.ProcessKey(rootKey.OpenReadonlySubKeySafe(text), text);
			}
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x000245DC File Offset: 0x000227DC
		private void ProcessKey(RegistryKey key, string keyName)
		{
			if (key != null)
			{
				List<RegistrySeeker.RegValueData> list = new List<RegistrySeeker.RegValueData>();
				foreach (string name in key.GetValueNames())
				{
					RegistryValueKind valueKind = key.GetValueKind(name);
					object value = key.GetValue(name);
					list.Add(RegistryKeyHelper.CreateRegValueData(name, valueKind, value));
				}
				this.AddMatch(keyName, RegistryKeyHelper.AddDefaultValue(list), key.SubKeyCount);
				return;
			}
			this.AddMatch(keyName, RegistryKeyHelper.GetDefaultValues(), 0);
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x0002464E File Offset: 0x0002284E
		private void AddMatch(string key, RegistrySeeker.RegValueData[] values, int subkeycount)
		{
			this._matches.Add(new RegistrySeeker.RegSeekerMatch
			{
				Key = key,
				Data = values,
				HasSubKeys = (subkeycount > 0)
			});
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x00024678 File Offset: 0x00022878
		public static RegistryKey GetRootKey(string subkeyFullPath)
		{
			string[] array = subkeyFullPath.Split(new char[]
			{
				'\\'
			});
			RegistryKey result;
			try
			{
				string a = array[0];
				if (a == "HKEY_CLASSES_ROOT")
				{
					result = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64);
				}
				else if (a == "HKEY_CURRENT_USER")
				{
					result = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
				}
				else if (a == "HKEY_LOCAL_MACHINE")
				{
					result = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
				}
				else if (a == "HKEY_USERS")
				{
					result = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64);
				}
				else
				{
					if (!(a == "HKEY_CURRENT_CONFIG"))
					{
						throw new Exception("Invalid rootkey, could not be found.");
					}
					result = RegistryKey.OpenBaseKey(RegistryHive.CurrentConfig, RegistryView.Registry64);
				}
			}
			catch (SystemException)
			{
				throw new Exception("Unable to open root registry key, you do not have the needed permissions.");
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return result;
		}

		// Token: 0x060002AA RID: 682 RVA: 0x00024770 File Offset: 0x00022970
		public static List<RegistryKey> GetRootKeys()
		{
			List<RegistryKey> list = new List<RegistryKey>();
			try
			{
				list.Add(RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64));
				list.Add(RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64));
				list.Add(RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64));
				list.Add(RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64));
				list.Add(RegistryKey.OpenBaseKey(RegistryHive.CurrentConfig, RegistryView.Registry64));
			}
			catch (SystemException)
			{
				throw new Exception("Could not open root registry keys, you may not have the needed permission");
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return list;
		}

		// Token: 0x04000180 RID: 384
		private readonly List<RegistrySeeker.RegSeekerMatch> _matches;

		// Token: 0x0200021D RID: 541
		public class RegSeekerMatch
		{
			// Token: 0x17000078 RID: 120
			// (get) Token: 0x060008DE RID: 2270 RVA: 0x000628FA File Offset: 0x00060AFA
			// (set) Token: 0x060008DF RID: 2271 RVA: 0x00062902 File Offset: 0x00060B02
			public string Key { get; set; }

			// Token: 0x17000079 RID: 121
			// (get) Token: 0x060008E0 RID: 2272 RVA: 0x0006290B File Offset: 0x00060B0B
			// (set) Token: 0x060008E1 RID: 2273 RVA: 0x00062913 File Offset: 0x00060B13
			public RegistrySeeker.RegValueData[] Data { get; set; }

			// Token: 0x1700007A RID: 122
			// (get) Token: 0x060008E2 RID: 2274 RVA: 0x0006291C File Offset: 0x00060B1C
			// (set) Token: 0x060008E3 RID: 2275 RVA: 0x00062924 File Offset: 0x00060B24
			public bool HasSubKeys { get; set; }

			// Token: 0x060008E4 RID: 2276 RVA: 0x0006292D File Offset: 0x00060B2D
			public override string ToString()
			{
				return string.Format("({0}:{1})", this.Key, this.Data);
			}
		}

		// Token: 0x0200021E RID: 542
		public class RegValueData
		{
			// Token: 0x1700007B RID: 123
			// (get) Token: 0x060008E6 RID: 2278 RVA: 0x0006294D File Offset: 0x00060B4D
			// (set) Token: 0x060008E7 RID: 2279 RVA: 0x00062955 File Offset: 0x00060B55
			public string Name { get; set; }

			// Token: 0x1700007C RID: 124
			// (get) Token: 0x060008E8 RID: 2280 RVA: 0x0006295E File Offset: 0x00060B5E
			// (set) Token: 0x060008E9 RID: 2281 RVA: 0x00062966 File Offset: 0x00060B66
			public RegistryValueKind Kind { get; set; }

			// Token: 0x1700007D RID: 125
			// (get) Token: 0x060008EA RID: 2282 RVA: 0x0006296F File Offset: 0x00060B6F
			// (set) Token: 0x060008EB RID: 2283 RVA: 0x00062977 File Offset: 0x00060B77
			public byte[] Data { get; set; }
		}
	}
}
