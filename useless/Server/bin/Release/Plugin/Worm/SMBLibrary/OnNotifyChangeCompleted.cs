using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public delegate void OnNotifyChangeCompleted(NTStatus status, byte[] buffer, object context);
