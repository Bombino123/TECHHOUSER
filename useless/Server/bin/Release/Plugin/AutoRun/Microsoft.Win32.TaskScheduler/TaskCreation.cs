using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[DefaultValue(TaskCreation.CreateOrUpdate)]
[ComVisible(true)]
public enum TaskCreation
{
	Create = 2,
	CreateOrUpdate = 6,
	Disable = 8,
	DontAddPrincipalAce = 16,
	IgnoreRegistrationTriggers = 32,
	Update = 4,
	ValidateOnly = 1
}
