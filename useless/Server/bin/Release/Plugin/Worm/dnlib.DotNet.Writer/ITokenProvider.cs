using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public interface ITokenProvider : IWriterError
{
	MDToken GetToken(object o);

	MDToken GetToken(IList<TypeSig> locals, uint origToken);
}
