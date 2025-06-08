using System.Data.Entity.Core.Common.Utils;
using System.Diagnostics;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal sealed class ConfigViewGenerator : InternalBase
{
	private ViewGenTraceLevel m_traceLevel;

	private readonly TimeSpan[] m_breakdownTimes;

	private readonly Stopwatch m_watch;

	private readonly Stopwatch m_singleWatch;

	private PerfType m_singlePerfOp;

	private bool m_enableValidation = true;

	private bool m_generateUpdateViews = true;

	internal bool GenerateEsql { get; set; }

	internal TimeSpan[] BreakdownTimes => m_breakdownTimes;

	internal ViewGenTraceLevel TraceLevel
	{
		get
		{
			return m_traceLevel;
		}
		set
		{
			m_traceLevel = value;
		}
	}

	internal bool IsValidationEnabled
	{
		get
		{
			return m_enableValidation;
		}
		set
		{
			m_enableValidation = value;
		}
	}

	internal bool GenerateUpdateViews
	{
		get
		{
			return m_generateUpdateViews;
		}
		set
		{
			m_generateUpdateViews = value;
		}
	}

	internal bool GenerateViewsForEachType { get; set; }

	internal bool IsViewTracing => IsTraceAllowed(ViewGenTraceLevel.ViewsOnly);

	internal bool IsNormalTracing => IsTraceAllowed(ViewGenTraceLevel.Normal);

	internal bool IsVerboseTracing => IsTraceAllowed(ViewGenTraceLevel.Verbose);

	internal ConfigViewGenerator()
	{
		m_watch = new Stopwatch();
		m_singleWatch = new Stopwatch();
		int num = Enum.GetNames(typeof(PerfType)).Length;
		m_breakdownTimes = new TimeSpan[num];
		m_traceLevel = ViewGenTraceLevel.None;
		m_generateUpdateViews = false;
		StartWatch();
	}

	private void StartWatch()
	{
		m_watch.Start();
	}

	internal void StartSingleWatch(PerfType perfType)
	{
		m_singleWatch.Start();
		m_singlePerfOp = perfType;
	}

	internal void StopSingleWatch(PerfType perfType)
	{
		TimeSpan elapsed = m_singleWatch.Elapsed;
		m_singleWatch.Stop();
		m_singleWatch.Reset();
		BreakdownTimes[(int)perfType] = BreakdownTimes[(int)perfType].Add(elapsed);
	}

	internal void SetTimeForFinishedActivity(PerfType perfType)
	{
		TimeSpan elapsed = m_watch.Elapsed;
		BreakdownTimes[(int)perfType] = BreakdownTimes[(int)perfType].Add(elapsed);
		m_watch.Reset();
		m_watch.Start();
	}

	internal bool IsTraceAllowed(ViewGenTraceLevel traceLevel)
	{
		return TraceLevel >= traceLevel;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		StringUtil.FormatStringBuilder(builder, "Trace Switch: {0}", m_traceLevel);
	}
}
