using System.Collections.Generic;
using System.Data.Entity.Core.Common.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Text;

namespace System.Data.Entity.Core.Objects;

internal sealed class Span
{
	internal class SpanPath
	{
		public readonly List<string> Navigations;

		public SpanPath(List<string> navigations)
		{
			Navigations = navigations;
		}

		public bool IsSubPath(SpanPath rhs)
		{
			if (Navigations.Count > rhs.Navigations.Count)
			{
				return false;
			}
			for (int i = 0; i < Navigations.Count; i++)
			{
				if (!Navigations[i].Equals(rhs.Navigations[i], StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}
			return true;
		}
	}

	private readonly List<SpanPath> _spanList;

	private string _cacheKey;

	internal List<SpanPath> SpanList => _spanList;

	internal Span()
	{
		_spanList = new List<SpanPath>();
	}

	internal static bool RequiresRelationshipSpan(MergeOption mergeOption)
	{
		return mergeOption != MergeOption.NoTracking;
	}

	internal static Span IncludeIn(Span spanToIncludeIn, string pathToInclude)
	{
		if (spanToIncludeIn == null)
		{
			spanToIncludeIn = new Span();
		}
		spanToIncludeIn.Include(pathToInclude);
		return spanToIncludeIn;
	}

	internal static Span CopyUnion(Span span1, Span span2)
	{
		if (span1 == null)
		{
			return span2;
		}
		if (span2 == null)
		{
			return span1;
		}
		Span span3 = span1.Clone();
		foreach (SpanPath span4 in span2.SpanList)
		{
			span3.AddSpanPath(span4);
		}
		return span3;
	}

	internal string GetCacheKey()
	{
		if (_cacheKey == null && _spanList.Count > 0)
		{
			if (_spanList.Count == 1 && _spanList[0].Navigations.Count == 1)
			{
				_cacheKey = _spanList[0].Navigations[0];
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < _spanList.Count; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(";");
					}
					SpanPath spanPath = _spanList[i];
					stringBuilder.Append(spanPath.Navigations[0]);
					for (int j = 1; j < spanPath.Navigations.Count; j++)
					{
						stringBuilder.Append(".");
						stringBuilder.Append(spanPath.Navigations[j]);
					}
				}
				_cacheKey = stringBuilder.ToString();
			}
		}
		return _cacheKey;
	}

	public void Include(string path)
	{
		Check.NotEmpty(path, "path");
		SpanPath spanPath = new SpanPath(ParsePath(path));
		AddSpanPath(spanPath);
		_cacheKey = null;
	}

	internal Span Clone()
	{
		Span span = new Span();
		span.SpanList.AddRange(_spanList);
		span._cacheKey = _cacheKey;
		return span;
	}

	internal void AddSpanPath(SpanPath spanPath)
	{
		if (ValidateSpanPath(spanPath))
		{
			RemoveExistingSubPaths(spanPath);
			_spanList.Add(spanPath);
		}
	}

	private bool ValidateSpanPath(SpanPath spanPath)
	{
		for (int i = 0; i < _spanList.Count; i++)
		{
			if (spanPath.IsSubPath(_spanList[i]))
			{
				return false;
			}
		}
		return true;
	}

	private void RemoveExistingSubPaths(SpanPath spanPath)
	{
		List<SpanPath> list = new List<SpanPath>();
		for (int i = 0; i < _spanList.Count; i++)
		{
			if (_spanList[i].IsSubPath(spanPath))
			{
				list.Add(_spanList[i]);
			}
		}
		foreach (SpanPath item in list)
		{
			_spanList.Remove(item);
		}
	}

	private static List<string> ParsePath(string path)
	{
		List<string> list = MultipartIdentifier.ParseMultipartIdentifier(path, "[", "]", '.');
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num] == null)
			{
				list.RemoveAt(num);
			}
			else if (list[num].Length == 0)
			{
				throw new ArgumentException(Strings.ObjectQuery_Span_SpanPathSyntaxError);
			}
		}
		return list;
	}
}
