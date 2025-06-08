using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class CodeGen
{
	private readonly PlanCompiler m_compilerState;

	private List<Node> m_subCommands;

	private Command Command => m_compilerState.Command;

	internal static void Process(PlanCompiler compilerState, out List<ProviderCommandInfo> childCommands, out ColumnMap resultColumnMap, out int columnCount)
	{
		new CodeGen(compilerState).Process(out childCommands, out resultColumnMap, out columnCount);
	}

	private CodeGen(PlanCompiler compilerState)
	{
		m_compilerState = compilerState;
	}

	private void Process(out List<ProviderCommandInfo> childCommands, out ColumnMap resultColumnMap, out int columnCount)
	{
		PhysicalProjectOp physicalProjectOp = (PhysicalProjectOp)Command.Root.Op;
		m_subCommands = new List<Node>(new Node[1] { Command.Root });
		childCommands = new List<ProviderCommandInfo>(new ProviderCommandInfo[1] { ProviderCommandInfoUtils.Create(Command, Command.Root) });
		resultColumnMap = BuildResultColumnMap(physicalProjectOp);
		columnCount = physicalProjectOp.Outputs.Count;
	}

	private ColumnMap BuildResultColumnMap(PhysicalProjectOp projectOp)
	{
		Dictionary<Var, KeyValuePair<int, int>> varToCommandColumnMap = BuildVarMap();
		return ColumnMapTranslator.Translate(projectOp.ColumnMap, varToCommandColumnMap);
	}

	private Dictionary<Var, KeyValuePair<int, int>> BuildVarMap()
	{
		Dictionary<Var, KeyValuePair<int, int>> dictionary = new Dictionary<Var, KeyValuePair<int, int>>();
		int num = 0;
		foreach (Node subCommand in m_subCommands)
		{
			PhysicalProjectOp obj = (PhysicalProjectOp)subCommand.Op;
			int num2 = 0;
			foreach (Var output in obj.Outputs)
			{
				KeyValuePair<int, int> value = new KeyValuePair<int, int>(num, num2);
				dictionary[output] = value;
				num2++;
			}
			num++;
		}
		return dictionary;
	}
}
