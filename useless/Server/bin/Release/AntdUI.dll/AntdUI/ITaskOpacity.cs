using System;
using System.Windows.Forms;

namespace AntdUI;

public class ITaskOpacity : IDisposable
{
	private Control control;

	private Action action;

	private ITask? Thread;

	private bool enable = true;

	private bool _switch;

	private bool _down;

	public bool Enable
	{
		get
		{
			return enable;
		}
		set
		{
			if (enable != value)
			{
				enable = value;
				action();
			}
		}
	}

	public bool Switch
	{
		get
		{
			return _switch;
		}
		set
		{
			if (value && !enable)
			{
				value = false;
			}
			if (_switch == value)
			{
				return;
			}
			_switch = value;
			if (Config.Animation)
			{
				Thread?.Dispose();
				Animation = true;
				int prog = (int)((float)MaxValue * 0.078f);
				if (value)
				{
					Thread = new ITask(control, delegate
					{
						Value += prog;
						if (Value > MaxValue)
						{
							Value = MaxValue;
							return false;
						}
						action();
						return true;
					}, 10, delegate
					{
						Value = MaxValue;
						Animation = false;
						action();
					});
					return;
				}
				Thread = new ITask(control, delegate
				{
					Value -= prog;
					if (Value < 1)
					{
						Value = 0;
						return false;
					}
					action();
					return true;
				}, 10, delegate
				{
					Value = 0;
					Animation = false;
					action();
				});
			}
			else
			{
				Value = (_switch ? MaxValue : 0);
				action();
			}
		}
	}

	public bool Down
	{
		get
		{
			return _down;
		}
		set
		{
			if (_down != value)
			{
				_down = value;
				Thread?.Dispose();
				action();
			}
		}
	}

	public int MaxValue { get; set; } = 255;


	public int Value { get; private set; }

	public bool Animation { get; private set; }

	public ITaskOpacity(ILayeredFormOpacityDown _control)
	{
		ILayeredFormOpacityDown _control2 = _control;
		base._002Ector();
		control = (Control)(object)_control2;
		action = delegate
		{
			if (!_control2.RunAnimation)
			{
				_control2.Print();
			}
		};
	}

	public ITaskOpacity(ILayeredForm _control)
	{
		ILayeredForm _control2 = _control;
		base._002Ector();
		control = (Control)(object)_control2;
		action = delegate
		{
			_control2.Print();
		};
	}

	public ITaskOpacity(Form _control)
	{
		Form _control2 = _control;
		base._002Ector();
		control = (Control)(object)_control2;
		action = delegate
		{
			((Control)_control2).Invalidate();
		};
	}

	public ITaskOpacity(IControl _control)
	{
		IControl _control2 = _control;
		base._002Ector();
		control = (Control)(object)_control2;
		action = delegate
		{
			((Control)_control2).Invalidate();
		};
	}

	public void Dispose()
	{
		Thread?.Dispose();
	}
}
