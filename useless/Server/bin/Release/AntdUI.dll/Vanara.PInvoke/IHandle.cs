using System;

namespace Vanara.PInvoke;

public interface IHandle
{
	IntPtr DangerousGetHandle();
}
