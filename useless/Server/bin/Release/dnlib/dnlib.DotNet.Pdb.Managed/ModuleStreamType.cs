namespace dnlib.DotNet.Pdb.Managed;

internal enum ModuleStreamType : uint
{
	Symbols = 241u,
	Lines,
	StringTable,
	FileInfo,
	FrameData,
	InlineeLines,
	CrossScopeImports,
	CrossScopeExports,
	ILLines,
	FuncMDTokenMap,
	TypeMDTokenMap,
	MergedAssemblyInput
}
