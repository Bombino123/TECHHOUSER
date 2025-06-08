namespace System.Data.Entity.Core.Metadata.Edm;

[Serializable]
public class DataModelErrorEventArgs : EventArgs
{
	[NonSerialized]
	private MetadataItem _item;

	public string PropertyName { get; internal set; }

	public string ErrorMessage { get; internal set; }

	public MetadataItem Item
	{
		get
		{
			return _item;
		}
		set
		{
			_item = value;
		}
	}
}
