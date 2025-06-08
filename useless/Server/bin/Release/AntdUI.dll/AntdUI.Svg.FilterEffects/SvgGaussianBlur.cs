using System.Drawing;

namespace AntdUI.Svg.FilterEffects;

public class SvgGaussianBlur : SvgFilterPrimitive
{
	private float _stdDeviation;

	private BlurType _blurType;

	private int[] _kernel;

	private int _kernelSum;

	private int[,] _multable;

	public override string ClassName => "feGaussianBlur";

	[SvgAttribute("stdDeviation")]
	public float StdDeviation
	{
		get
		{
			return _stdDeviation;
		}
		set
		{
			if (value <= 0f)
			{
				value = 0f;
			}
			_stdDeviation = value;
			PreCalculate();
		}
	}

	public BlurType BlurType
	{
		get
		{
			return _blurType;
		}
		set
		{
			_blurType = value;
		}
	}

	public SvgGaussianBlur()
		: this(1f, BlurType.Both)
	{
	}

	public SvgGaussianBlur(float stdDeviation)
		: this(stdDeviation, BlurType.Both)
	{
	}

	public SvgGaussianBlur(float stdDeviation, BlurType blurType)
	{
		_stdDeviation = stdDeviation;
		_blurType = blurType;
		PreCalculate();
	}

	private void PreCalculate()
	{
		int num = (int)(_stdDeviation * 2f + 1f);
		_kernel = new int[num];
		_multable = new int[num, 256];
		for (int i = 1; (float)i <= _stdDeviation; i++)
		{
			int num2 = (int)(_stdDeviation - (float)i);
			int num3 = (int)(_stdDeviation + (float)i);
			_kernel[num3] = (_kernel[num2] = (num2 + 1) * (num2 + 1));
			_kernelSum += _kernel[num3] + _kernel[num2];
			for (int j = 0; j < 256; j++)
			{
				_multable[num3, j] = (_multable[num2, j] = _kernel[num3] * j);
			}
		}
		_kernel[(int)_stdDeviation] = (int)((_stdDeviation + 1f) * (_stdDeviation + 1f));
		_kernelSum += _kernel[(int)_stdDeviation];
		for (int k = 0; k < 256; k++)
		{
			_multable[(int)_stdDeviation, k] = _kernel[(int)_stdDeviation] * k;
		}
	}

	public Bitmap Apply(Image inputImage)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		Bitmap val = (Bitmap)(object)((inputImage is Bitmap) ? inputImage : null);
		if (val == null)
		{
			val = new Bitmap(inputImage);
		}
		using RawBitmap rawBitmap = new RawBitmap(val);
		using RawBitmap rawBitmap2 = new RawBitmap(new Bitmap(inputImage.Width, inputImage.Height));
		int num = rawBitmap.Width * rawBitmap.Height;
		int[] array = new int[num];
		int[] array2 = new int[num];
		int[] array3 = new int[num];
		int[] array4 = new int[num];
		int[] array5 = new int[num];
		int[] array6 = new int[num];
		int[] array7 = new int[num];
		int[] array8 = new int[num];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			array[i] = rawBitmap.ArgbValues[num2];
			array2[i] = rawBitmap.ArgbValues[++num2];
			array3[i] = rawBitmap.ArgbValues[++num2];
			array4[i] = rawBitmap.ArgbValues[++num2];
			num2++;
		}
		int num3 = 0;
		int num4 = 0;
		if (_blurType != BlurType.VerticalOnly)
		{
			for (int j = 0; j < num; j++)
			{
				int num7;
				int num6;
				int num5;
				int num8 = (num7 = (num6 = (num5 = 0)));
				int num9 = (int)((float)j - _stdDeviation);
				for (int k = 0; k < _kernel.Length; k++)
				{
					num2 = ((num9 >= num3) ? ((num9 <= num3 + rawBitmap.Width - 1) ? num9 : (num3 + rawBitmap.Width - 1)) : num3);
					num8 += _multable[k, array[num2]];
					num7 += _multable[k, array2[num2]];
					num6 += _multable[k, array3[num2]];
					num5 += _multable[k, array4[num2]];
					num9++;
				}
				array5[j] = num8 / _kernelSum;
				array6[j] = num7 / _kernelSum;
				array7[j] = num6 / _kernelSum;
				array8[j] = num5 / _kernelSum;
				if (_blurType == BlurType.HorizontalOnly)
				{
					rawBitmap2.ArgbValues[num4] = (byte)(num8 / _kernelSum);
					rawBitmap2.ArgbValues[++num4] = (byte)(num7 / _kernelSum);
					rawBitmap2.ArgbValues[++num4] = (byte)(num6 / _kernelSum);
					rawBitmap2.ArgbValues[++num4] = (byte)(num5 / _kernelSum);
					num4++;
				}
				if (j > 0 && j % rawBitmap.Width == 0)
				{
					num3 += rawBitmap.Width;
				}
			}
		}
		if (_blurType == BlurType.HorizontalOnly)
		{
			return rawBitmap2.Bitmap;
		}
		num4 = 0;
		for (int l = 0; l < rawBitmap.Height; l++)
		{
			int num10 = (int)((float)l - _stdDeviation);
			num3 = num10 * rawBitmap.Width;
			for (int m = 0; m < rawBitmap.Width; m++)
			{
				int num7;
				int num6;
				int num5;
				int num8 = (num7 = (num6 = (num5 = 0)));
				int num9 = num3 + m;
				int num11 = num10;
				for (int n = 0; n < _kernel.Length; n++)
				{
					if (_blurType == BlurType.VerticalOnly)
					{
						num2 = ((num11 < 0) ? m : ((num11 <= rawBitmap.Height - 1) ? num9 : (num - (rawBitmap.Width - m))));
						num8 += _multable[n, array[num2]];
						num7 += _multable[n, array2[num2]];
						num6 += _multable[n, array3[num2]];
						num5 += _multable[n, array4[num2]];
					}
					else
					{
						num2 = ((num11 < 0) ? m : ((num11 <= rawBitmap.Height - 1) ? num9 : (num - (rawBitmap.Width - m))));
						num8 += _multable[n, array5[num2]];
						num7 += _multable[n, array6[num2]];
						num6 += _multable[n, array7[num2]];
						num5 += _multable[n, array8[num2]];
					}
					num9 += rawBitmap.Width;
					num11++;
				}
				rawBitmap2.ArgbValues[num4] = (byte)(num8 / _kernelSum);
				rawBitmap2.ArgbValues[++num4] = (byte)(num7 / _kernelSum);
				rawBitmap2.ArgbValues[++num4] = (byte)(num6 / _kernelSum);
				rawBitmap2.ArgbValues[++num4] = (byte)(num5 / _kernelSum);
				num4++;
			}
		}
		return rawBitmap2.Bitmap;
	}

	public override void Process(ImageBuffer buffer)
	{
		Bitmap val = buffer[base.Input];
		if (val != null)
		{
			Bitmap value = Apply((Image)(object)val);
			buffer[base.Result] = value;
		}
	}
}
