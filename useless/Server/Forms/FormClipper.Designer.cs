namespace Server.Forms
{
	// Token: 0x0200009D RID: 157
	public partial class FormClipper : global::Server.Helper.FormMaterial
	{
		// Token: 0x06000449 RID: 1097 RVA: 0x00034C2C File Offset: 0x00032E2C
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x00034C4C File Offset: 0x00032E4C
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new global::System.Windows.Forms.DataGridViewCellStyle();
			global::System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new global::System.Windows.Forms.DataGridViewCellStyle();
			this.GridClients = new global::System.Windows.Forms.DataGridView();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.materialSwitch7 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.materialSwitch2 = new global::MaterialSkin.Controls.MaterialSwitch();
			this.rjButton2 = new global::CustomControls.RJControls.RJButton();
			this.rjTextBox10 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox9 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox7 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox8 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox5 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox6 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox3 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox4 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox1 = new global::CustomControls.RJControls.RJTextBox();
			this.rjTextBox2 = new global::CustomControls.RJControls.RJTextBox();
			this.GridLog = new global::System.Windows.Forms.DataGridView();
			this.ColumnClient = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnType = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnAddress = new global::System.Windows.Forms.DataGridViewTextBoxColumn();
			this.timer1 = new global::System.Windows.Forms.Timer(this.components);
			((global::System.ComponentModel.ISupportInitialize)this.GridClients).BeginInit();
			this.panel1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.GridLog).BeginInit();
			base.SuspendLayout();
			this.GridClients.AllowUserToAddRows = false;
			this.GridClients.AllowUserToDeleteRows = false;
			this.GridClients.AllowUserToResizeColumns = false;
			this.GridClients.AllowUserToResizeRows = false;
			this.GridClients.BackgroundColor = global::System.Drawing.Color.White;
			this.GridClients.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.GridClients.CellBorderStyle = global::System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.GridClients.ColumnHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle.SelectionBackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle.SelectionForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.GridClients.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
			this.GridClients.ColumnHeadersHeightSizeMode = global::System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.GridClients.Cursor = global::System.Windows.Forms.Cursors.Default;
			dataGridViewCellStyle2.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle2.Font = new global::System.Drawing.Font("Arial", 9f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			dataGridViewCellStyle2.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionBackColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle2.SelectionForeColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle2.WrapMode = global::System.Windows.Forms.DataGridViewTriState.False;
			this.GridClients.DefaultCellStyle = dataGridViewCellStyle2;
			this.GridClients.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.GridClients.EnableHeadersVisualStyles = false;
			this.GridClients.GridColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			this.GridClients.Location = new global::System.Drawing.Point(3, 230);
			this.GridClients.Name = "GridClients";
			this.GridClients.ReadOnly = true;
			this.GridClients.RowHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle3.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			dataGridViewCellStyle3.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.Padding = new global::System.Windows.Forms.Padding(1, 0, 0, 0);
			dataGridViewCellStyle3.SelectionBackColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle3.SelectionForeColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle3.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.GridClients.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.GridClients.RowHeadersVisible = false;
			this.GridClients.SelectionMode = global::System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.GridClients.ShowCellErrors = false;
			this.GridClients.ShowCellToolTips = false;
			this.GridClients.ShowEditingIcon = false;
			this.GridClients.ShowRowErrors = false;
			this.GridClients.Size = new global::System.Drawing.Size(801, 359);
			this.GridClients.TabIndex = 12;
			this.panel1.Controls.Add(this.materialSwitch7);
			this.panel1.Controls.Add(this.materialSwitch2);
			this.panel1.Controls.Add(this.rjButton2);
			this.panel1.Controls.Add(this.rjTextBox10);
			this.panel1.Controls.Add(this.rjTextBox9);
			this.panel1.Controls.Add(this.rjTextBox7);
			this.panel1.Controls.Add(this.rjTextBox8);
			this.panel1.Controls.Add(this.rjTextBox5);
			this.panel1.Controls.Add(this.rjTextBox6);
			this.panel1.Controls.Add(this.rjTextBox3);
			this.panel1.Controls.Add(this.rjTextBox4);
			this.panel1.Controls.Add(this.rjTextBox1);
			this.panel1.Controls.Add(this.rjTextBox2);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new global::System.Drawing.Point(3, 64);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(801, 166);
			this.panel1.TabIndex = 11;
			this.materialSwitch7.AutoSize = true;
			this.materialSwitch7.Depth = 0;
			this.materialSwitch7.Location = new global::System.Drawing.Point(271, 117);
			this.materialSwitch7.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch7.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch7.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch7.Name = "materialSwitch7";
			this.materialSwitch7.Ripple = true;
			this.materialSwitch7.Size = new global::System.Drawing.Size(92, 37);
			this.materialSwitch7.TabIndex = 61;
			this.materialSwitch7.Text = "Start";
			this.materialSwitch7.UseVisualStyleBackColor = true;
			this.materialSwitch7.CheckedChanged += new global::System.EventHandler(this.materialSwitch7_CheckedChanged);
			this.materialSwitch2.AutoSize = true;
			this.materialSwitch2.Depth = 0;
			this.materialSwitch2.Location = new global::System.Drawing.Point(388, 117);
			this.materialSwitch2.Margin = new global::System.Windows.Forms.Padding(0);
			this.materialSwitch2.MouseLocation = new global::System.Drawing.Point(-1, -1);
			this.materialSwitch2.MouseState = global::MaterialSkin.MouseState.HOVER;
			this.materialSwitch2.Name = "materialSwitch2";
			this.materialSwitch2.Ripple = true;
			this.materialSwitch2.Size = new global::System.Drawing.Size(129, 37);
			this.materialSwitch2.TabIndex = 60;
			this.materialSwitch2.Text = "Auto Start";
			this.materialSwitch2.UseVisualStyleBackColor = true;
			this.materialSwitch2.CheckedChanged += new global::System.EventHandler(this.materialSwitch2_CheckedChanged);
			this.rjButton2.BackColor = global::System.Drawing.Color.White;
			this.rjButton2.BackgroundColor = global::System.Drawing.Color.White;
			this.rjButton2.BorderColor = global::System.Drawing.Color.MediumPurple;
			this.rjButton2.BorderRadius = 0;
			this.rjButton2.BorderSize = 1;
			this.rjButton2.FlatAppearance.BorderSize = 0;
			this.rjButton2.FlatStyle = global::System.Windows.Forms.FlatStyle.Flat;
			this.rjButton2.ForeColor = global::System.Drawing.Color.MediumPurple;
			this.rjButton2.Location = new global::System.Drawing.Point(679, 121);
			this.rjButton2.Name = "rjButton2";
			this.rjButton2.Size = new global::System.Drawing.Size(92, 27);
			this.rjButton2.TabIndex = 59;
			this.rjButton2.Text = "Hide";
			this.rjButton2.TextColor = global::System.Drawing.Color.MediumPurple;
			this.rjButton2.UseVisualStyleBackColor = false;
			this.rjButton2.Click += new global::System.EventHandler(this.rjButton2_Click);
			this.rjTextBox10.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox10.BorderColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox10.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox10.BorderRadius = 0;
			this.rjTextBox10.BorderSize = 1;
			this.rjTextBox10.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox10.ForeColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox10.Location = new global::System.Drawing.Point(17, 121);
			this.rjTextBox10.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox10.Multiline = false;
			this.rjTextBox10.Name = "rjTextBox10";
			this.rjTextBox10.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox10.PasswordChar = false;
			this.rjTextBox10.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox10.PlaceholderText = "tron";
			this.rjTextBox10.Size = new global::System.Drawing.Size(246, 28);
			this.rjTextBox10.TabIndex = 57;
			this.rjTextBox10.Texts = "";
			this.rjTextBox10.UnderlinedStyle = false;
			this.rjTextBox9.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox9.BorderColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox9.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox9.BorderRadius = 0;
			this.rjTextBox9.BorderSize = 1;
			this.rjTextBox9.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox9.ForeColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox9.Location = new global::System.Drawing.Point(525, 85);
			this.rjTextBox9.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox9.Multiline = false;
			this.rjTextBox9.Name = "rjTextBox9";
			this.rjTextBox9.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox9.PasswordChar = false;
			this.rjTextBox9.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox9.PlaceholderText = "dash";
			this.rjTextBox9.Size = new global::System.Drawing.Size(246, 28);
			this.rjTextBox9.TabIndex = 56;
			this.rjTextBox9.Texts = "";
			this.rjTextBox9.UnderlinedStyle = false;
			this.rjTextBox7.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox7.BorderColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox7.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox7.BorderRadius = 0;
			this.rjTextBox7.BorderSize = 1;
			this.rjTextBox7.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox7.ForeColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox7.Location = new global::System.Drawing.Point(525, 49);
			this.rjTextBox7.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox7.Multiline = false;
			this.rjTextBox7.Name = "rjTextBox7";
			this.rjTextBox7.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox7.PasswordChar = false;
			this.rjTextBox7.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox7.PlaceholderText = "doge";
			this.rjTextBox7.Size = new global::System.Drawing.Size(246, 28);
			this.rjTextBox7.TabIndex = 55;
			this.rjTextBox7.Texts = "";
			this.rjTextBox7.UnderlinedStyle = false;
			this.rjTextBox8.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox8.BorderColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox8.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox8.BorderRadius = 0;
			this.rjTextBox8.BorderSize = 1;
			this.rjTextBox8.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox8.ForeColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox8.Location = new global::System.Drawing.Point(525, 13);
			this.rjTextBox8.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox8.Multiline = false;
			this.rjTextBox8.Name = "rjTextBox8";
			this.rjTextBox8.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox8.PasswordChar = false;
			this.rjTextBox8.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox8.PlaceholderText = "zcash";
			this.rjTextBox8.Size = new global::System.Drawing.Size(246, 28);
			this.rjTextBox8.TabIndex = 54;
			this.rjTextBox8.Texts = "";
			this.rjTextBox8.UnderlinedStyle = false;
			this.rjTextBox5.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox5.BorderColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox5.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox5.BorderRadius = 0;
			this.rjTextBox5.BorderSize = 1;
			this.rjTextBox5.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox5.ForeColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox5.Location = new global::System.Drawing.Point(271, 85);
			this.rjTextBox5.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox5.Multiline = false;
			this.rjTextBox5.Name = "rjTextBox5";
			this.rjTextBox5.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox5.PasswordChar = false;
			this.rjTextBox5.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox5.PlaceholderText = "monero";
			this.rjTextBox5.Size = new global::System.Drawing.Size(246, 28);
			this.rjTextBox5.TabIndex = 53;
			this.rjTextBox5.Texts = "";
			this.rjTextBox5.UnderlinedStyle = false;
			this.rjTextBox6.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox6.BorderColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox6.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox6.BorderRadius = 0;
			this.rjTextBox6.BorderSize = 1;
			this.rjTextBox6.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox6.ForeColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox6.Location = new global::System.Drawing.Point(17, 85);
			this.rjTextBox6.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox6.Multiline = false;
			this.rjTextBox6.Name = "rjTextBox6";
			this.rjTextBox6.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox6.PasswordChar = false;
			this.rjTextBox6.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox6.PlaceholderText = "Bitcoin Cash";
			this.rjTextBox6.Size = new global::System.Drawing.Size(246, 28);
			this.rjTextBox6.TabIndex = 52;
			this.rjTextBox6.Texts = "";
			this.rjTextBox6.UnderlinedStyle = false;
			this.rjTextBox3.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox3.BorderColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox3.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox3.BorderRadius = 0;
			this.rjTextBox3.BorderSize = 1;
			this.rjTextBox3.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox3.ForeColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox3.Location = new global::System.Drawing.Point(271, 49);
			this.rjTextBox3.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox3.Multiline = false;
			this.rjTextBox3.Name = "rjTextBox3";
			this.rjTextBox3.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox3.PasswordChar = false;
			this.rjTextBox3.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox3.PlaceholderText = "Litecoin";
			this.rjTextBox3.Size = new global::System.Drawing.Size(246, 28);
			this.rjTextBox3.TabIndex = 51;
			this.rjTextBox3.Texts = "";
			this.rjTextBox3.UnderlinedStyle = false;
			this.rjTextBox4.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox4.BorderColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox4.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox4.BorderRadius = 0;
			this.rjTextBox4.BorderSize = 1;
			this.rjTextBox4.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox4.ForeColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox4.Location = new global::System.Drawing.Point(17, 49);
			this.rjTextBox4.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox4.Multiline = false;
			this.rjTextBox4.Name = "rjTextBox4";
			this.rjTextBox4.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox4.PasswordChar = false;
			this.rjTextBox4.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox4.PlaceholderText = "Stellar";
			this.rjTextBox4.Size = new global::System.Drawing.Size(246, 28);
			this.rjTextBox4.TabIndex = 50;
			this.rjTextBox4.Texts = "";
			this.rjTextBox4.UnderlinedStyle = false;
			this.rjTextBox1.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox1.BorderColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox1.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox1.BorderRadius = 0;
			this.rjTextBox1.BorderSize = 1;
			this.rjTextBox1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox1.ForeColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox1.Location = new global::System.Drawing.Point(271, 13);
			this.rjTextBox1.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox1.Multiline = false;
			this.rjTextBox1.Name = "rjTextBox1";
			this.rjTextBox1.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox1.PasswordChar = false;
			this.rjTextBox1.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox1.PlaceholderText = "Eth";
			this.rjTextBox1.Size = new global::System.Drawing.Size(246, 28);
			this.rjTextBox1.TabIndex = 49;
			this.rjTextBox1.Texts = "";
			this.rjTextBox1.UnderlinedStyle = false;
			this.rjTextBox2.BackColor = global::System.Drawing.Color.White;
			this.rjTextBox2.BorderColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox2.BorderFocusColor = global::System.Drawing.Color.Yellow;
			this.rjTextBox2.BorderRadius = 0;
			this.rjTextBox2.BorderSize = 1;
			this.rjTextBox2.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.rjTextBox2.ForeColor = global::System.Drawing.Color.MediumPurple;
			this.rjTextBox2.Location = new global::System.Drawing.Point(17, 13);
			this.rjTextBox2.Margin = new global::System.Windows.Forms.Padding(4);
			this.rjTextBox2.Multiline = false;
			this.rjTextBox2.Name = "rjTextBox2";
			this.rjTextBox2.Padding = new global::System.Windows.Forms.Padding(10, 7, 10, 7);
			this.rjTextBox2.PasswordChar = false;
			this.rjTextBox2.PlaceholderColor = global::System.Drawing.Color.DarkGray;
			this.rjTextBox2.PlaceholderText = "Btc";
			this.rjTextBox2.Size = new global::System.Drawing.Size(246, 28);
			this.rjTextBox2.TabIndex = 48;
			this.rjTextBox2.Texts = "";
			this.rjTextBox2.UnderlinedStyle = false;
			this.GridLog.AllowUserToAddRows = false;
			this.GridLog.AllowUserToDeleteRows = false;
			this.GridLog.AllowUserToResizeColumns = false;
			this.GridLog.AllowUserToResizeRows = false;
			this.GridLog.BackgroundColor = global::System.Drawing.Color.White;
			this.GridLog.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.GridLog.CellBorderStyle = global::System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.GridLog.ColumnHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle4.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle4.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle4.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle4.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle4.SelectionBackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle4.SelectionForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle4.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.GridLog.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
			this.GridLog.ColumnHeadersHeightSizeMode = global::System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.GridLog.Columns.AddRange(new global::System.Windows.Forms.DataGridViewColumn[]
			{
				this.ColumnClient,
				this.ColumnType,
				this.ColumnAddress
			});
			this.GridLog.Cursor = global::System.Windows.Forms.Cursors.Default;
			dataGridViewCellStyle5.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle5.BackColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle5.Font = new global::System.Drawing.Font("Arial", 9f);
			dataGridViewCellStyle5.ForeColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle5.SelectionBackColor = global::System.Drawing.Color.Purple;
			dataGridViewCellStyle5.SelectionForeColor = global::System.Drawing.Color.White;
			dataGridViewCellStyle5.WrapMode = global::System.Windows.Forms.DataGridViewTriState.False;
			this.GridLog.DefaultCellStyle = dataGridViewCellStyle5;
			this.GridLog.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.GridLog.EnableHeadersVisualStyles = false;
			this.GridLog.GridColor = global::System.Drawing.Color.MediumPurple;
			this.GridLog.Location = new global::System.Drawing.Point(3, 230);
			this.GridLog.Name = "GridLog";
			this.GridLog.ReadOnly = true;
			this.GridLog.RowHeadersBorderStyle = global::System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle6.Alignment = global::System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle6.BackColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle6.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			dataGridViewCellStyle6.ForeColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle6.Padding = new global::System.Windows.Forms.Padding(1, 0, 0, 0);
			dataGridViewCellStyle6.SelectionBackColor = global::System.Drawing.Color.FromArgb(0, 192, 0);
			dataGridViewCellStyle6.SelectionForeColor = global::System.Drawing.Color.FromArgb(17, 17, 17);
			dataGridViewCellStyle6.WrapMode = global::System.Windows.Forms.DataGridViewTriState.True;
			this.GridLog.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
			this.GridLog.RowHeadersVisible = false;
			this.GridLog.SelectionMode = global::System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.GridLog.ShowCellErrors = false;
			this.GridLog.ShowCellToolTips = false;
			this.GridLog.ShowEditingIcon = false;
			this.GridLog.ShowRowErrors = false;
			this.GridLog.Size = new global::System.Drawing.Size(801, 359);
			this.GridLog.TabIndex = 34;
			this.ColumnClient.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColumnClient.HeaderText = "Client";
			this.ColumnClient.MinimumWidth = 150;
			this.ColumnClient.Name = "ColumnClient";
			this.ColumnClient.ReadOnly = true;
			this.ColumnClient.Width = 150;
			this.ColumnType.HeaderText = "Type";
			this.ColumnType.Name = "ColumnType";
			this.ColumnType.ReadOnly = true;
			this.ColumnAddress.AutoSizeMode = global::System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColumnAddress.HeaderText = "Address";
			this.ColumnAddress.Name = "ColumnAddress";
			this.ColumnAddress.ReadOnly = true;
			this.timer1.Interval = 1000;
			this.timer1.Tick += new global::System.EventHandler(this.timer1_Tick);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.Color.White;
			base.ClientSize = new global::System.Drawing.Size(807, 592);
			base.Controls.Add(this.GridLog);
			base.Controls.Add(this.GridClients);
			base.Controls.Add(this.panel1);
			base.Name = "FormClipper";
			this.Text = "Clipper           Online [0]";
			base.Load += new global::System.EventHandler(this.FormProcess_Load);
			((global::System.ComponentModel.ISupportInitialize)this.GridClients).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.GridLog).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x04000297 RID: 663
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000298 RID: 664
		public global::System.Windows.Forms.DataGridView GridClients;

		// Token: 0x04000299 RID: 665
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x0400029A RID: 666
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch2;

		// Token: 0x0400029B RID: 667
		private global::CustomControls.RJControls.RJButton rjButton2;

		// Token: 0x0400029C RID: 668
		public global::CustomControls.RJControls.RJTextBox rjTextBox10;

		// Token: 0x0400029D RID: 669
		public global::CustomControls.RJControls.RJTextBox rjTextBox9;

		// Token: 0x0400029E RID: 670
		public global::CustomControls.RJControls.RJTextBox rjTextBox7;

		// Token: 0x0400029F RID: 671
		public global::CustomControls.RJControls.RJTextBox rjTextBox8;

		// Token: 0x040002A0 RID: 672
		public global::CustomControls.RJControls.RJTextBox rjTextBox5;

		// Token: 0x040002A1 RID: 673
		public global::CustomControls.RJControls.RJTextBox rjTextBox6;

		// Token: 0x040002A2 RID: 674
		public global::CustomControls.RJControls.RJTextBox rjTextBox3;

		// Token: 0x040002A3 RID: 675
		public global::CustomControls.RJControls.RJTextBox rjTextBox4;

		// Token: 0x040002A4 RID: 676
		public global::CustomControls.RJControls.RJTextBox rjTextBox1;

		// Token: 0x040002A5 RID: 677
		public global::CustomControls.RJControls.RJTextBox rjTextBox2;

		// Token: 0x040002A6 RID: 678
		public global::System.Windows.Forms.DataGridView GridLog;

		// Token: 0x040002A7 RID: 679
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnClient;

		// Token: 0x040002A8 RID: 680
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnType;

		// Token: 0x040002A9 RID: 681
		private global::System.Windows.Forms.DataGridViewTextBoxColumn ColumnAddress;

		// Token: 0x040002AA RID: 682
		private global::System.Windows.Forms.Timer timer1;

		// Token: 0x040002AB RID: 683
		public global::MaterialSkin.Controls.MaterialSwitch materialSwitch7;
	}
}
