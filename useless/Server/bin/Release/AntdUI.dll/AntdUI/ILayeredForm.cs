using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace AntdUI;

public abstract class ILayeredForm : Form, IMessageFilter
{
	private class RenderQueue : IDisposable
	{
		public class M
		{
			public byte alpha { get; private set; }

			public Bitmap bmp { get; private set; }

			public Rectangle? rect { get; private set; }

			public M(byte a, Bitmap b)
			{
				bmp = b;
				alpha = a;
			}

			public M(byte a, Bitmap b, Rectangle r)
			{
				bmp = b;
				alpha = a;
				rect = r;
			}
		}

		private ILayeredForm call;

		private ConcurrentQueue<M?> Queue = new ConcurrentQueue<M>();

		private ManualResetEvent Event = new ManualResetEvent(initialState: false);

		private bool isDispose;

		public RenderQueue(ILayeredForm it)
		{
			call = it;
			Thread thread = new Thread(LongTask);
			thread.IsBackground = true;
			thread.Start();
		}

		public void Set()
		{
			if (!isDispose)
			{
				Queue.Enqueue(null);
				Event.SetWait();
			}
		}

		public void Set(byte alpha, Bitmap bmp)
		{
			if (!isDispose)
			{
				Queue.Enqueue(new M(alpha, bmp));
				Event.SetWait();
			}
		}

		public void Set(byte alpha, Bitmap bmp, Rectangle rect)
		{
			if (!isDispose)
			{
				Queue.Enqueue(new M(alpha, bmp, rect));
				Event.SetWait();
			}
		}

		private void LongTask()
		{
			while (!Event.Wait())
			{
				int num = 0;
				M result;
				while (Queue.TryDequeue(out result))
				{
					if (!call.CanRender(out var han))
					{
						continue;
					}
					if (result == null)
					{
						num++;
						if (num > 2)
						{
							num = 0;
							call.Render(han);
						}
					}
					else if (result.rect.HasValue)
					{
						Bitmap bmp = result.bmp;
						try
						{
							call.Render(han, result.alpha, result.bmp, result.rect.Value);
						}
						finally
						{
							((IDisposable)bmp)?.Dispose();
						}
					}
					else
					{
						call.Render(han, result.alpha, result.bmp);
					}
				}
				if (num > 0 && call.CanRender(out var han2))
				{
					call.Render(han2);
				}
				if (isDispose || Event.ResetWait())
				{
					break;
				}
			}
		}

		public void Dispose()
		{
			if (!isDispose)
			{
				isDispose = true;
				M result;
				while (Queue.TryDequeue(out result))
				{
				}
				Event.WaitDispose();
				GC.SuppressFinalize(this);
			}
		}
	}

	private IntPtr? handle;

	private RenderQueue renderQueue;

	public Control? PARENT;

	public Func<Keys, bool>? KeyCall;

	private Action actionLoadMessage;

	public byte alpha = 10;

	private Rectangle target_rect = new Rectangle(-1000, -1000, 0, 0);

	private Action<IntPtr, byte, Bitmap, Rectangle> actionRender;

	private Action<bool> actionCursor;

	private bool switchClose = true;

	private bool switchDispose = true;

	private bool mdown;

	private int mdownd;

	private int oldX;

	private int oldY;

	private int oldMY;

	private ITask? taskTouch;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public virtual bool CanLoadMessage { get; set; } = true;


	public virtual bool UFocus => true;

	public Rectangle TargetRect => target_rect;

