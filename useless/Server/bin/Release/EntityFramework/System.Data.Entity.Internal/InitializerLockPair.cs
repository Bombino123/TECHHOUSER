namespace System.Data.Entity.Internal;

internal class InitializerLockPair : Tuple<Action<DbContext>, bool>
{
	public Action<DbContext> InitializerDelegate => base.Item1;

	public bool IsLocked => base.Item2;

	public InitializerLockPair(Action<DbContext> initializerDelegate, bool isLocked)
		: base(initializerDelegate, isLocked)
	{
	}
}
