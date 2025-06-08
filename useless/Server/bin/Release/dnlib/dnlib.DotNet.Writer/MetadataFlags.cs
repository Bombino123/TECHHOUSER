using System;

namespace dnlib.DotNet.Writer;

[Flags]
public enum MetadataFlags : uint
{
	PreserveTypeRefRids = 1u,
	PreserveTypeDefRids = 2u,
	PreserveFieldRids = 4u,
	PreserveMethodRids = 8u,
	PreserveParamRids = 0x10u,
	PreserveMemberRefRids = 0x20u,
	PreserveStandAloneSigRids = 0x40u,
	PreserveEventRids = 0x80u,
	PreservePropertyRids = 0x100u,
	PreserveTypeSpecRids = 0x200u,
	PreserveMethodSpecRids = 0x400u,
	PreserveAllMethodRids = 0x428u,
	PreserveRids = 0x7FFu,
	PreserveStringsOffsets = 0x800u,
	PreserveUSOffsets = 0x1000u,
	PreserveBlobOffsets = 0x2000u,
	PreserveExtraSignatureData = 0x4000u,
	PreserveAll = 0x7FFFu,
	KeepOldMaxStack = 0x8000u,
	AlwaysCreateGuidHeap = 0x10000u,
	AlwaysCreateStringsHeap = 0x20000u,
	AlwaysCreateUSHeap = 0x40000u,
	AlwaysCreateBlobHeap = 0x80000u,
	RoslynSortInterfaceImpl = 0x100000u,
	NoMethodBodies = 0x200000u,
	NoDotNetResources = 0x400000u,
	NoFieldData = 0x800000u,
	OptimizeCustomAttributeSerializedTypeNames = 0x1000000u
}
