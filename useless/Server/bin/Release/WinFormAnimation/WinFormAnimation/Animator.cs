using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WinFormAnimation;

public class Animator : IAnimator
{
	public enum KnownProperties
	{
		Value,
		Text,
		Caption,
		BackColor,
		ForeColor,
		Opacity
	}

	private readonly List<Path> _paths = new List<Path>();

	private readonly List<Path> _tempPaths = new List<Path>();

	private readonly Timer _timer;

	private bool _tempReverseRepeat;

	protected SafeInvoker EndCallback;

	protected SafeInvoker<float> FrameCallback;

	protected object TargetObject;

	public Path[] Paths
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
				return;
			}
			throw new InvalidOperationException("Animation is running.");
		}
	}

	public Path ActivePath { get; private set; }

	public virtual bool Repeat { get; set; }

	public virtual bool ReverseRepeat { get; set; }

	public virtual AnimatorStatus CurrentStatus { get; private set; }

	public Animator()
		: this(new Path[0])
	{
	}

	public Animator(FPSLimiterKnownValues fpsLimiter)
		: this(new Path[0], fpsLimiter)
	{
	}

	public Animator(Path path)
		: this(new Path[1] { path })
	{
	}

	public Animator(Path path, FPSLimiterKnownValues fpsLimiter)
		: this(new Path[1] { path }, fpsLimiter)
	{
	}

	public Animator(Path[] paths)
		: this(paths, FPSLimiterKnownValues.LimitThirty)
	{
	}

	public Animator(Path[] paths, FPSLimiterKnownValues fpsLimiter)
	{
		CurrentStatus = AnimatorStatus.Stopped;
		_timer = new Timer(Elapsed, fpsLimiter);
		Paths = paths;
	}

	public virtual void Pause()
	{
		if (CurrentStatus == AnimatorStatus.OnHold || CurrentStatus == AnimatorStatus.Playing)
		{
			_timer.Stop();
			CurrentStatus = AnimatorStatus.Paused;
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
			Play(new SafeInvoker<float>(delegate(float value)
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
			Play(new SafeInvoker<float>(delegate(float value)
			{
				property.SetValue(TargetObject, Convert.ChangeType(value, property.PropertyType), null);
			}, TargetObject), endCallback);
		}
	}

	public virtual void Resume()
	{
		if (CurrentStatus == AnimatorStatus.Paused)
		{
			_timer.Resume();
		}
	}

	public virtual void Stop()
	{
		_timer.Stop();
		lock (_tempPaths)
		{
			_tempPaths.Clear();
		}
		ActivePath = null;
		CurrentStatus = AnimatorStatus.Stopped;
		_tempReverseRepeat = false;
	}

	public virtual void Play(object targetObject, KnownProperties property)
	{
		Play(targetObject, property, null);
	}

	public virtual void Play(object targetObject, KnownProperties property, SafeInvoker endCallback)
	{
		Play(targetObject, property.ToString(), endCallback);
	}

	public virtual void Play(SafeInvoker<float> frameCallback)
	{
		Play(frameCallback, (SafeInvoker)null);
	}

	public virtual void Play(SafeInvoker<float> frameCallback, SafeInvoker endCallback)
	{
		Stop();
		FrameCallback = frameCallback;
		EndCallback = endCallback;
		_timer.ResetClock();
		lock (_tempPaths)
		{
			_tempPaths.AddRange(_paths);
		}
		_timer.Start();
	}

	private void Elapsed(ulong millSinceBeginning = 0uL)
	{
		while (true)
		{
			lock (_tempPaths)
			{
				if (_tempPaths != null && ActivePath == null && _tempPaths.Count > 0)
				{
					while (ActivePath == null)
					{
						if (_tempReverseRepeat)
						{
							ActivePath = _tempPaths.LastOrDefault();
							_tempPaths.RemoveAt(_tempPaths.Count - 1);
						}
						else
						{
							ActivePath = _tempPaths.FirstOrDefault();
							_tempPaths.RemoveAt(0);
						}
						_timer.ResetClock();
						millSinceBeginning = 0uL;
					}
				}
				bool flag = ActivePath == null;
				if (ActivePath != null)
				{
					if (!_tempReverseRepeat && millSinceBeginning < ActivePath.Delay)
					{
						CurrentStatus = AnimatorStatus.OnHold;
						return;
					}
					if (millSinceBeginning - ((!_tempReverseRepeat) ? ActivePath.Delay : 0) <= ActivePath.Duration)
					{
						if (CurrentStatus != AnimatorStatus.Playing)
						{
							CurrentStatus = AnimatorStatus.Playing;
						}
						float value = ActivePath.Function(_tempReverseRepeat ? (ActivePath.Duration - millSinceBeginning) : (millSinceBeginning - ActivePath.Delay), ActivePath.Start, ActivePath.Change, ActivePath.Duration);
						FrameCallback.Invoke(value);
						return;
					}
					if (CurrentStatus == AnimatorStatus.Playing)
					{
						if (_tempPaths.Count == 0)
						{
							FrameCallback.Invoke(_tempReverseRepeat ? ActivePath.Start : ActivePath.End);
							flag = true;
						}
						else
						{
							if (_tempReverseRepeat && ActivePath.Delay != 0)
							{
								goto IL_01f4;
							}
							if (!_tempReverseRepeat)
							{
								Path? path = _tempPaths.FirstOrDefault();
								if (path != null && path.Delay != 0)
								{
									goto IL_01f4;
								}
							}
						}
					}
					goto IL_021f;
				}
				goto IL_0254;
				IL_021f:
				if (_tempReverseRepeat && millSinceBeginning - ActivePath.Duration < ActivePath.Delay)
				{
					CurrentStatus = AnimatorStatus.OnHold;
					return;
				}
				ActivePath = null;
				goto IL_0254;
				IL_0254:
				if (!flag)
				{
					return;
				}
				goto end_IL_0009;
				IL_01f4:
				FrameCallback.Invoke(_tempReverseRepeat ? ActivePath.Start : ActivePath.End);
				goto IL_021f;
				end_IL_0009:;
			}
			if (!Repeat)
			{
				break;
			}
			lock (_tempPaths)
			{
				_tempPaths.AddRange(_paths);
				_tempReverseRepeat = ReverseRepeat && !_tempReverseRepeat;
			}
			millSinceBeginning = 0uL;
		}
		Stop();
		EndCallback?.Invoke();
	}
}
