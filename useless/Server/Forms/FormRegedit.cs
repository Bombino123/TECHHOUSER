using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Win32;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000B7 RID: 183
	public partial class FormRegedit : FormMaterial
	{
		// Token: 0x060005C0 RID: 1472 RVA: 0x00053C84 File Offset: 0x00051E84
		public FormRegedit()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x00053CA4 File Offset: 0x00051EA4
		private void FormRegedit_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			this.tvRegistryDirectory.AfterLabelEdit += this.tvRegistryDirectory_AfterLabelEdit;
			this.tvRegistryDirectory.BeforeExpand += this.tvRegistryDirectory_BeforeExpand;
			this.tvRegistryDirectory.BeforeSelect += this.tvRegistryDirectory_BeforeSelect;
			this.tvRegistryDirectory.NodeMouseClick += this.tvRegistryDirectory_NodeMouseClick;
			this.tvRegistryDirectory.KeyUp += this.tvRegistryDirectory_KeyUp;
			typeof(RegistryTreeView).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, this.tvRegistryDirectory, new object[]
			{
				true
			});
			typeof(AeroListView).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, this.lstRegistryValues, new object[]
			{
				true
			});
		}

		// Token: 0x060005C2 RID: 1474 RVA: 0x00053DAC File Offset: 0x00051FAC
		private void ChangeScheme(object sender)
		{
			this.tvRegistryDirectory.ForeColor = FormMaterial.PrimaryColor;
			this.lstRegistryValues.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060005C3 RID: 1475 RVA: 0x00053DCE File Offset: 0x00051FCE
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x00053DE3 File Offset: 0x00051FE3
		private void timer1_Tick(object sender, EventArgs e)
		{
			if (!this.parrent.itsConnect)
			{
				base.Close();
			}
			if (this.client != null && !this.client.itsConnect)
			{
				base.Close();
			}
		}

		// Token: 0x060005C5 RID: 1477 RVA: 0x00053E14 File Offset: 0x00052014
		private void AddRootKey(RegistrySeeker.RegSeekerMatch match)
		{
			TreeNode treeNode = this.CreateNode(match.Key, match.Key, match.Data);
			treeNode.Nodes.Add(new TreeNode());
			this.tvRegistryDirectory.Nodes.Add(treeNode);
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x00053E60 File Offset: 0x00052060
		private TreeNode AddKeyToTree(TreeNode parent, RegistrySeeker.RegSeekerMatch subKey)
		{
			TreeNode treeNode = this.CreateNode(subKey.Key, subKey.Key, subKey.Data);
			parent.Nodes.Add(treeNode);
			if (subKey.HasSubKeys)
			{
				treeNode.Nodes.Add(new TreeNode());
			}
			return treeNode;
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x00053EAD File Offset: 0x000520AD
		private TreeNode CreateNode(string key, string text, object tag)
		{
			return new TreeNode
			{
				Text = text,
				Name = key,
				Tag = tag
			};
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x00053ECC File Offset: 0x000520CC
		public void AddKeys(string rootKey, RegistrySeeker.RegSeekerMatch[] matches)
		{
			if (string.IsNullOrEmpty(rootKey))
			{
				this.tvRegistryDirectory.BeginUpdate();
				foreach (RegistrySeeker.RegSeekerMatch match in matches)
				{
					this.AddRootKey(match);
				}
				this.tvRegistryDirectory.SelectedNode = this.tvRegistryDirectory.Nodes[0];
				this.tvRegistryDirectory.EndUpdate();
				return;
			}
			TreeNode treeNode = this.GetTreeNode(rootKey);
			if (treeNode == null)
			{
				return;
			}
			this.tvRegistryDirectory.BeginUpdate();
			foreach (RegistrySeeker.RegSeekerMatch subKey in matches)
			{
				this.AddKeyToTree(treeNode, subKey);
			}
			treeNode.Expand();
			this.tvRegistryDirectory.EndUpdate();
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x00053F78 File Offset: 0x00052178
		public void CreateNewKey(string rootKey, RegistrySeeker.RegSeekerMatch match)
		{
			TreeNode treeNode = this.AddKeyToTree(this.GetTreeNode(rootKey), match);
			treeNode.EnsureVisible();
			this.tvRegistryDirectory.SelectedNode = treeNode;
			treeNode.Expand();
			this.tvRegistryDirectory.LabelEdit = true;
			treeNode.BeginEdit();
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x00053FC0 File Offset: 0x000521C0
		public void DeleteKey(string rootKey, string subKey)
		{
			TreeNode treeNode = this.GetTreeNode(rootKey);
			if (!treeNode.Nodes.ContainsKey(subKey))
			{
				return;
			}
			treeNode.Nodes.RemoveByKey(subKey);
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x00053FF0 File Offset: 0x000521F0
		public void RenameKey(string rootKey, string oldName, string newName)
		{
			TreeNode treeNode = this.GetTreeNode(rootKey);
			if (!treeNode.Nodes.ContainsKey(oldName))
			{
				return;
			}
			treeNode.Nodes[oldName].Text = newName;
			treeNode.Nodes[oldName].Name = newName;
			this.tvRegistryDirectory.SelectedNode = treeNode.Nodes[newName];
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x00054050 File Offset: 0x00052250
		private TreeNode GetTreeNode(string path)
		{
			string[] array = path.Split(new char[]
			{
				'\\'
			});
			TreeNode treeNode = this.tvRegistryDirectory.Nodes[array[0]];
			if (treeNode == null)
			{
				return null;
			}
			for (int i = 1; i < array.Length; i++)
			{
				treeNode = treeNode.Nodes[array[i]];
				if (treeNode == null)
				{
					return null;
				}
			}
			return treeNode;
		}

		// Token: 0x060005CD RID: 1485 RVA: 0x000540AC File Offset: 0x000522AC
		public void CreateValue(string keyPath, RegistrySeeker.RegValueData value)
		{
			TreeNode treeNode = this.GetTreeNode(keyPath);
			if (treeNode == null)
			{
				return;
			}
			List<RegistrySeeker.RegValueData> list = ((RegistrySeeker.RegValueData[])treeNode.Tag).ToList<RegistrySeeker.RegValueData>();
			list.Add(value);
			treeNode.Tag = list.ToArray();
			if (this.tvRegistryDirectory.SelectedNode == treeNode)
			{
				RegistryValueLstItem registryValueLstItem = new RegistryValueLstItem(value);
				this.lstRegistryValues.Items.Add(registryValueLstItem);
				this.lstRegistryValues.SelectedIndices.Clear();
				registryValueLstItem.Selected = true;
				this.lstRegistryValues.LabelEdit = true;
				registryValueLstItem.BeginEdit();
			}
			this.tvRegistryDirectory.SelectedNode = treeNode;
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x00054148 File Offset: 0x00052348
		public void DeleteValue(string keyPath, string valueName)
		{
			TreeNode treeNode = this.GetTreeNode(keyPath);
			if (treeNode == null)
			{
				return;
			}
			if (!RegValueHelper.IsDefaultValue(valueName))
			{
				treeNode.Tag = (from value in (RegistrySeeker.RegValueData[])treeNode.Tag
				where value.Name != valueName
				select value).ToArray<RegistrySeeker.RegValueData>();
				if (this.tvRegistryDirectory.SelectedNode == treeNode)
				{
					this.lstRegistryValues.Items.RemoveByKey(valueName);
				}
			}
			else
			{
				RegistrySeeker.RegValueData regValueData = ((RegistrySeeker.RegValueData[])treeNode.Tag).First((RegistrySeeker.RegValueData item) => item.Name == valueName);
				if (this.tvRegistryDirectory.SelectedNode == treeNode)
				{
					RegistryValueLstItem registryValueLstItem = this.lstRegistryValues.Items.Cast<RegistryValueLstItem>().SingleOrDefault((RegistryValueLstItem item) => item.Name == valueName);
					if (registryValueLstItem != null)
					{
						registryValueLstItem.Data = regValueData.Kind.RegistryTypeToString(null);
					}
				}
			}
			this.tvRegistryDirectory.SelectedNode = treeNode;
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x00054238 File Offset: 0x00052438
		public void RenameValue(string keyPath, string oldName, string newName)
		{
			TreeNode treeNode = this.GetTreeNode(keyPath);
			if (treeNode == null)
			{
				return;
			}
			((RegistrySeeker.RegValueData[])treeNode.Tag).First((RegistrySeeker.RegValueData item) => item.Name == oldName).Name = newName;
			if (this.tvRegistryDirectory.SelectedNode == treeNode)
			{
				RegistryValueLstItem registryValueLstItem = this.lstRegistryValues.Items.Cast<RegistryValueLstItem>().SingleOrDefault((RegistryValueLstItem item) => item.Name == oldName);
				if (registryValueLstItem != null)
				{
					registryValueLstItem.RegName = newName;
				}
			}
			this.tvRegistryDirectory.SelectedNode = treeNode;
		}

		// Token: 0x060005D0 RID: 1488 RVA: 0x000542C8 File Offset: 0x000524C8
		public void ChangeValue(string keyPath, RegistrySeeker.RegValueData value)
		{
			TreeNode treeNode = this.GetTreeNode(keyPath);
			if (treeNode == null)
			{
				return;
			}
			RegistrySeeker.RegValueData dest = ((RegistrySeeker.RegValueData[])treeNode.Tag).First((RegistrySeeker.RegValueData item) => item.Name == value.Name);
			this.ChangeRegistryValue(value, dest);
			if (this.tvRegistryDirectory.SelectedNode == treeNode)
			{
				RegistryValueLstItem registryValueLstItem = this.lstRegistryValues.Items.Cast<RegistryValueLstItem>().SingleOrDefault((RegistryValueLstItem item) => item.Name == value.Name);
				if (registryValueLstItem != null)
				{
					registryValueLstItem.Data = RegValueHelper.RegistryValueToString(value);
				}
			}
			this.tvRegistryDirectory.SelectedNode = treeNode;
		}

		// Token: 0x060005D1 RID: 1489 RVA: 0x00054368 File Offset: 0x00052568
		private void ChangeRegistryValue(RegistrySeeker.RegValueData source, RegistrySeeker.RegValueData dest)
		{
			if (source.Kind != dest.Kind)
			{
				return;
			}
			dest.Data = source.Data;
		}

		// Token: 0x060005D2 RID: 1490 RVA: 0x00054388 File Offset: 0x00052588
		private void tvRegistryDirectory_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (e.Label == null)
			{
				this.tvRegistryDirectory.LabelEdit = false;
				return;
			}
			e.CancelEdit = true;
			if (e.Label.Length <= 0)
			{
				MessageBox.Show("Invalid label. \nThe label cannot be blank.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				e.Node.BeginEdit();
				return;
			}
			if (e.Node.Parent.Nodes.ContainsKey(e.Label))
			{
				MessageBox.Show("Invalid label. \nA node with that label already exists.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				e.Node.BeginEdit();
				return;
			}
			this.client.Send(new object[]
			{
				"RenameRegistryKey",
				e.Node.Name,
				e.Label,
				e.Node.Parent.FullPath
			});
			this.tvRegistryDirectory.LabelEdit = false;
		}

		// Token: 0x060005D3 RID: 1491 RVA: 0x00054470 File Offset: 0x00052670
		private void tvRegistryDirectory_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			TreeNode node = e.Node;
			if (!string.IsNullOrEmpty(node.FirstNode.Name))
			{
				return;
			}
			this.tvRegistryDirectory.SuspendLayout();
			node.Nodes.Clear();
			this.client.Send(new object[]
			{
				"LoadRegistryKey",
				e.Node.FullPath
			});
			Thread.Sleep(500);
			this.tvRegistryDirectory.ResumeLayout();
			e.Cancel = true;
		}

		// Token: 0x060005D4 RID: 1492 RVA: 0x000544F0 File Offset: 0x000526F0
		private void tvRegistryDirectory_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
			{
				return;
			}
			this.tvRegistryDirectory.SelectedNode = e.Node;
			Point position = new Point(e.X, e.Y);
			this.CreateTreeViewMenuStrip();
			this.tv_ContextMenuStrip.Show(this.tvRegistryDirectory, position);
		}

		// Token: 0x060005D5 RID: 1493 RVA: 0x00054547 File Offset: 0x00052747
		private void tvRegistryDirectory_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			this.UpdateLstRegistryValues(e.Node);
		}

		// Token: 0x060005D6 RID: 1494 RVA: 0x00054555 File Offset: 0x00052755
		private void UpdateLstRegistryValues(TreeNode node)
		{
			this.materialLabel1.Text = "Update Values: " + node.FullPath;
			this.PopulateLstRegistryValues((RegistrySeeker.RegValueData[])node.Tag);
		}

		// Token: 0x060005D7 RID: 1495 RVA: 0x00054583 File Offset: 0x00052783
		private void tvRegistryDirectory_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Delete || !this.GetDeleteState())
			{
				return;
			}
			this.deleteRegistryKey_Click(this, e);
		}

		// Token: 0x060005D8 RID: 1496 RVA: 0x000545A0 File Offset: 0x000527A0
		private void PopulateLstRegistryValues(RegistrySeeker.RegValueData[] values)
		{
			this.lstRegistryValues.BeginUpdate();
			this.lstRegistryValues.Items.Clear();
			values = (from value in values
			orderby value.Name
			select value).ToArray<RegistrySeeker.RegValueData>();
			foreach (RegistrySeeker.RegValueData value2 in values)
			{
				this.lstRegistryValues.Items.Add(new RegistryValueLstItem(value2));
			}
			this.lstRegistryValues.EndUpdate();
		}

		// Token: 0x060005D9 RID: 1497 RVA: 0x0005462A File Offset: 0x0005282A
		private void CreateTreeViewMenuStrip()
		{
			this.renameToolStripMenuItem.Enabled = (this.tvRegistryDirectory.SelectedNode.Parent != null);
			this.deleteToolStripMenuItem.Enabled = (this.tvRegistryDirectory.SelectedNode.Parent != null);
		}

		// Token: 0x060005DA RID: 1498 RVA: 0x00054668 File Offset: 0x00052868
		private void CreateListViewMenuStrip()
		{
			this.modifyToolStripMenuItem.Enabled = (this.modifyBinaryDataToolStripMenuItem.Enabled = (this.lstRegistryValues.SelectedItems.Count == 1));
			this.renameToolStripMenuItem1.Enabled = (this.lstRegistryValues.SelectedItems.Count == 1 && !RegValueHelper.IsDefaultValue(this.lstRegistryValues.SelectedItems[0].Name));
			this.deleteToolStripMenuItem1.Enabled = (this.tvRegistryDirectory.SelectedNode != null && this.lstRegistryValues.SelectedItems.Count > 0);
		}

		// Token: 0x060005DB RID: 1499 RVA: 0x0005470E File Offset: 0x0005290E
		private void menuStripExit_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		// Token: 0x060005DC RID: 1500 RVA: 0x00054716 File Offset: 0x00052916
		private void menuStripDelete_Click(object sender, EventArgs e)
		{
			if (this.tvRegistryDirectory.Focused)
			{
				this.deleteRegistryKey_Click(this, e);
				return;
			}
			if (!this.lstRegistryValues.Focused)
			{
				return;
			}
			this.deleteRegistryValue_Click(this, e);
		}

		// Token: 0x060005DD RID: 1501 RVA: 0x00054744 File Offset: 0x00052944
		private void menuStripRename_Click(object sender, EventArgs e)
		{
			if (this.tvRegistryDirectory.Focused)
			{
				this.renameRegistryKey_Click(this, e);
				return;
			}
			if (!this.lstRegistryValues.Focused)
			{
				return;
			}
			this.renameRegistryValue_Click(this, e);
		}

		// Token: 0x060005DE RID: 1502 RVA: 0x00054774 File Offset: 0x00052974
		private void lstRegistryKeys_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
			{
				return;
			}
			Point position = new Point(e.X, e.Y);
			if (this.lstRegistryValues.GetItemAt(position.X, position.Y) == null)
			{
				this.lst_ContextMenuStrip.Show(this.lstRegistryValues, position);
				return;
			}
			this.CreateListViewMenuStrip();
			this.selectedItem_ContextMenuStrip.Show(this.lstRegistryValues, position);
		}

		// Token: 0x060005DF RID: 1503 RVA: 0x000547E8 File Offset: 0x000529E8
		private void lstRegistryKeys_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			if (e.Label == null || this.tvRegistryDirectory.SelectedNode == null)
			{
				this.lstRegistryValues.LabelEdit = false;
				return;
			}
			e.CancelEdit = true;
			int item = e.Item;
			if (e.Label.Length <= 0)
			{
				MessageBox.Show("Invalid label. \nThe label cannot be blank.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				this.lstRegistryValues.Items[item].BeginEdit();
				return;
			}
			if (this.lstRegistryValues.Items.ContainsKey(e.Label))
			{
				MessageBox.Show("Invalid label. \nA node with that label already exists.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				this.lstRegistryValues.Items[item].BeginEdit();
				return;
			}
			this.client.Send(new object[]
			{
				"RenameRegistryValue",
				this.lstRegistryValues.Items[item].Name,
				e.Label,
				this.tvRegistryDirectory.SelectedNode.FullPath
			});
			this.lstRegistryValues.LabelEdit = false;
		}

		// Token: 0x060005E0 RID: 1504 RVA: 0x00054902 File Offset: 0x00052B02
		private void lstRegistryKeys_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Delete || !this.GetDeleteState())
			{
				return;
			}
			this.deleteRegistryValue_Click(this, e);
		}

		// Token: 0x060005E1 RID: 1505 RVA: 0x00054920 File Offset: 0x00052B20
		private void createNewRegistryKey_Click(object sender, EventArgs e)
		{
			if (!this.tvRegistryDirectory.SelectedNode.IsExpanded && this.tvRegistryDirectory.SelectedNode.Nodes.Count > 0)
			{
				this.tvRegistryDirectory.AfterExpand += this.createRegistryKey_AfterExpand;
				this.tvRegistryDirectory.SelectedNode.Expand();
				return;
			}
			this.client.Send(new object[]
			{
				"CreateRegistryKey",
				this.tvRegistryDirectory.SelectedNode.FullPath
			});
		}

		// Token: 0x060005E2 RID: 1506 RVA: 0x000549AC File Offset: 0x00052BAC
		private void deleteRegistryKey_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to permanently delete this key and all of its subkeys?", "Confirm Key Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
			{
				return;
			}
			string fullPath = this.tvRegistryDirectory.SelectedNode.Parent.FullPath;
			this.client.Send(new object[]
			{
				"DeleteRegistryKey",
				this.tvRegistryDirectory.SelectedNode.Name,
				fullPath
			});
		}

		// Token: 0x060005E3 RID: 1507 RVA: 0x00054A15 File Offset: 0x00052C15
		private void renameRegistryKey_Click(object sender, EventArgs e)
		{
			this.tvRegistryDirectory.LabelEdit = true;
			this.tvRegistryDirectory.SelectedNode.BeginEdit();
		}

		// Token: 0x060005E4 RID: 1508 RVA: 0x00054A34 File Offset: 0x00052C34
		private void createStringRegistryValue_Click(object sender, EventArgs e)
		{
			if (this.tvRegistryDirectory.SelectedNode == null)
			{
				return;
			}
			this.client.Send(new object[]
			{
				"CreateRegistryValue",
				this.tvRegistryDirectory.SelectedNode.FullPath,
				"1"
			});
		}

		// Token: 0x060005E5 RID: 1509 RVA: 0x00054A84 File Offset: 0x00052C84
		private void createBinaryRegistryValue_Click(object sender, EventArgs e)
		{
			if (this.tvRegistryDirectory.SelectedNode == null)
			{
				return;
			}
			this.client.Send(new object[]
			{
				"CreateRegistryValue",
				this.tvRegistryDirectory.SelectedNode.FullPath,
				"3"
			});
		}

		// Token: 0x060005E6 RID: 1510 RVA: 0x00054AD4 File Offset: 0x00052CD4
		private void createDwordRegistryValue_Click(object sender, EventArgs e)
		{
			if (this.tvRegistryDirectory.SelectedNode == null)
			{
				return;
			}
			this.client.Send(new object[]
			{
				"CreateRegistryValue",
				this.tvRegistryDirectory.SelectedNode.FullPath,
				"4"
			});
		}

		// Token: 0x060005E7 RID: 1511 RVA: 0x00054B24 File Offset: 0x00052D24
		private void createQwordRegistryValue_Click(object sender, EventArgs e)
		{
			if (this.tvRegistryDirectory.SelectedNode == null)
			{
				return;
			}
			this.client.Send(new object[]
			{
				"CreateRegistryValue",
				this.tvRegistryDirectory.SelectedNode.FullPath,
				"11"
			});
		}

		// Token: 0x060005E8 RID: 1512 RVA: 0x00054B74 File Offset: 0x00052D74
		private void createMultiStringRegistryValue_Click(object sender, EventArgs e)
		{
			if (this.tvRegistryDirectory.SelectedNode == null)
			{
				return;
			}
			this.client.Send(new object[]
			{
				"CreateRegistryValue",
				this.tvRegistryDirectory.SelectedNode.FullPath,
				"7"
			});
		}

		// Token: 0x060005E9 RID: 1513 RVA: 0x00054BC4 File Offset: 0x00052DC4
		private void createExpandStringRegistryValue_Click(object sender, EventArgs e)
		{
			if (this.tvRegistryDirectory.SelectedNode == null)
			{
				return;
			}
			this.client.Send(new object[]
			{
				"CreateRegistryValue",
				this.tvRegistryDirectory.SelectedNode.FullPath,
				"2"
			});
		}

		// Token: 0x060005EA RID: 1514 RVA: 0x00054C14 File Offset: 0x00052E14
		private void deleteRegistryValue_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Deleting certain registry values could cause system instability. Are you sure you want to permanently delete " + ((this.lstRegistryValues.SelectedItems.Count == 1) ? "this value?" : "these values?"), "Confirm Value Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
			{
				return;
			}
			foreach (object obj in this.lstRegistryValues.SelectedItems)
			{
				if (obj.GetType() == typeof(RegistryValueLstItem))
				{
					RegistryValueLstItem registryValueLstItem = (RegistryValueLstItem)obj;
					this.client.Send(new object[]
					{
						"DeleteRegistryValue",
						this.tvRegistryDirectory.SelectedNode.FullPath,
						registryValueLstItem.RegName
					});
				}
			}
		}

		// Token: 0x060005EB RID: 1515 RVA: 0x00054CF4 File Offset: 0x00052EF4
		private void renameRegistryValue_Click(object sender, EventArgs e)
		{
			this.lstRegistryValues.LabelEdit = true;
			this.lstRegistryValues.SelectedItems[0].BeginEdit();
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x00054D18 File Offset: 0x00052F18
		private void modifyRegistryValue_Click(object sender, EventArgs e)
		{
			this.CreateEditForm(false);
		}

		// Token: 0x060005ED RID: 1517 RVA: 0x00054D21 File Offset: 0x00052F21
		private void modifyBinaryDataRegistryValue_Click(object sender, EventArgs e)
		{
			this.CreateEditForm(true);
		}

		// Token: 0x060005EE RID: 1518 RVA: 0x00054D2A File Offset: 0x00052F2A
		private void createRegistryKey_AfterExpand(object sender, TreeViewEventArgs e)
		{
			if (e.Node != this.tvRegistryDirectory.SelectedNode)
			{
				return;
			}
			this.createNewRegistryKey_Click(this, e);
			this.tvRegistryDirectory.AfterExpand -= this.createRegistryKey_AfterExpand;
		}

		// Token: 0x060005EF RID: 1519 RVA: 0x00054D60 File Offset: 0x00052F60
		private bool GetDeleteState()
		{
			if (this.lstRegistryValues.Focused)
			{
				return this.lstRegistryValues.SelectedItems.Count > 0;
			}
			return this.tvRegistryDirectory.Focused && this.tvRegistryDirectory.SelectedNode != null && this.tvRegistryDirectory.SelectedNode.Parent != null;
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x00054DC0 File Offset: 0x00052FC0
		private bool GetRenameState()
		{
			if (!this.lstRegistryValues.Focused)
			{
				return this.tvRegistryDirectory.Focused && this.tvRegistryDirectory.SelectedNode != null && this.tvRegistryDirectory.SelectedNode.Parent != null;
			}
			return this.lstRegistryValues.SelectedItems.Count == 1 && !RegValueHelper.IsDefaultValue(this.lstRegistryValues.SelectedItems[0].Name);
		}

		// Token: 0x060005F1 RID: 1521 RVA: 0x00054E40 File Offset: 0x00053040
		private Form GetEditForm(RegistrySeeker.RegValueData value, RegistryValueKind valueKind)
		{
			switch (valueKind)
			{
			case RegistryValueKind.String:
			case RegistryValueKind.ExpandString:
				return new FormRegisterValueEditString(value);
			case RegistryValueKind.Binary:
				return new FormRegisterValueEditBinary(value);
			case RegistryValueKind.DWord:
			case RegistryValueKind.QWord:
				return new FormRegisterValueEditWord(value);
			case RegistryValueKind.MultiString:
				return new FormRegisterValueEditMultiString(value);
			}
			return null;
		}

		// Token: 0x060005F2 RID: 1522 RVA: 0x00054EA0 File Offset: 0x000530A0
		private void CreateEditForm(bool isBinary)
		{
			string fullPath = this.tvRegistryDirectory.SelectedNode.FullPath;
			string name = this.lstRegistryValues.SelectedItems[0].Name;
			RegistrySeeker.RegValueData regValueData = ((RegistrySeeker.RegValueData[])this.tvRegistryDirectory.SelectedNode.Tag).ToList<RegistrySeeker.RegValueData>().Find((RegistrySeeker.RegValueData item) => item.Name == name);
			RegistryValueKind valueKind = isBinary ? RegistryValueKind.Binary : regValueData.Kind;
			using (Form editForm = this.GetEditForm(regValueData, valueKind))
			{
				if (editForm.ShowDialog() == DialogResult.OK)
				{
					this.client.Send(new object[]
					{
						"ChangeRegistryValue"
					});
				}
			}
		}

		// Token: 0x04000496 RID: 1174
		public Clients client;

		// Token: 0x04000497 RID: 1175
		public Clients parrent;
	}
}
