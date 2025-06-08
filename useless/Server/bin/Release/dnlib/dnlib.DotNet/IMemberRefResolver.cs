namespace dnlib.DotNet;

public interface IMemberRefResolver
{
	IMemberForwarded Resolve(MemberRef memberRef);
}
