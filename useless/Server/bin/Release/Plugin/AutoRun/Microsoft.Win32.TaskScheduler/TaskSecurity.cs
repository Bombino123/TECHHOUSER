using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public sealed class TaskSecurity : CommonObjectSecurity
{
	public override Type AccessRightType => typeof(TaskRights);

	public override Type AccessRuleType => typeof(TaskAccessRule);

	public override Type AuditRuleType => typeof(TaskAuditRule);

	public static TaskSecurity DefaultTaskSecurity
	{
		get
		{
			TaskSecurity taskSecurity = new TaskSecurity();
			taskSecurity.AddAccessRule(new TaskAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), TaskRights.FullControl, AccessControlType.Allow));
			taskSecurity.AddAccessRule(new TaskAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), TaskRights.Write | TaskRights.ReadAttributes | TaskRights.ReadExtendedAttributes | TaskRights.ReadData, AccessControlType.Allow));
			taskSecurity.AddAccessRule(new TaskAccessRule(new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null), TaskRights.Read, AccessControlType.Allow));
			taskSecurity.AddAccessRule(new TaskAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), TaskRights.Read, AccessControlType.Allow));
			taskSecurity.AddAccessRule(new TaskAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null), TaskRights.Read, AccessControlType.Allow));
			return taskSecurity;
		}
	}

	public TaskSecurity()
		: base(isContainer: false)
	{
	}

	public TaskSecurity([NotNull] Task task, AccessControlSections sections = AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group)
		: base(isContainer: false)
	{
		SetSecurityDescriptorSddlForm(task.GetSecurityDescriptorSddlForm(Convert(sections)), sections);
		this.CanonicalizeAccessRules();
	}

	public TaskSecurity([NotNull] TaskFolder folder, AccessControlSections sections = AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group)
		: base(isContainer: false)
	{
		SetSecurityDescriptorSddlForm(folder.GetSecurityDescriptorSddlForm(Convert(sections)), sections);
		this.CanonicalizeAccessRules();
	}

	public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
	{
		return new TaskAccessRule(identityReference, (TaskRights)accessMask, type);
	}

	public void AddAccessRule([NotNull] TaskAccessRule rule)
	{
		AddAccessRule((AccessRule)rule);
	}

	public void AddAuditRule([NotNull] TaskAuditRule rule)
	{
		AddAuditRule((AuditRule)rule);
	}

	public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
	{
		return new TaskAuditRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
	}

	public bool RemoveAccessRule([NotNull] TaskAccessRule rule)
	{
		return RemoveAccessRule((AccessRule)rule);
	}

	public void RemoveAccessRuleAll([NotNull] TaskAccessRule rule)
	{
		RemoveAccessRuleAll((AccessRule)rule);
	}

	public void RemoveAccessRuleSpecific([NotNull] TaskAccessRule rule)
	{
		RemoveAccessRuleSpecific((AccessRule)rule);
	}

	public bool RemoveAuditRule([NotNull] TaskAuditRule rule)
	{
		return RemoveAuditRule((AuditRule)rule);
	}

	public void RemoveAuditRuleAll(TaskAuditRule rule)
	{
		RemoveAuditRuleAll((AuditRule)rule);
	}

	public void RemoveAuditRuleSpecific([NotNull] TaskAuditRule rule)
	{
		RemoveAuditRuleSpecific((AuditRule)rule);
	}

	public void ResetAccessRule([NotNull] TaskAccessRule rule)
	{
		ResetAccessRule((AccessRule)rule);
	}

	public void SetAccessRule([NotNull] TaskAccessRule rule)
	{
		SetAccessRule((AccessRule)rule);
	}

	public void SetAuditRule([NotNull] TaskAuditRule rule)
	{
		SetAuditRule((AuditRule)rule);
	}

	public override string ToString()
	{
		return GetSecurityDescriptorSddlForm(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
	}

	private static SecurityInfos Convert(AccessControlSections si)
	{
		SecurityInfos securityInfos = (SecurityInfos)0;
		if ((si & AccessControlSections.Audit) != 0)
		{
			securityInfos |= SecurityInfos.SystemAcl;
		}
		if ((si & AccessControlSections.Access) != 0)
		{
			securityInfos |= SecurityInfos.DiscretionaryAcl;
		}
		if ((si & AccessControlSections.Group) != 0)
		{
			securityInfos |= SecurityInfos.Group;
		}
		if ((si & AccessControlSections.Owner) != 0)
		{
			securityInfos |= SecurityInfos.Owner;
		}
		return securityInfos;
	}

	private static AccessControlSections Convert(SecurityInfos si)
	{
		AccessControlSections accessControlSections = AccessControlSections.None;
		if ((si & SecurityInfos.SystemAcl) != 0)
		{
			accessControlSections |= AccessControlSections.Audit;
		}
		if ((si & SecurityInfos.DiscretionaryAcl) != 0)
		{
			accessControlSections |= AccessControlSections.Access;
		}
		if ((si & SecurityInfos.Group) != 0)
		{
			accessControlSections |= AccessControlSections.Group;
		}
		if ((si & SecurityInfos.Owner) != 0)
		{
			accessControlSections |= AccessControlSections.Owner;
		}
		return accessControlSections;
	}

	private AccessControlSections GetAccessControlSectionsFromChanges()
	{
		AccessControlSections accessControlSections = AccessControlSections.None;
		if (base.AccessRulesModified)
		{
			accessControlSections = AccessControlSections.Access;
		}
		if (base.AuditRulesModified)
		{
			accessControlSections |= AccessControlSections.Audit;
		}
		if (base.OwnerModified)
		{
			accessControlSections |= AccessControlSections.Owner;
		}
		if (base.GroupModified)
		{
			accessControlSections |= AccessControlSections.Group;
		}
		return accessControlSections;
	}

	[SecurityCritical]
	internal void Persist([NotNull] Task task, AccessControlSections includeSections = AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group)
	{
		WriteLock();
		try
		{
			AccessControlSections accessControlSectionsFromChanges = GetAccessControlSectionsFromChanges();
			if (accessControlSectionsFromChanges != 0)
			{
				task.SetSecurityDescriptorSddlForm(GetSecurityDescriptorSddlForm(accessControlSectionsFromChanges));
				bool flag2 = (base.AuditRulesModified = false);
				bool flag4 = (base.AccessRulesModified = flag2);
				bool ownerModified = (base.GroupModified = flag4);
				base.OwnerModified = ownerModified;
			}
		}
		finally
		{
			WriteUnlock();
		}
	}

	[SecurityCritical]
	internal void Persist([NotNull] TaskFolder folder, AccessControlSections includeSections = AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group)
	{
		WriteLock();
		try
		{
			AccessControlSections accessControlSectionsFromChanges = GetAccessControlSectionsFromChanges();
			if (accessControlSectionsFromChanges != 0)
			{
				folder.SetSecurityDescriptorSddlForm(GetSecurityDescriptorSddlForm(accessControlSectionsFromChanges));
				bool flag2 = (base.AuditRulesModified = false);
				bool flag4 = (base.AccessRulesModified = flag2);
				bool ownerModified = (base.GroupModified = flag4);
				base.OwnerModified = ownerModified;
			}
		}
		finally
		{
			WriteUnlock();
		}
	}

	protected override void Persist([NotNull] string name, AccessControlSections includeSections = AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group)
	{
		using TaskService taskService = new TaskService();
		Task task = taskService.GetTask(name);
		Persist(task, includeSections);
	}
}
