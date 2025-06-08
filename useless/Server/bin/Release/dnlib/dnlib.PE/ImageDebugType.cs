namespace dnlib.PE;

public enum ImageDebugType : uint
{
	Unknown = 0u,
	Coff = 1u,
	CodeView = 2u,
	FPO = 3u,
	Misc = 4u,
	Exception = 5u,
	Fixup = 6u,
	OmapToSrc = 7u,
	OmapFromSrc = 8u,
	Borland = 9u,
	Reserved10 = 10u,
	CLSID = 11u,
	VcFeature = 12u,
	POGO = 13u,
	ILTCG = 14u,
	MPX = 15u,
	Reproducible = 16u,
	EmbeddedPortablePdb = 17u,
	PdbChecksum = 19u
}
