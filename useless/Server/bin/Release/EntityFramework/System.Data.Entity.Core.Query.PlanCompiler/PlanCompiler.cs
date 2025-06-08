using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class PlanCompiler
{
	private static readonly BooleanSwitch _legacyApplyTransformationsRegardlessOfSize = new BooleanSwitch("System.Data.Entity.Core.EntityClient.IgnoreOptimizationLimit", "The Entity Framework should try to optimize the query regardless of its size");

	private static bool? _applyTransformationsRegardlessOfSize;

	private static bool? _disableTransformationsRegardlessOfSize;

	private static int? _maxNodeCountForTransformations;

	private readonly DbCommandTree m_ctree;

	private Command m_command;

	private PlanCompilerPhase m_phase;

	private int _precedingPhases;

	private int m_neededPhases;

	private ConstraintManager m_constraintManager;

	private bool? m_mayApplyTransformationRules;

	private static bool applyTransformationsRegardlessOfSize
	{
		get
		{
			if (!_applyTransformationsRegardlessOfSize.HasValue)
			{
				string value = ConfigurationManager.AppSettings["EntityFramework_EntityClient_IgnoreOptimizationLimit"];
				_applyTransformationsRegardlessOfSize = bool.TryParse(value, out var result) && result;
			}
			return _applyTransformationsRegardlessOfSize.Value;
		}
	}

	private static bool disableTransformationsRegardlessOfSize
	{
		get
		{
			if (!_disableTransformationsRegardlessOfSize.HasValue)
			{
				string value = ConfigurationManager.AppSettings["EntityFramework_EntityClient_DisableOptimization"];
				_disableTransformationsRegardlessOfSize = bool.TryParse(value, out var result) && result;
			}
			return _disableTransformationsRegardlessOfSize.Value;
		}
	}

	private static int maxNodeCountForTransformations
	{
		get
		{
			if (!_maxNodeCountForTransformations.HasValue)
			{
				string s = ConfigurationManager.AppSettings["EntityFramework_EntityClient_MaxNodeCountForTransformations"];
				_maxNodeCountForTransformations = (int.TryParse(s, out var result) ? result : 10000);
			}
			return _maxNodeCountForTransformations.Value;
		}
	}

	internal Command Command => m_command;

	internal bool HasSortingOnNullSentinels { get; set; }

	internal ConstraintManager ConstraintManager
	{
		get
		{
			if (m_constraintManager == null)
			{
				m_constraintManager = new ConstraintManager();
			}
			return m_constraintManager;
		}
	}

	internal bool DisableFilterOverProjectionSimplificationForCustomFunctions => m_ctree.DisableFilterOverProjectionSimplificationForCustomFunctions;

	internal MetadataWorkspace MetadataWorkspace => m_ctree.MetadataWorkspace;

	private bool MayApplyTransformationRules
	{
		get
		{
			if (!m_mayApplyTransformationRules.HasValue)
			{
				m_mayApplyTransformationRules = ComputeMayApplyTransformations();
			}
			return m_mayApplyTransformationRules.Value;
		}
	}

	internal bool TransformationsDeferred { get; set; }

	private PlanCompiler(DbCommandTree ctree)
	{
		m_ctree = ctree;
	}

	internal static void Assert(bool condition, string message)
	{
		if (!condition)
		{
			throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.AssertionFailed, 0, message);
		}
	}

	internal static void Compile(DbCommandTree ctree, out List<ProviderCommandInfo> providerCommands, out ColumnMap resultColumnMap, out int columnCount, out Set<EntitySet> entitySets)
	{
		Assert(ctree != null, "Expected a valid, non-null Command Tree input");
		new PlanCompiler(ctree).Compile(out providerCommands, out resultColumnMap, out columnCount, out entitySets);
	}

	internal bool IsPhaseNeeded(PlanCompilerPhase phase)
	{
		return (m_neededPhases & (1 << (int)phase)) != 0;
	}

	internal void MarkPhaseAsNeeded(PlanCompilerPhase phase)
	{
		m_neededPhases |= 1 << (int)phase;
	}

	internal bool IsAfterPhase(PlanCompilerPhase phase)
	{
		return (_precedingPhases & (1 << (int)phase)) != 0;
	}

	private void Compile(out List<ProviderCommandInfo> providerCommands, out ColumnMap resultColumnMap, out int columnCount, out Set<EntitySet> entitySets)
	{
		Initialize();
		_ = string.Empty;
		_ = string.Empty;
		_ = string.Empty;
		_ = string.Empty;
		_ = string.Empty;
		_ = string.Empty;
		_ = string.Empty;
		string dumpString = string.Empty;
		_ = string.Empty;
		string dumpString2 = string.Empty;
		_ = string.Empty;
		string dumpString3 = string.Empty;
		_ = string.Empty;
		string dumpString4 = string.Empty;
		_ = string.Empty;
		m_neededPhases = 593;
		SwitchToPhase(PlanCompilerPhase.PreProcessor);
		PreProcessor.Process(this, out var typeInfo, out var tvfResultKeys);
		entitySets = typeInfo.GetEntitySets();
		if (IsPhaseNeeded(PlanCompilerPhase.AggregatePushdown))
		{
			SwitchToPhase(PlanCompilerPhase.AggregatePushdown);
			AggregatePushdown.Process(this);
		}
		if (IsPhaseNeeded(PlanCompilerPhase.Normalization))
		{
			SwitchToPhase(PlanCompilerPhase.Normalization);
			Normalizer.Process(this);
		}
		if (IsPhaseNeeded(PlanCompilerPhase.NTE))
		{
			SwitchToPhase(PlanCompilerPhase.NTE);
			NominalTypeEliminator.Process(this, typeInfo, tvfResultKeys);
		}
		if (IsPhaseNeeded(PlanCompilerPhase.ProjectionPruning))
		{
			SwitchToPhase(PlanCompilerPhase.ProjectionPruning);
			ProjectionPruner.Process(this);
		}
		if (IsPhaseNeeded(PlanCompilerPhase.NestPullup))
		{
			SwitchToPhase(PlanCompilerPhase.NestPullup);
			NestPullup.Process(this);
			SwitchToPhase(PlanCompilerPhase.ProjectionPruning);
			ProjectionPruner.Process(this);
		}
		if (IsPhaseNeeded(PlanCompilerPhase.Transformations) && ApplyTransformations(ref dumpString, TransformationRulesGroup.All))
		{
			SwitchToPhase(PlanCompilerPhase.ProjectionPruning);
			ProjectionPruner.Process(this);
			ApplyTransformations(ref dumpString2, TransformationRulesGroup.Project);
		}
		if (IsPhaseNeeded(PlanCompilerPhase.NullSemantics))
		{
			SwitchToPhase(PlanCompilerPhase.NullSemantics);
			if (!m_ctree.UseDatabaseNullSemantics && NullSemantics.Process(Command))
			{
				ApplyTransformations(ref dumpString3, TransformationRulesGroup.NullSemantics);
			}
		}
		if (IsPhaseNeeded(PlanCompilerPhase.JoinElimination))
		{
			for (int i = 0; i < 10; i++)
			{
				SwitchToPhase(PlanCompilerPhase.JoinElimination);
				if (!JoinElimination.Process(this) && !TransformationsDeferred)
				{
					break;
				}
				TransformationsDeferred = false;
				ApplyTransformations(ref dumpString4, TransformationRulesGroup.PostJoinElimination);
			}
		}
		SwitchToPhase(PlanCompilerPhase.CodeGen);
		CodeGen.Process(this, out providerCommands, out resultColumnMap, out columnCount);
	}

	private bool ApplyTransformations(ref string dumpString, TransformationRulesGroup rulesGroup)
	{
		if (MayApplyTransformationRules)
		{
			dumpString = SwitchToPhase(PlanCompilerPhase.Transformations);
			return TransformationRules.Process(this, rulesGroup);
		}
		return false;
	}

	private string SwitchToPhase(PlanCompilerPhase newPhase)
	{
		string empty = string.Empty;
		if (newPhase != m_phase)
		{
			_precedingPhases |= 1 << (int)m_phase;
		}
		m_phase = newPhase;
		return empty;
	}

	private bool ComputeMayApplyTransformations()
	{
		if (disableTransformationsRegardlessOfSize)
		{
			return false;
		}
		if (applyTransformationsRegardlessOfSize || _legacyApplyTransformationsRegardlessOfSize.Enabled || m_command.NextNodeId < maxNodeCountForTransformations)
		{
			return true;
		}
		return NodeCounter.Count(m_command.Root) < maxNodeCountForTransformations;
	}

	private void Initialize()
	{
		DbQueryCommandTree dbQueryCommandTree = m_ctree as DbQueryCommandTree;
		Assert(dbQueryCommandTree != null, "Unexpected command tree kind. Only query command tree is supported.");
		m_command = ITreeGenerator.Generate(dbQueryCommandTree);
		Assert(m_command != null, "Unable to generate internal tree from Command Tree");
	}
}
