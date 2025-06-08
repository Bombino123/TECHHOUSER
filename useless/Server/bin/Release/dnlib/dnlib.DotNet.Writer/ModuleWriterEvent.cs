namespace dnlib.DotNet.Writer;

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
