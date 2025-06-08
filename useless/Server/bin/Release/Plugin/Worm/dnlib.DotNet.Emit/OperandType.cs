using System.Runtime.InteropServices;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public enum OperandType : byte
{
	InlineBrTarget,
	InlineField,
	InlineI,
	InlineI8,
	InlineMethod,
	InlineNone,
	InlinePhi,
	InlineR,
	NOT_USED_8,
	InlineSig,
	InlineString,
	InlineSwitch,
	InlineTok,
	InlineType,
	InlineVar,
	ShortInlineBrTarget,
	ShortInlineI,
	ShortInlineR,
	ShortInlineVar
}
