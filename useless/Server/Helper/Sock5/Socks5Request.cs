using System;
using System.Text;

namespace Server.Helper.Sock5
{
	// Token: 0x02000087 RID: 135
	public class Socks5Request
	{
		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000307 RID: 775 RVA: 0x0002624C File Offset: 0x0002444C
		// (set) Token: 0x06000308 RID: 776 RVA: 0x00026254 File Offset: 0x00024454
		public byte Version { get; set; }

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000309 RID: 777 RVA: 0x0002625D File Offset: 0x0002445D
		// (set) Token: 0x0600030A RID: 778 RVA: 0x00026265 File Offset: 0x00024465
		public byte Command { get; set; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x0600030B RID: 779 RVA: 0x0002626E File Offset: 0x0002446E
		// (set) Token: 0x0600030C RID: 780 RVA: 0x00026276 File Offset: 0x00024476
		public byte Reserved { get; set; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x0600030D RID: 781 RVA: 0x0002627F File Offset: 0x0002447F
		// (set) Token: 0x0600030E RID: 782 RVA: 0x00026287 File Offset: 0x00024487
		public byte AddressType { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x0600030F RID: 783 RVA: 0x00026290 File Offset: 0x00024490
		// (set) Token: 0x06000310 RID: 784 RVA: 0x00026298 File Offset: 0x00024498
		public string DestinationAddress { get; set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x06000311 RID: 785 RVA: 0x000262A1 File Offset: 0x000244A1
		// (set) Token: 0x06000312 RID: 786 RVA: 0x000262A9 File Offset: 0x000244A9
		public ushort DestinationPort { get; set; }

		// Token: 0x06000313 RID: 787 RVA: 0x000262B4 File Offset: 0x000244B4
		public static Socks5Request Parse(byte[] data)
		{
			if (data.Length < 10)
			{
				throw new ArgumentException("Invalid data length");
			}
			Socks5Request socks5Request = new Socks5Request();
			socks5Request.Version = data[0];
			socks5Request.Command = data[1];
			socks5Request.Reserved = data[2];
			socks5Request.AddressType = data[3];
			if (socks5Request.AddressType != 1)
			{
				if (socks5Request.AddressType == 3)
				{
					byte b = data[4];
				}
				else if (socks5Request.AddressType != 4)
				{
					throw new ArgumentException("Invalid address type");
				}
			}
			if (socks5Request.AddressType == 3)
			{
				socks5Request.DestinationAddress = Encoding.ASCII.GetString(data, 5, (int)data[4]);
			}
			if (socks5Request.AddressType == 1)
			{
				socks5Request.DestinationAddress = string.Concat(new string[]
				{
					data[4].ToString(),
					".",
					data[5].ToString(),
					".",
					data[6].ToString(),
					".",
					data[7].ToString()
				});
			}
			Array.Reverse(data, data.Length - 2, 2);
			socks5Request.DestinationPort = BitConverter.ToUInt16(data, data.Length - 2);
			return socks5Request;
		}

		// Token: 0x06000314 RID: 788 RVA: 0x000263D4 File Offset: 0x000245D4
		public override string ToString()
		{
			return string.Format("Version: {0}, Command: {1}, Reserved: {2}, AddressType: {3}, DestinationAddress: {4}, DestinationPort: {5}", new object[]
			{
				this.Version,
				this.Command,
				this.Reserved,
				this.AddressType,
				this.DestinationAddress,
				this.DestinationPort
			});
		}
	}
}
