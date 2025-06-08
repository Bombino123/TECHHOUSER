using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace System.Data.Entity.Infrastructure;

public class ModelNamespaceConvention : Convention
{
	private readonly string _modelNamespace;

	internal ModelNamespaceConvention(string modelNamespace)
	{
		_modelNamespace = modelNamespace;
	}

	internal override void ApplyModelConfiguration(System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		base.ApplyModelConfiguration(modelConfiguration);
		modelConfiguration.ModelNamespace = _modelNamespace;
	}
}
