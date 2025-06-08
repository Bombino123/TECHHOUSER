using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IFullName
{
	string FullName { get; }

	UTF8String Name { get; set; }
}
