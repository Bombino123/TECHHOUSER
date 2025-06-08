using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IGenericParameterProvider : ICodedToken, IMDTokenProvider, IIsTypeOrMethod
{
	int NumberOfGenericParameters { get; }
}
