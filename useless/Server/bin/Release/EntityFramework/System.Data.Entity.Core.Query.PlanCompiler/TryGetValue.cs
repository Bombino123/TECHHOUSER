using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal delegate bool TryGetValue(Node key, out Node value);
