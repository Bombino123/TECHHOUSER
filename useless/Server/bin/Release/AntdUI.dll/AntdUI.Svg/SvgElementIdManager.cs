using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AntdUI.Svg;

public class SvgElementIdManager
{
	private SvgDocument _document;

	private Dictionary<string, SvgElement> _idValueMap;

	private static readonly Regex regex = new Regex("#\\d+$");

	public event EventHandler<SvgElementEventArgs> ElementAdded;

	public event EventHandler<SvgElementEventArgs> ElementRemoved;

	public virtual SvgElement GetElementById(string id)
	{
		if (id.StartsWith("url("))
		{
			id = id.Substring(4);
			id = id.TrimEnd(new char[1] { ')' });
			if (id.StartsWith("\""))
			{
				id = id.Substring(1);
				id = id.TrimEnd(new char[1] { '"' });
			}
		}
		if (id.StartsWith("#"))
		{
			id = id.Substring(1);
		}
		_idValueMap.TryGetValue(id, out SvgElement value);
		return value;
	}

	public virtual SvgElement? GetElementById(Uri uri)
	{
		if (uri.ToString().StartsWith("url("))
		{
			uri = new Uri(uri.ToString().Substring(4).TrimEnd(new char[1] { ')' }), UriKind.Relative);
		}
		if (!uri.IsAbsoluteUri && _document.BaseUri != null && !uri.ToString().StartsWith("#"))
		{
			Uri uri2 = new Uri(_document.BaseUri, uri);
			string text = uri2.OriginalString.Substring(uri2.OriginalString.LastIndexOf('#'));
			if (uri2.Scheme.ToLowerInvariant() == "file")
			{
				return SvgDocument.Open<SvgDocument>(uri2.LocalPath.Substring(0, uri2.LocalPath.Length - text.Length))?.IdManager.GetElementById(text);
			}
			throw new NotSupportedException();
		}
		return GetElementById(uri.ToString());
	}

	public virtual void Add(SvgElement element)
	{
		AddAndForceUniqueID(element, null, autoForceUniqueID: false);
	}

	public virtual bool AddAndForceUniqueID(SvgElement element, SvgElement sibling, bool autoForceUniqueID = true, Action<SvgElement, string, string> logElementOldIDNewID = null)
	{
		bool result = false;
		if (!string.IsNullOrEmpty(element.ID))
		{
			string text = EnsureValidId(element.ID, autoForceUniqueID);
			if (autoForceUniqueID && text != element.ID)
			{
				logElementOldIDNewID?.Invoke(element, element.ID, text);
				element.ForceUniqueID(text);
				result = true;
			}
			_idValueMap.Add(element.ID, element);
		}
		OnAdded(element);
		return result;
	}

	public virtual void Remove(SvgElement element)
	{
		if (!string.IsNullOrEmpty(element.ID))
		{
			_idValueMap.Remove(element.ID);
		}
		OnRemoved(element);
	}

	public string EnsureValidId(string id, bool autoForceUniqueID = false)
	{
		if (string.IsNullOrEmpty(id))
		{
			return id;
		}
		if (char.IsDigit(id[0]))
		{
			if (autoForceUniqueID)
			{
				return EnsureValidId("id" + id, autoForceUniqueID: true);
			}
			throw new SvgIDWrongFormatException("ID cannot start with a digit: '" + id + "'.");
		}
		if (_idValueMap.ContainsKey(id))
		{
			if (autoForceUniqueID)
			{
				Match match = regex.Match(id);
				id = ((!match.Success || !int.TryParse(match.Value.Substring(1), out var result)) ? (id + "#1") : regex.Replace(id, "#" + (result + 1)));
				return EnsureValidId(id, autoForceUniqueID: true);
			}
			throw new SvgIDExistsException("An element with the same ID already exists: '" + id + "'.");
		}
		return id;
	}

	public SvgElementIdManager(SvgDocument document)
	{
		_document = document;
		_idValueMap = new Dictionary<string, SvgElement>();
	}

	protected void OnAdded(SvgElement element)
	{
		this.ElementAdded?.Invoke(_document, new SvgElementEventArgs
		{
			Element = element
		});
	}

	protected void OnRemoved(SvgElement element)
	{
		this.ElementRemoved?.Invoke(_document, new SvgElementEventArgs
		{
			Element = element
		});
	}
}
