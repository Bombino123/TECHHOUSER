namespace Server.Forms
{
	// Token: 0x02000097 RID: 151
	public partial class FormMap : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000417 RID: 1047 RVA: 0x000323AB File Offset: 0x000305AB
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x000323CC File Offset: 0x000305CC
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.gMapControl1 = new global::GMap.NET.WindowsForms.GMapControl();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			base.SuspendLayout();
			this.gMapControl1.Bearing = 0f;
			this.gMapControl1.CanDragMap = true;
			this.gMapControl1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.gMapControl1.EmptyTileColor = global::System.Drawing.Color.Navy;
			this.gMapControl1.Enabled = false;
			this.gMapControl1.GrayScaleMode = false;
			this.gMapControl1.HelperLineOption = global::GMap.NET.WindowsForms.HelperLineOptions.DontShow;
			this.gMapControl1.LevelsKeepInMemory = 5;
			this.gMapControl1.Location = new global::System.Drawing.Point(3, 64);
			this.gMapControl1.MarkersEnabled = true;
			this.gMapControl1.MaxZoom = 15;
			this.gMapControl1.MinZoom = 5;
			this.gMapControl1.MouseWheelZoomEnabled = true;
			this.gMapControl1.MouseWheelZoomType = global::GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
			this.gMapControl1.Name = "gMapControl1";
			this.gMapControl1.NegativeMode = false;
			this.gMapControl1.PolygonsEnabled = true;
			this.gMapControl1.RetryLoadTile = 0;
			this.gMapControl1.RoutesEnabled = true;
			this.gMapControl1.ScaleMode = global::GMap.NET.WindowsForms.ScaleModes.Integer;
			this.gMapControl1.SelectedAreaFillColor = global::System.Drawing.Color.FromArgb(33, 65, 105, 225);
			this.gMapControl1.ShowTileGridLines = false;
			this.gMapControl1.Size = new global::System.Drawing.Size(794, 383);
			this.gMapControl1.TabIndex = 0;
			this.gMapControl1.Zoom = 12.0;
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(800, 450);
			base.Controls.Add(this.gMapControl1);
			base.Name = "FormMap";
			this.Text = "Map";
			base.Load += new global::System.EventHandler(this.FormMap_Load);
			base.ResumeLayout(false);
		}

		// Token: 0x04000265 RID: 613
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000266 RID: 614
		public global::System.Windows.Forms.Timer timer1;

		// Token: 0x04000267 RID: 615
		public global::GMap.NET.WindowsForms.GMapControl gMapControl1;
	}
}
