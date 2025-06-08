using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace System.Data.Entity.Infrastructure.Design;

internal class ForwardingProxy<T> : RealProxy
{
	private readonly MarshalByRefObject _target;

	public ForwardingProxy(object target)
		: base(typeof(T))
	{
		_target = (MarshalByRefObject)target;
	}

	public override IMessage Invoke(IMessage msg)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		new MethodCallMessageWrapper((IMethodCallMessage)msg).Uri = RemotingServices.GetObjectUri(_target);
		return RemotingServices.GetEnvoyChainForProxy(_target).SyncProcessMessage(msg);
	}

	public T GetTransparentProxy()
	{
		return (T)((RealProxy)this).GetTransparentProxy();
	}
}
