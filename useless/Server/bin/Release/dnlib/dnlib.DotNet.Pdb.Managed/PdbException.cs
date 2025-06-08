using System;
using System.Runtime.Serialization;

namespace dnlib.DotNet.Pdb.Managed;

[Serializable]
internal sealed class PdbException : Exception
{
	public PdbException()
	{
	}

	public PdbException(string message)
		: base("Failed to read PDB: " + message)
	{
	}

	public PdbException(Exception innerException)
		: base("Failed to read PDB: " + innerException.Message, innerException)
	{
	}

	public PdbException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
