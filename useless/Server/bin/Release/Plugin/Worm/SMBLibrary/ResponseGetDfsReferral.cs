using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public class ResponseGetDfsReferral
{
	public ushort PathConsumed;

	public ushort NumberOfReferrals;

	public uint ReferralHeaderFlags;

	public List<DfsReferralEntry> ReferralEntries;

	public List<string> StringBuffer;

	public ResponseGetDfsReferral()
	{
		throw new NotImplementedException();
	}

	public ResponseGetDfsReferral(byte[] buffer)
	{
		throw new NotImplementedException();
	}

	public byte[] GetBytes()
	{
		throw new NotImplementedException();
	}
}
