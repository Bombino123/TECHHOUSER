namespace dnlib.DotNet;

public interface IHasFieldMarshal : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IHasConstant, IFullName
{
	int HasFieldMarshalTag { get; }

	MarshalType MarshalType { get; set; }

	bool HasMarshalType { get; }
}
