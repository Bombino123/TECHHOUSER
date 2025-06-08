using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Configuration.Types;

internal class ComplexTypeConfiguration : StructuralTypeConfiguration
{
	internal ComplexTypeConfiguration(Type structuralType)
		: base(structuralType)
	{
	}

	private ComplexTypeConfiguration(ComplexTypeConfiguration source)
		: base(source)
	{
	}

	internal virtual ComplexTypeConfiguration Clone()
	{
		return new ComplexTypeConfiguration(this);
	}

	internal virtual void Configure(ComplexType complexType)
	{
		Configure(complexType.Name, complexType.Properties, complexType.GetMetadataProperties());
	}
}
