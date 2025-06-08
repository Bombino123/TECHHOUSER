namespace SharpDX.Direct3D11;

public enum ContentProtectionCaps
{
	Software = 1,
	Hardware = 2,
	ProtectionAlwaysOn = 4,
	PartialDecryption = 8,
	ContentKey = 0x10,
	FreshenSessionKey = 0x20,
	EncryptedReadBack = 0x40,
	EncryptedReadBackKey = 0x80,
	SequentialCtrIv = 0x100,
	EncryptSlicedataOnly = 0x200,
	DecryptionBlit = 0x400,
	HardwareProtectUncompressed = 0x800,
	HardwareProtectedMemoryPageable = 0x1000,
	HardwareTeardown = 0x2000,
	HardwareDrmCommunication = 0x4000
}
