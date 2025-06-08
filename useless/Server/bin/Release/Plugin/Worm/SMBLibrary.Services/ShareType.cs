using System.Runtime.InteropServices;

namespace SMBLibrary.Services;

[ComVisible(true)]
public enum ShareType : uint
{
	DiskDrive = 0u,
	PrintQueue = 1u,
	CommunicationDevice = 2u,
	IPC = 3u,
	ClusterShare = 33554432u,
	ScaleOutClusterShare = 67108864u,
	DfsShareInCluster = 134217728u
}
