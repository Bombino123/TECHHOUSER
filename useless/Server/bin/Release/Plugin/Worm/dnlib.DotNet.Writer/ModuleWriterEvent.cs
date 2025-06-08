using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public enum ModuleWriterEvent
{
	Begin,
	PESectionsCreated,
	ChunksCreated,
	ChunksAddedToSections,
	MDBeginCreateTables,
	MDAllocateTypeDefRids,
	MDAllocateMemberDefRids,
	MDMemberDefRidsAllocated,
	MDMemberDefsInitialized,
	MDBeforeSortTables,
	MDMostTablesSorted,
	MDMemberDefCustomAttributesWritten,
	MDBeginAddResources,
	MDEndAddResources,
	MDBeginWriteMethodBodies,
	MDEndWriteMethodBodies,
	MDOnAllTablesSorted,
	MDEndCreateTables,
	BeginWritePdb,
	EndWritePdb,
	BeginCalculateRvasAndFileOffsets,
	EndCalculateRvasAndFileOffsets,
	BeginWriteChunks,
	EndWriteChunks,
	BeginStrongNameSign,
	EndStrongNameSign,
	BeginWritePEChecksum,
	EndWritePEChecksum,
	End
}
