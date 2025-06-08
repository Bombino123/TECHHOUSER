namespace System.Data.Entity.Core.Metadata.Edm;

internal interface INamedDataModelItem
{
	string Name { get; }

	string Identity { get; }
}
