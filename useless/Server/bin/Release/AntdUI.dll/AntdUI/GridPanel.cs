using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace AntdUI;

[Description("GridPanel 格栅布局")]
[ToolboxItem(true)]
[DefaultProperty("Span")]
[Designer(typeof(IControlDesigner))]
public class GridPanel : IControl
{
	internal class GridLayout : LayoutEngine
	{
		public string Span { get; set; } = "50% 50%;50% 50%";


		public int Gap { get; set; }

		public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Expected O, but got Unknown
			if (container is GridPanel gridPanel && ((Control)gridPanel).IsHandleCreated)
			{
				if (gridPanel.PauseLayout)
				{
					return false;
				}
				Rectangle displayRectangle = ((Control)gridPanel).DisplayRectangle;
				if (!string.IsNullOrEmpty(Span) && ((ArrangedElementCollection)((Control)gridPanel).Controls).Count > 0)
				{
					List<Control> list = new List<Control>(((ArrangedElementCollection)((Control)gridPanel).Controls).Count);
					foreach (Control item2 in (ArrangedElementCollection)((Control)gridPanel).Controls)
					{
						Control val = item2;
						if (val.Visible)
						{
							list.Insert(0, val);
						}
					}
					if (list.Count > 0)
					{
						Dictionary<List<int>, int> dictionary = new Dictionary<List<int>, int>();
						string[] array = Span.Split('-', '\n');
						string[] array2 = array[0].Split(new char[1] { ';' });
						string[] array3 = ((array.Length != 2) ? new string[0] : array[1]?.Split(new char[1] { ' ' }));
						int num = 0;
						int num2 = 0;
						string[] array4 = array2;
						foreach (string text in array4)
						{
							if (string.IsNullOrEmpty(text))
							{
								continue;
							}
							string[] array5 = text.Split(' ', ',');
							List<object> list2 = new List<object>(array5.Length);
							int num3 = 0;
							string[] array6 = array5;
							for (int j = 0; j < array6.Length; j++)
							{
								string text2 = array6[j].Trim();
								int result2;
								float result3;
								if (text2.EndsWith("%") && float.TryParse(text2.TrimEnd(new char[1] { '%' }), out var result))
								{
									list2.Add(result / 100f);
								}
								else if (int.TryParse(text2, out result2))
								{
									int num4 = (int)Math.Round((float)result2 * Config.Dpi);
									list2.Add(num4);
									num3 += num4;
								}
								else if (float.TryParse(text2, out result3))
								{
									list2.Add(result3);
								}
							}
							int num5 = displayRectangle.Width - num3;
							List<int> list3 = new List<int>(list2.Count);
							foreach (object item3 in list2)
							{
								if (item3 is float num6)
								{
									list3.Add((int)Math.Round((float)num5 * num6));
								}
								else if (item3 is int item)
								{
									list3.Add(item);
								}
							}
							int value = 0;
							if (array3 != null && array3.Length != 0)
							{
								int num7 = array3.Length;
								int num8 = Array.IndexOf(array2, text);
								if (num8 < num7)
								{
									string text3 = array3[num8].Trim();
									int num9 = displayRectangle.Height - num2;
									int result5;
									float result6;
									if (text3.EndsWith("%") && float.TryParse(text3.TrimEnd(new char[1] { '%' }), out var result4))
									{
										value = (int)Math.Round((float)num9 * (result4 / 100f));
									}
									else if (int.TryParse(text3, out result5))
									{
										int num10 = (int)Math.Round((float)result5 * Config.Dpi);
										value = num10;
										num2 += num10;
									}
									else if (float.TryParse(text3, out result6))
									{
										value = (int)Math.Round((float)num9 * result6);
									}
								}
								else
								{
									value = -999;
								}
							}
							else
							{
								value = -999;
							}
							if (list3.Count > 0)
							{
								num += list3.Count;
								dictionary.Add(list3, value);
							}
						}
						if (dictionary.Count > 0)
						{
							Rectangle[] rects;
							if (dictionary.Count > 1)
							{
								List<Rectangle> list4 = new List<Rectangle>();
								int num11 = 0;
								int num12 = 0;
								foreach (KeyValuePair<List<int>, int> item4 in dictionary)
								{
									int num13 = ((item4.Value == -999) ? (displayRectangle.Height / dictionary.Count) : item4.Value);
									foreach (int item5 in item4.Key)
									{
										list4.Add(new Rectangle(displayRectangle.X + num11, displayRectangle.Y + num12, item5, num13));
										num11 += item5;
									}
									num11 = 0;
									num12 += num13;
								}
								rects = list4.ToArray();
							}
							else
							{
								List<int> key = dictionary.First().Key;
								List<Rectangle> list5 = new List<Rectangle>(key.Count);
								int num14 = 0;
								int num15 = 0;
								foreach (int item6 in key)
								{
									list5.Add(new Rectangle(displayRectangle.X + num14, displayRectangle.Y + num15, item6, displayRectangle.Height));
									num14 += item6;
								}
								rects = list5.ToArray();
							}
							HandLayout(list, rects);
						}
					}
				}
			}
			return false;
		}

		private void HandLayout(List<Control> controls, Rectangle[] rects)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			int num = (int)Math.Round((float)Gap * Config.Dpi);
			int num2 = num * 2;
			int num3 = controls.Count;
			if (rects.Length < controls.Count)
			{
				num3 = rects.Length;
			}
			for (int i = 0; i < num3; i++)
			{
				Control val = controls[i];
				Point location = rects[i].Location;
				Padding margin = val.Margin;
				int dx = ((Padding)(ref margin)).Left + num;
				margin = val.Margin;
				location.Offset(dx, ((Padding)(ref margin)).Top + num);
				val.Location = location;
				int width = rects[i].Width;
				margin = val.Margin;
				int width2 = width - ((Padding)(ref margin)).Horizontal - num2;
				int height = rects[i].Height;
				margin = val.Margin;
				val.Size = new Size(width2, height - ((Padding)(ref margin)).Vertical - num2);
			}
		}
	}

	private bool pauseLayout;

	private GridLayout layoutengine = new GridLayout();

	public override Rectangle DisplayRectangle => ((Control)this).ClientRectangle.DeflateRect(((Control)this).Padding);

	[Description("跨度")]
	[Category("外观")]
	[DefaultValue("50% 50%;50% 50%")]
	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
	public string Span
	{
		get
		{
			return layoutengine.Span;
		}
		set
		{
			if (!(layoutengine.Span == value))
			{
				layoutengine.Span = value;
				if (((Control)this).IsHandleCreated)
				{
					IOnSizeChanged();
				}
				OnPropertyChanged("Span");
			}
		}
	}

	[Description("间距")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Gap
	{
		get
		{
			return layoutengine.Gap;
		}
		set
		{
			if (layoutengine.Gap != value)
			{
				layoutengine.Gap = value;
				if (((Control)this).IsHandleCreated)
				{
					IOnSizeChanged();
				}
				OnPropertyChanged("Gap");
			}
		}
	}

	[Browsable(false)]
	[Description("暂停布局")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool PauseLayout
	{
		get
		{
			return pauseLayout;
		}
		set
		{
			if (pauseLayout != value)
			{
				pauseLayout = value;
				if (!value)
				{
					((Control)this).Invalidate();
					IOnSizeChanged();
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	public override LayoutEngine LayoutEngine => (LayoutEngine)(object)layoutengine;

	protected override void OnHandleCreated(EventArgs e)
	{
		IOnSizeChanged();
		((Control)this).OnHandleCreated(e);
	}
}
