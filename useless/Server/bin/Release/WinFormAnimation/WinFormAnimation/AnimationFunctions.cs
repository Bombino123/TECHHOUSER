using System;

namespace WinFormAnimation;

public static class AnimationFunctions
{
	public delegate float Function(float time, float beginningValue, float changeInValue, float duration);

	public static Function FromKnown(KnownAnimationFunctions knownFunction)
	{
		return knownFunction switch
		{
			KnownAnimationFunctions.CubicEaseIn => CubicEaseIn, 
			KnownAnimationFunctions.CubicEaseInOut => CubicEaseInOut, 
			KnownAnimationFunctions.CubicEaseOut => CubicEaseOut, 
			KnownAnimationFunctions.Liner => Liner, 
			KnownAnimationFunctions.CircularEaseInOut => CircularEaseInOut, 
			KnownAnimationFunctions.CircularEaseIn => CircularEaseIn, 
			KnownAnimationFunctions.CircularEaseOut => CircularEaseOut, 
			KnownAnimationFunctions.QuadraticEaseIn => QuadraticEaseIn, 
			KnownAnimationFunctions.QuadraticEaseOut => QuadraticEaseOut, 
			KnownAnimationFunctions.QuadraticEaseInOut => QuadraticEaseInOut, 
			KnownAnimationFunctions.QuarticEaseIn => QuarticEaseIn, 
			KnownAnimationFunctions.QuarticEaseOut => QuarticEaseOut, 
			KnownAnimationFunctions.QuarticEaseInOut => QuarticEaseInOut, 
			KnownAnimationFunctions.QuinticEaseInOut => QuinticEaseInOut, 
			KnownAnimationFunctions.QuinticEaseIn => QuinticEaseIn, 
			KnownAnimationFunctions.QuinticEaseOut => QuinticEaseOut, 
			KnownAnimationFunctions.SinusoidalEaseIn => SinusoidalEaseIn, 
			KnownAnimationFunctions.SinusoidalEaseOut => SinusoidalEaseOut, 
			KnownAnimationFunctions.SinusoidalEaseInOut => SinusoidalEaseInOut, 
			KnownAnimationFunctions.ExponentialEaseIn => ExponentialEaseIn, 
			KnownAnimationFunctions.ExponentialEaseOut => ExponentialEaseOut, 
			KnownAnimationFunctions.ExponentialEaseInOut => ExponentialEaseInOut, 
			_ => throw new ArgumentOutOfRangeException("knownFunction", knownFunction, "The passed animation function is unknown."), 
		};
	}

	public static float CubicEaseIn(float t, float b, float c, float d)
	{
		t /= d;
		return c * t * t * t + b;
	}

	public static float CubicEaseInOut(float t, float b, float c, float d)
	{
		t /= d / 2f;
		if (t < 1f)
		{
			return c / 2f * t * t * t + b;
		}
		t -= 2f;
		return c / 2f * (t * t * t + 2f) + b;
	}

	public static float CubicEaseOut(float t, float b, float c, float d)
	{
		t /= d;
		t -= 1f;
		return c * (t * t * t + 1f) + b;
	}

	public static float Liner(float t, float b, float c, float d)
	{
		return c * t / d + b;
	}

	public static float CircularEaseInOut(float t, float b, float c, float d)
	{
		t /= d / 2f;
		if (t < 1f)
		{
			return (float)((double)((0f - c) / 2f) * (Math.Sqrt(1f - t * t) - 1.0) + (double)b);
		}
		t -= 2f;
		return (float)((double)(c / 2f) * (Math.Sqrt(1f - t * t) + 1.0) + (double)b);
	}

	public static float CircularEaseIn(float t, float b, float c, float d)
	{
		t /= d;
		return (float)((double)(0f - c) * (Math.Sqrt(1f - t * t) - 1.0) + (double)b);
	}

	public static float CircularEaseOut(float t, float b, float c, float d)
	{
		t /= d;
		t -= 1f;
		return (float)((double)c * Math.Sqrt(1f - t * t) + (double)b);
	}

	public static float QuadraticEaseIn(float t, float b, float c, float d)
	{
		t /= d;
		return c * t * t + b;
	}

	public static float QuadraticEaseOut(float t, float b, float c, float d)
	{
		t /= d;
		return (0f - c) * t * (t - 2f) + b;
	}

	public static float QuadraticEaseInOut(float t, float b, float c, float d)
	{
		t /= d / 2f;
		if (t < 1f)
		{
			return c / 2f * t * t + b;
		}
		t -= 1f;
		return (0f - c) / 2f * (t * (t - 2f) - 1f) + b;
	}

	public static float QuarticEaseIn(float t, float b, float c, float d)
	{
		t /= d;
		return c * t * t * t * t + b;
	}

	public static float QuarticEaseOut(float t, float b, float c, float d)
	{
		t /= d;
		t -= 1f;
		return (0f - c) * (t * t * t * t - 1f) + b;
	}

	public static float QuarticEaseInOut(float t, float b, float c, float d)
	{
		t /= d / 2f;
		if (t < 1f)
		{
			return c / 2f * t * t * t * t + b;
		}
		t -= 2f;
		return (0f - c) / 2f * (t * t * t * t - 2f) + b;
	}

	public static float QuinticEaseInOut(float t, float b, float c, float d)
	{
		t /= d / 2f;
		if (t < 1f)
		{
			return c / 2f * t * t * t * t * t + b;
		}
		t -= 2f;
		return c / 2f * (t * t * t * t * t + 2f) + b;
	}

	public static float QuinticEaseIn(float t, float b, float c, float d)
	{
		t /= d;
		return c * t * t * t * t * t + b;
	}

	public static float QuinticEaseOut(float t, float b, float c, float d)
	{
		t /= d;
		t -= 1f;
		return c * (t * t * t * t * t + 1f) + b;
	}

	public static float SinusoidalEaseIn(float t, float b, float c, float d)
	{
		return (float)((double)(0f - c) * Math.Cos((double)(t / d) * (Math.PI / 2.0)) + (double)c + (double)b);
	}

	public static float SinusoidalEaseOut(float t, float b, float c, float d)
	{
		return (float)((double)c * Math.Sin((double)(t / d) * (Math.PI / 2.0)) + (double)b);
	}

	public static float SinusoidalEaseInOut(float t, float b, float c, float d)
	{
		return (float)((double)((0f - c) / 2f) * (Math.Cos(Math.PI * (double)t / (double)d) - 1.0) + (double)b);
	}

	public static float ExponentialEaseIn(float t, float b, float c, float d)
	{
		return (float)((double)c * Math.Pow(2.0, 10f * (t / d - 1f)) + (double)b);
	}

	public static float ExponentialEaseOut(float t, float b, float c, float d)
	{
		return (float)((double)c * (0.0 - Math.Pow(2.0, -10f * t / d) + 1.0) + (double)b);
	}

	public static float ExponentialEaseInOut(float t, float b, float c, float d)
	{
		t /= d / 2f;
		if (t < 1f)
		{
			return (float)((double)(c / 2f) * Math.Pow(2.0, 10f * (t - 1f)) + (double)b);
		}
		t -= 1f;
		return (float)((double)(c / 2f) * (0.0 - Math.Pow(2.0, -10f * t) + 2.0) + (double)b);
	}
}
