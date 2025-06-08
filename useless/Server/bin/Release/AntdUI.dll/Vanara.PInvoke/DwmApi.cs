using System.Runtime.InteropServices;
using System.Security;

namespace Vanara.PInvoke;

public static class DwmApi
{
	[PInvokeData("dwmapi.h")]
	public struct MARGINS
	{
		public int cxLeftWidth;

		public int cxRightWidth;

		public int cyTopHeight;

		public int cyBottomHeight;

		public static readonly MARGINS Empty = new MARGINS(0);

		public static readonly MARGINS Infinite = new MARGINS(-1);

		public int Left
		{
			get
			{
				return cxLeftWidth;
			}
			set
			{
				cxLeftWidth = value;
			}
		}

		public int Right
		{
			get
			{
				return cxRightWidth;
			}
			set
			{
				cxRightWidth = value;
			}
		}

		public int Top
		{
			get
			{
				return cyTopHeight;
			}
			set
			{
				cyTopHeight = value;
			}
		}

		public int Bottom
		{
			get
			{
				return cyBottomHeight;
			}
			set
			{
				cyBottomHeight = value;
			}
		}

		public MARGINS(int left, int right, int top, int bottom)
		{
			cxLeftWidth = left;
			cxRightWidth = right;
			cyTopHeight = top;
			cyBottomHeight = bottom;
		}

		public MARGINS(int allMargins)
		{
			cxLeftWidth = (cxRightWidth = (cyTopHeight = (cyBottomHeight = allMargins)));
		}

		public static bool operator !=(MARGINS m1, MARGINS m2)
		{
			return !m1.Equals(m2);
		}

		public static bool operator ==(MARGINS m1, MARGINS m2)
		{
			return m1.Equals(m2);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MARGINS mARGINS))
			{
				return base.Equals(obj);
			}
			if (cxLeftWidth == mARGINS.cxLeftWidth && cxRightWidth == mARGINS.cxRightWidth && cyTopHeight == mARGINS.cyTopHeight)
			{
				return cyBottomHeight == mARGINS.cyBottomHeight;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return cxLeftWidth ^ RotateLeft(cyTopHeight, 8) ^ RotateLeft(cxRightWidth, 16) ^ RotateLeft(cyBottomHeight, 24);
			static int RotateLeft(int value, int nBits)
			{
				nBits %= 32;
				return (value << nBits) | (value >> 32 - nBits);
			}
		}

		public override string ToString()
		{
			return $"{{Left={cxLeftWidth},Right={cxRightWidth},Top={cyTopHeight},Bottom={cyBottomHeight}}}";
		}
	}

	[DllImport("dwmapi.dll", ExactSpelling = true)]
	[SecurityCritical]
	[PInvokeData("dwmapi.h")]
	public static extern void DwmExtendFrameIntoClientArea(HWND hWnd, in MARGINS pMarInset);
}
