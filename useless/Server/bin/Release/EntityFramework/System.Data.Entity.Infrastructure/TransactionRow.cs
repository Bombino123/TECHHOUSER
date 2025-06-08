namespace System.Data.Entity.Infrastructure;

public class TransactionRow
{
	public Guid Id { get; set; }

	public DateTime CreationTime { get; set; }

	public override bool Equals(object obj)
	{
		if (obj is TransactionRow transactionRow)
		{
			return Id == transactionRow.Id;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}
}
