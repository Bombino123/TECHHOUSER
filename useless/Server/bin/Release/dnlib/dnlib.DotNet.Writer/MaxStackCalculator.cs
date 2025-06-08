using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Writer;

public struct MaxStackCalculator
{
	private IList<Instruction> instructions;

	private IList<ExceptionHandler> exceptionHandlers;

	private readonly Dictionary<Instruction, int> stackHeights;

	private bool hasError;

	private int currentMaxStack;

	public static uint GetMaxStack(IList<Instruction> instructions, IList<ExceptionHandler> exceptionHandlers)
	{
		new MaxStackCalculator(instructions, exceptionHandlers).Calculate(out var maxStack);
		return maxStack;
	}

	public static bool GetMaxStack(IList<Instruction> instructions, IList<ExceptionHandler> exceptionHandlers, out uint maxStack)
	{
		return new MaxStackCalculator(instructions, exceptionHandlers).Calculate(out maxStack);
	}

	internal static MaxStackCalculator Create()
	{
		return new MaxStackCalculator(dummy: true);
	}

	private MaxStackCalculator(bool dummy)
	{
		instructions = null;
		exceptionHandlers = null;
		stackHeights = new Dictionary<Instruction, int>();
		hasError = false;
		currentMaxStack = 0;
	}

	private MaxStackCalculator(IList<Instruction> instructions, IList<ExceptionHandler> exceptionHandlers)
	{
		this.instructions = instructions;
		this.exceptionHandlers = exceptionHandlers;
		stackHeights = new Dictionary<Instruction, int>();
		hasError = false;
		currentMaxStack = 0;
	}

	internal void Reset(IList<Instruction> instructions, IList<ExceptionHandler> exceptionHandlers)
	{
		this.instructions = instructions;
		this.exceptionHandlers = exceptionHandlers;
		stackHeights.Clear();
		hasError = false;
		currentMaxStack = 0;
	}

	internal bool Calculate(out uint maxStack)
	{
		IList<ExceptionHandler> list = exceptionHandlers;
		Dictionary<Instruction, int> dictionary = stackHeights;
		for (int i = 0; i < list.Count; i++)
		{
			ExceptionHandler exceptionHandler = list[i];
			if (exceptionHandler == null)
			{
				continue;
			}
			Instruction tryStart;
			if ((tryStart = exceptionHandler.TryStart) != null)
			{
				dictionary[tryStart] = 0;
			}
			if ((tryStart = exceptionHandler.FilterStart) != null)
			{
				dictionary[tryStart] = 1;
				currentMaxStack = 1;
			}
			if ((tryStart = exceptionHandler.HandlerStart) != null)
			{
				if (exceptionHandler.IsCatch || exceptionHandler.IsFilter)
				{
					dictionary[tryStart] = 1;
					currentMaxStack = 1;
				}
				else
				{
					dictionary[tryStart] = 0;
				}
			}
		}
		int value = 0;
		bool flag = false;
		IList<Instruction> list2 = instructions;
		for (int j = 0; j < list2.Count; j++)
		{
			Instruction instruction = list2[j];
			if (instruction == null)
			{
				continue;
			}
			if (flag)
			{
				dictionary.TryGetValue(instruction, out value);
				flag = false;
			}
			value = WriteStack(instruction, value);
			OpCode opCode = instruction.OpCode;
			Code code = opCode.Code;
			if (code == Code.Jmp)
			{
				if (value != 0)
				{
					hasError = true;
				}
			}
			else
			{
				instruction.CalculateStackUsage(out var pushes, out var pops);
				if (pops == -1)
				{
					value = 0;
				}
				else
				{
					value -= pops;
					if (value < 0)
					{
						hasError = true;
						value = 0;
					}
					value += pushes;
				}
			}
			if (value < 0)
			{
				hasError = true;
				value = 0;
			}
			switch (opCode.FlowControl)
			{
			case FlowControl.Branch:
				WriteStack(instruction.Operand as Instruction, value);
				flag = true;
				break;
			case FlowControl.Call:
				if (code == Code.Jmp)
				{
					flag = true;
				}
				break;
			case FlowControl.Cond_Branch:
				if (code == Code.Switch)
				{
					if (instruction.Operand is IList<Instruction> list3)
					{
						for (int k = 0; k < list3.Count; k++)
						{
							WriteStack(list3[k], value);
						}
					}
				}
				else
				{
					WriteStack(instruction.Operand as Instruction, value);
				}
				break;
			case FlowControl.Return:
			case FlowControl.Throw:
				flag = true;
				break;
			}
		}
		maxStack = (uint)currentMaxStack;
		return !hasError;
	}

	private int WriteStack(Instruction instr, int stack)
	{
		if (instr == null)
		{
			hasError = true;
			return stack;
		}
		Dictionary<Instruction, int> dictionary = stackHeights;
		if (dictionary.TryGetValue(instr, out var value))
		{
			if (stack != value)
			{
				hasError = true;
			}
			return value;
		}
		dictionary[instr] = stack;
		if (stack > currentMaxStack)
		{
			currentMaxStack = stack;
		}
		return stack;
	}
}
