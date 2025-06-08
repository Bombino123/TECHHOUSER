using System.Runtime.InteropServices;

namespace SMBLibrary.Authentication.NTLM;

[ComVisible(true)]
public delegate string GetUserPassword(string userName);
