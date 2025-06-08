namespace dnlib.DotNet;

public enum SecurityAction : short
{
	ActionMask = 31,
	ActionNil = 0,
	Request = 1,
	Demand = 2,
	Assert = 3,
	Deny = 4,
	PermitOnly = 5,
	LinktimeCheck = 6,
	LinkDemand = 6,
	InheritanceCheck = 7,
	InheritDemand = 7,
	RequestMinimum = 8,
	RequestOptional = 9,
	RequestRefuse = 10,
	PrejitGrant = 11,
	PreJitGrant = 11,
	PrejitDenied = 12,
	PreJitDeny = 12,
	NonCasDemand = 13,
	NonCasLinkDemand = 14,
	NonCasInheritance = 15,
	MaximumValue = 15
}
