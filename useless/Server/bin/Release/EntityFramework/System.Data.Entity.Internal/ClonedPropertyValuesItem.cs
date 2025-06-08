namespace System.Data.Entity.Internal;

internal class ClonedPropertyValuesItem : IPropertyValuesItem
{
	private readonly string _name;

	private readonly bool _isComplex;

	private readonly Type _type;

	public object Value { get; set; }

	public string Name => _name;

	public bool IsComplex => _isComplex;

	public Type Type => _type;

	public ClonedPropertyValuesItem(string name, object value, Type type, bool isComplex)
	{
		_name = name;
		_type = type;
		_isComplex = isComplex;
		Value = value;
	}
}
