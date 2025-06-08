namespace dnlib.DotNet;

public interface IHasConstant : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName
{
	int HasConstantTag { get; }

	Constant Constant { get; set; }
}
