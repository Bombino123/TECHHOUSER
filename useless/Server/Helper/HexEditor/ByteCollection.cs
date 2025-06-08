using System;
using System.Collections.Generic;

namespace Server.Helper.HexEditor
{
	// Token: 0x02000089 RID: 137
	public class ByteCollection
	{
		// Token: 0x1700003F RID: 63
		// (get) Token: 0x0600031C RID: 796 RVA: 0x000264F9 File Offset: 0x000246F9
		public int Length
		{
			get
			{
				return this._bytes.Count;
			}
		}

		// Token: 0x0600031D RID: 797 RVA: 0x00026506 File Offset: 0x00024706
		public ByteCollection()
		{
			this._bytes = new List<byte>();
		}

		// Token: 0x0600031E RID: 798 RVA: 0x00026519 File Offset: 0x00024719
		public ByteCollection(byte[] bytes)
		{
			this._bytes = new List<byte>(bytes);
		}

		// Token: 0x0600031F RID: 799 RVA: 0x0002652D File Offset: 0x0002472D
		public void Add(byte item)
		{
			this._bytes.Add(item);
		}

		// Token: 0x06000320 RID: 800 RVA: 0x0002653B File Offset: 0x0002473B
		public void Insert(int index, byte item)
		{
			this._bytes.Insert(index, item);
		}

		// Token: 0x06000321 RID: 801 RVA: 0x0002654A File Offset: 0x0002474A
		public void Remove(byte item)
		{
			this._bytes.Remove(item);
		}

		// Token: 0x06000322 RID: 802 RVA: 0x00026559 File Offset: 0x00024759
		public void RemoveAt(int index)
		{
			this._bytes.RemoveAt(index);
		}

		// Token: 0x06000323 RID: 803 RVA: 0x00026567 File Offset: 0x00024767
		public void RemoveRange(int startIndex, int count)
		{
			this._bytes.RemoveRange(startIndex, count);
		}

		// Token: 0x06000324 RID: 804 RVA: 0x00026576 File Offset: 0x00024776
		public byte GetAt(int index)
		{
			return this._bytes[index];
		}

		// Token: 0x06000325 RID: 805 RVA: 0x00026584 File Offset: 0x00024784
		public void SetAt(int index, byte item)
		{
			this._bytes[index] = item;
		}

		// Token: 0x06000326 RID: 806 RVA: 0x00026593 File Offset: 0x00024793
		public char GetCharAt(int index)
		{
			return Convert.ToChar(this._bytes[index]);
		}

		// Token: 0x06000327 RID: 807 RVA: 0x000265A6 File Offset: 0x000247A6
		public byte[] ToArray()
		{
			return this._bytes.ToArray();
		}

		// Token: 0x040001D0 RID: 464
		private List<byte> _bytes;
	}
}
