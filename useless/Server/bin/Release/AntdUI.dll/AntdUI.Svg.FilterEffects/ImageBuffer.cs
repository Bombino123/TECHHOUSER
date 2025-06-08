using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg.FilterEffects;

public class ImageBuffer : IDictionary<string, Bitmap?>, ICollection<KeyValuePair<string, Bitmap?>>, IEnumerable<KeyValuePair<string, Bitmap?>>, IEnumerable
{
	private const string BufferKey = "__!!BUFFER";

	private Dictionary<string, Bitmap?> _images;

	private RectangleF _bounds;

	private ISvgRenderer _renderer;

	private Action<ISvgRenderer> _renderMethod;

	private float _inflate;

	public Matrix Transform { get; set; }

	public Bitmap? Buffer => _images["__!!BUFFER"];

	public int Count => _images.Count;

	public Bitmap? this[string key]
	{
		get
		{
			return ProcessResult(key, _images[ProcessKey(key)]);
		}
		set
		{
			if (value != null)
			{
				_images[ProcessKey(key)] = value;
				if (key != null)
				{
					_images["__!!BUFFER"] = value;
				}
			}
		}
	}

	bool ICollection<KeyValuePair<string, Bitmap>>.IsReadOnly => false;

	ICollection<string> IDictionary<string, Bitmap>.Keys => _images.Keys;

	ICollection<Bitmap?> IDictionary<string, Bitmap>.Values => _images.Values;

	public ImageBuffer(RectangleF bounds, float inflate, ISvgRenderer renderer, Matrix transform, Action<ISvgRenderer> renderMethod)
	{
		_bounds = bounds;
		_inflate = inflate;
		_renderer = renderer;
		Transform = transform;
		_renderMethod = renderMethod;
		_images = new Dictionary<string, Bitmap>();
		_images["BackgroundAlpha"] = null;
		_images["BackgroundImage"] = null;
		_images["FillPaint"] = null;
		_images["SourceAlpha"] = null;
		_images["SourceGraphic"] = null;
		_images["StrokePaint"] = null;
	}

	public void Add(string key, Bitmap? value)
	{
		_images.Add(ProcessKey(key), value);
	}

	public bool ContainsKey(string key)
	{
		return _images.ContainsKey(ProcessKey(key));
	}

	public void Clear()
	{
		_images.Clear();
	}

	public IEnumerator<KeyValuePair<string, Bitmap?>> GetEnumerator()
	{
		return _images.GetEnumerator();
	}

	public bool Remove(string key)
	{
		switch (key)
		{
		case "BackgroundAlpha":
		case "BackgroundImage":
		case "FillPaint":
		case "SourceAlpha":
		case "SourceGraphic":
		case "StrokePaint":
			return false;
		default:
			return _images.Remove(ProcessKey(key));
		}
	}

	public bool TryGetValue(string key, out Bitmap? value)
	{
		if (_images.TryGetValue(ProcessKey(key), out value))
		{
			value = ProcessResult(key, value);
			return true;
		}
		return false;
	}

	private Bitmap? ProcessResult(string key, Bitmap? curr)
	{
		if (curr == null)
		{
			switch (key)
			{
			case "BackgroundAlpha":
			case "BackgroundImage":
			case "FillPaint":
			case "StrokePaint":
				return null;
			case "SourceAlpha":
				_images[key] = CreateSourceAlpha();
				return _images[key];
			case "SourceGraphic":
				_images[key] = CreateSourceGraphic();
				return _images[key];
			}
		}
		return curr;
	}

	private string ProcessKey(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			if (!_images.ContainsKey("__!!BUFFER"))
			{
				return "SourceGraphic";
			}
			return "__!!BUFFER";
		}
		return key;
	}

	private Bitmap CreateSourceGraphic()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		Bitmap val = new Bitmap((int)(_bounds.Width + 2f * _inflate * _bounds.Width + _bounds.X), (int)(_bounds.Height + 2f * _inflate * _bounds.Height + _bounds.Y));
		using ISvgRenderer svgRenderer = SvgRenderer.FromImage((Image)(object)val);
		svgRenderer.SetBoundable(_renderer.GetBoundable());
		Matrix val2 = new Matrix();
		val2.Translate(_bounds.Width * _inflate, _bounds.Height * _inflate);
		svgRenderer.Transform = val2;
		_renderMethod(svgRenderer);
		return val;
	}

	private Bitmap CreateSourceAlpha()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		Bitmap val = new Bitmap((int)(_bounds.Width + 2f * _inflate * _bounds.Width + _bounds.X), (int)(_bounds.Height + 2f * _inflate * _bounds.Height + _bounds.Y));
		using ISvgRenderer svgRenderer = SvgRenderer.FromImage((Image)(object)val);
		Matrix val2 = new Matrix();
		try
		{
			svgRenderer.SetBoundable(_renderer.GetBoundable());
			val2.Translate(_bounds.Width * _inflate, _bounds.Height * _inflate);
			svgRenderer.Transform = val2;
			_renderMethod(svgRenderer);
			return val;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	void ICollection<KeyValuePair<string, Bitmap>>.Add(KeyValuePair<string, Bitmap?> item)
	{
		_images.Add(item.Key, item.Value);
	}

	bool ICollection<KeyValuePair<string, Bitmap>>.Contains(KeyValuePair<string, Bitmap?> item)
	{
		return ((ICollection<KeyValuePair<string, Bitmap>>)_images).Contains(item);
	}

	void ICollection<KeyValuePair<string, Bitmap>>.CopyTo(KeyValuePair<string, Bitmap?>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<string, Bitmap>>)_images).CopyTo(array, arrayIndex);
	}

	bool ICollection<KeyValuePair<string, Bitmap>>.Remove(KeyValuePair<string, Bitmap?> item)
	{
		return _images.Remove(item.Key);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _images.GetEnumerator();
	}
}
