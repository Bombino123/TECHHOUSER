namespace AntdUI.Svg;

public class SvgOrient
{
	private bool _isAuto = true;

	private float _angle;

	public float Angle
	{
		get
		{
			return _angle;
		}
		set
		{
			_angle = value;
			_isAuto = false;
		}
	}

	public bool IsAuto
	{
		get
		{
			return _isAuto;
		}
		set
		{
			_isAuto = value;
			_angle = 0f;
		}
	}

	public SvgOrient()
	{
		IsAuto = false;
		Angle = 0f;
	}

	public SvgOrient(bool isAuto)
	{
		IsAuto = isAuto;
	}

	public SvgOrient(float angle)
	{
		Angle = angle;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj.GetType() == typeof(SvgOrient)))
		{
			return false;
		}
		SvgOrient svgOrient = (SvgOrient)obj;
		if (svgOrient.IsAuto == IsAuto)
		{
			return svgOrient.Angle == Angle;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		if (IsAuto)
		{
			return "auto";
		}
		return Angle.ToString();
	}

	public static implicit operator SvgOrient(float value)
	{
		return new SvgOrient(value);
	}
}
