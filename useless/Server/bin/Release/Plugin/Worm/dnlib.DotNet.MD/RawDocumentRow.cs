using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawDocumentRow
{
	public readonly uint Name;

	public readonly uint HashAlgorithm;

	public readonly uint Hash;

	public readonly uint Language;

	public uint this[int index] => index switch
	{
		0 => Name, 
		1 => HashAlgorithm, 
		2 => Hash, 
		3 => Language, 
		_ => 0u, 
	};

	public RawDocumentRow(uint Name, uint HashAlgorithm, uint Hash, uint Language)
	{
		this.Name = Name;
		this.HashAlgorithm = HashAlgorithm;
		this.Hash = Hash;
		this.Language = Language;
	}
}
