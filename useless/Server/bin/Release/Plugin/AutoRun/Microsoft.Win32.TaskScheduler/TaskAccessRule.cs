using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public sealed class TaskAccessRule : AccessRule
{
	public TaskRights TaskRights => (TaskRights)base.AccessMask;

	public TaskAccessRule([NotNull] IdentityReference identity, TaskRights eventRights, AccessControlType type)
		: this(identity, (int)eventRights, isInherited: false, InheritanceFlags.None, PropagationFlags.None, type)
	{
	}

	public TaskAccessRule([NotNull] string identity, TaskRights eventRights, AccessControlType type)
		: this(new NTAccount(identity), (int)eventRights, isInherited: false, InheritanceFlags.None, PropagationFlags.None, type)
	{
	}

	private TaskAccessRule([NotNull] IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		: base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, type)
	{
	}
}
