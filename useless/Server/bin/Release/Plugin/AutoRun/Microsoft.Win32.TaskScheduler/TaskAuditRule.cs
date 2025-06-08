using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public sealed class TaskAuditRule : AuditRule
{
	public TaskRights TaskRights => (TaskRights)base.AccessMask;

	public TaskAuditRule([NotNull] IdentityReference identity, TaskRights eventRights, AuditFlags flags)
		: this(identity, (int)eventRights, isInherited: false, InheritanceFlags.None, PropagationFlags.None, flags)
	{
	}

	internal TaskAuditRule([NotNull] IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		: base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, flags)
	{
	}
}
