using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public interface ISignatureWriterHelper : IWriterError
{
	uint ToEncodedToken(ITypeDefOrRef typeDefOrRef);
}
