namespace AntdUI;

public class HSV
{
	public float h { get; set; }

	public float s { get; set; }

	public float v { get; set; }

	public HSV(float hue, float saturation, float value)
	{
		h = hue;
		s = saturation;
		v = value;
	}

	public HSV()
	{
	}
}
