using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal abstract class ConversionContext<T_Identifier>
{
	internal readonly Solver Solver = new Solver();

	internal abstract Vertex TranslateTermToVertex(TermExpr<T_Identifier> term);

	internal abstract IEnumerable<LiteralVertexPair<T_Identifier>> GetSuccessors(Vertex vertex);
}
