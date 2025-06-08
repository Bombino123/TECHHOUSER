namespace dnlib.DotNet;

public interface IVariable
{
	TypeSig Type { get; }

	int Index { get; }

	string Name { get; set; }
}
