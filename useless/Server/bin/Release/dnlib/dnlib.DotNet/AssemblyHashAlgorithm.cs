namespace dnlib.DotNet;

public enum AssemblyHashAlgorithm : uint
{
	None = 0u,
	MD2 = 32769u,
	MD4 = 32770u,
	MD5 = 32771u,
	SHA1 = 32772u,
	MAC = 32773u,
	SSL3_SHAMD5 = 32776u,
	HMAC = 32777u,
	TLS1PRF = 32778u,
	HASH_REPLACE_OWF = 32779u,
	SHA_256 = 32780u,
	SHA_384 = 32781u,
	SHA_512 = 32782u
}
