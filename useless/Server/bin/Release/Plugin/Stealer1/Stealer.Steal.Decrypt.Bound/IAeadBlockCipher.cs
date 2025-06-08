namespace Stealer.Steal.Decrypt.Bound;

public interface IAeadBlockCipher : IAeadCipher
{
	int GetBlockSize();

	IBlockCipher GetUnderlyingCipher();
}
