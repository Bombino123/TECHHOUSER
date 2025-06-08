using System.Runtime.InteropServices;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public interface IInstructionOperandResolver : ITokenResolver, IStringResolver
{
}
