using System.Runtime.InteropServices;

namespace SMBLibrary.Authentication.GSSAPI;

[ComVisible(true)]
public interface IGSSMechanism
{
	byte[] Identifier { get; }

	NTStatus AcceptSecurityContext(ref object context, byte[] inputToken, out byte[] outputToken);

	bool DeleteSecurityContext(ref object context);

	object GetContextAttribute(object context, GSSAttributeName attributeName);
}
