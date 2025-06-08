using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public enum FileSystemControlFlags : uint
{
	QuotaTrack = 1u,
	QuotaEnforce = 2u,
	ContentIndexingDisabled = 8u,
	LogQuotaThreshold = 0x10u,
	LogQuotaLimit = 0x20u,
	LogVolumeThreshold = 0x40u,
	LogVolumeLimit = 0x80u,
	QuotasIncomplete = 0x100u,
	QuotasRebuilding = 0x200u
}
