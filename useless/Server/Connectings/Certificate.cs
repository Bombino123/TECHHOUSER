using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

namespace Server.Connectings
{
	// Token: 0x020000C9 RID: 201
	internal class Certificate
	{
		// Token: 0x17000064 RID: 100
		// (get) Token: 0x0600065D RID: 1629 RVA: 0x0005BC99 File Offset: 0x00059E99
		// (set) Token: 0x0600065E RID: 1630 RVA: 0x0005BCA0 File Offset: 0x00059EA0
		public static X509Certificate2 certificate { get; private set; }

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x0600065F RID: 1631 RVA: 0x0005BCA8 File Offset: 0x00059EA8
		// (set) Token: 0x06000660 RID: 1632 RVA: 0x0005BCAF File Offset: 0x00059EAF
		public static bool Imported { get; private set; }

		// Token: 0x06000661 RID: 1633 RVA: 0x0005BCB7 File Offset: 0x00059EB7
		public static void Import()
		{
			if (File.Exists("ServerCertificate.p12"))
			{
				Certificate.certificate = new X509Certificate2("ServerCertificate.p12");
				Certificate.Imported = true;
			}
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x0005BCDC File Offset: 0x00059EDC
		public static X509Certificate2 CreateCertificateAuthority(string caName)
		{
			int strength = 4090;
			SecureRandom random = new SecureRandom(new CryptoApiRandomGenerator());
			RsaKeyPairGenerator rsaKeyPairGenerator = new RsaKeyPairGenerator();
			rsaKeyPairGenerator.Init(new KeyGenerationParameters(random, strength));
			AsymmetricCipherKeyPair asymmetricCipherKeyPair = rsaKeyPairGenerator.GenerateKeyPair();
			X509V3CertificateGenerator x509V3CertificateGenerator = new X509V3CertificateGenerator();
			X509Name x509Name = new X509Name("CN=" + caName);
			BigInteger serialNumber = BigInteger.ProbablePrime(120, random);
			x509V3CertificateGenerator.SetSerialNumber(serialNumber);
			x509V3CertificateGenerator.SetSubjectDN(x509Name);
			x509V3CertificateGenerator.SetIssuerDN(x509Name);
			x509V3CertificateGenerator.SetNotAfter(DateTime.MaxValue);
			x509V3CertificateGenerator.SetNotBefore(DateTime.UtcNow.Subtract(new TimeSpan(2, 0, 0, 0)));
			x509V3CertificateGenerator.SetPublicKey(asymmetricCipherKeyPair.Public);
			x509V3CertificateGenerator.AddExtension(X509Extensions.SubjectKeyIdentifier, false, new SubjectKeyIdentifierStructure(asymmetricCipherKeyPair.Public));
			x509V3CertificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(true));
			ISignatureFactory signatureCalculatorFactory = new Asn1SignatureFactory("SHA512WITHRSA", asymmetricCipherKeyPair.Private, random);
			X509Certificate2 x509Certificate = new X509Certificate2(DotNetUtilities.ToX509Certificate(x509V3CertificateGenerator.Generate(signatureCalculatorFactory)));
			x509Certificate.PrivateKey = DotNetUtilities.ToRSA(asymmetricCipherKeyPair.Private as RsaPrivateCrtKeyParameters);
			File.WriteAllBytes("ServerCertificate.p12", x509Certificate.Export(X509ContentType.Pfx));
			Certificate.Imported = true;
			return x509Certificate;
		}

		// Token: 0x0400057C RID: 1404
		public const string certificate_name = "ServerCertificate.p12";
	}
}
