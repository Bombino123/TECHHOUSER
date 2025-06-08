namespace dnlib.DotNet.Pdb;

public enum PdbCustomDebugInfoKind
{
	UsingGroups = 0,
	ForwardMethodInfo = 1,
	ForwardModuleInfo = 2,
	StateMachineHoistedLocalScopes = 3,
	StateMachineTypeName = 4,
	DynamicLocals = 5,
	EditAndContinueLocalSlotMap = 6,
	EditAndContinueLambdaMap = 7,
	TupleElementNames = 8,
	Unknown = int.MinValue,
	TupleElementNames_PortablePdb = -2147483647,
	DefaultNamespace = -2147483646,
	DynamicLocalVariables = -2147483645,
	EmbeddedSource = -2147483644,
	SourceLink = -2147483643,
	SourceServer = -2147483642,
	AsyncMethod = -2147483641,
	IteratorMethod = -2147483640,
	CompilationMetadataReferences = -2147483639,
	CompilationOptions = -2147483638,
	TypeDefinitionDocuments = -2147483637,
	EditAndContinueStateMachineStateMap = -2147483636,
	PrimaryConstructorInformationBlob = -2147483635
}
