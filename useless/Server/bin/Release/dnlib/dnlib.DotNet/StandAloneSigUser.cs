namespace dnlib.DotNet;

public class StandAloneSigUser : StandAloneSig
{
	public StandAloneSigUser()
	{
	}

	public StandAloneSigUser(LocalSig localSig)
	{
		signature = localSig;
	}

	public StandAloneSigUser(MethodSig methodSig)
	{
		signature = methodSig;
	}
}
