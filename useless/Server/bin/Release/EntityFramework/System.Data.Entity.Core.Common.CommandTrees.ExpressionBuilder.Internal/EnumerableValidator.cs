using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.Internal;

internal sealed class EnumerableValidator<TElementIn, TElementOut, TResult>
{
	private readonly string argumentName;

	private readonly IEnumerable<TElementIn> target;

	private int expectedElementCount = -1;

	public bool AllowEmpty { get; set; }

	public int ExpectedElementCount
	{
		get
		{
			return expectedElementCount;
		}
		set
		{
			expectedElementCount = value;
		}
	}

	public Func<TElementIn, int, TElementOut> ConvertElement { get; set; }

	public Func<List<TElementOut>, TResult> CreateResult { get; set; }

	public Func<TElementIn, int, string> GetName { get; set; }

	internal EnumerableValidator(IEnumerable<TElementIn> argument, string argumentName)
	{
		this.argumentName = argumentName;
		target = argument;
	}

	internal TResult Validate()
	{
		return Validate(target, argumentName, ExpectedElementCount, AllowEmpty, ConvertElement, CreateResult, GetName);
	}

	private static TResult Validate(IEnumerable<TElementIn> argument, string argumentName, int expectedElementCount, bool allowEmpty, Func<TElementIn, int, TElementOut> map, Func<List<TElementOut>, TResult> collect, Func<TElementIn, int, string> deriveName)
	{
		bool flag = default(TElementIn) == null;
		bool flag2 = expectedElementCount != -1;
		Dictionary<string, int> dictionary = null;
		if (deriveName != null)
		{
			dictionary = new Dictionary<string, int>();
		}
		int num = 0;
		List<TElementOut> list = new List<TElementOut>();
		foreach (TElementIn item2 in argument)
		{
			if (flag2 && num == expectedElementCount)
			{
				throw new ArgumentException(Strings.Cqt_ExpressionList_IncorrectElementCount, argumentName);
			}
			if (flag && item2 == null)
			{
				throw new ArgumentNullException(StringUtil.FormatIndex(argumentName, num));
			}
			TElementOut item = map(item2, num);
			list.Add(item);
			if (deriveName != null)
			{
				string text = deriveName(item2, num);
				int value = -1;
				if (dictionary.TryGetValue(text, out value))
				{
					throw new ArgumentException(Strings.Cqt_Util_CheckListDuplicateName(value, num, text), StringUtil.FormatIndex(argumentName, num));
				}
				dictionary[text] = num;
			}
			num++;
		}
		if (flag2)
		{
			if (num != expectedElementCount)
			{
				throw new ArgumentException(Strings.Cqt_ExpressionList_IncorrectElementCount, argumentName);
			}
		}
		else if (num == 0 && !allowEmpty)
		{
			throw new ArgumentException(Strings.Cqt_Util_CheckListEmptyInvalid, argumentName);
		}
		return collect(list);
	}
}
