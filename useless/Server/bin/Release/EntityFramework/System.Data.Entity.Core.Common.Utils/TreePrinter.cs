using System.Collections.Generic;
using System.Text;

namespace System.Data.Entity.Core.Common.Utils;

internal abstract class TreePrinter
{
	private readonly List<TreeNode> _scopes = new List<TreeNode>();

	private bool _showLines = true;

	private char _horizontals = '_';

	private char _verticals = '|';

	internal virtual string Print(TreeNode node)
	{
		PreProcess(node);
		StringBuilder stringBuilder = new StringBuilder();
		PrintNode(stringBuilder, node);
		return stringBuilder.ToString();
	}

	internal virtual void PreProcess(TreeNode node)
	{
	}

	internal virtual void AfterAppend(TreeNode node, StringBuilder text)
	{
	}

	internal virtual void BeforeAppend(TreeNode node, StringBuilder text)
	{
	}

	internal virtual void PrintNode(StringBuilder text, TreeNode node)
	{
		IndentLine(text);
		BeforeAppend(node, text);
		text.Append((object?)node.Text);
		AfterAppend(node, text);
		PrintChildren(text, node);
	}

	internal virtual void PrintChildren(StringBuilder text, TreeNode node)
	{
		_scopes.Add(node);
		node.Position = 0;
		foreach (TreeNode child in node.Children)
		{
			text.AppendLine();
			node.Position++;
			PrintNode(text, child);
		}
		_scopes.RemoveAt(_scopes.Count - 1);
	}

	private void IndentLine(StringBuilder text)
	{
		int num = 0;
		for (int i = 0; i < _scopes.Count; i++)
		{
			TreeNode treeNode = _scopes[i];
			if (!_showLines || (treeNode.Position == treeNode.Children.Count && i != _scopes.Count - 1))
			{
				text.Append(' ');
			}
			else
			{
				text.Append(_verticals);
			}
			num++;
			if (_scopes.Count == num && _showLines)
			{
				text.Append(_horizontals);
			}
			else
			{
				text.Append(' ');
			}
		}
	}
}
