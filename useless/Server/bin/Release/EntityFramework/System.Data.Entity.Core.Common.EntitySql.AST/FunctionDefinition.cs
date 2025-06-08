namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class FunctionDefinition : Node
{
	private readonly Identifier _name;

	private readonly NodeList<PropDefinition> _paramDefList;

	private readonly Node _body;

	private readonly int _startPosition;

	private readonly int _endPosition;

	internal string Name => _name.Name;

	internal NodeList<PropDefinition> Parameters => _paramDefList;

	internal Node Body => _body;

	internal int StartPosition => _startPosition;

	internal int EndPosition => _endPosition;

	internal FunctionDefinition(Identifier name, NodeList<PropDefinition> argDefList, Node body, int startPosition, int endPosition)
	{
		_name = name;
		_paramDefList = argDefList;
		_body = body;
		_startPosition = startPosition;
		_endPosition = endPosition;
	}
}
