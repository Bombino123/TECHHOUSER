namespace dnlib.DotNet;

public interface IIsTypeOrMethod
{
	bool IsType { get; }

	bool IsMethod { get; }
}
