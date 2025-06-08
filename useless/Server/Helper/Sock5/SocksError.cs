using System;

namespace Server.Helper.Sock5
{
	// Token: 0x02000082 RID: 130
	public enum SocksError
	{
		// Token: 0x0400019F RID: 415
		Granted,
		// Token: 0x040001A0 RID: 416
		Failure,
		// Token: 0x040001A1 RID: 417
		NotAllowed,
		// Token: 0x040001A2 RID: 418
		Unreachable,
		// Token: 0x040001A3 RID: 419
		HostUnreachable,
		// Token: 0x040001A4 RID: 420
		Refused,
		// Token: 0x040001A5 RID: 421
		Expired,
		// Token: 0x040001A6 RID: 422
		NotSupported,
		// Token: 0x040001A7 RID: 423
		AddressNotSupported,
		// Token: 0x040001A8 RID: 424
		LoginRequired = 144,
		// Token: 0x040001A9 RID: 425
		Fail = 255
	}
}
