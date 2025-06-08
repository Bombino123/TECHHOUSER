namespace dnlib.DotNet;

internal struct RecursionCounter
{
	public const int MAX_RECURSION_COUNT = 100;

	private int counter;

	public int Counter => counter;

	public bool Increment()
	{
		if (counter >= 100)
		{
			return false;
		}
		counter++;
		return true;
	}

	public void Decrement()
	{
		counter--;
	}

	public override string ToString()
	{
		return counter.ToString();
	}
}
