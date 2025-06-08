using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public enum TaskRunLevel
{
	[XmlEnum("LeastPrivilege")]
	LUA,
	[XmlEnum("HighestAvailable")]
	Highest
}
