namespace System.Data.Entity.Core.Objects;

public class ExecutionOptions
{
	internal static readonly ExecutionOptions Default = new ExecutionOptions(MergeOption.AppendOnly);

	public MergeOption MergeOption { get; private set; }

	[Obsolete("Queries are now streaming by default unless a retrying ExecutionStrategy is used. This property no longer returns an accurate value.")]
	public bool Streaming => UserSpecifiedStreaming ?? true;

	internal bool? UserSpecifiedStreaming { get; private set; }

	public ExecutionOptions(MergeOption mergeOption)
	{
		MergeOption = mergeOption;
	}

	public ExecutionOptions(MergeOption mergeOption, bool streaming)
	{
		MergeOption = mergeOption;
		UserSpecifiedStreaming = streaming;
	}

	internal ExecutionOptions(MergeOption mergeOption, bool? streaming)
	{
		MergeOption = mergeOption;
		UserSpecifiedStreaming = streaming;
	}

	public static bool operator ==(ExecutionOptions left, ExecutionOptions right)
	{
		if ((object)left == right)
		{
			return true;
		}
		return left?.Equals(right) ?? false;
	}

	public static bool operator !=(ExecutionOptions left, ExecutionOptions right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ExecutionOptions executionOptions))
		{
			return false;
		}
		if (MergeOption == executionOptions.MergeOption)
		{
			return UserSpecifiedStreaming == executionOptions.UserSpecifiedStreaming;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return MergeOption.GetHashCode() ^ UserSpecifiedStreaming.GetHashCode();
	}
}
