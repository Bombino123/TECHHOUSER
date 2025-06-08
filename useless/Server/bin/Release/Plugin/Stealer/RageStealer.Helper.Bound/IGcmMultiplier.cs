namespace RageStealer.Helper.Bound;

public interface IGcmMultiplier
{
	void Init(byte[] H);

	void MultiplyH(byte[] x);
}
