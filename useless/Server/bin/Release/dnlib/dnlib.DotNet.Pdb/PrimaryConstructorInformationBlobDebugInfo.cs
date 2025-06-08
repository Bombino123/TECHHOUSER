using System;

namespace dnlib.DotNet.Pdb;

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
