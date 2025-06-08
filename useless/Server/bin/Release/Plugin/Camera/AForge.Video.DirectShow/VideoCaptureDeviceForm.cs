using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video.DirectShow.Properties;

namespace AForge.Video.DirectShow;

public class VideoCaptureDeviceForm : Form
{
	private IContainer components;

	private Button cancelButton;

	private Button okButton;

	private ComboBox devicesCombo;

	private GroupBox groupBox1;

	private PictureBox pictureBox;

	private Label label1;

	private Label snapshotsLabel;

	private ComboBox snapshotResolutionsCombo;

	private ComboBox videoResolutionsCombo;

	private Label label2;

	private ComboBox videoInputsCombo;

	private Label label3;

	private FilterInfoCollection videoDevices;

	private VideoCaptureDevice videoDevice;

	private Dictionary<string, VideoCapabilities> videoCapabilitiesDictionary = new Dictionary<string, VideoCapabilities>();

	private Dictionary<string, VideoCapabilities> snapshotCapabilitiesDictionary = new Dictionary<string, VideoCapabilities>();

	private VideoInput[] availableVideoInputs;

	private bool configureSnapshots;

	private string videoDeviceMoniker = string.Empty;

	private Size captureSize = new Size(0, 0);

	private Size snapshotSize = new Size(0, 0);

	private VideoInput videoInput = VideoInput.Default;

	public bool ConfigureSnapshots
	{
		get
		{
			return configureSnapshots;
		}
		set
		{
			configureSnapshots = value;
			((Control)snapshotsLabel).Visible = value;
			((Control)snapshotResolutionsCombo).Visible = value;
		}
	}

	public VideoCaptureDevice VideoDevice => videoDevice;

	public string VideoDeviceMoniker
	{
		get
		{
			return videoDeviceMoniker;
		}
		set
		{
			videoDeviceMoniker = value;
		}
	}

	public Size CaptureSize
	{
		get
		{
			return captureSize;
		}
		set
		{
			captureSize = value;
		}
	}

	public Size SnapshotSize
	{
		get
		{
			return snapshotSize;
		}
		set
		{
			snapshotSize = value;
		}
	}

