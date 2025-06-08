namespace dnlib.DotNet.Writer;

public enum MetadataEvent
{
	BeginCreateTables,
	AllocateTypeDefRids,
	AllocateMemberDefRids,
	MemberDefRidsAllocated,
	MemberDefsInitialized,
	BeforeSortTables,
	MostTablesSorted,
	MemberDefCustomAttributesWritten,
	BeginAddResources,
	EndAddResources,
	BeginWriteMethodBodies,
	EndWriteMethodBodies,
	OnAllTablesSorted,
	EndCreateTables
}
