using System.Linq;

namespace WinFormAnimation;

public static class Path3DExtensions
{
	public static Path3D[] ContinueTo(this Path3D[] paths, Float3D end, ulong duration)
	{
		return paths.Concat(new Path3D[1]
		{
			new Path3D(paths.Last().End, end, duration)
		}).ToArray();
	}

	public static Path3D[] ContinueTo(this Path3D[] paths, Float3D end, ulong duration, AnimationFunctions.Function function)
	{
		return paths.Concat(new Path3D[1]
		{
			new Path3D(paths.Last().End, end, duration, function)
		}).ToArray();
	}

	public static Path3D[] ContinueTo(this Path3D[] paths, Float3D end, ulong duration, ulong delay)
	{
		return paths.Concat(new Path3D[1]
		{
			new Path3D(paths.Last().End, end, duration, delay)
		}).ToArray();
	}

	public static Path3D[] ContinueTo(this Path3D[] paths, Float3D end, ulong duration, ulong delay, AnimationFunctions.Function function)
	{
		return paths.Concat(new Path3D[1]
		{
			new Path3D(paths.Last().End, end, duration, delay, function)
		}).ToArray();
	}

	public static Path3D[] ContinueTo(this Path3D[] paths, float endX, float endY, float endZ, ulong duration)
	{
		return paths.Concat(new Path3D[1]
		{
			new Path3D(paths.Last().End, new Float3D(endX, endY, endZ), duration)
		}).ToArray();
	}

	public static Path3D[] ContinueTo(this Path3D[] paths, float endX, float endY, float endZ, ulong duration, AnimationFunctions.Function function)
	{
		return paths.Concat(new Path3D[1]
		{
			new Path3D(paths.Last().End, new Float3D(endX, endY, endZ), duration, function)
		}).ToArray();
	}

	public static Path3D[] ContinueTo(this Path3D[] paths, float endX, float endY, float endZ, ulong duration, ulong delay)
	{
		return paths.Concat(new Path3D[1]
		{
			new Path3D(paths.Last().End, new Float3D(endX, endY, endZ), duration, delay)
		}).ToArray();
	}

	public static Path3D[] ContinueTo(this Path3D[] paths, float endX, float endY, float endZ, ulong duration, ulong delay, AnimationFunctions.Function function)
	{
		return paths.Concat(new Path3D[1]
		{
			new Path3D(paths.Last().End, new Float3D(endX, endY, endZ), duration, delay, function)
		}).ToArray();
	}

	public static Path3D[] ContinueTo(this Path3D path, Float3D end, ulong duration)
	{
		return path.ToArray().ContinueTo(end, duration);
	}

	public static Path3D[] ContinueTo(this Path3D path, Float3D end, ulong duration, AnimationFunctions.Function function)
	{
		return path.ToArray().ContinueTo(end, duration, function);
	}

	public static Path3D[] ContinueTo(this Path3D path, Float3D end, ulong duration, ulong delay)
	{
		return path.ToArray().ContinueTo(end, duration, delay);
	}

	public static Path3D[] ContinueTo(this Path3D path, Float3D end, ulong duration, ulong delay, AnimationFunctions.Function function)
	{
		return path.ToArray().ContinueTo(end, duration, delay, function);
	}

	public static Path3D[] ContinueTo(this Path3D path, float endX, float endY, float endZ, ulong duration)
	{
		return path.ToArray().ContinueTo(endX, endY, endZ, duration);
	}

	public static Path3D[] ContinueTo(this Path3D path, float endX, float endY, float endZ, ulong duration, AnimationFunctions.Function function)
	{
		return path.ToArray().ContinueTo(endX, endY, endZ, duration, function);
	}

	public static Path3D[] ContinueTo(this Path3D path, float endX, float endY, float endZ, ulong duration, ulong delay)
	{
		return path.ToArray().ContinueTo(endX, endY, endZ, duration, delay);
	}

	public static Path3D[] ContinueTo(this Path3D path, float endX, float endY, float endZ, ulong duration, ulong delay, AnimationFunctions.Function function)
	{
		return path.ToArray().ContinueTo(endX, endY, endZ, duration, delay, function);
	}

	public static Path3D[] ContinueTo(this Path3D[] paths, params Path3D[] newPaths)
	{
		return paths.Concat(newPaths).ToArray();
	}

	public static Path3D[] ContinueTo(this Path3D path, params Path3D[] newPaths)
	{
		return path.ToArray().ContinueTo(newPaths);
	}

	public static Path3D[] ToArray(this Path3D path)
	{
		return new Path3D[1] { path };
	}
}
