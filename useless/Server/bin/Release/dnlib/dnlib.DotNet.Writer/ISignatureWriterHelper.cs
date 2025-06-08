namespace dnlib.DotNet.Writer;

public interface ISignatureWriterHelper : IWriterError
{
	uint ToEncodedToken(ITypeDefOrRef typeDefOrRef);
}
