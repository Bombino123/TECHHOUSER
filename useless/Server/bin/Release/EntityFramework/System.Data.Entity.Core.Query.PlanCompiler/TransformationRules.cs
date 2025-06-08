using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class TransformationRules
{
	internal static readonly ReadOnlyCollection<ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>> AllRulesTable = BuildLookupTableForRules(AllRules);

	internal static readonly ReadOnlyCollection<ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>> ProjectRulesTable = BuildLookupTableForRules(ProjectOpRules.Rules);

	internal static readonly ReadOnlyCollection<ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>> PostJoinEliminationRulesTable = BuildLookupTableForRules(PostJoinEliminationRules);

	internal static readonly ReadOnlyCollection<ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>> NullabilityRulesTable = BuildLookupTableForRules(NullabilityRules);

	internal static readonly HashSet<System.Data.Entity.Core.Query.InternalTrees.Rule> RulesRequiringProjectionPruning = InitializeRulesRequiringProjectionPruning();

	internal static readonly HashSet<System.Data.Entity.Core.Query.InternalTrees.Rule> RulesRequiringNullabilityRulesToBeReapplied = InitializeRulesRequiringNullabilityRulesToBeReapplied();

	internal static readonly ReadOnlyCollection<ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>> NullSemanticsRulesTable = BuildLookupTableForRules(NullSemanticsRules);

	private static List<System.Data.Entity.Core.Query.InternalTrees.Rule> allRules;

	private static List<System.Data.Entity.Core.Query.InternalTrees.Rule> postJoinEliminationRules;

	private static List<System.Data.Entity.Core.Query.InternalTrees.Rule> nullabilityRules;

	private static List<System.Data.Entity.Core.Query.InternalTrees.Rule> nullSemanticsRules;

	private static List<System.Data.Entity.Core.Query.InternalTrees.Rule> AllRules
	{
		get
		{
			if (allRules == null)
			{
				allRules = new List<System.Data.Entity.Core.Query.InternalTrees.Rule>();
				allRules.AddRange(ScalarOpRules.Rules);
				allRules.AddRange(FilterOpRules.Rules);
				allRules.AddRange(ProjectOpRules.Rules);
				allRules.AddRange(ApplyOpRules.Rules);
				allRules.AddRange(JoinOpRules.Rules);
				allRules.AddRange(SingleRowOpRules.Rules);
				allRules.AddRange(SetOpRules.Rules);
				allRules.AddRange(GroupByOpRules.Rules);
				allRules.AddRange(SortOpRules.Rules);
				allRules.AddRange(ConstrainedSortOpRules.Rules);
				allRules.AddRange(DistinctOpRules.Rules);
			}
			return allRules;
		}
	}

	private static List<System.Data.Entity.Core.Query.InternalTrees.Rule> PostJoinEliminationRules
	{
		get
		{
			if (postJoinEliminationRules == null)
			{
				postJoinEliminationRules = new List<System.Data.Entity.Core.Query.InternalTrees.Rule>();
				postJoinEliminationRules.AddRange(ProjectOpRules.Rules);
				postJoinEliminationRules.AddRange(DistinctOpRules.Rules);
				postJoinEliminationRules.AddRange(FilterOpRules.Rules);
				postJoinEliminationRules.AddRange(ApplyOpRules.Rules);
				postJoinEliminationRules.AddRange(JoinOpRules.Rules);
				postJoinEliminationRules.AddRange(NullabilityRules);
			}
			return postJoinEliminationRules;
		}
	}

	private static List<System.Data.Entity.Core.Query.InternalTrees.Rule> NullabilityRules
	{
		get
		{
			if (nullabilityRules == null)
			{
				nullabilityRules = new List<System.Data.Entity.Core.Query.InternalTrees.Rule>();
				nullabilityRules.Add(ScalarOpRules.Rule_IsNullOverVarRef);
				nullabilityRules.Add(ScalarOpRules.Rule_AndOverConstantPred1);
				nullabilityRules.Add(ScalarOpRules.Rule_AndOverConstantPred2);
				nullabilityRules.Add(ScalarOpRules.Rule_SimplifyCase);
				nullabilityRules.Add(ScalarOpRules.Rule_NotOverConstantPred);
			}
			return nullabilityRules;
		}
	}

	private static List<System.Data.Entity.Core.Query.InternalTrees.Rule> NullSemanticsRules
	{
		get
		{
			if (nullSemanticsRules == null)
			{
				nullSemanticsRules = new List<System.Data.Entity.Core.Query.InternalTrees.Rule>();
				nullSemanticsRules.Add(ScalarOpRules.Rule_IsNullOverAnything);
				nullSemanticsRules.Add(ScalarOpRules.Rule_NullCast);
				nullSemanticsRules.Add(ScalarOpRules.Rule_EqualsOverConstant);
				nullSemanticsRules.Add(ScalarOpRules.Rule_AndOverConstantPred1);
				nullSemanticsRules.Add(ScalarOpRules.Rule_AndOverConstantPred2);
				nullSemanticsRules.Add(ScalarOpRules.Rule_OrOverConstantPred1);
				nullSemanticsRules.Add(ScalarOpRules.Rule_OrOverConstantPred2);
				nullSemanticsRules.Add(ScalarOpRules.Rule_NotOverConstantPred);
				nullSemanticsRules.Add(ScalarOpRules.Rule_LikeOverConstants);
				nullSemanticsRules.Add(ScalarOpRules.Rule_SimplifyCase);
				nullSemanticsRules.Add(ScalarOpRules.Rule_FlattenCase);
			}
			return nullSemanticsRules;
		}
	}

	private static ReadOnlyCollection<ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>> BuildLookupTableForRules(IEnumerable<System.Data.Entity.Core.Query.InternalTrees.Rule> rules)
	{
		ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule> readOnlyCollection = new ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>(new System.Data.Entity.Core.Query.InternalTrees.Rule[0]);
		List<System.Data.Entity.Core.Query.InternalTrees.Rule>[] array = new List<System.Data.Entity.Core.Query.InternalTrees.Rule>[73];
		foreach (System.Data.Entity.Core.Query.InternalTrees.Rule rule in rules)
		{
			List<System.Data.Entity.Core.Query.InternalTrees.Rule> list = array[(int)rule.RuleOpType];
			if (list == null)
			{
				list = new List<System.Data.Entity.Core.Query.InternalTrees.Rule>();
				array[(int)rule.RuleOpType] = list;
			}
			list.Add(rule);
		}
		ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>[] array2 = new ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				array2[i] = new ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>(array[i].ToArray());
			}
			else
			{
				array2[i] = readOnlyCollection;
			}
		}
		return new ReadOnlyCollection<ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>>(array2);
	}

	private static HashSet<System.Data.Entity.Core.Query.InternalTrees.Rule> InitializeRulesRequiringProjectionPruning()
	{
		return new HashSet<System.Data.Entity.Core.Query.InternalTrees.Rule>
		{
			ApplyOpRules.Rule_OuterApplyOverProject,
			JoinOpRules.Rule_CrossJoinOverProject1,
			JoinOpRules.Rule_CrossJoinOverProject2,
			JoinOpRules.Rule_InnerJoinOverProject1,
			JoinOpRules.Rule_InnerJoinOverProject2,
			JoinOpRules.Rule_OuterJoinOverProject2,
			ProjectOpRules.Rule_ProjectWithNoLocalDefs,
			FilterOpRules.Rule_FilterOverProject,
			FilterOpRules.Rule_FilterWithConstantPredicate,
			GroupByOpRules.Rule_GroupByOverProject,
			GroupByOpRules.Rule_GroupByOpWithSimpleVarRedefinitions
		};
	}

	private static HashSet<System.Data.Entity.Core.Query.InternalTrees.Rule> InitializeRulesRequiringNullabilityRulesToBeReapplied()
	{
		return new HashSet<System.Data.Entity.Core.Query.InternalTrees.Rule> { FilterOpRules.Rule_FilterOverLeftOuterJoin };
	}

	internal static bool Process(PlanCompiler compilerState, TransformationRulesGroup rulesGroup)
	{
		ReadOnlyCollection<ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>> rulesTable = null;
		switch (rulesGroup)
		{
		case TransformationRulesGroup.All:
			rulesTable = AllRulesTable;
			break;
		case TransformationRulesGroup.PostJoinElimination:
			rulesTable = PostJoinEliminationRulesTable;
			break;
		case TransformationRulesGroup.Project:
			rulesTable = ProjectRulesTable;
			break;
		case TransformationRulesGroup.NullSemantics:
			rulesTable = NullSemanticsRulesTable;
			break;
		}
		if (Process(compilerState, rulesTable, out var projectionPruningRequired))
		{
			Process(compilerState, NullabilityRulesTable, out var projectionPruningRequired2);
			return projectionPruningRequired || projectionPruningRequired2;
		}
		return projectionPruningRequired;
	}

	private static bool Process(PlanCompiler compilerState, ReadOnlyCollection<ReadOnlyCollection<System.Data.Entity.Core.Query.InternalTrees.Rule>> rulesTable, out bool projectionPruningRequired)
	{
		RuleProcessor ruleProcessor = new RuleProcessor();
		TransformationRulesContext transformationRulesContext = new TransformationRulesContext(compilerState);
		compilerState.Command.Root = ruleProcessor.ApplyRulesToSubtree(transformationRulesContext, rulesTable, compilerState.Command.Root);
		projectionPruningRequired = transformationRulesContext.ProjectionPruningRequired;
		return transformationRulesContext.ReapplyNullabilityRules;
	}
}
