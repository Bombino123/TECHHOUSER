using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Mail;
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
[XmlRoot("SendEmail", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = false)]
[ComVisible(true)]
public sealed class EmailAction : Action, IBindAsExecAction
{
	private const string ImportanceHeader = "Importance";

	private NamedValueCollection nvc;

	private bool validateAttachments = true;

	[XmlArray("Attachments", IsNullable = true)]
	[XmlArrayItem("File", typeof(string))]
	[DefaultValue(null)]
	public object[] Attachments
	{
		get
		{
			return GetProperty<object[], IEmailAction>("Attachments");
		}
		set
		{
			if (value != null)
			{
				if (value.Length > 8)
				{
					throw new ArgumentOutOfRangeException("Attachments", "Attachments array cannot contain more than 8 items.");
				}
				if (validateAttachments)
				{
					foreach (object obj in value)
					{
						if (!(obj is string) || !File.Exists((string)obj))
						{
							throw new ArgumentException("Each value of the array must contain a valid file reference.", "Attachments");
						}
					}
				}
			}
			if (iAction == null && (value == null || value.Length == 0))
			{
				unboundValues.Remove("Attachments");
				OnPropertyChanged("Attachments");
			}
			else
			{
				SetProperty<object[], IEmailAction>("Attachments", value);
			}
		}
	}

	[DefaultValue(null)]
	public string Bcc
	{
		get
		{
			return GetProperty<string, IEmailAction>("Bcc");
		}
		set
		{
			SetProperty<string, IEmailAction>("Bcc", value);
		}
	}

	[DefaultValue(null)]
	public string Body
	{
		get
		{
			return GetProperty<string, IEmailAction>("Body");
		}
		set
		{
			SetProperty<string, IEmailAction>("Body", value);
		}
	}

	[DefaultValue(null)]
	public string Cc
	{
		get
		{
			return GetProperty<string, IEmailAction>("Cc");
		}
		set
		{
			SetProperty<string, IEmailAction>("Cc", value);
		}
	}

	[DefaultValue(null)]
	public string From
	{
		get
		{
			return GetProperty<string, IEmailAction>("From");
		}
		set
		{
			SetProperty<string, IEmailAction>("From", value);
		}
	}

	[XmlArray]
	[XmlArrayItem("HeaderField", typeof(NameValuePair))]
	[NotNull]
	public NamedValueCollection HeaderFields
	{
		get
		{
			if (nvc == null)
			{
				nvc = ((iAction == null) ? new NamedValueCollection() : new NamedValueCollection(((IEmailAction)iAction).HeaderFields));
				nvc.AttributedXmlFormat = false;
				nvc.CollectionChanged += delegate
				{
					OnPropertyChanged("HeaderFields");
				};
			}
			return nvc;
		}
	}

	[XmlIgnore]
	[DefaultValue(typeof(MailPriority), "Normal")]
	public MailPriority Priority
	{
		get
		{
			if (nvc != null && HeaderFields.TryGetValue("Importance", out var value))
			{
				return Action.TryParse(value, MailPriority.Normal);
			}
			return MailPriority.Normal;
		}
		set
		{
			HeaderFields["Importance"] = value.ToString();
		}
	}

	[DefaultValue(null)]
	public string ReplyTo
	{
		get
		{
			return GetProperty<string, IEmailAction>("ReplyTo");
		}
		set
		{
			SetProperty<string, IEmailAction>("ReplyTo", value);
		}
	}

	[DefaultValue(null)]
	public string Server
	{
		get
		{
			return GetProperty<string, IEmailAction>("Server");
		}
		set
		{
			SetProperty<string, IEmailAction>("Server", value);
		}
	}

	[DefaultValue(null)]
	public string Subject
	{
		get
		{
			return GetProperty<string, IEmailAction>("Subject");
		}
		set
		{
			SetProperty<string, IEmailAction>("Subject", value);
		}
	}

	[DefaultValue(null)]
	public string To
	{
		get
		{
			return GetProperty<string, IEmailAction>("To");
		}
		set
		{
			SetProperty<string, IEmailAction>("To", value);
		}
	}

	internal override TaskActionType InternalActionType => TaskActionType.SendEmail;

	public EmailAction()
	{
	}

	public EmailAction([CanBeNull] string subject, [NotNull] string from, [NotNull] string to, [CanBeNull] string body, [NotNull] string mailServer)
	{
		Subject = subject;
		From = from;
		To = to;
		Body = body;
		Server = mailServer;
	}

	internal EmailAction([NotNull] ITask task)
		: base(task)
	{
	}

	internal EmailAction([NotNull] IAction action)
		: base(action)
	{
	}

	public override bool Equals(Action other)
	{
		if (base.Equals(other))
		{
			return GetPowerShellCommand() == other.GetPowerShellCommand();
		}
		return false;
	}

	public override string ToString()
	{
		return string.Format(Resources.EmailAction, Subject, To, Cc, Bcc, From, ReplyTo, Body, Server, Id);
	}

