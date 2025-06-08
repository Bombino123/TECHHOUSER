namespace dnlib.DotNet;

public interface IHasCustomAttribute : ICodedToken, IMDTokenProvider
{
	int HasCustomAttributeTag { get; }

	CustomAttributeCollection CustomAttributes { get; }

	bool HasCustomAttributes { get; }
}
