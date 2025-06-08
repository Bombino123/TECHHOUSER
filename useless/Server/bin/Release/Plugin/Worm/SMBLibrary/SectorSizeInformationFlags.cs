using System;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[Flags]
[ComVisible(true)]
public enum SectorSizeInformationFlags : uint
{
	AlignedDevice = 1u,
	PartitionAlignedOnDevice = 2u,
	NoSeekPenalty = 4u,
	TrimEnabled = 8u
}
