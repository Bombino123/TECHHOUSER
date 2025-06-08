using System;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg.Transforms;

public abstract class SvgTransform : ICloneable
{
	public abstract Matrix Matrix(float w, float h);

	public abstract string WriteToString();

	public abstract object Clone();

	public override string ToString()
	{
		return WriteToString();
	}
}
