using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal abstract class ConstraintBase : InternalBase
{
	internal abstract ErrorLog.Record GetErrorRecord();
}
