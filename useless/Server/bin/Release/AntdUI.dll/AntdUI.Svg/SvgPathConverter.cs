using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AntdUI.Svg.Pathing;

namespace AntdUI.Svg;

internal class SvgPathConverter
{
	public static SvgPathSegmentList Parse(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new ArgumentNullException("path");
		}
		SvgPathSegmentList svgPathSegmentList = new SvgPathSegmentList();
		try
		{
			foreach (string item in SplitCommands(value.TrimEnd(null)))
			{
				char num = item[0];
				CreatePathSegment(isRelative: char.IsLower(num), command: num, segments: svgPathSegmentList, parser: new CoordinateParser(item.Trim()));
			}
		}
		catch
		{
		}
		return svgPathSegmentList;
	}

	private static void CreatePathSegment(char command, SvgPathSegmentList segments, CoordinateParser parser, bool isRelative)
	{
		float[] array = new float[6];
		switch (command)
		{
		case 'M':
		case 'm':
			if (parser.TryGetFloat(out array[0]) && parser.TryGetFloat(out array[1]))
			{
				segments.Add(new SvgMoveToSegment(ToAbsolute(array[0], array[1], segments, isRelative)));
			}
			while (parser.TryGetFloat(out array[0]) && parser.TryGetFloat(out array[1]))
			{
				segments.Add(new SvgLineSegment(segments.Last.End, ToAbsolute(array[0], array[1], segments, isRelative)));
			}
			break;
		case 'A':
		case 'a':
		{
			bool result;
			bool result2;
			while (parser.TryGetFloat(out array[0]) && parser.TryGetFloat(out array[1]) && parser.TryGetFloat(out array[2]) && parser.TryGetBool(out result) && parser.TryGetBool(out result2) && parser.TryGetFloat(out array[3]) && parser.TryGetFloat(out array[4]))
			{
				segments.Add(new SvgArcSegment(segments.Last.End, array[0], array[1], array[2], result ? SvgArcSize.Large : SvgArcSize.Small, result2 ? SvgArcSweep.Positive : SvgArcSweep.Negative, ToAbsolute(array[3], array[4], segments, isRelative)));
			}
			break;
		}
		case 'L':
		case 'l':
			while (parser.TryGetFloat(out array[0]) && parser.TryGetFloat(out array[1]))
			{
				segments.Add(new SvgLineSegment(segments.Last.End, ToAbsolute(array[0], array[1], segments, isRelative)));
			}
			break;
		case 'H':
		case 'h':
			while (parser.TryGetFloat(out array[0]))
			{
				segments.Add(new SvgLineSegment(segments.Last.End, ToAbsolute(array[0], segments.Last.End.Y, segments, isRelative, isRelativeY: false)));
			}
			break;
		case 'V':
		case 'v':
			while (parser.TryGetFloat(out array[0]))
			{
				segments.Add(new SvgLineSegment(segments.Last.End, ToAbsolute(segments.Last.End.X, array[0], segments, isRelativeX: false, isRelative)));
			}
			break;
		case 'Q':
		case 'q':
			while (parser.TryGetFloat(out array[0]) && parser.TryGetFloat(out array[1]) && parser.TryGetFloat(out array[2]) && parser.TryGetFloat(out array[3]))
			{
				segments.Add(new SvgQuadraticCurveSegment(segments.Last.End, ToAbsolute(array[0], array[1], segments, isRelative), ToAbsolute(array[2], array[3], segments, isRelative)));
			}
			break;
		case 'T':
		case 't':
			while (parser.TryGetFloat(out array[0]) && parser.TryGetFloat(out array[1]))
			{
				PointF controlPoint = ((segments.Last is SvgQuadraticCurveSegment svgQuadraticCurveSegment) ? Reflect(svgQuadraticCurveSegment.ControlPoint, segments.Last.End) : segments.Last.End);
				segments.Add(new SvgQuadraticCurveSegment(segments.Last.End, controlPoint, ToAbsolute(array[0], array[1], segments, isRelative)));
			}
			break;
		case 'C':
		case 'c':
			while (parser.TryGetFloat(out array[0]) && parser.TryGetFloat(out array[1]) && parser.TryGetFloat(out array[2]) && parser.TryGetFloat(out array[3]) && parser.TryGetFloat(out array[4]) && parser.TryGetFloat(out array[5]))
			{
				segments.Add(new SvgCubicCurveSegment(segments.Last.End, ToAbsolute(array[0], array[1], segments, isRelative), ToAbsolute(array[2], array[3], segments, isRelative), ToAbsolute(array[4], array[5], segments, isRelative)));
			}
			break;
		case 'S':
		case 's':
			while (parser.TryGetFloat(out array[0]) && parser.TryGetFloat(out array[1]) && parser.TryGetFloat(out array[2]) && parser.TryGetFloat(out array[3]))
			{
				PointF firstControlPoint = ((segments.Last is SvgCubicCurveSegment svgCubicCurveSegment) ? Reflect(svgCubicCurveSegment.SecondControlPoint, segments.Last.End) : segments.Last.End);
				segments.Add(new SvgCubicCurveSegment(segments.Last.End, firstControlPoint, ToAbsolute(array[0], array[1], segments, isRelative), ToAbsolute(array[2], array[3], segments, isRelative)));
			}
			break;
		case 'Z':
		case 'z':
			segments.Add(new SvgClosePathSegment());
			break;
		}
	}

	private static PointF Reflect(PointF point, PointF mirror)
	{
		float num = Math.Abs(mirror.X - point.X);
		float num2 = Math.Abs(mirror.Y - point.Y);
		float x = ((!(mirror.X >= point.X)) ? (mirror.X - num) : (mirror.X + num));
		float y = ((!(mirror.Y >= point.Y)) ? (mirror.Y - num2) : (mirror.Y + num2));
		return new PointF(x, y);
	}

	private static PointF ToAbsolute(float x, float y, SvgPathSegmentList segments, bool isRelativeBoth)
	{
		return ToAbsolute(x, y, segments, isRelativeBoth, isRelativeBoth);
	}

	private static PointF ToAbsolute(float x, float y, SvgPathSegmentList segments, bool isRelativeX, bool isRelativeY)
	{
		PointF result = new PointF(x, y);
		if ((isRelativeX || isRelativeY) && segments.Count > 0)
		{
			SvgPathSegment svgPathSegment = segments.Last;
			if (svgPathSegment is SvgClosePathSegment)
			{
				svgPathSegment = segments.Reverse().OfType<SvgMoveToSegment>().First();
			}
			if (isRelativeX)
			{
				result.X += svgPathSegment.End.X;
			}
			if (isRelativeY)
			{
				result.Y += svgPathSegment.End.Y;
			}
		}
		return result;
	}

	private static IEnumerable<string> SplitCommands(string path)
	{
		int commandStart = 0;
		for (int i = 0; i < path.Length; i++)
		{
			if (char.IsLetter(path[i]) && path[i] != 'e' && path[i] != 'E')
			{
				string text = path.Substring(commandStart, i - commandStart).Trim();
				commandStart = i;
				if (!string.IsNullOrEmpty(text))
				{
					yield return text;
				}
				if (path.Length == i + 1)
				{
					yield return path[i].ToString();
				}
			}
			else if (path.Length == i + 1)
			{
				string text = path.Substring(commandStart, i - commandStart + 1).Trim();
				if (!string.IsNullOrEmpty(text))
				{
					yield return text;
				}
			}
		}
	}
}
