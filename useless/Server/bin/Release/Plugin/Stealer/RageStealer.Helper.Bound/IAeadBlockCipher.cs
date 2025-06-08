namespace RageStealer.Helper.Bound;

public interface IAeadBlockCipher : IAeadCipher
{
	int GetBlockSize();

	IBlockCipher GetUnderlyingCipher();
}
