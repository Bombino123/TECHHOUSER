using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;

namespace AntdUI.Svg.Transforms;

public class SvgTransformCollection : List<SvgTransform>, ICloneable
{
	public new SvgTransform this[int i]
	{
		get
		{
			return base[i];
		}
		set
		{
			SvgTransform svgTransform = base[i];
			base[i] = value;
			if (svgTransform != value)
			{
				OnTransformChanged();
			}
		}
	}

	public event EventHandler<AttributeEventArgs> TransformChanged;

	private void AddItem(SvgTransform item)
	{
		base.Add(item);
	}

	public new void Add(SvgTransform item)
	{
		AddItem(item);
		OnTransformChanged();
	}

	public new void AddRange(IEnumerable<SvgTransform> collection)
	{
		base.AddRange(collection);
		OnTransformChanged();
	}

	public new void Remove(SvgTransform item)
	{
		base.Remove(item);
		OnTransformChanged();
	}

	public new void RemoveAt(int index)
	{
		base.RemoveAt(index);
		OnTransformChanged();
	}

	public Matrix GetMatrix()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		Matrix val = new Matrix();
		if (base.Count == 0)
		{
			return val;
		}
		using List<SvgTransform>.Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			SvgTransform current = enumerator.Current;
			val.Multiply(current.Matrix(0f, 0f));
		}
		return val;
	}

	public override bool Equals(object obj)
	{
		if (base.Count == 0 && base.Count == base.Count)
		{
			return true;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	protected void OnTransformChanged()
	{
		this.TransformChanged?.Invoke(this, new AttributeEventArgs
		{
			Attribute = "transform",
			Value = Clone()
		});
	}

	public object Clone()
	{
		SvgTransformCollection svgTransformCollection = new SvgTransformCollection();
		using List<SvgTransform>.Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			SvgTransform current = enumerator.Current;
			svgTransformCollection.AddItem(current.Clone() as SvgTransform);
		}
		return svgTransformCollection;
	}

	public override string ToString()
	{
		if (base.Count < 1)
		{
			return string.Empty;
		}
		return this.Select((SvgTransform t) => t.ToString()).Aggregate((string p, string c) => p + " " + c);
	}
}
