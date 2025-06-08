namespace System.Data.Entity.Internal;

internal interface IPropertyValuesItem
{
	object Value { get; set; }

	string Name { get; }

	bool IsComplex { get; }

	Type Type { get; }
}
