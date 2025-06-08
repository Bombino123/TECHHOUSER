using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace AntdUI.Svg.FilterEffects;

public class SvgColourMatrix : SvgFilterPrimitive
{
	public override string ClassName => "feColorMatrix";

	[SvgAttribute("type")]
	public SvgColourMatrixType Type { get; set; }

	[SvgAttribute("values")]
	public string Values { get; set; }

	public override void Process(ImageBuffer buffer)
	{
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Expected O, but got Unknown
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Expected O, but got Unknown
		//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Expected O, but got Unknown
		Bitmap val = buffer[base.Input];
		if (val == null)
		{
			return;
		}
		float[][] array;
		switch (Type)
		{
		case SvgColourMatrixType.HueRotate:
		{
			float num = (string.IsNullOrEmpty(Values) ? 0f : float.Parse(Values));
			array = new float[5][]
			{
				new float[5]
				{
					(float)(0.213 + Math.Cos(num) * 0.787 + Math.Sin(num) * -0.213),
					(float)(0.715 + Math.Cos(num) * -0.715 + Math.Sin(num) * -0.715),
					(float)(0.072 + Math.Cos(num) * -0.072 + Math.Sin(num) * 0.928),
					0f,
					0f
				},
				new float[5]
				{
					(float)(0.213 + Math.Cos(num) * -0.213 + Math.Sin(num) * 0.143),
					(float)(0.715 + Math.Cos(num) * 0.285 + Math.Sin(num) * 0.14),
					(float)(0.072 + Math.Cos(num) * -0.072 + Math.Sin(num) * -0.283),
					0f,
					0f
				},
				new float[5]
				{
					(float)(0.213 + Math.Cos(num) * -0.213 + Math.Sin(num) * -0.787),
					(float)(0.715 + Math.Cos(num) * -0.715 + Math.Sin(num) * 0.715),
					(float)(0.072 + Math.Cos(num) * 0.928 + Math.Sin(num) * 0.072),
					0f,
					0f
				},
				new float[5] { 0f, 0f, 0f, 1f, 0f },
				new float[5] { 0f, 0f, 0f, 0f, 1f }
			};
			break;
		}
		case SvgColourMatrixType.LuminanceToAlpha:
			array = new float[5][]
			{
				new float[5],
				new float[5],
				new float[5],
				new float[5] { 0.2125f, 0.7154f, 0.0721f, 0f, 0f },
				new float[5] { 0f, 0f, 0f, 0f, 1f }
			};
			break;
		case SvgColourMatrixType.Saturate:
		{
			float num = (string.IsNullOrEmpty(Values) ? 1f : float.Parse(Values));
			array = new float[5][]
			{
				new float[5]
				{
					(float)(0.213 + 0.787 * (double)num),
					(float)(0.715 - 0.715 * (double)num),
					(float)(0.072 - 0.072 * (double)num),
					0f,
					0f
				},
				new float[5]
				{
					(float)(0.213 - 0.213 * (double)num),
					(float)(0.715 + 0.285 * (double)num),
					(float)(0.072 - 0.072 * (double)num),
					0f,
					0f
				},
				new float[5]
				{
					(float)(0.213 - 0.213 * (double)num),
					(float)(0.715 - 0.715 * (double)num),
					(float)(0.072 + 0.928 * (double)num),
					0f,
					0f
				},
				new float[5] { 0f, 0f, 0f, 1f, 0f },
				new float[5] { 0f, 0f, 0f, 0f, 1f }
			};
			break;
		}
		default:
		{
			string[] source = Values.Replace("  ", " ").Split(' ', '\t', '\n', '\r', ',');
			array = new float[5][];
			for (int i = 0; i < 4; i++)
			{
				array[i] = (from v in source.Skip(i * 5).Take(5)
					select float.Parse(v)).ToArray();
			}
			array[4] = new float[5] { 0f, 0f, 0f, 0f, 1f };
			break;
		}
		}
		ColorMatrix val2 = new ColorMatrix(array);
		ImageAttributes val3 = new ImageAttributes();
		try
		{
			val3.SetColorMatrix(val2, (ColorMatrixFlag)0, (ColorAdjustType)1);
			Bitmap val4 = new Bitmap(((Image)val).Width, ((Image)val).Height);
			Graphics val5 = Graphics.FromImage((Image)(object)val4);
			try
			{
				val5.DrawImage((Image)(object)val, new Rectangle(0, 0, ((Image)val).Width, ((Image)val).Height), 0, 0, ((Image)val).Width, ((Image)val).Height, (GraphicsUnit)2, val3);
				val5.Flush();
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
			buffer[base.Result] = val4;
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}
}
