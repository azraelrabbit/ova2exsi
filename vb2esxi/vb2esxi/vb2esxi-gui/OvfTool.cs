using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace vb2esxi_gui
{
    public class OvfTool
    {
        private static string OvfToolMainPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"ovftool");
        public static bool ConvertOva(string ovaFile,string ovfFileTarget)
        {
            var OvfToolMain = Path.Combine(OvfToolMainPath, "ovftool.exe");
            try
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo("cmd.exe");
                p.StartInfo.Arguments = string.Format(" /c  {0} {1} {2}", OvfToolMain, ovaFile, ovfFileTarget);
                p.StartInfo.CreateNoWindow = false;
                p.Start();


                p.WaitForExit();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return false;
            }

        }
    }
}
