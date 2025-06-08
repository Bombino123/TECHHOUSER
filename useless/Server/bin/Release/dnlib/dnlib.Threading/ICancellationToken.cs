namespace dnlib.Threading;

public interface ICancellationToken
{
	void ThrowIfCancellationRequested();
}
