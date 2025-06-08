namespace NAudio.Wave;

public class Cue
{
	public int Position { get; }

	public string Label { get; }

	public Cue(int position, string label)
	{
		Position = position;
		Label = label ?? string.Empty;
	}
}
