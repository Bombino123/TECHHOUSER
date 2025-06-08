using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Leb128;
using Server.Connectings;
using Server.Forms;
using Server.Helper;

namespace Server.Messages
{
	// Token: 0x02000060 RID: 96
	internal class HandlerExplorer
	{
		// Token: 0x060001F7 RID: 503 RVA: 0x00020120 File Offset: 0x0001E320
		public static void Read(Clients client, object[] objects)
		{
			string text = (string)objects[1];
			if (text != null)
			{
				switch (text.Length)
				{
				case 5:
				{
					char c = text[0];
					if (c != 'E')
					{
						if (c != 'F')
						{
							return;
						}
						if (!(text == "Files"))
						{
							return;
						}
						if (client.Tag == null)
						{
							client.Disconnect();
							return;
						}
						FormExplorer form = (FormExplorer)client.Tag;
						form.Invoke(new MethodInvoker(delegate()
						{
							form.rjTextBox1.Texts = (string)objects[2];
							form.materialLabel1.Text = "Succues Get Folder's and File's";
							form.dataGridView2.Rows.Clear();
							DataGridViewRow dataGridViewRow = new DataGridViewRow();
							dataGridViewRow.Cells.Add(new DataGridViewImageCell
							{
								Value = form.imageList1.Images["folder.png"],
								Tag = 2,
								ImageLayout = DataGridViewImageCellLayout.Zoom
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = "..."
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = ""
							});
							dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
							{
								Value = ""
							});
							form.dataGridView2.Rows.Add(dataGridViewRow);
							object[] array = LEB128.Read((byte[])objects[3]);
							int i = 0;
							while (i < array.Length)
							{
								string value = (string)array[i++];
								string value2 = (string)array[i++];
								string value3 = (string)array[i++];
								DataGridViewRow dataGridViewRow2 = new DataGridViewRow();
								dataGridViewRow2.Cells.Add(new DataGridViewImageCell
								{
									Value = form.imageList1.Images["folder.png"],
									Tag = 1,
									ImageLayout = DataGridViewImageCellLayout.Zoom
								});
								dataGridViewRow2.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = value
								});
								dataGridViewRow2.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = value3
								});
								dataGridViewRow2.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = value2
								});
								dataGridViewRow2.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = ""
								});
								form.dataGridView2.Rows.Add(dataGridViewRow2);
							}
							object[] array2 = LEB128.Read((byte[])objects[4]);
							object[] list = LEB128.Read((byte[])objects[5]);
							int j = 0;
							while (j < array2.Length)
							{
								string value4 = (string)array2[j++];
								string hash = (string)array2[j++];
								string value5 = (string)array2[j++];
								string value6 = (string)array2[j++];
								long byteCount = (long)array2[j++];
								DataGridViewRow dataGridViewRow3 = new DataGridViewRow();
								using (MemoryStream memoryStream = new MemoryStream(Methods.getIcon(hash, list)))
								{
									dataGridViewRow3.Cells.Add(new DataGridViewImageCell
									{
										Value = Image.FromStream(memoryStream),
										Tag = 0,
										ImageLayout = DataGridViewImageCellLayout.Zoom
									});
								}
								dataGridViewRow3.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = value4
								});
								dataGridViewRow3.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = value6
								});
								dataGridViewRow3.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = value5
								});
								dataGridViewRow3.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = Methods.BytesToString(byteCount)
								});
								form.dataGridView2.Rows.Add(dataGridViewRow3);
							}
							form.dataGridView1.Enabled = true;
							form.dataGridView2.Enabled = true;
							form.dataGridView3.Enabled = true;
						}));
						return;
					}
					else
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
						FormExplorer form = (FormExplorer)client.Tag;
						form.Invoke(new MethodInvoker(delegate()
						{
							form.materialLabel1.Text = "Error: " + (string)objects[2];
							form.dataGridView1.Enabled = true;
							form.dataGridView2.Enabled = true;
							form.dataGridView3.Enabled = true;
						}));
						return;
					}
					break;
				}
				case 6:
				case 8:
				case 9:
				case 14:
					break;
				case 7:
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
							if (!(text == "Renamed"))
							{
								return;
							}
							if (client.Tag == null)
							{
								client.Disconnect();
								return;
							}
							FormExplorer form = (FormExplorer)client.Tag;
							form.Invoke(new MethodInvoker(delegate()
							{
								form.materialLabel1.Text = "Renamed file or directory";
								foreach (object obj in ((IEnumerable)form.dataGridView2.Rows))
								{
									DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
									if ((string)dataGridViewRow.Cells[1].Value == (string)objects[2])
									{
										dataGridViewRow.Cells[1].Value = (string)objects[3];
										dataGridViewRow.Cells[2].Value = (string)objects[4];
										dataGridViewRow.Cells[3].Value = (string)objects[5];
										if (objects.Length > 5)
										{
											using (MemoryStream memoryStream = new MemoryStream((byte[])objects[6]))
											{
												dataGridViewRow.Cells[0].Value = Image.FromStream(memoryStream);
											}
											dataGridViewRow.Cells[4].Value = Methods.BytesToString((long)objects[7]);
											break;
										}
										break;
									}
								}
							}));
							return;
						}
						else
						{
							if (!(text == "Deleted"))
							{
								return;
							}
							if (client.Tag == null)
							{
								client.Disconnect();
								return;
							}
							FormExplorer form = (FormExplorer)client.Tag;
							form.Invoke(new MethodInvoker(delegate()
							{
								form.materialLabel1.Text = "Deleted file or directory";
								foreach (object obj in ((IEnumerable)form.dataGridView2.Rows))
								{
									DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
									if ((string)dataGridViewRow.Cells[1].Value == (string)objects[2])
									{
										form.dataGridView2.Rows.Remove(dataGridViewRow);
										break;
									}
								}
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
						FormExplorer form = (FormExplorer)Application.OpenForms["Explorer:" + (string)objects[2]];
						if (form == null)
						{
							client.Disconnect();
							return;
						}
						form.Invoke(new MethodInvoker(delegate()
						{
							form.Text = "Explorer [" + (string)objects[2] + "]";
							form.client = client;
							form.materialLabel1.Text = "Succues Connect";
							foreach (string value in ((string)objects[3]).Split(new char[]
							{
								','
							}))
							{
								DataGridViewRow dataGridViewRow = new DataGridViewRow();
								dataGridViewRow.Cells.Add(new DataGridViewImageCell
								{
									Value = form.imageList1.Images["folder.png"]
								});
								dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = value
								});
								form.dataGridView3.Rows.Add(dataGridViewRow);
							}
							foreach (string text2 in ((string)objects[4]).Split(new char[]
							{
								','
							}))
							{
								string value2 = text2.Split(new char[]
								{
									';'
								})[0];
								string a = text2.Split(new char[]
								{
									';'
								})[1];
								DataGridViewRow dataGridViewRow2 = new DataGridViewRow();
								if (a == "Drive")
								{
									dataGridViewRow2.Cells.Add(new DataGridViewImageCell
									{
										Value = form.imageList1.Images["hard-disk.png"]
									});
								}
								else
								{
									dataGridViewRow2.Cells.Add(new DataGridViewImageCell
									{
										Value = form.imageList1.Images["usb-drive.png"]
									});
								}
								dataGridViewRow2.Cells.Add(new DataGridViewTextBoxCell
								{
									Value = value2
								});
								form.dataGridView1.Rows.Add(dataGridViewRow2);
							}
							form.dataGridView1.Enabled = true;
							form.dataGridView2.Enabled = true;
							form.dataGridView3.Enabled = true;
						}));
						client.Tag = form;
						client.Hwid = (string)objects[2];
						return;
					}
					break;
				}
				case 10:
				{
					if (!(text == "CreatedDir"))
					{
						return;
					}
					if (client.Tag == null)
					{
						client.Disconnect();
						return;
					}
					FormExplorer form = (FormExplorer)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.materialLabel1.Text = "Created New Directory";
						DataGridViewRow dataGridViewRow = new DataGridViewRow();
						dataGridViewRow.Cells.Add(new DataGridViewImageCell
						{
							Value = form.imageList1.Images["folder.png"],
							Tag = 1,
							ImageLayout = DataGridViewImageCellLayout.Zoom
						});
						dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = (string)objects[2]
						});
						dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = (string)objects[3]
						});
						dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = (string)objects[4]
						});
						dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = ""
						});
						form.dataGridView2.Rows.Add(dataGridViewRow);
						form.dataGridView2.Sort(new CustomComparer());
					}));
					return;
				}
				case 11:
				{
					if (!(text == "CreatedFile"))
					{
						return;
					}
					if (client.Tag == null)
					{
						client.Disconnect();
						return;
					}
					FormExplorer form = (FormExplorer)client.Tag;
					form.Invoke(new MethodInvoker(delegate()
					{
						form.materialLabel1.Text = "Created New File";
						DataGridViewRow dataGridViewRow = new DataGridViewRow();
						using (MemoryStream memoryStream = new MemoryStream((byte[])objects[2]))
						{
							dataGridViewRow.Cells.Add(new DataGridViewImageCell
							{
								Value = Image.FromStream(memoryStream),
								Tag = 0,
								ImageLayout = DataGridViewImageCellLayout.Zoom
							});
						}
						dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = (string)objects[3]
						});
						dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = (string)objects[4]
						});
						dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = (string)objects[5]
						});
						dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = Methods.BytesToString((long)objects[6])
						});
						form.dataGridView2.Rows.Add(dataGridViewRow);
						form.dataGridView2.Sort(new CustomComparer());
					}));
					return;
				}
				case 12:
				{
					if (!(text == "DownloadFile"))
					{
						return;
					}
					if (client.Tag == null)
					{
						client.Disconnect();
						return;
					}
					FormDownload formDownload = (FormDownload)client.Tag;
					formDownload.Close();
					if (!Directory.Exists("Users\\" + client.Hwid + "\\Downloads"))
					{
						Directory.CreateDirectory("Users\\" + client.Hwid + "\\Downloads");
					}
					File.WriteAllBytes("Users\\" + client.Hwid + "\\Downloads\\" + formDownload.NameFile, (byte[])objects[2]);
					client.Disconnect();
					break;
				}
				case 13:
				{
					if (!(text == "UploadConnect"))
					{
						return;
					}
					FormUpload form = (FormUpload)Application.OpenForms[(string)objects[2]];
					if (form == null)
					{
						client.Disconnect();
						return;
					}
					form.Invoke(new MethodInvoker(delegate()
					{
						form.client = client;
						form.Connected();
					}));
					client.SendChunk(LEB128.Write(new object[]
					{
						"Uploaded",
						form.pathto,
						form.bytes
					}));
					return;
				}
				case 15:
				{
					if (!(text == "DownloadConnect"))
					{
						return;
					}
					FormExplorer form1 = (FormExplorer)Application.OpenForms["Explorer:" + (string)objects[2]];
					if (form1 == null)
					{
						client.Disconnect();
						return;
					}
					client.Hwid = (string)objects[2];
					form1.Invoke(new MethodInvoker(delegate()
					{
						FormDownload formDownload2 = new FormDownload();
						formDownload2.Text = "Download: " + (string)objects[2];
						formDownload2.Name = "Download: " + (string)objects[2] + "." + (string)objects[4];
						formDownload2.parrent = form1.client;
						formDownload2.client = client;
						formDownload2.SizeFile = (long)objects[3];
						formDownload2.NameFile = (string)objects[4];
						client.Tag = formDownload2;
						formDownload2.Show();
					}));
					return;
				}
				default:
					return;
				}
			}
		}
	}
}
