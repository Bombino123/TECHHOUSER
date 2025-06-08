using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
[ComVisible(true)]
public class CallerMemberNameAttribute : Attribute
{
}