	public Rectangle TargetRectXY => new Rectangle(0, 0, target_rect.Width, target_rect.Height);

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = ((Form)this).CreateParams;
			createParams.ExStyle |= 0x8080000;
			createParams.Parent = IntPtr.Zero;
			return createParams;
		}
	}

	protected override bool ShowWithoutActivation => UFocus;

	public virtual bool MessageEnable => false;

	public virtual bool MessageCloseSub => false;

	public virtual bool MessageClickMe => true;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public bool MessageCloseMouseLeave { get; set; }

	public ILayeredForm()
	{
		((Control)this).SetStyle((ControlStyles)204818, true);
		((Control)this).UpdateStyles();
		((Form)this).FormBorderStyle = (FormBorderStyle)0;
		((Form)this).ShowInTaskbar = false;
		((Form)this).Size = new Size(0, 0);
		actionRender = delegate(IntPtr handle, byte alpha, Bitmap bmp, Rectangle rect)
		{
			Win32.SetBits(bmp, rect, handle, alpha);
		};
		actionLoadMessage = LoadMessage;
		actionCursor = delegate(bool val)
		{
			SetCursor(val);
		};
		renderQueue = new RenderQueue(this);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		handle = ((Control)this).Handle;
		((Form)this).OnHandleCreated(e);
	}

	public virtual void LoadMessage()
	{
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)actionLoadMessage);
		}
		else if (MessageEnable)
		{
			Application.AddMessageFilter((IMessageFilter)(object)this);
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		((Form)this).OnLoad(e);
		if (CanLoadMessage)
		{
			LoadMessage();
		}
	}

	protected override void Dispose(bool disposing)
	{
		Application.RemoveMessageFilter((IMessageFilter)(object)this);
		((Form)this).Dispose(disposing);
		renderQueue.Dispose();
	}

	public abstract Bitmap PrintBit();

	public bool CanRender(out IntPtr han)
	{
		if (handle.HasValue && target_rect.Width > 0 && target_rect.Height > 0)
		{
			han = handle.Value;
			return true;
		}
		han = IntPtr.Zero;
		return false;
	}

	public void SetRect(Rectangle rect)
	{
		target_rect.X = rect.X;
		target_rect.Y = rect.Y;
		target_rect.Width = rect.Width;
		target_rect.Height = rect.Height;
	}

	public void SetSize(Size size)
	{
		target_rect.Width = size.Width;
		target_rect.Height = size.Height;
	}

	public void SetSize(int w, int h)
	{
		target_rect.Width = w;
		target_rect.Height = h;
	}

	public void SetSize(int size)
	{
		ref Rectangle reference = ref target_rect;
		int width = (target_rect.Height = size);
		reference.Width = width;
	}

	public void SetSizeW(int w)
	{
		target_rect.Width = w;
	}

	public void SetSizeH(int h)
	{
		target_rect.Height = h;
	}

	public void SetLocation(Point point)
	{
		target_rect.X = point.X;
		target_rect.Y = point.Y;
	}

	public void SetLocationX(int x)
	{
		target_rect.X = x;
	}

	public void SetLocationY(int y)
	{
		target_rect.Y = y;
	}

	public void SetLocation(int x, int y)
	{
		target_rect.X = x;
		target_rect.Y = y;
	}

	public void Print(bool fore = false)
	{
		if (fore)
		{
			Render();
		}
		else
		{
			renderQueue.Set();
		}
	}

	public void Print(Bitmap bmp)
	{
		renderQueue.Set(alpha, bmp);
	}

	public void Print(Bitmap bmp, Rectangle rect)
	{
		renderQueue.Set(alpha, bmp, rect);
	}

	private void Render()
	{
		if (CanRender(out var han))
		{
			Render(han);
		}
	}

	private void Render(IntPtr handle)
	{
		try
		{
			Bitmap val = PrintBit();
			try
			{
				if (val == null)
				{
					return;
				}
				Render(handle, val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			GC.Collect();
		}
		catch
		{
		}
	}

	private void Render(IntPtr handle, Bitmap bmp)
	{
		Render(handle, alpha, bmp, target_rect);
	}

	private void Render(IntPtr handle, byte alpha, Bitmap bmp)
	{
		Render(handle, alpha, bmp, target_rect);
	}

	private void Render(IntPtr handle, byte alpha, Bitmap bmp, Rectangle rect)
	{
		try
		{
			if (((Control)this).InvokeRequired)
			{
				((Control)this).Invoke((Delegate)actionRender, new object[4] { handle, alpha, bmp, rect });
			}
			else
			{
				actionRender(handle, alpha, bmp, rect);
			}
		}
		catch
		{
		}
	}

	public void SetCursor(bool val)
	{
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)actionCursor, new object[1] { val });
		}
		else
		{
			((Control)this).Cursor = (val ? Cursors.Hand : ((Control)this).DefaultCursor);
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (UFocus && ((Message)(ref m)).Msg == 33)
		{
			((Message)(ref m)).Result = new IntPtr(3);
		}
		else
		{
			((Form)this).WndProc(ref m);
		}
	}

	public void IClose(bool isdispose = false)
	{
		if (!((Control)this).IsHandleCreated)
		{
			return;
		}
		try
		{
			if (((Control)this).InvokeRequired)
			{
				((Control)this).Invoke((Delegate)(Action)delegate
				{
					IClose(isdispose);
				});
				return;
			}
			if (switchClose)
			{
				((Form)this).Close();
			}
			switchClose = false;
			if (isdispose)
			{
				if (switchDispose)
				{
					((Component)this).Dispose();
				}
				switchDispose = false;
			}
		}
		catch
		{
		}
	}

	public bool PreFilterMessage(ref Message m)
	{
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		if (((Message)(ref m)).Msg == 513 || ((Message)(ref m)).Msg == 516 || ((Message)(ref m)).Msg == 519 || ((Message)(ref m)).Msg == 160)
		{
			Point mousePosition = Control.MousePosition;
			if (!target_rect.Contains(mousePosition))
			{
				try
				{
					if (PARENT != null && PARENT.IsHandleCreated)
					{
						if (MessageClickMe)
						{
							if (ContainsPosition(PARENT, mousePosition))
							{
								return false;
							}
							if (new Rectangle(PARENT.PointToScreen(Point.Empty), PARENT.Size).Contains(mousePosition))
							{
								return false;
							}
						}
						if (MessageCloseSub && FunSub(PARENT, mousePosition))
						{
							return false;
						}
					}
					IClose();
				}
				catch
				{
				}
				return false;
			}
		}
		else if (((Message)(ref m)).Msg == 675 && MessageCloseMouseLeave)
		{
			Point mousePosition2 = Control.MousePosition;
			if (!target_rect.Contains(mousePosition2))
			{
				try
				{
					if (PARENT != null && PARENT.IsHandleCreated)
					{
						if (ContainsPosition(PARENT, mousePosition2))
						{
							return false;
						}
						if (new Rectangle(PARENT.PointToScreen(Point.Empty), PARENT.Size).Contains(mousePosition2))
						{
							return false;
						}
						if (MessageCloseSub && FunSub(PARENT, mousePosition2))
						{
							return false;
						}
					}
					IClose();
				}
				catch
				{
				}
				return false;
			}
		}
		else if (((Message)(ref m)).Msg == 256 && KeyCall != null)
		{
			Keys arg = (Keys)(int)((Message)(ref m)).WParam;
			return KeyCall(arg);
		}
		return false;
	}

	private bool FunSub(Control control, Point mousePosition)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		if (control is SubLayeredForm subLayeredForm)
		{
			ILayeredForm layeredForm = subLayeredForm.SubForm();
			if (layeredForm != null && ContainsPosition(layeredForm, mousePosition) > 0)
			{
				return true;
			}
		}
		if (control.Controls == null || ((ArrangedElementCollection)control.Controls).Count == 0)
		{
			return false;
		}
		foreach (Control item in (ArrangedElementCollection)control.Controls)
		{
			Control control2 = item;
			if (FunSub(control2, mousePosition))
			{
				return true;
			}
		}
		return false;
	}

	private bool ContainsPosition(Control control, Point mousePosition)
	{
		if (new Rectangle(control.PointToScreen(Point.Empty), control.Size).Contains(mousePosition))
		{
			return true;
		}
		try
		{
			if (control is SubLayeredForm subLayeredForm)
			{
				ILayeredForm layeredForm = subLayeredForm.SubForm();
				if (layeredForm != null && ContainsPosition(layeredForm, mousePosition) > 0)
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	private int ContainsPosition(ILayeredForm control, Point mousePosition)
	{
		int num = 0;
		try
		{
			if (control.TargetRect.Contains(mousePosition))
			{
				num++;
			}
			if (control is SubLayeredForm subLayeredForm)
			{
				ILayeredForm layeredForm = subLayeredForm.SubForm();
				if (layeredForm != null)
				{
					num += ContainsPosition(layeredForm, mousePosition);
				}
			}
		}
		catch
		{
		}
		return num;
	}

	protected virtual void OnTouchDown(int x, int y)
	{
		oldMY = 0;
		oldX = x;
		oldY = y;
		if (Config.TouchEnabled)
		{
			taskTouch?.Dispose();
			taskTouch = null;
			mdownd = 0;
			mdown = true;
		}
	}

	protected virtual bool OnTouchMove(int x, int y)
	{
		if (mdown)
		{
			int num = oldX - x;
			int num2 = oldY - y;
			int num3 = Math.Abs(num);
			int num4 = Math.Abs(num2);
			int num5 = (int)((float)Config.TouchThreshold * Config.Dpi);
			if (mdownd > 0 || num3 > num5 || num4 > num5)
			{
				oldMY = num2;
				if (mdownd > 0)
				{
					if (mdownd == 1)
					{
						OnTouchScrollY(-num2);
					}
					else
					{
						OnTouchScrollX(-num);
					}
					oldX = x;
					oldY = y;
					return false;
				}
				if (num4 > num3)
				{
					mdownd = 1;
				}
				else
				{
					mdownd = 2;
				}
				oldX = x;
				oldY = y;
				return false;
			}
		}
		return true;
	}

	protected virtual bool OnTouchUp()
	{
		taskTouch?.Dispose();
		taskTouch = null;
		mdown = false;
		if (mdownd > 0)
		{
			if (mdownd == 1)
			{
				int num = oldMY;
				int moveYa = Math.Abs(num);
				if (moveYa > 10)
				{
					int duration = (int)Math.Ceiling((float)moveYa * 0.1f);
					int incremental = moveYa / 2;
					int interval = 20;
					if (num > 0)
					{
						taskTouch = new ITask((Control)(object)this, delegate
						{
							if (moveYa > 0 && OnTouchScrollY(-incremental))
							{
								moveYa -= duration;
								return true;
							}
							return false;
						}, interval);
					}
					else
					{
						taskTouch = new ITask((Control)(object)this, delegate
						{
							if (moveYa > 0 && OnTouchScrollY(incremental))
							{
								moveYa -= duration;
								return true;
							}
							return false;
						}, interval);
					}
				}
			}
			return false;
		}
		return true;
	}

	protected virtual bool OnTouchScrollX(int value)
	{
		return false;
	}

	protected virtual bool OnTouchScrollY(int value)
	{
		return false;
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		taskTouch?.Dispose();
		taskTouch = null;
		((ScrollableControl)this).OnMouseWheel(e);
	}
}
