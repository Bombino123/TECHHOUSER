using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

internal abstract class DataModelValidationRuleSet
{
	private readonly List<DataModelValidationRule> _rules = new List<DataModelValidationRule>();

	protected void AddRule(DataModelValidationRule rule)
	{
		_rules.Add(rule);
	}

	protected void RemoveRule(DataModelValidationRule rule)
	{
		_rules.Remove(rule);
	}

	internal IEnumerable<DataModelValidationRule> GetRules(MetadataItem itemToValidate)
	{
		return _rules.Where((DataModelValidationRule r) => r.ValidatedType.IsInstanceOfType(itemToValidate));
	}
}
