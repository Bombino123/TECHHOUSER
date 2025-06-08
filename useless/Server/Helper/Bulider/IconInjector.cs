using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace Server.Helper.Bulider
{
	// Token: 0x02000091 RID: 145
	public static class IconInjector
	{
		// Token: 0x060003D4 RID: 980 RVA: 0x00029D56 File Offset: 0x00027F56
		public static void InjectIcon(string exeFileName, string iconFileName)
		{
			IconInjector.InjectIcon(exeFileName, iconFileName, 1U, 1U);
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x00029D64 File Offset: 0x00027F64
		public static void InjectIcon(string exeFileName, string iconFileName, uint iconGroupID, uint iconBaseID)
		{
			IconInjector.IconFile iconFile = IconInjector.IconFile.FromFile(iconFileName);
			IntPtr hUpdate = IconInjector.NativeMethods.BeginUpdateResource(exeFileName, false);
			byte[] array = iconFile.CreateIconGroupData(iconBaseID);
			IconInjector.NativeMethods.UpdateResource(hUpdate, new IntPtr(14L), new IntPtr((long)((ulong)iconGroupID)), 0, array, array.Length);
			for (int i = 0; i <= iconFile.ImageCount - 1; i++)
			{
				byte[] array2 = iconFile.ImageData(i);
				IconInjector.NativeMethods.UpdateResource(hUpdate, new IntPtr(3L), new IntPtr((long)((ulong)iconBaseID + (ulong)((long)i))), 0, array2, array2.Length);
			}
			IconInjector.NativeMethods.EndUpdateResource(hUpdate, false);
		}

		// Token: 0x0200022C RID: 556
		[SuppressUnmanagedCodeSecurity]
		private class NativeMethods
		{
			// Token: 0x0600090D RID: 2317
			[DllImport("kernel32")]
			public static extern IntPtr BeginUpdateResource(string fileName, [MarshalAs(UnmanagedType.Bool)] bool deleteExistingResources);

			// Token: 0x0600090E RID: 2318
			[DllImport("kernel32")]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool UpdateResource(IntPtr hUpdate, IntPtr type, IntPtr name, short language, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] byte[] data, int dataSize);

			// Token: 0x0600090F RID: 2319
			[DllImport("kernel32")]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool EndUpdateResource(IntPtr hUpdate, [MarshalAs(UnmanagedType.Bool)] bool discard);
		}

		// Token: 0x0200022D RID: 557
		private struct ICONDIR
		{
			// Token: 0x04000827 RID: 2087
			public ushort Reserved;

			// Token: 0x04000828 RID: 2088
			public ushort Type;

			// Token: 0x04000829 RID: 2089
			public ushort Count;
		}

		// Token: 0x0200022E RID: 558
		private struct ICONDIRENTRY
		{
			// Token: 0x0400082A RID: 2090
			public byte Width;

			// Token: 0x0400082B RID: 2091
			public byte Height;

			// Token: 0x0400082C RID: 2092
			public byte ColorCount;

			// Token: 0x0400082D RID: 2093
			public byte Reserved;

			// Token: 0x0400082E RID: 2094
			public ushort Planes;

			// Token: 0x0400082F RID: 2095
			public ushort BitCount;

			// Token: 0x04000830 RID: 2096
			public int BytesInRes;

			// Token: 0x04000831 RID: 2097
			public int ImageOffset;
		}

		// Token: 0x0200022F RID: 559
		private struct BITMAPINFOHEADER
		{
			// Token: 0x04000832 RID: 2098
			public uint Size;

			// Token: 0x04000833 RID: 2099
			public int Width;

			// Token: 0x04000834 RID: 2100
			public int Height;

			// Token: 0x04000835 RID: 2101
			public ushort Planes;

			// Token: 0x04000836 RID: 2102
			public ushort BitCount;

			// Token: 0x04000837 RID: 2103
			public uint Compression;

			// Token: 0x04000838 RID: 2104
			public uint SizeImage;

			// Token: 0x04000839 RID: 2105
			public int XPelsPerMeter;

			// Token: 0x0400083A RID: 2106
			public int YPelsPerMeter;

			// Token: 0x0400083B RID: 2107
			public uint ClrUsed;

			// Token: 0x0400083C RID: 2108
			public uint ClrImportant;
		}

		// Token: 0x02000230 RID: 560
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		private struct GRPICONDIRENTRY
		{
			// Token: 0x0400083D RID: 2109
			public byte Width;

			// Token: 0x0400083E RID: 2110
			public byte Height;

			// Token: 0x0400083F RID: 2111
			public byte ColorCount;

			// Token: 0x04000840 RID: 2112
			public byte Reserved;

			// Token: 0x04000841 RID: 2113
			public ushort Planes;

			// Token: 0x04000842 RID: 2114
			public ushort BitCount;

			// Token: 0x04000843 RID: 2115
			public int BytesInRes;

			// Token: 0x04000844 RID: 2116
			public ushort ID;
		}

		// Token: 0x02000231 RID: 561
		private class IconFile
		{
			// Token: 0x1700007E RID: 126
			// (get) Token: 0x06000911 RID: 2321 RVA: 0x00062A72 File Offset: 0x00060C72
			public int ImageCount
			{
				get
				{
					return (int)this.iconDir.Count;
				}
			}

			// Token: 0x06000912 RID: 2322 RVA: 0x00062A7F File Offset: 0x00060C7F
			public byte[] ImageData(int index)
			{
				return this.iconImage[index];
			}

			// Token: 0x06000913 RID: 2323 RVA: 0x00062A8C File Offset: 0x00060C8C
			public static IconInjector.IconFile FromFile(string filename)
			{
				IconInjector.IconFile iconFile = new IconInjector.IconFile();
				byte[] array = File.ReadAllBytes(filename);
				GCHandle gchandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				iconFile.iconDir = (IconInjector.ICONDIR)Marshal.PtrToStructure(gchandle.AddrOfPinnedObject(), typeof(IconInjector.ICONDIR));
				iconFile.iconEntry = new IconInjector.ICONDIRENTRY[(int)iconFile.iconDir.Count];
				iconFile.iconImage = new byte[(int)iconFile.iconDir.Count][];
				int num = Marshal.SizeOf<IconInjector.ICONDIR>(iconFile.iconDir);
				Type typeFromHandle = typeof(IconInjector.ICONDIRENTRY);
				int num2 = Marshal.SizeOf(typeFromHandle);
				for (int i = 0; i <= (int)(iconFile.iconDir.Count - 1); i++)
				{
					IconInjector.ICONDIRENTRY icondirentry = (IconInjector.ICONDIRENTRY)Marshal.PtrToStructure(new IntPtr(gchandle.AddrOfPinnedObject().ToInt64() + (long)num), typeFromHandle);
					iconFile.iconEntry[i] = icondirentry;
					iconFile.iconImage[i] = new byte[icondirentry.BytesInRes];
					Buffer.BlockCopy(array, icondirentry.ImageOffset, iconFile.iconImage[i], 0, icondirentry.BytesInRes);
					num += num2;
				}
				gchandle.Free();
				return iconFile;
			}

			// Token: 0x06000914 RID: 2324 RVA: 0x00062BB0 File Offset: 0x00060DB0
			public byte[] CreateIconGroupData(uint iconBaseID)
			{
				byte[] array = new byte[Marshal.SizeOf(typeof(IconInjector.ICONDIR)) + Marshal.SizeOf(typeof(IconInjector.GRPICONDIRENTRY)) * this.ImageCount];
				GCHandle gchandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				Marshal.StructureToPtr<IconInjector.ICONDIR>(this.iconDir, gchandle.AddrOfPinnedObject(), false);
				int num = Marshal.SizeOf<IconInjector.ICONDIR>(this.iconDir);
				for (int i = 0; i <= this.ImageCount - 1; i++)
				{
					IconInjector.GRPICONDIRENTRY structure = default(IconInjector.GRPICONDIRENTRY);
					IconInjector.BITMAPINFOHEADER bitmapinfoheader = default(IconInjector.BITMAPINFOHEADER);
					GCHandle gchandle2 = GCHandle.Alloc(bitmapinfoheader, GCHandleType.Pinned);
					Marshal.Copy(this.ImageData(i), 0, gchandle2.AddrOfPinnedObject(), Marshal.SizeOf(typeof(IconInjector.BITMAPINFOHEADER)));
					gchandle2.Free();
					structure.Width = this.iconEntry[i].Width;
					structure.Height = this.iconEntry[i].Height;
					structure.ColorCount = this.iconEntry[i].ColorCount;
					structure.Reserved = this.iconEntry[i].Reserved;
					structure.Planes = bitmapinfoheader.Planes;
					structure.BitCount = bitmapinfoheader.BitCount;
					structure.BytesInRes = this.iconEntry[i].BytesInRes;
					structure.ID = Convert.ToUInt16((long)((ulong)iconBaseID + (ulong)((long)i)));
					Marshal.StructureToPtr<IconInjector.GRPICONDIRENTRY>(structure, new IntPtr(gchandle.AddrOfPinnedObject().ToInt64() + (long)num), false);
					num += Marshal.SizeOf(typeof(IconInjector.GRPICONDIRENTRY));
				}
				gchandle.Free();
				return array;
			}

			// Token: 0x04000845 RID: 2117
			private IconInjector.ICONDIR iconDir;

			// Token: 0x04000846 RID: 2118
			private IconInjector.ICONDIRENTRY[] iconEntry;

			// Token: 0x04000847 RID: 2119
			private byte[][] iconImage;
		}
	}
}
