using System.Threading;
using System.Windows.Forms;

namespace Plugin;

internal static class Clipboard
{
	public static string GetCurrentText()
	{
		string ReturnValue = string.Empty;
		Thread thread = new Thread((ThreadStart)delegate
		{
			ReturnValue = Clipboard.GetText();
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();
		return ReturnValue;
	}
}
