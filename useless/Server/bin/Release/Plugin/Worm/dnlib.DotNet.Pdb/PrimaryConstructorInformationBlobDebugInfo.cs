using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PrimaryConstructorInformationBlobDebugInfo : PdbCustomDebugInfo
{
	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.PrimaryConstructorInformationBlob;

	public override Guid Guid => CustomDebugInfoGuids.PrimaryConstructorInformationBlob;

	public byte[] Blob { get; set; }

	public PrimaryConstructorInformationBlobDebugInfo()
	{
	}

	public PrimaryConstructorInformationBlobDebugInfo(byte[] blob)
	{
		Blob = blob;
	}
}
