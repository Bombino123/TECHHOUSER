using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Internal;

internal class ModelHashCalculator
{
	public virtual string Calculate(DbCompiledModel compiledModel)
	{
		DbProviderInfo providerInfo = compiledModel.ProviderInfo;
		DbModelBuilder dbModelBuilder = compiledModel.CachedModelBuilder.Clone();
		EdmMetadataContext.ConfigureEdmMetadata(dbModelBuilder.ModelConfiguration);
		EdmModel database = dbModelBuilder.Build(providerInfo).DatabaseMapping.Database;
		database.SchemaVersion = 2.0;
		StringBuilder stringBuilder = new StringBuilder();
		using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings
		{
			Indent = true
		}))
		{
			new SsdlSerializer().Serialize(database, providerInfo.ProviderInvariantName, providerInfo.ProviderManifestToken, xmlWriter);
		}
		return ComputeSha256Hash(stringBuilder.ToString());
	}

	private static string ComputeSha256Hash(string input)
	{
		byte[] array = GetSha256HashAlgorithm().ComputeHash(Encoding.ASCII.GetBytes(input));
		StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
		}
		return stringBuilder.ToString();
	}

	private static SHA256 GetSha256HashAlgorithm()
	{
		try
		{
			return new SHA256CryptoServiceProvider();
		}
		catch (PlatformNotSupportedException)
		{
			return new SHA256Managed();
		}
	}
}
