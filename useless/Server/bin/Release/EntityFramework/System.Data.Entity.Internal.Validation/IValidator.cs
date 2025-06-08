using System.Collections.Generic;
using System.Data.Entity.Validation;

namespace System.Data.Entity.Internal.Validation;

internal interface IValidator
{
	IEnumerable<DbValidationError> Validate(EntityValidationContext entityValidationContext, InternalMemberEntry property);
}
