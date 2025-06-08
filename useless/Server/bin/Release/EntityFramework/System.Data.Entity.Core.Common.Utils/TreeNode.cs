using System.Collections.Generic;
using System.Text;

namespace System.Data.Entity.Core.Common.Utils;

internal class TreeNode
{
	private readonly StringBuilder _text;

	private readonly List<TreeNode> _children = new List<TreeNode>();

	internal StringBuilder Text => _text;

	internal IList<TreeNode> Children => _children;

	internal int Position { get; set; }

	internal TreeNode()
	{
		_text = new StringBuilder();
	}

	internal TreeNode(string text, params TreeNode[] children)
	{
		if (string.IsNullOrEmpty(text))
		{
			_text = new StringBuilder();
		}
		else
		{
			_text = new StringBuilder(text);
		}
		if (children != null)
		{
			_children.AddRange(children);
		}
	}

	internal TreeNode(string text, List<TreeNode> children)
		: this(text)
	{
		if (children != null)
		{
			_children.AddRange(children);
		}
	}
}
