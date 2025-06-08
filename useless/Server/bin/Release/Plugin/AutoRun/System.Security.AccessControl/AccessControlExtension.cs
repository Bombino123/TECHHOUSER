using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace System.Security.AccessControl;

[ComVisible(true)]
public static class AccessControlExtension
{
	public static void Canonicalize(this RawAcl acl)
	{
		if (acl == null)
		{
			throw new ArgumentNullException("acl");
		}
		List<GenericAce> list = new List<GenericAce>(acl.Cast<GenericAce>());
		list.Sort((GenericAce a, GenericAce b) => Comparer<byte>.Default.Compare(GetComparisonValue(a), GetComparisonValue(b)));
		while (acl.Count > 0)
		{
			acl.RemoveAce(0);
		}
		int aceIndex = 0;
		list.ForEach(delegate(GenericAce ace)
		{
			acl.InsertAce(aceIndex++, ace);
		});
	}

	public static void CanonicalizeAccessRules(this ObjectSecurity objectSecurity)
	{
		if (objectSecurity == null)
		{
			throw new ArgumentNullException("objectSecurity");
		}
		if (!objectSecurity.AreAccessRulesCanonical)
		{
			RawSecurityDescriptor rawSecurityDescriptor = new RawSecurityDescriptor(objectSecurity.GetSecurityDescriptorBinaryForm(), 0);
			rawSecurityDescriptor.DiscretionaryAcl.Canonicalize();
			objectSecurity.SetSecurityDescriptorBinaryForm(rawSecurityDescriptor.GetBinaryForm());
		}
	}

	public static byte[] GetBinaryForm(this GenericSecurityDescriptor sd)
	{
		if (sd == null)
		{
			throw new ArgumentNullException("sd");
		}
		byte[] array = new byte[sd.BinaryLength];
		sd.GetBinaryForm(array, 0);
		return array;
	}

	private static byte GetComparisonValue(GenericAce ace)
	{
		if ((ace.AceFlags & AceFlags.Inherited) != 0)
		{
			return 5;
		}
		switch (ace.AceType)
		{
		case AceType.AccessDenied:
		case AceType.SystemAudit:
		case AceType.SystemAlarm:
		case AceType.AccessDeniedCallback:
		case AceType.SystemAuditCallback:
		case AceType.SystemAlarmCallback:
			return 0;
		case AceType.AccessDeniedObject:
		case AceType.SystemAuditObject:
		case AceType.SystemAlarmObject:
		case AceType.AccessDeniedCallbackObject:
		case AceType.SystemAuditCallbackObject:
		case AceType.MaxDefinedAceType:
			return 1;
		case AceType.AccessAllowed:
		case AceType.AccessAllowedCallback:
			return 2;
		case AceType.AccessAllowedObject:
		case AceType.AccessAllowedCallbackObject:
			return 3;
		default:
			return 4;
		}
	}
}
