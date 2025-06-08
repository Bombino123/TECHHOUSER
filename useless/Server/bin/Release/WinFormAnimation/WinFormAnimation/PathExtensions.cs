using System.Linq;

namespace WinFormAnimation;

public static class PathExtensions
{
	public static Path[] ContinueTo(this Path[] paths, float end, ulong duration)
	{
		return paths.Concat(new Path[1]
		{
			new Path(paths.Last().End, end, duration)
		}).ToArray();
	}

	public static Path[] ContinueTo(this Path[] paths, float end, ulong duration, AnimationFunctions.Function function)
	{
		return paths.Concat(new Path[1]
		{
			new Path(paths.Last().End, end, duration, function)
		}).ToArray();
	}

	public static Path[] ContinueTo(this Path[] paths, float end, ulong duration, ulong delay)
	{
		return paths.Concat(new Path[1]
		{
			new Path(paths.Last().End, end, duration, delay)
		}).ToArray();
	}

	public static Path[] ContinueTo(this Path[] paths, float end, ulong duration, ulong delay, AnimationFunctions.Function function)
	{
		return paths.Concat(new Path[1]
		{
			new Path(paths.Last().End, end, duration, delay, function)
		}).ToArray();
	}

	public static Path[] ContinueTo(this Path path, float end, ulong duration)
	{
		return path.ToArray().ContinueTo(end, duration);
	}

	public static Path[] ContinueTo(this Path path, float end, ulong duration, AnimationFunctions.Function function)
	{
		return path.ToArray().ContinueTo(end, duration, function);
	}

	public static Path[] ContinueTo(this Path path, float end, ulong duration, ulong delay)
	{
		return path.ToArray().ContinueTo(end, duration, delay);
	}

	public static Path[] ContinueTo(this Path path, float end, ulong duration, ulong delay, AnimationFunctions.Function function)
	{
		return path.ToArray().ContinueTo(end, duration, delay, function);
	}

	public static Path[] ContinueTo(this Path[] paths, params Path[] newPaths)
	{
		return paths.Concat(newPaths).ToArray();
	}

	public static Path[] ContinueTo(this Path path, params Path[] newPaths)
	{
		return path.ToArray().ContinueTo(newPaths);
	}

	public static Path[] ToArray(this Path path)
	{
		return new Path[1] { path };
	}
}
