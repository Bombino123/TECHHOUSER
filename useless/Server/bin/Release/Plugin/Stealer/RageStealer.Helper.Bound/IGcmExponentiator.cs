namespace RageStealer.Helper.Bound;

public interface IGcmExponentiator
{
	void Init(byte[] x);

	void ExponentiateX(long pow, byte[] output);
}
