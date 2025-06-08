using System;
using System.Collections;

namespace AForge;

public static class PolishExpression
{
	public static double Evaluate(string expression, double[] variables)
	{
		string[] array = expression.Trim().Split(new char[1] { ' ' });
		Stack stack = new Stack();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (char.IsDigit(text[0]))
			{
				stack.Push(double.Parse(text));
				continue;
			}
			if (text[0] == '$')
			{
				stack.Push(variables[int.Parse(text.Substring(1))]);
				continue;
			}
			double num = (double)stack.Pop();
			switch (text)
			{
			case "+":
				stack.Push((double)stack.Pop() + num);
				break;
			case "-":
				stack.Push((double)stack.Pop() - num);
				break;
			case "*":
				stack.Push((double)stack.Pop() * num);
				break;
			case "/":
				stack.Push((double)stack.Pop() / num);
				break;
			case "sin":
				stack.Push(Math.Sin(num));
				break;
			case "cos":
				stack.Push(Math.Cos(num));
				break;
			case "ln":
				stack.Push(Math.Log(num));
				break;
			case "exp":
				stack.Push(Math.Exp(num));
				break;
			case "sqrt":
				stack.Push(Math.Sqrt(num));
				break;
			default:
				throw new ArgumentException("Unsupported function: " + text);
			}
		}
		if (stack.Count != 1)
		{
			throw new ArgumentException("Incorrect expression.");
		}
		return (double)stack.Pop();
	}
}
