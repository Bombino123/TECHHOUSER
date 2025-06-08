using System.Collections.Generic;

namespace System.Data.Entity.Core.Metadata.Edm;

public class EdmFunctionPayload
{
	public string Schema { get; set; }

	public string StoreFunctionName { get; set; }

	public string CommandText { get; set; }

	public IList<EntitySet> EntitySets { get; set; }

	public bool? IsAggregate { get; set; }

	public bool? IsBuiltIn { get; set; }

	public bool? IsNiladic { get; set; }

	public bool? IsComposable { get; set; }

	public bool? IsFromProviderManifest { get; set; }

	public bool? IsCachedStoreFunction { get; set; }

	public bool? IsFunctionImport { get; set; }

	public IList<FunctionParameter> ReturnParameters { get; set; }

	public ParameterTypeSemantics? ParameterTypeSemantics { get; set; }

	public IList<FunctionParameter> Parameters { get; set; }
}
