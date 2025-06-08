using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

internal class BinaryPropertyConfiguration : LengthPropertyConfiguration
{
	public bool? IsRowVersion { get; set; }

	public BinaryPropertyConfiguration()
	{
	}

	private BinaryPropertyConfiguration(BinaryPropertyConfiguration source)
		: base(source)
	{
		IsRowVersion = source.IsRowVersion;
	}

	internal override PrimitivePropertyConfiguration Clone()
	{
		return new BinaryPropertyConfiguration(this);
	}

	protected override void ConfigureProperty(EdmProperty property)
	{
		if (IsRowVersion.HasValue && IsRowVersion.Value)
		{
			base.ConcurrencyMode = base.ConcurrencyMode ?? System.Data.Entity.Core.Metadata.Edm.ConcurrencyMode.Fixed;
			base.DatabaseGeneratedOption = base.DatabaseGeneratedOption ?? System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed;
			base.IsNullable = base.IsNullable.GetValueOrDefault();
			base.MaxLength = base.MaxLength ?? 8;
		}
		base.ConfigureProperty(property);
	}

	protected override void ConfigureColumn(EdmProperty column, EntityType table, DbProviderManifest providerManifest)
	{
		if (IsRowVersion.HasValue && IsRowVersion.Value)
		{
			base.ColumnType = base.ColumnType ?? "rowversion";
		}
		base.ConfigureColumn(column, table, providerManifest);
		if (IsRowVersion.HasValue && IsRowVersion.Value)
		{
			column.MaxLength = null;
		}
	}

	internal override void CopyFrom(PrimitivePropertyConfiguration other)
	{
		base.CopyFrom(other);
		if (other is BinaryPropertyConfiguration binaryPropertyConfiguration)
		{
			IsRowVersion = binaryPropertyConfiguration.IsRowVersion;
		}
	}

	internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		base.FillFrom(other, inCSpace);
		if (other is BinaryPropertyConfiguration binaryPropertyConfiguration && !IsRowVersion.HasValue)
		{
			IsRowVersion = binaryPropertyConfiguration.IsRowVersion;
		}
	}

	internal override void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		base.MakeCompatibleWith(other, inCSpace);
		if (other is BinaryPropertyConfiguration { IsRowVersion: not null })
		{
			IsRowVersion = null;
		}
	}

	internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
	{
		BinaryPropertyConfiguration binaryPropertyConfiguration = other as BinaryPropertyConfiguration;
		bool flag = base.IsCompatible(other, inCSpace, out errorMessage);
		bool flag2 = binaryPropertyConfiguration == null || IsCompatible((BinaryPropertyConfiguration c) => c.IsRowVersion, binaryPropertyConfiguration, ref errorMessage);
		return flag && flag2;
	}
}
