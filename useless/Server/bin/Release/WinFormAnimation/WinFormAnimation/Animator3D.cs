using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace WinFormAnimation;

public class Animator3D : IAnimator
{
	public enum KnownProperties
	{
		BackColor,
		ForeColor
	}

	private readonly List<Path3D> _paths = new List<Path3D>();

	protected SafeInvoker EndCallback;

	protected SafeInvoker<Float3D> FrameCallback;

	protected bool IsEnded;

	protected object TargetObject;

	protected float? XValue;

	protected float? YValue;

	protected float? ZValue;

	public Path3D ActivePath => new Path3D(HorizontalAnimator.ActivePath, VerticalAnimator.ActivePath, DepthAnimator.ActivePath);

	public Animator HorizontalAnimator { get; protected set; }

	public Animator VerticalAnimator { get; protected set; }

	public Animator DepthAnimator { get; protected set; }

	public Path3D[] Paths
	{
		get
		{
			return _paths.ToArray();
		}
		set
		{
			if (CurrentStatus == AnimatorStatus.Stopped)
			{
				_paths.Clear();
				_paths.AddRange(value);
				List<Path> list = new List<Path>();
				List<Path> list2 = new List<Path>();
				List<Path> list3 = new List<Path>();
				foreach (Path3D path3D in value)
				{
					list.Add(path3D.HorizontalPath);
					list2.Add(path3D.VerticalPath);
					list3.Add(path3D.DepthPath);
				}
				HorizontalAnimator.Paths = list.ToArray();
				VerticalAnimator.Paths = list2.ToArray();
				DepthAnimator.Paths = list3.ToArray();
				return;
			}
			throw new NotSupportedException("Animation is running.");
		}
	}

	public virtual bool Repeat
	{
		get
		{
			if (HorizontalAnimator.Repeat && VerticalAnimator.Repeat)
			{
				return DepthAnimator.Repeat;
			}
			return false;
		}
		set
		{
			Animator horizontalAnimator = HorizontalAnimator;
			Animator verticalAnimator = VerticalAnimator;
			bool flag2 = (DepthAnimator.Repeat = value);
			bool repeat = (verticalAnimator.Repeat = flag2);
			horizontalAnimator.Repeat = repeat;
		}
	}

	public virtual bool ReverseRepeat
	{
		get
		{
			if (HorizontalAnimator.ReverseRepeat && VerticalAnimator.ReverseRepeat)
			{
				return DepthAnimator.ReverseRepeat;
			}
			return false;
		}
		set
		{
			Animator horizontalAnimator = HorizontalAnimator;
			Animator verticalAnimator = VerticalAnimator;
			bool flag2 = (DepthAnimator.ReverseRepeat = value);
			bool reverseRepeat = (verticalAnimator.ReverseRepeat = flag2);
			horizontalAnimator.ReverseRepeat = reverseRepeat;
		}
	}

	public virtual AnimatorStatus CurrentStatus
	{
		get
		{
			if (HorizontalAnimator.CurrentStatus == AnimatorStatus.Stopped && VerticalAnimator.CurrentStatus == AnimatorStatus.Stopped && DepthAnimator.CurrentStatus == AnimatorStatus.Stopped)
			{
				return AnimatorStatus.Stopped;
			}
			if (HorizontalAnimator.CurrentStatus == AnimatorStatus.Paused && VerticalAnimator.CurrentStatus == AnimatorStatus.Paused && DepthAnimator.CurrentStatus == AnimatorStatus.Paused)
			{
				return AnimatorStatus.Paused;
			}
			if (HorizontalAnimator.CurrentStatus == AnimatorStatus.OnHold && VerticalAnimator.CurrentStatus == AnimatorStatus.OnHold && DepthAnimator.CurrentStatus == AnimatorStatus.OnHold)
			{
				return AnimatorStatus.OnHold;
			}
			return AnimatorStatus.Playing;
		}
	}

	public Animator3D()
		: this(new Path3D[0])
	{
	}

	public Animator3D(FPSLimiterKnownValues fpsLimiter)
		: this(new Path3D[0], fpsLimiter)
	{
	}

	public Animator3D(Path3D path)
		: this(new Path3D[1] { path })
	{
	}

	public Animator3D(Path3D path, FPSLimiterKnownValues fpsLimiter)
		: this(new Path3D[1] { path }, fpsLimiter)
	{
	}