	internal static Action FromPowerShellCommand(string p)
	{
		Match match = Regex.Match(p, "^Send-MailMessage -From '(?<from>(?:[^']|'')*)' -Subject '(?<subject>(?:[^']|'')*)' -SmtpServer '(?<server>(?:[^']|'')*)'(?: -Encoding UTF8)?(?: -To (?<to>'(?:(?:[^']|'')*)'(?:, '(?:(?:[^']|'')*)')*))?(?: -Cc (?<cc>'(?:(?:[^']|'')*)'(?:, '(?:(?:[^']|'')*)')*))?(?: -Bcc (?<bcc>'(?:(?:[^']|'')*)'(?:, '(?:(?:[^']|'')*)')*))?(?:(?: -BodyAsHtml)? -Body '(?<body>(?:[^']|'')*)')?(?: -Attachments (?<att>'(?:(?:[^']|'')*)'(?:, '(?:(?:[^']|'')*)')*))?(?: -Priority (?<imp>High|Normal|Low))?;?\\s*$");
		if (match.Success)
		{
			EmailAction emailAction = new EmailAction(UnPrep(FromUTF8(match.Groups["subject"].Value)), UnPrep(match.Groups["from"].Value), FromPS(match.Groups["to"]), UnPrep(FromUTF8(match.Groups["body"].Value)), UnPrep(match.Groups["server"].Value))
			{
				Cc = FromPS(match.Groups["cc"]),
				Bcc = FromPS(match.Groups["bcc"])
			};
			emailAction.validateAttachments = false;
			if (match.Groups["att"].Success)
			{
				emailAction.Attachments = Array.ConvertAll(FromPS(match.Groups["att"].Value), (Converter<string, object>)((string s) => s));
			}
			emailAction.validateAttachments = true;
			if (match.Groups["imp"].Success)
			{
				emailAction.HeaderFields["Importance"] = match.Groups["imp"].Value;
			}
			return emailAction;
		}
		return null;
	}

	internal override void Bind(ITaskDefinition iTaskDef)
	{
		base.Bind(iTaskDef);
		nvc?.Bind(((IEmailAction)iAction).HeaderFields);
	}

	internal override void CopyProperties(Action sourceAction)
	{
		if (sourceAction.GetType() == GetType())
		{
			base.CopyProperties(sourceAction);
			if (((EmailAction)sourceAction).Attachments != null)
			{
				Attachments = (object[])((EmailAction)sourceAction).Attachments.Clone();
			}
			Bcc = ((EmailAction)sourceAction).Bcc;
			Body = ((EmailAction)sourceAction).Body;
			Cc = ((EmailAction)sourceAction).Cc;
			From = ((EmailAction)sourceAction).From;
			if (((EmailAction)sourceAction).nvc != null)
			{
				((EmailAction)sourceAction).HeaderFields.CopyTo(HeaderFields);
			}
			ReplyTo = ((EmailAction)sourceAction).ReplyTo;
			Server = ((EmailAction)sourceAction).Server;
			Subject = ((EmailAction)sourceAction).Subject;
			To = ((EmailAction)sourceAction).To;
		}
	}

	internal override void CreateV2Action(IActionCollection iActions)
	{
		iAction = iActions.Create(TaskActionType.SendEmail);
	}

	internal override string GetPowerShellCommand()
	{
		bool num = Body != null && Body.Trim().StartsWith("<") && Body.Trim().EndsWith(">");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Send-MailMessage -From '{0}' -Subject '{1}' -SmtpServer '{2}' -Encoding UTF8", Prep(From), ToUTF8(Prep(Subject)), Prep(Server));
		if (!string.IsNullOrEmpty(To))
		{
			stringBuilder.AppendFormat(" -To {0}", ToPS(To));
		}
		if (!string.IsNullOrEmpty(Cc))
		{
			stringBuilder.AppendFormat(" -Cc {0}", ToPS(Cc));
		}
		if (!string.IsNullOrEmpty(Bcc))
		{
			stringBuilder.AppendFormat(" -Bcc {0}", ToPS(Bcc));
		}
		if (num)
		{
			stringBuilder.Append(" -BodyAsHtml");
		}
		if (!string.IsNullOrEmpty(Body))
		{
			stringBuilder.AppendFormat(" -Body '{0}'", ToUTF8(Prep(Body)));
		}
		if (Attachments != null && Attachments.Length != 0)
		{
			stringBuilder.AppendFormat(" -Attachments {0}", ToPS(Array.ConvertAll(Attachments, (object o) => Prep(o.ToString()))));
		}
		List<string> list = new List<string>(HeaderFields.Names);
		if (list.Contains("Importance"))
		{
			MailPriority priority = Priority;
			if (priority != 0)
			{
				stringBuilder.Append($" -Priority {priority}");
			}
			list.Remove("Importance");
		}
		if (list.Count > 0)
		{
			throw new InvalidOperationException("Under Windows 8 and later, EmailAction objects are converted to PowerShell. This action contains headers that are not supported.");
		}
		stringBuilder.Append("; ");
		return stringBuilder.ToString();
	}

	private static string[] FromPS(string p)
	{
		return Array.ConvertAll(p.Split(new string[1] { ", " }, StringSplitOptions.RemoveEmptyEntries), (string i) => UnPrep(i).Trim(new char[1] { '\'' }));
	}

	private static string FromPS(Group g, string delimeter = ";")
	{
		if (!g.Success)
		{
			return null;
		}
		return string.Join(delimeter, FromPS(g.Value));
	}

	private static string FromUTF8(string s)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		return Encoding.Default.GetString(bytes);
	}

	private static string Prep(string s)
	{
		return s?.Replace("'", "''");
	}

	private static string ToPS(string input, char[] delimeters = null)
	{
		if (delimeters == null)
		{
			delimeters = new char[2] { ';', ',' };
		}
		return ToPS(Array.ConvertAll(input.Split(delimeters), (string i) => Prep(i.Trim())));
	}

	private static string ToPS(string[] input)
	{
		return string.Join(", ", Array.ConvertAll(input, (string i) => "'" + i.Trim() + "'"));
	}

	private static string ToUTF8(string s)
	{
		if (s == null)
		{
			return null;
		}
		byte[] bytes = Encoding.Default.GetBytes(s);
		return Encoding.UTF8.GetString(bytes);
	}

	private static string UnPrep(string s)
	{
		return s?.Replace("''", "'");
	}
}
