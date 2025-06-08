using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace WinFormAnimation;

public class Animator2D : IAnimator
{
	public enum KnownProperties
	{
		Size,
		Location
	}

	private readonly List<Path2D> _paths = new List<Path2D>();

	protected SafeInvoker EndCallback;

	protected SafeInvoker<Float2D> FrameCallback;

	protected bool IsEnded;

	protected object TargetObject;

	protected float? XValue;

	protected float? YValue;

	public Path2D ActivePath => new Path2D(HorizontalAnimator.ActivePath, VerticalAnimator.ActivePath);

	public Animator HorizontalAnimator { get; protected set; }

	public Animator VerticalAnimator { get; protected set; }

	public Path2D[] Paths
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
				foreach (Path2D path2D in value)
				{
					list.Add(path2D.HorizontalPath);
					list2.Add(path2D.VerticalPath);
				}
				HorizontalAnimator.Paths = list.ToArray();
				VerticalAnimator.Paths = list2.ToArray();
				return;
			}
			throw new InvalidOperationException("Animation is running.");
		}
	}

	public virtual bool Repeat
	{
		get
		{
			if (HorizontalAnimator.Repeat)
			{
				return VerticalAnimator.Repeat;
			}
			return false;
		}
		set
		{
			Animator horizontalAnimator = HorizontalAnimator;
			bool repeat = (VerticalAnimator.Repeat = value);
			horizontalAnimator.Repeat = repeat;
		}
	}

	public virtual bool ReverseRepeat
	{
		get
		{
			if (HorizontalAnimator.ReverseRepeat)
			{
				return VerticalAnimator.ReverseRepeat;
			}
			return false;
		}
		set
		{
			Animator horizontalAnimator = HorizontalAnimator;
			bool reverseRepeat = (VerticalAnimator.ReverseRepeat = value);
			horizontalAnimator.ReverseRepeat = reverseRepeat;
		}
	}

	public virtual AnimatorStatus CurrentStatus
	{
		get
		{
			if (HorizontalAnimator.CurrentStatus == AnimatorStatus.Stopped && VerticalAnimator.CurrentStatus == AnimatorStatus.Stopped)
			{
				return AnimatorStatus.Stopped;
			}
			if (HorizontalAnimator.CurrentStatus == AnimatorStatus.Paused && VerticalAnimator.CurrentStatus == AnimatorStatus.Paused)
			{
				return AnimatorStatus.Paused;
			}
			if (HorizontalAnimator.CurrentStatus == AnimatorStatus.OnHold && VerticalAnimator.CurrentStatus == AnimatorStatus.OnHold)
			{
				return AnimatorStatus.OnHold;
			}
			return AnimatorStatus.Playing;
		}
	}

	public Animator2D()
		: this(new Path2D[0])
	{
	}

	public Animator2D(FPSLimiterKnownValues fpsLimiter)
		: this(new Path2D[0], fpsLimiter)
	{
	}

	public Animator2D(Path2D path)
		: this(new Path2D[1] { path })
	{
	}

	public Animator2D(Path2D path, FPSLimiterKnownValues fpsLimiter)
		: this(new Path2D[1] { path }, fpsLimiter)
	{
	}

	public Animator2D(Path2D[] paths)
		: this(paths, FPSLimiterKnownValues.LimitThirty)
	{
	}

	public Animator2D(Path2D[] paths, FPSLimiterKnownValues fpsLimiter)
	{
		HorizontalAnimator = new Animator(fpsLimiter);
		VerticalAnimator = new Animator(fpsLimiter);
		Paths = paths;
	}

	public virtual void Pause()
	{
		if (CurrentStatus == AnimatorStatus.OnHold || CurrentStatus == AnimatorStatus.Playing)
		{
			HorizontalAnimator.Pause();
			VerticalAnimator.Pause();
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
			Play(new SafeInvoker<Float2D>(delegate(Float2D value)
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
			Play(new SafeInvoker<Float2D>(delegate(Float2D value)
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
		}
	}

	public virtual void Stop()
	{
		HorizontalAnimator.Stop();
		VerticalAnimator.Stop();
		XValue = (YValue = null);
	}

	public void Play(object targetObject, KnownProperties property)
	{
		Play(targetObject, property, null);
	}

	public void Play(object targetObject, KnownProperties property, SafeInvoker endCallback)
	{
		Play(targetObject, property.ToString(), endCallback);
	}

	public void Play(SafeInvoker<Float2D> frameCallback)
	{
		Play(frameCallback, (SafeInvoker)null);
	}

	public void Play(SafeInvoker<Float2D> frameCallback, SafeInvoker endCallback)
	{
		Stop();
		FrameCallback = frameCallback;
		EndCallback = endCallback;
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
		if (XValue.HasValue && YValue.HasValue)
		{
			FrameCallback.Invoke(new Float2D(XValue.Value, YValue.Value));
		}
	}
}
