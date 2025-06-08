using System;

namespace AntdUI;

public static class Animation
{
	public static int TotalFrames(int interval, int lastTime)
	{
		if (lastTime % interval <= 0)
		{
			return lastTime / interval;
		}
		return lastTime / interval + 1;
	}

	public static float Animate(double currentFrames, double totalFrames, float maxValue, AnimationType type)
	{
		return Animate(currentFrames / totalFrames, maxValue, type);
	}

	public static float Animate(double progress, float maxValue, AnimationType type)
	{
		return (float)((double)maxValue * type.CalculateValue(progress));
	}

	public static double Animate(double progress, double maxValue, AnimationType type)
	{
		return maxValue * type.CalculateValue(progress);
	}

	internal static double CalculateValue(this AnimationType type, double v)
	{
		return type switch
		{
			AnimationType.Liner => v, 
			AnimationType.Ease => Math.Sqrt(v), 
			AnimationType.Ball => Math.Sqrt(1.0 - Math.Pow(v - 1.0, 2.0)), 
			AnimationType.Resilience => -1.6666666666666667 * v * (v - 1.6), 
			_ => 1.0, 
		};
	}
}
