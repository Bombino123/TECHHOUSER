namespace System.Data.Entity.Core.Common.EntitySql;

internal class Disposer : IDisposable
{
	private readonly Action _action;

	internal Disposer(Action action)
	{
		_action = action;
	}

	public void Dispose()
	{
		_action();
		GC.SuppressFinalize(this);
	}
}
