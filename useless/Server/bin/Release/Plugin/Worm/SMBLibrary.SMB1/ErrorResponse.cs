using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class ErrorResponse : SMB1Command
{
	private CommandName m_commandName;

	public override CommandName CommandName => m_commandName;

	public ErrorResponse(CommandName commandName)
	{
		m_commandName = commandName;
	}
}