	public Animator3D(Path3D[] paths)
		: this(paths, FPSLimiterKnownValues.LimitThirty)
	{
	}

	public Animator3D(Path3D[] paths, FPSLimiterKnownValues fpsLimiter)
	{
		HorizontalAnimator = new Animator(fpsLimiter);
		VerticalAnimator = new Animator(fpsLimiter);
		DepthAnimator = new Animator(fpsLimiter);
		Paths = paths;
	}

	public virtual void Pause()
	{
		if (CurrentStatus == AnimatorStatus.OnHold || CurrentStatus == AnimatorStatus.Playing)
		{
			HorizontalAnimator.Pause();
			VerticalAnimator.Pause();
			DepthAnimator.Pause();
		}
	}

	public virtual void Play(object targetObject, string propertyName)
	{
		Play(targetObject, propertyName, null);
	}

	public virtual void Play(object targetObject, string propertyName, SafeInvoker endCallback)
	{
		TargetObject = targetObject;
		PropertyInfo prop = TargetObject.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.SetProperty);
		if (!(prop == null))
		{
			Play(new SafeInvoker<Float3D>(delegate(Float3D value)
			{
				prop.SetValue(TargetObject, Convert.ChangeType(value, prop.PropertyType), null);
			}, TargetObject), endCallback);
		}
	}

	public virtual void Play<T>(T targetObject, Expression<Func<T, object>> propertySetter)
	{
		Play(targetObject, propertySetter, null);
	}

	public virtual void Play<T>(T targetObject, Expression<Func<T, object>> propertySetter, SafeInvoker endCallback)
	{
		if (propertySetter != null)
		{
			TargetObject = targetObject;
			PropertyInfo property = ((propertySetter.Body as MemberExpression) ?? (((UnaryExpression)propertySetter.Body).Operand as MemberExpression))?.Member as PropertyInfo;
			if (property == null)
			{
				throw new ArgumentException("propertySetter");
			}
			Play(new SafeInvoker<Float3D>(delegate(Float3D value)
			{
				property.SetValue(TargetObject, Convert.ChangeType(value, property.PropertyType), null);
			}, TargetObject), endCallback);
		}
	}

	public virtual void Resume()
	{
		if (CurrentStatus == AnimatorStatus.Paused)
		{
			HorizontalAnimator.Resume();
			VerticalAnimator.Resume();
			DepthAnimator.Resume();
		}
	}

	public virtual void Stop()
	{
		HorizontalAnimator.Stop();
		VerticalAnimator.Stop();
		DepthAnimator.Stop();
		XValue = (YValue = (ZValue = null));
	}

	public void Play(object targetObject, KnownProperties property)
	{
		Play(targetObject, property, null);
	}

	public void Play(object targetObject, KnownProperties property, SafeInvoker endCallback)
	{
		Play(targetObject, property.ToString(), endCallback);
	}

	public void Play(SafeInvoker<Float3D> frameCallback)
	{
		Play(frameCallback, (SafeInvoker)null);
	}

	public void Play(SafeInvoker<Float3D> frameCallback, SafeInvoker endCallback)
	{
		Stop();
		FrameCallback = frameCallback;
		EndCallback = endCallback;
		IsEnded = false;
		HorizontalAnimator.Play(new SafeInvoker<float>(delegate(float value)
		{
			XValue = value;
			InvokeSetter();
		}), new SafeInvoker(InvokeFinisher));
		VerticalAnimator.Play(new SafeInvoker<float>(delegate(float value)
		{
			YValue = value;
			InvokeSetter();
		}), new SafeInvoker(InvokeFinisher));
		DepthAnimator.Play(new SafeInvoker<float>(delegate(float value)
		{
			ZValue = value;
			InvokeSetter();
		}), new SafeInvoker(InvokeFinisher));
	}

	private void InvokeFinisher()
	{
		if (EndCallback == null || IsEnded)
		{
			return;
		}
		lock (EndCallback)
		{
			if (CurrentStatus == AnimatorStatus.Stopped)
			{
				IsEnded = true;
				EndCallback.Invoke();
			}
		}
	}

	private void InvokeSetter()
	{
		if (XValue.HasValue && YValue.HasValue && ZValue.HasValue)
		{
			FrameCallback.Invoke(new Float3D(XValue.Value, YValue.Value, ZValue.Value));
		}
	}
}
