using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public enum FileAction : uint
{
	Added = 1u,
	Removed,
	Modified,
	RenamedOldName,
	RenamedNewName,
	AddedStream,
	RemovedStream,
	ModifiedStream,
	RemovedByDelete,
	IDNotTunneled,
	TunneledIDCollision
}
