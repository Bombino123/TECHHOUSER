using System.Runtime.InteropServices;

namespace SMBLibrary.Authentication.GSSAPI;

[ComVisible(true)]
public class GSSContext
{
	internal IGSSMechanism Mechanism;

	internal object MechanismContext;

	internal GSSContext(IGSSMechanism mechanism, object mechanismContext)
	{
		Mechanism = mechanism;
		MechanismContext = mechanismContext;
	}
}
