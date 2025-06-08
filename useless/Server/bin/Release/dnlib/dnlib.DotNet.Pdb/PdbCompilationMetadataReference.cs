using System;

namespace dnlib.DotNet.Pdb;

public sealed class PdbCompilationMetadataReference
{
	public string Name { get; set; }

	public string Aliases { get; set; }

	public PdbCompilationMetadataReferenceFlags Flags { get; set; }

	public uint Timestamp { get; set; }

	public uint SizeOfImage { get; set; }

	public Guid Mvid { get; set; }

	public PdbCompilationMetadataReference()
	{
		Name = string.Empty;
		Aliases = string.Empty;
	}

	public PdbCompilationMetadataReference(string name, string aliases, PdbCompilationMetadataReferenceFlags flags, uint timestamp, uint sizeOfImage, Guid mvid)
	{
		Name = name;
		Aliases = aliases;
		Flags = flags;
		Timestamp = timestamp;
		SizeOfImage = sizeOfImage;
		Mvid = mvid;
	}
}
