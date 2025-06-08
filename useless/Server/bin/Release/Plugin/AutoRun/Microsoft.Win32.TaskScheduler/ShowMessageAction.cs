using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Properties;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlType(IncludeInSchema = true)]
[XmlRoot("ShowMessage", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = false)]
[ComVisible(true)]
public sealed class ShowMessageAction : Action, IBindAsExecAction
{
	[XmlElement("Body")]
	[DefaultValue(null)]
	public string MessageBody
	{
		get
		{
			return GetProperty<string, IShowMessageAction>("MessageBody");
		}
		set
		{
			SetProperty<string, IShowMessageAction>("MessageBody", value);
		}
	}

	[DefaultValue(null)]
	public string Title
	{
		get
		{
			return GetProperty<string, IShowMessageAction>("Title");
		}
		set
		{
			SetProperty<string, IShowMessageAction>("Title", value);
		}
	}

	internal override TaskActionType InternalActionType => TaskActionType.ShowMessage;

	public ShowMessageAction()
	{
	}

	public ShowMessageAction([CanBeNull] string messageBody, [CanBeNull] string title)
	{
		MessageBody = messageBody;
		Title = title;
	}

	internal ShowMessageAction([NotNull] ITask task)
		: base(task)
	{
	}

	internal ShowMessageAction([NotNull] IAction action)
		: base(action)
	{
	}

	public override bool Equals(Action other)
	{
		if (base.Equals(other) && string.Equals(Title, (other as ShowMessageAction)?.Title))
		{
			return string.Equals(MessageBody, (other as ShowMessageAction)?.MessageBody);
		}
		return false;
	}

	public override string ToString()
	{
		return string.Format(Resources.ShowMessageAction, Title, MessageBody, Id);
	}

	internal static Action FromPowerShellCommand(string p)
	{
		Match match = Regex.Match(p, "^\\[System.Reflection.Assembly\\]::LoadWithPartialName\\('System.Windows.Forms'\\); \\[System.Windows.Forms.MessageBox\\]::Show\\('(?<msg>(?:[^']|'')*)'(?:,'(?<t>(?:[^']|'')*)')?\\);?\\s*$");
		if (!match.Success)
		{
			return null;
		}
		return new ShowMessageAction(match.Groups["msg"].Value.Replace("''", "'"), match.Groups["t"].Success ? match.Groups["t"].Value.Replace("''", "'") : null);
	}

	internal override void CopyProperties(Action sourceAction)
	{
		if (sourceAction.GetType() == GetType())
		{
			base.CopyProperties(sourceAction);
			Title = ((ShowMessageAction)sourceAction).Title;
			MessageBody = ((ShowMessageAction)sourceAction).MessageBody;
		}
	}

	internal override void CreateV2Action(IActionCollection iActions)
	{
		iAction = iActions.Create(TaskActionType.ShowMessage);
	}

	internal override string GetPowerShellCommand()
	{
		StringBuilder stringBuilder = new StringBuilder("[System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms'); [System.Windows.Forms.MessageBox]::Show('");
		stringBuilder.Append(MessageBody.Replace("'", "''"));
		if (Title != null)
		{
			stringBuilder.Append("','");
			stringBuilder.Append(Title.Replace("'", "''"));
		}
		stringBuilder.Append("'); ");
		return stringBuilder.ToString();
	}
}
