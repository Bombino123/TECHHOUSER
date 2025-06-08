namespace System.Data.Entity.Core.Metadata.Edm;

public sealed class Documentation : MetadataItem
{
	private string _summary = "";

	private string _longDescription = "";

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.Documentation;

	public string Summary
	{
		get
		{
			return _summary;
		}
		internal set
		{
			if (value != null)
			{
				_summary = value;
			}
			else
			{
				_summary = "";
			}
		}
	}

	public string LongDescription
	{
		get
		{
			return _longDescription;
		}
		internal set
		{
			if (value != null)
			{
				_longDescription = value;
			}
			else
			{
				_longDescription = "";
			}
		}
	}

	internal override string Identity => "Documentation";

	public bool IsEmpty
	{
		get
		{
			if (string.IsNullOrEmpty(_summary) && string.IsNullOrEmpty(_longDescription))
			{
				return true;
			}
			return false;
		}
	}

	internal Documentation()
	{
	}

	public Documentation(string summary, string longDescription)
	{
		Summary = summary;
		LongDescription = longDescription;
	}

	public override string ToString()
	{
		return _summary;
	}
}
