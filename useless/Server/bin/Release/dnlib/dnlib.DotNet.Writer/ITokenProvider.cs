using System.Collections.Generic;

namespace dnlib.DotNet.Writer;

public interface ITokenProvider : IWriterError
{
	MDToken GetToken(object o);

	MDToken GetToken(IList<TypeSig> locals, uint origToken);
}
