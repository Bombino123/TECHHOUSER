namespace dnlib.DotNet;

public interface IMemberDef : IDnlibDef, ICodedToken, IMDTokenProvider, IFullName, IHasCustomAttribute, IMemberRef, IOwnerModule, IIsTypeOrMethod
{
	new TypeDef DeclaringType { get; }
}