	public VideoInput VideoInput
	{
		get
		{
			return videoInput;
		}
		set
		{
			videoInput = value;
		}
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
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		cancelButton = new Button();
		okButton = new Button();
		devicesCombo = new ComboBox();
		groupBox1 = new GroupBox();
		videoInputsCombo = new ComboBox();
		label3 = new Label();
		snapshotsLabel = new Label();
		snapshotResolutionsCombo = new ComboBox();
		videoResolutionsCombo = new ComboBox();
		label2 = new Label();
		label1 = new Label();
		pictureBox = new PictureBox();
		((Control)groupBox1).SuspendLayout();
		((ISupportInitialize)pictureBox).BeginInit();
		((Control)this).SuspendLayout();
		cancelButton.DialogResult = (DialogResult)2;
		((ButtonBase)cancelButton).FlatStyle = (FlatStyle)3;
		((Control)cancelButton).Location = new System.Drawing.Point(239, 190);
		((Control)cancelButton).Name = "cancelButton";
		((Control)cancelButton).Size = new Size(75, 23);
		((Control)cancelButton).TabIndex = 11;
		((Control)cancelButton).Text = "Cancel";
		okButton.DialogResult = (DialogResult)1;
		((ButtonBase)okButton).FlatStyle = (FlatStyle)3;
		((Control)okButton).Location = new System.Drawing.Point(149, 190);
		((Control)okButton).Name = "okButton";
		((Control)okButton).Size = new Size(75, 23);
		((Control)okButton).TabIndex = 10;
		((Control)okButton).Text = "OK";
		((Control)okButton).Click += okButton_Click;
		devicesCombo.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)devicesCombo).FormattingEnabled = true;
		((Control)devicesCombo).Location = new System.Drawing.Point(100, 40);
		((Control)devicesCombo).Name = "devicesCombo";
		((Control)devicesCombo).Size = new Size(325, 21);
		((Control)devicesCombo).TabIndex = 9;
		devicesCombo.SelectedIndexChanged += devicesCombo_SelectedIndexChanged;
		((Control)groupBox1).Controls.Add((Control)(object)videoInputsCombo);
		((Control)groupBox1).Controls.Add((Control)(object)label3);
		((Control)groupBox1).Controls.Add((Control)(object)snapshotsLabel);
		((Control)groupBox1).Controls.Add((Control)(object)snapshotResolutionsCombo);
		((Control)groupBox1).Controls.Add((Control)(object)videoResolutionsCombo);
		((Control)groupBox1).Controls.Add((Control)(object)label2);
		((Control)groupBox1).Controls.Add((Control)(object)label1);
		((Control)groupBox1).Controls.Add((Control)(object)pictureBox);
		((Control)groupBox1).Controls.Add((Control)(object)devicesCombo);
		((Control)groupBox1).Location = new System.Drawing.Point(10, 10);
		((Control)groupBox1).Name = "groupBox1";
		((Control)groupBox1).Size = new Size(440, 165);
		((Control)groupBox1).TabIndex = 12;
		groupBox1.TabStop = false;
		((Control)groupBox1).Text = "Video capture device settings";
		videoInputsCombo.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)videoInputsCombo).FormattingEnabled = true;
		((Control)videoInputsCombo).Location = new System.Drawing.Point(100, 130);
		((Control)videoInputsCombo).Name = "videoInputsCombo";
		((Control)videoInputsCombo).Size = new Size(150, 21);
		((Control)videoInputsCombo).TabIndex = 17;
		((Control)label3).AutoSize = true;
		((Control)label3).Location = new System.Drawing.Point(100, 115);
		((Control)label3).Name = "label3";
		((Control)label3).Size = new Size(63, 13);
		((Control)label3).TabIndex = 16;
		((Control)label3).Text = "Video input:";
		((Control)snapshotsLabel).AutoSize = true;
		((Control)snapshotsLabel).Location = new System.Drawing.Point(275, 70);
		((Control)snapshotsLabel).Name = "snapshotsLabel";
		((Control)snapshotsLabel).Size = new Size(101, 13);
		((Control)snapshotsLabel).TabIndex = 15;
		((Control)snapshotsLabel).Text = "Snapshot resoluton:";
		snapshotResolutionsCombo.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)snapshotResolutionsCombo).FormattingEnabled = true;
		((Control)snapshotResolutionsCombo).Location = new System.Drawing.Point(275, 85);
		((Control)snapshotResolutionsCombo).Name = "snapshotResolutionsCombo";
		((Control)snapshotResolutionsCombo).Size = new Size(150, 21);
		((Control)snapshotResolutionsCombo).TabIndex = 14;
		videoResolutionsCombo.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)videoResolutionsCombo).FormattingEnabled = true;
		((Control)videoResolutionsCombo).Location = new System.Drawing.Point(100, 85);
		((Control)videoResolutionsCombo).Name = "videoResolutionsCombo";
		((Control)videoResolutionsCombo).Size = new Size(150, 21);
		((Control)videoResolutionsCombo).TabIndex = 13;
		((Control)label2).AutoSize = true;
		((Control)label2).Location = new System.Drawing.Point(100, 70);
		((Control)label2).Name = "label2";
		((Control)label2).Size = new Size(83, 13);
		((Control)label2).TabIndex = 12;
		((Control)label2).Text = "Video resoluton:";
		((Control)label1).AutoSize = true;
		((Control)label1).Location = new System.Drawing.Point(100, 25);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(72, 13);
		((Control)label1).TabIndex = 11;
		((Control)label1).Text = "Video device:";
		pictureBox.Image = (Image)(object)Resources.camera;
		((Control)pictureBox).Location = new System.Drawing.Point(20, 28);
		((Control)pictureBox).Name = "pictureBox";
		((Control)pictureBox).Size = new Size(64, 64);
		pictureBox.TabIndex = 10;
		pictureBox.TabStop = false;
		((Form)this).AcceptButton = (IButtonControl)(object)okButton;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).CancelButton = (IButtonControl)(object)cancelButton;
		((Form)this).ClientSize = new Size(462, 221);
		((Control)this).Controls.Add((Control)(object)groupBox1);
		((Control)this).Controls.Add((Control)(object)cancelButton);
		((Control)this).Controls.Add((Control)(object)okButton);
		((Form)this).FormBorderStyle = (FormBorderStyle)5;
		((Control)this).Name = "VideoCaptureDeviceForm";
		((Form)this).StartPosition = (FormStartPosition)4;
		((Control)this).Text = "Open local  video capture device";
		((Form)this).Load += VideoCaptureDeviceForm_Load;
		((Control)groupBox1).ResumeLayout(false);
		((Control)groupBox1).PerformLayout();
		((ISupportInitialize)pictureBox).EndInit();
		((Control)this).ResumeLayout(false);
	}

	public VideoCaptureDeviceForm()
	{
		InitializeComponent();
		ConfigureSnapshots = false;
		try
		{
			videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			if (videoDevices.Count == 0)
			{
				throw new ApplicationException();
			}
			foreach (FilterInfo videoDevice in videoDevices)
			{
				devicesCombo.Items.Add((object)videoDevice.Name);
			}
		}
		catch (ApplicationException)
		{
			devicesCombo.Items.Add((object)"No local capture devices");
			((Control)devicesCombo).Enabled = false;
			((Control)okButton).Enabled = false;
		}
	}

	private void VideoCaptureDeviceForm_Load(object sender, EventArgs e)
	{
		int selectedIndex = 0;
		for (int i = 0; i < videoDevices.Count; i++)
		{
			if (videoDeviceMoniker == videoDevices[i].MonikerString)
			{
				selectedIndex = i;
				break;
			}
		}
		((ListControl)devicesCombo).SelectedIndex = selectedIndex;
	}

	private void okButton_Click(object sender, EventArgs e)
	{
		videoDeviceMoniker = videoDevice.Source;
		if (videoCapabilitiesDictionary.Count != 0)
		{
			VideoCapabilities videoCapabilities = videoCapabilitiesDictionary[(string)videoResolutionsCombo.SelectedItem];
			videoDevice.VideoResolution = videoCapabilities;
			captureSize = videoCapabilities.FrameSize;
		}
		if (configureSnapshots && snapshotCapabilitiesDictionary.Count != 0)
		{
			VideoCapabilities videoCapabilities2 = snapshotCapabilitiesDictionary[(string)snapshotResolutionsCombo.SelectedItem];
			videoDevice.ProvideSnapshots = true;
			videoDevice.SnapshotResolution = videoCapabilities2;
			snapshotSize = videoCapabilities2.FrameSize;
		}
		if (availableVideoInputs.Length != 0)
		{
			videoInput = availableVideoInputs[((ListControl)videoInputsCombo).SelectedIndex];
			videoDevice.CrossbarVideoInput = videoInput;
		}
	}

	private void devicesCombo_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (videoDevices.Count != 0)
		{
			videoDevice = new VideoCaptureDevice(videoDevices[((ListControl)devicesCombo).SelectedIndex].MonikerString);
			EnumeratedSupportedFrameSizes(videoDevice);
		}
	}

	private void EnumeratedSupportedFrameSizes(VideoCaptureDevice videoDevice)
	{
		((Control)this).Cursor = Cursors.WaitCursor;
		videoResolutionsCombo.Items.Clear();
		snapshotResolutionsCombo.Items.Clear();
		videoInputsCombo.Items.Clear();
		videoCapabilitiesDictionary.Clear();
		snapshotCapabilitiesDictionary.Clear();
		try
		{
			VideoCapabilities[] videoCapabilities = videoDevice.VideoCapabilities;
			int selectedIndex = 0;
			VideoCapabilities[] array = videoCapabilities;
			foreach (VideoCapabilities videoCapabilities2 in array)
			{
				Size frameSize = videoCapabilities2.FrameSize;
				object arg = frameSize.Width;
				Size frameSize2 = videoCapabilities2.FrameSize;
				string text = $"{arg} x {frameSize2.Height}";
				if (!videoResolutionsCombo.Items.Contains((object)text))
				{
					if (captureSize == videoCapabilities2.FrameSize)
					{
						selectedIndex = videoResolutionsCombo.Items.Count;
					}
					videoResolutionsCombo.Items.Add((object)text);
				}
				if (!videoCapabilitiesDictionary.ContainsKey(text))
				{
					videoCapabilitiesDictionary.Add(text, videoCapabilities2);
				}
			}
			if (videoCapabilities.Length == 0)
			{
				videoResolutionsCombo.Items.Add((object)"Not supported");
			}
			((ListControl)videoResolutionsCombo).SelectedIndex = selectedIndex;
			if (configureSnapshots)
			{
				VideoCapabilities[] snapshotCapabilities = videoDevice.SnapshotCapabilities;
				int selectedIndex2 = 0;
				VideoCapabilities[] array2 = snapshotCapabilities;
				foreach (VideoCapabilities videoCapabilities3 in array2)
				{
					Size frameSize3 = videoCapabilities3.FrameSize;
					object arg2 = frameSize3.Width;
					Size frameSize4 = videoCapabilities3.FrameSize;
					string text2 = $"{arg2} x {frameSize4.Height}";
					if (!snapshotResolutionsCombo.Items.Contains((object)text2))
					{
						if (snapshotSize == videoCapabilities3.FrameSize)
						{
							selectedIndex2 = snapshotResolutionsCombo.Items.Count;
						}
						snapshotResolutionsCombo.Items.Add((object)text2);
						snapshotCapabilitiesDictionary.Add(text2, videoCapabilities3);
					}
				}
				if (snapshotCapabilities.Length == 0)
				{
					snapshotResolutionsCombo.Items.Add((object)"Not supported");
				}
				((ListControl)snapshotResolutionsCombo).SelectedIndex = selectedIndex2;
			}
			availableVideoInputs = videoDevice.AvailableCrossbarVideoInputs;
			int selectedIndex3 = 0;
			VideoInput[] array3 = availableVideoInputs;
			foreach (VideoInput videoInput in array3)
			{
				string text3 = $"{videoInput.Index}: {videoInput.Type}";
				if (videoInput.Index == this.videoInput.Index && videoInput.Type == this.videoInput.Type)
				{
					selectedIndex3 = videoInputsCombo.Items.Count;
				}
				videoInputsCombo.Items.Add((object)text3);
			}
			if (availableVideoInputs.Length == 0)
			{
				videoInputsCombo.Items.Add((object)"Not supported");
			}
			((ListControl)videoInputsCombo).SelectedIndex = selectedIndex3;
		}
		finally
		{
			((Control)this).Cursor = Cursors.Default;
		}
	}
}
