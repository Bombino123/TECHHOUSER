namespace dnlib.DotNet;

public interface IGenericParameterProvider : ICodedToken, IMDTokenProvider, IIsTypeOrMethod
{
	int NumberOfGenericParameters { get; }
}
