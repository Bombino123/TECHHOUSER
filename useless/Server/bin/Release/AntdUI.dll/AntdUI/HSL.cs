namespace AntdUI;

public class HSL
{
	public float h { get; set; }

	public float s { get; set; }

	public float l { get; set; }

	public HSL(float hue, float saturation, float lightness)
	{
		h = hue;
		s = saturation;
		l = lightness;
	}

	public HSL()
	{
	}
}
