namespace Server.Helper
{
	// Token: 0x02000070 RID: 112
	public partial class FormMaterial : global::MaterialSkin.Controls.MaterialForm
	{
		// Token: 0x0600025F RID: 607 RVA: 0x00023604 File Offset: 0x00021804
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000260 RID: 608 RVA: 0x00023624 File Offset: 0x00021824
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Server.Helper.FormMaterial));
			base.SuspendLayout();
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(800, 450);
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "FormMaterial";
			this.Text = "FormMaterial";
			base.ResumeLayout(false);
		}

		// Token: 0x04000174 RID: 372
		private global::System.ComponentModel.IContainer components;
	}
}
