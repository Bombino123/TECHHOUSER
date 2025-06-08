using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace System.Data.Entity.Migrations.Edm;

internal class ModelCompressor
{
	public virtual byte[] Compress(XDocument model)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
		{
			model.Save((Stream)gZipStream);
		}
		return memoryStream.ToArray();
	}

	public virtual XDocument Decompress(byte[] bytes)
	{
		using MemoryStream stream = new MemoryStream(bytes);
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		return XDocument.Load((Stream)gZipStream);
	}
}
