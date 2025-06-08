namespace System.Data.Entity.Core.Query.PlanCompiler;

internal enum PlanCompilerPhase
{
	PreProcessor,
	AggregatePushdown,
	Normalization,
	NTE,
	ProjectionPruning,
	NestPullup,
	Transformations,
	JoinElimination,
	NullSemantics,
	CodeGen,
	PostCodeGen,
	MaxMarker
}
