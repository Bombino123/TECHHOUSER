using System;
using System.Reflection;
using System.Threading;

namespace WinFormAnimation;

public class SafeInvoker
{
	private MethodInfo _invokeMethod;

	private PropertyInfo _invokeRequiredProperty;

	private object _targetControl;

	protected object TargetControl
	{
		get
		{
			return _targetControl;
		}
		set
		{
			_invokeRequiredProperty = value.GetType().GetProperty("InvokeRequired", BindingFlags.Instance | BindingFlags.Public);
			_invokeMethod = value.GetType().GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new Type[1] { typeof(Delegate) }, null);
			if (_invokeRequiredProperty != null && _invokeMethod != null)
			{
				_targetControl = value;
			}
		}
	}

	protected Delegate UnderlyingDelegate { get; }

	public SafeInvoker(Action action, object targetControl)
		: this((Delegate)action, targetControl)
	{
	}

	protected SafeInvoker(Delegate action, object targetControl)
	{
		UnderlyingDelegate = action;
		if (targetControl != null)
		{
			TargetControl = targetControl;
		}
	}

	public SafeInvoker(Action action)
		: this(action, null)
	{
	}

	public virtual void Invoke()
	{
		Invoke(null);
	}

	protected void Invoke(object value)
	{
		try
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					if (TargetControl != null && (bool)_invokeRequiredProperty.GetValue(TargetControl, null))
					{
						_invokeMethod.Invoke(TargetControl, new object[1] { (Action)delegate
						{
							UnderlyingDelegate.DynamicInvoke((value == null) ? null : new object[1] { value });
						} });
						return;
					}
				}
				catch
				{
				}
				UnderlyingDelegate.DynamicInvoke((value == null) ? null : new object[1] { value });
			});
		}
		catch
		{
		}
	}
}
public class SafeInvoker<T> : SafeInvoker
{
	public SafeInvoker(Action<T> action, object targetControl)
		: base(action, targetControl)
	{
	}

	public SafeInvoker(Action<T> action)
		: this(action, (object)null)
	{
	}

	public void Invoke(T value)
	{
		Invoke((object)value);
	}
}
