using SMBLibrary.Client;

namespace SmbWorm;

public class Brute
{
	public SMB2Client SMB2Client { get; set; }

	public string Ip { get; set; }

	public string Login { get; set; }

	public string Password { get; set; }

	public bool Bruted { get; set; }
}
