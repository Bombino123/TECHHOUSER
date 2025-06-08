namespace WinFormAnimation;

public class Path
{
	public float Change => End - Start;

	public ulong Delay { get; set; }

	public ulong Duration { get; set; }

	public float End { get; set; }

	public AnimationFunctions.Function Function { get; set; }

	public float Start { get; set; }

	public Path()
		: this(0f, 0f, 0uL, 0uL, null)
	{
	}

	public Path(float start, float end, ulong duration)
		: this(start, end, duration, 0uL, null)
	{
	}

	public Path(float start, float end, ulong duration, AnimationFunctions.Function function)
		: this(start, end, duration, 0uL, function)
	{
	}

	public Path(float start, float end, ulong duration, ulong delay)
		: this(start, end, duration, delay, null)
	{
	}

	public Path(float start, float end, ulong duration, ulong delay, AnimationFunctions.Function function)
	{
		Start = start;
		End = end;
		Function = function ?? new AnimationFunctions.Function(AnimationFunctions.Liner);
		Duration = duration;
		Delay = delay;
	}

	public Path Reverse()
	{
		return new Path(End, Start, Duration, Delay, Function);
	}
}
