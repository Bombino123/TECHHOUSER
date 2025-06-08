using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class PathStatistics
{
	private interface ISegment
	{
		double StartOffset { get; set; }

		double Length { get; }

		void LocationAngleAtOffset(double offset, out PointF point, out float rotation);
	}

	private class LineSegment : ISegment
	{
		private double _length;

		private double _rotation;

		private PointF _start;

		private PointF _end;

		public double StartOffset { get; set; }

		public double Length => _length;

		public LineSegment(PointF start, PointF end)
		{
			_start = start;
			_end = end;
			_length = Math.Sqrt(Math.Pow(end.X - start.X, 2.0) + Math.Pow(end.Y - start.Y, 2.0));
			_rotation = Math.Atan2(end.Y - start.Y, end.X - start.X) * 180.0 / Math.PI;
		}

		public void LocationAngleAtOffset(double offset, out PointF point, out float rotation)
		{
			offset -= StartOffset;
			if (offset < 0.0 || offset > _length)
			{
				throw new ArgumentOutOfRangeException();
			}
			point = new PointF((float)((double)_start.X + offset / _length * (double)(_end.X - _start.X)), (float)((double)_start.Y + offset / _length * (double)(_end.Y - _start.Y)));
			rotation = (float)_rotation;
		}
	}

	private class CubicBezierSegment : ISegment
	{
		private PointF _p0;

		private PointF _p1;

		private PointF _p2;

		private PointF _p3;

		private double _length;

		private Func<double, double> _integral;

		private SortedList<double, double> _lengths = new SortedList<double, double>();

		public double StartOffset { get; set; }

		public double Length => _length;

		public CubicBezierSegment(PointF p0, PointF p1, PointF p2, PointF p3)
		{
			_p0 = p0;
			_p1 = p1;
			_p2 = p2;
			_p3 = p3;
			_integral = (double t) => CubicBezierArcLengthIntegrand(_p0, _p1, _p2, _p3, t);
			_length = GetLength(0.0, 1.0, 9.99999993922529E-09);
			_lengths.Add(0.0, 0.0);
			_lengths.Add(_length, 1.0);
		}

		private double GetLength(double left, double right, double epsilon)
		{
			double fullInt = GaussianQuadrature(_integral, left, right, 4);
			return Subdivide(left, right, fullInt, 0.0, epsilon);
		}

		private double Subdivide(double left, double right, double fullInt, double totalLength, double epsilon)
		{
			double num = (left + right) / 2.0;
			double num2 = GaussianQuadrature(_integral, left, num, 4);
			double num3 = GaussianQuadrature(_integral, num, right, 4);
			if (Math.Abs(fullInt - (num2 + num3)) > epsilon)
			{
				double num4 = Subdivide(left, num, num2, totalLength, epsilon / 2.0);
				totalLength += num4;
				AddElementToTable(num, totalLength);
				return Subdivide(num, right, num3, totalLength, epsilon / 2.0) + num4;
			}
			return num2 + num3;
		}

		private void AddElementToTable(double position, double totalLength)
		{
			_lengths.Add(totalLength, position);
		}

		public void LocationAngleAtOffset(double offset, out PointF point, out float rotation)
		{
			offset -= StartOffset;
			if (offset < 0.0 || offset > _length)
			{
				throw new ArgumentOutOfRangeException();
			}
			double t = BinarySearchForParam(offset, 0, _lengths.Count - 1);
			point = CubicBezierCurve(_p0, _p1, _p2, _p3, t);
			PointF pointF = CubicBezierDerivative(_p0, _p1, _p2, _p3, t);
			rotation = (float)(Math.Atan2(pointF.Y, pointF.X) * 180.0 / Math.PI);
		}

		private double BinarySearchForParam(double length, int first, int last)
		{
			if (last == first)
			{
				return _lengths.Values[last];
			}
			if (last - first == 1)
			{
				return _lengths.Values[first] + (_lengths.Values[last] - _lengths.Values[first]) * (length - _lengths.Keys[first]) / (_lengths.Keys[last] - _lengths.Keys[first]);
			}
			int num = (last + first) / 2;
			if (length < _lengths.Keys[num])
			{
				return BinarySearchForParam(length, first, num);
			}
			return BinarySearchForParam(length, num, last);
		}

		public static double GaussianQuadrature(Func<double, double> func, double a, double b, int points)
		{
			return points switch
			{
				1 => (b - a) * func((a + b) / 2.0), 
				2 => (b - a) / 2.0 * (func((b - a) / 2.0 * -1.0 * 0.5773502691896257 + (a + b) / 2.0) + func((b - a) / 2.0 * 0.5773502691896257 + (a + b) / 2.0)), 
				3 => (b - a) / 2.0 * (5.0 / 9.0 * func((b - a) / 2.0 * -1.0 * 0.7745966692414834 + (a + b) / 2.0) + 8.0 / 9.0 * func((a + b) / 2.0) + 5.0 / 9.0 * func((b - a) / 2.0 * 0.7745966692414834 + (a + b) / 2.0)), 
				4 => (b - a) / 2.0 * (0.6521451548625462 * func((b - a) / 2.0 * -1.0 * 0.3399810435848563 + (a + b) / 2.0) + 0.6521451548625462 * func((b - a) / 2.0 * 0.3399810435848563 + (a + b) / 2.0) + 0.34785484513745385 * func((b - a) / 2.0 * -1.0 * 0.8611363115940526 + (a + b) / 2.0) + 0.34785484513745385 * func((b - a) / 2.0 * 0.8611363115940526 + (a + b) / 2.0)), 
				_ => throw new NotSupportedException(), 
			};
		}

		private PointF CubicBezierCurve(PointF p0, PointF p1, PointF p2, PointF p3, double t)
		{
			return new PointF((float)(Math.Pow(1.0 - t, 3.0) * (double)p0.X + 3.0 * Math.Pow(1.0 - t, 2.0) * t * (double)p1.X + 3.0 * (1.0 - t) * Math.Pow(t, 2.0) * (double)p2.X + Math.Pow(t, 3.0) * (double)p3.X), (float)(Math.Pow(1.0 - t, 3.0) * (double)p0.Y + 3.0 * Math.Pow(1.0 - t, 2.0) * t * (double)p1.Y + 3.0 * (1.0 - t) * Math.Pow(t, 2.0) * (double)p2.Y + Math.Pow(t, 3.0) * (double)p3.Y));
		}

		private PointF CubicBezierDerivative(PointF p0, PointF p1, PointF p2, PointF p3, double t)
		{
			return new PointF((float)(3.0 * Math.Pow(1.0 - t, 2.0) * (double)(p1.X - p0.X) + 6.0 * (1.0 - t) * t * (double)(p2.X - p1.X) + 3.0 * Math.Pow(t, 2.0) * (double)(p3.X - p2.X)), (float)(3.0 * Math.Pow(1.0 - t, 2.0) * (double)(p1.Y - p0.Y) + 6.0 * (1.0 - t) * t * (double)(p2.Y - p1.Y) + 3.0 * Math.Pow(t, 2.0) * (double)(p3.Y - p2.Y)));
		}

		private double CubicBezierArcLengthIntegrand(PointF p0, PointF p1, PointF p2, PointF p3, double t)
		{
			return Math.Sqrt(Math.Pow(3.0 * Math.Pow(1.0 - t, 2.0) * (double)(p1.X - p0.X) + 6.0 * (1.0 - t) * t * (double)(p2.X - p1.X) + 3.0 * Math.Pow(t, 2.0) * (double)(p3.X - p2.X), 2.0) + Math.Pow(3.0 * Math.Pow(1.0 - t, 2.0) * (double)(p1.Y - p0.Y) + 6.0 * (1.0 - t) * t * (double)(p2.Y - p1.Y) + 3.0 * Math.Pow(t, 2.0) * (double)(p3.Y - p2.Y), 2.0));
		}
	}

	private const double GqBreak_TwoPoint = 0.5773502691896257;

	private const double GqBreak_ThreePoint = 0.7745966692414834;

	private const double GqBreak_FourPoint_01 = 0.3399810435848563;

	private const double GqBreak_FourPoint_02 = 0.8611363115940526;

	private const double GqWeight_FourPoint_01 = 0.6521451548625462;

	private const double GqWeight_FourPoint_02 = 0.34785484513745385;

	private PathData _data;

	private double _totalLength;

	private List<ISegment> _segments = new List<ISegment>();

	public double TotalLength => _totalLength;

	public PathStatistics(PathData data)
	{
		_data = data;
		int num = 1;
		_totalLength = 0.0;
		while (num < _data.Points.Length)
		{
			ISegment segment;
			switch (_data.Types[num])
			{
			case 1:
				segment = new LineSegment(_data.Points[num - 1], _data.Points[num]);
				num++;
				break;
			case 3:
				segment = new CubicBezierSegment(_data.Points[num - 1], _data.Points[num], _data.Points[num + 1], _data.Points[num + 2]);
				num += 3;
				break;
			default:
				throw new NotSupportedException();
			}
			segment.StartOffset = _totalLength;
			_segments.Add(segment);
			_totalLength += segment.Length;
		}
	}

	public void LocationAngleAtOffset(double offset, out PointF point, out float angle)
	{
		_segments[BinarySearchForSegment(offset, 0, _segments.Count - 1)].LocationAngleAtOffset(offset, out point, out angle);
	}

	public bool OffsetOnPath(double offset)
	{
		ISegment segment = _segments[BinarySearchForSegment(offset, 0, _segments.Count - 1)];
		offset -= segment.StartOffset;
		if (offset >= 0.0)
		{
			return offset <= segment.Length;
		}
		return false;
	}

	private int BinarySearchForSegment(double offset, int first, int last)
	{
		if (last == first)
		{
			return first;
		}
		if (last - first == 1)
		{
			if (!(offset >= _segments[last].StartOffset))
			{
				return first;
			}
			return last;
		}
		int num = (last + first) / 2;
		if (offset < _segments[num].StartOffset)
		{
			return BinarySearchForSegment(offset, first, num);
		}
		return BinarySearchForSegment(offset, num, last);
	}
}
