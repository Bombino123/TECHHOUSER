using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class FunctionParameterExtensions
{
	public static object GetConfiguration(this FunctionParameter functionParameter)
	{
		return functionParameter.Annotations.GetConfiguration();
	}

	public static void SetConfiguration(this FunctionParameter functionParameter, object configuration)
	{
		functionParameter.GetMetadataProperties().SetConfiguration(configuration);
	}
}
