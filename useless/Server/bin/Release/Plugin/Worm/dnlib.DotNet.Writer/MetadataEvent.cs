using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
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
