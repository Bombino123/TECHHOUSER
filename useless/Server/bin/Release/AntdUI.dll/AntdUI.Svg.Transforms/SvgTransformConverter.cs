using System;
using System.Collections.Generic;
using System.Globalization;

namespace AntdUI.Svg.Transforms;

internal class SvgTransformConverter
{
	private static IEnumerable<string> SplitTransforms(string transforms)
	{
		int num = 0;
		for (int i = 0; i < transforms.Length; i++)
		{
			if (transforms[i] == ')')
			{
				yield return transforms.Substring(num, i - num + 1).Trim();
				for (; i < transforms.Length && !char.IsLetter(transforms[i]); i++)
				{
				}
				num = i;
			}
		}
	}

	public static SvgTransformCollection Parse(string value)
	{
		SvgTransformCollection svgTransformCollection = new SvgTransformCollection();
		foreach (string item in SplitTransforms(value))
		{
			if (string.IsNullOrEmpty(item))
			{
				continue;
			}
			string[] array = item.Split('(', ')');
			string text = array[0].Trim();
			string text2 = array[1].Trim();
			switch (text)
			{
			case "translate":
			{
				string[] array7 = text2.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array7.Length == 0 || array7.Length > 2)
				{
					throw new FormatException("Translate transforms must be in the format 'translate(x [,y])'");
				}
				if (array7.Length > 1)
				{
					string text4 = array7[0].Trim();
					string text5 = array7[1].Trim();
					if (text4.EndsWith("%") || text5.EndsWith("%"))
					{
						if (text4.EndsWith("%") && text5.EndsWith("%"))
						{
							float x4 = float.Parse(text4.TrimEnd(new char[1] { '%' }), NumberStyles.Float, CultureInfo.InvariantCulture);
							float y4 = float.Parse(text5.TrimEnd(new char[1] { '%' }), NumberStyles.Float, CultureInfo.InvariantCulture);
							svgTransformCollection.Add(new SvgTranslate(x4, ratio_x: true, y4, ratio_y: true));
						}
						else if (text4.EndsWith("%"))
						{
							float x5 = float.Parse(text4.TrimEnd(new char[1] { '%' }), NumberStyles.Float, CultureInfo.InvariantCulture);
							float y5 = float.Parse(text5, NumberStyles.Float, CultureInfo.InvariantCulture);
							svgTransformCollection.Add(new SvgTranslate(x5, ratio_x: true, y5, ratio_y: false));
						}
						else
						{
							float x6 = float.Parse(text4, NumberStyles.Float, CultureInfo.InvariantCulture);
							float y6 = float.Parse(text5.TrimEnd(new char[1] { '%' }), NumberStyles.Float, CultureInfo.InvariantCulture);
							svgTransformCollection.Add(new SvgTranslate(x6, ratio_x: false, y6, ratio_y: true));
						}
					}
					else
					{
						float x7 = float.Parse(text4, NumberStyles.Float, CultureInfo.InvariantCulture);
						float y7 = float.Parse(text5.TrimEnd(new char[1] { '%' }), NumberStyles.Float, CultureInfo.InvariantCulture);
						svgTransformCollection.Add(new SvgTranslate(x7, ratio_x: false, y7, ratio_y: false));
					}
				}
				else
				{
					string text6 = array7[0].Trim();
					if (text6.EndsWith("%"))
					{
						float x8 = float.Parse(text6.TrimEnd(new char[1] { '%' }), NumberStyles.Float, CultureInfo.InvariantCulture);
						svgTransformCollection.Add(new SvgTranslate(x8, ratio_x: true, 0f, ratio_y: false));
					}
					else
					{
						float x9 = float.Parse(text6, NumberStyles.Float, CultureInfo.InvariantCulture);
						svgTransformCollection.Add(new SvgTranslate(x9, ratio_x: false, 0f, ratio_y: false));
					}
				}
				break;
			}
			case "rotate":
			{
				string[] array5 = text2.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array5.Length != 1 && array5.Length != 3)
				{
					throw new FormatException("Rotate transforms must be in the format 'rotate(angle [cx cy ])'");
				}
				float angle = float.Parse(array5[0], NumberStyles.Float, CultureInfo.InvariantCulture);
				if (array5.Length == 1)
				{
					svgTransformCollection.Add(new SvgRotate(angle));
					break;
				}
				float centerX = float.Parse(array5[1], NumberStyles.Float, CultureInfo.InvariantCulture);
				float centerY = float.Parse(array5[2], NumberStyles.Float, CultureInfo.InvariantCulture);
				svgTransformCollection.Add(new SvgRotate(angle, centerX, centerY));
				break;
			}
			case "scale":
			{
				string[] array6 = text2.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array6.Length == 0 || array6.Length > 2)
				{
					throw new FormatException("Scale transforms must be in the format 'scale(x [,y])'");
				}
				float x3 = float.Parse(array6[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
				if (array6.Length > 1)
				{
					float y3 = float.Parse(array6[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
					svgTransformCollection.Add(new SvgScale(x3, y3));
				}
				else
				{
					svgTransformCollection.Add(new SvgScale(x3));
				}
				break;
			}
			case "matrix":
			{
				string[] array3 = text2.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array3.Length != 6)
				{
					throw new FormatException("Matrix transforms must be in the format 'matrix(m11, m12, m21, m22, dx, dy)'");
				}
				List<float> list = new List<float>();
				string[] array4 = array3;
				foreach (string text3 in array4)
				{
					list.Add(float.Parse(text3.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture));
				}
				svgTransformCollection.Add(new SvgMatrix(list));
				break;
			}
			case "shear":
			{
				string[] array2 = text2.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length == 0 || array2.Length > 2)
				{
					throw new FormatException("Shear transforms must be in the format 'shear(x [,y])'");
				}
				float x2 = float.Parse(array2[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
				if (array2.Length > 1)
				{
					float y2 = float.Parse(array2[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
					svgTransformCollection.Add(new SvgShear(x2, y2));
				}
				else
				{
					svgTransformCollection.Add(new SvgShear(x2));
				}
				break;
			}
			case "skewX":
			{
				float x = float.Parse(text2, NumberStyles.Float, CultureInfo.InvariantCulture);
				svgTransformCollection.Add(new SvgSkew(x, 0f));
				break;
			}
			case "skewY":
			{
				float y = float.Parse(text2, NumberStyles.Float, CultureInfo.InvariantCulture);
				svgTransformCollection.Add(new SvgSkew(0f, y));
				break;
			}
			}
		}
		return svgTransformCollection;
	}
}
