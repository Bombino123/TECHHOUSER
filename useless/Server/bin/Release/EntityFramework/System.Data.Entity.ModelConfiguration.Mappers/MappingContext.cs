using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Utilities;

namespace System.Data.Entity.ModelConfiguration.Mappers;

internal sealed class MappingContext
{
	private readonly System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration _modelConfiguration;

	private readonly ConventionsConfiguration _conventionsConfiguration;

	private readonly EdmModel _model;

	private readonly AttributeProvider _attributeProvider;

	private readonly DbModelBuilderVersion _modelBuilderVersion;

	public System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration ModelConfiguration => _modelConfiguration;

	public ConventionsConfiguration ConventionsConfiguration => _conventionsConfiguration;

	public EdmModel Model => _model;

	public AttributeProvider AttributeProvider => _attributeProvider;

	public DbModelBuilderVersion ModelBuilderVersion => _modelBuilderVersion;

	public MappingContext(System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration, ConventionsConfiguration conventionsConfiguration, EdmModel model, DbModelBuilderVersion modelBuilderVersion = DbModelBuilderVersion.Latest, AttributeProvider attributeProvider = null)
	{
		_modelConfiguration = modelConfiguration;
		_conventionsConfiguration = conventionsConfiguration;
		_model = model;
		_modelBuilderVersion = modelBuilderVersion;
		_attributeProvider = attributeProvider ?? new AttributeProvider();
	}
}
