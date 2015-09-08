using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace vb2esxi_gui
{
    public partial class Form1 : Form
    {
        private const string EndString = "</Envelope>";

        List<VmRpItem> rpList { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();

            dialog.Filter = "ova文件(*.ova)|*.ova";

            var ret = dialog.ShowDialog(this);
            if (ret == DialogResult.OK)
            {
                var filePath = dialog.FileName;
                txtFile.Text = filePath;
                var targetFile = filePath.Replace(".ova", "-vbm.ovf");
                // var targetFileResult = filePath.Replace(".ova", ".ovf");

               


                //1 先转换为ovf,然后再修改
                var convertret = OvfTool.ConvertOva(filePath, targetFile);

                if (!convertret)
                {
                    MessageBox.Show("转换失败.");
                    return;
                }

              
                //2. 仅修改ovf文件
                ProcessOVF(targetFile);


                //3. 计算ovf文件的哈希值,并填入.mf文件中

                ProcessMfSha1(targetFile);
            }
        }

        private void ProcessMfSha1(string targetFile)
        {
            var mfFile = targetFile.Replace(".ovf", ".mf");
            var ovfSha1 = FileHashHelper.SHA1File(targetFile);

            var mfTemp = mfFile.Replace(".mf", "-tmp.mf");

            File.Copy(mfFile, mfTemp,true);

            if (File.Exists(mfFile))
            {
                File.Delete(mfFile);
            }

            var mfStrings = File.ReadAllLines(mfTemp);
            var newMfStr = new List<string>();

            var ovfName = new FileInfo(targetFile).Name;
            for (var i = 0; i < mfStrings.Length; i++)
            {
                var mfString = mfStrings[i];
                //SHA1(centos65 - openerp - vbm.ovf) = 1aeb68a117163bbc14ce6f0f28ae89695268259a
                //SHA1(centos65 - openerp - vbm - disk1.vmdk) = b06a6cafad171fca884ecf893dcd2b2d253689b4
                if (mfString.Contains(ovfName))
                {
                    var tmp = mfString.Replace(mfString.Substring(mfString.IndexOf("=")), " = " + ovfSha1);
                    newMfStr.Add(tmp);
                }
                else
                {
                    newMfStr.Add(mfString);
                }
            }

            File.WriteAllLines(mfFile, newMfStr);

            txtRet.AppendText("Sha1处理完毕!"); // = "处理完毕!";
            txtRet.AppendText(mfFile + "\r\n");
        }

        private static void RemoveSoundNodes(string ovfFile)
        {
            var xdoc = new XmlDocument();
            xdoc.Load(ovfFile);
            //<rasd:ElementName><Item><VirtualHardwareSection> <VirtualSystem ovf:id="Centos65_x64_OpenERP">  <Envelope 


            var parentNode = xdoc.GetElementsByTagName("VirtualHardwareSection").Item(0);

            var node = xdoc.GetElementsByTagName("Item");
            // xdoc.SelectNodes("/Envelope/VirtualSystem/VirtualHardwareSection/Item/rasd:ElementName");

            var needDelList = new List<XmlElement>();

            foreach (XmlElement n in node)
            {
                var childs = n.GetElementsByTagName("rasd:ElementName");

                if (childs.Count > 0)
                {
                    if (childs[0].InnerText.Trim().ToUpper() == "sound".ToUpper())
                    {
                        needDelList.Add(n);
                    }
                }
            }

            foreach (var element in needDelList)
            {
                parentNode.RemoveChild(element);
            }

            xdoc.Save(ovfFile);
        }
        private void ProcessOVF( string targetFile)
        {
            var tmpOvf = targetFile.Replace("-vbm.ovf", "-vbm-tmp.ovf");

            File.Copy(targetFile, tmpOvf,true);

            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }

            bool isxmlend = false;

            using (var fs = File.OpenRead(tmpOvf))
            {
                using (var sr = new StreamReader(fs, Encoding.ASCII))
                {
                    long count = fs.Length;
                    long wcount = 0;
                    var s = string.Empty;
                    //using (var ws = File.CreateText(file.Replace(".ova", "-vmx.ova")))
                    //{

                    var waitWrite = new List<string>();


                    while (!sr.EndOfStream)
                    {
                        s = sr.ReadLine();
                        // Console.WriteLine(s);

                        var wt = s;
                        wcount++;

                        if (!isxmlend)
                        {
                            Console.WriteLine(s);
                            foreach (var rp in rpList)
                            {
                                wt = wt.Replace(rp.Source, rp.Target);
                            }
                            Console.WriteLine(wt);
                            if (s.Equals(EndString))
                            {
                                isxmlend = true;
                                //  break;
                            }
                        }
                        else
                        {
                            //  break;
                        }

                        var wb = Encoding.ASCII.GetBytes(s);
                        var wbcount = wb.Length;
                        wcount += wbcount;


                        ////ws.WriteLine(wt);

                        waitWrite.Add(wt);

                        if (waitWrite.Count >= 200000)
                        {
                            File.AppendAllLines(targetFile, waitWrite, Encoding.ASCII);
                            waitWrite.Clear();
                            var recent = ((decimal) wcount/count)*100;

                            Console.WriteLine("Now processed: {0}.", recent);
                        }
                    }

                    if (waitWrite != null && waitWrite.Count > 0)
                    {
                        File.AppendAllLines(targetFile, waitWrite, Encoding.ASCII);
                        waitWrite.Clear();
                        var recent = 100; //((decimal)wcount / count) * 100;

                        Console.WriteLine("Now processed: {0}.", recent);

                        txtRet.AppendText("处理完毕!"); // = "处理完毕!";
                        txtRet.AppendText(targetFile + "\r\n");
                    }
                }
            }

            // 去除不识别的声卡设备等
            RemoveSoundNodes(targetFile);
        }

        List<VmRpItem> GenRplist()
        {
            var rplist = new List<VmRpItem>();
            rplist.Add(new VmRpItem()
            {
                Source = "<vssd:VirtualSystemType>virtualbox-2.2</vssd:VirtualSystemType>",
                Target = "<vssd:VirtualSystemType>vmx-07</vssd:VirtualSystemType>"
            });

            rplist.Add(new VmRpItem()
            {
                Source = "<rasd:Caption>sataController0</rasd:Caption>",
                Target = "<rasd:Caption>SCSIController</rasd:Caption>"
            });

            rplist.Add(new VmRpItem()
            {
                Source = "<rasd:Description>SATA Controller</rasd:Description>",
                Target = "<rasd:Description>SCSI Controller</rasd:Description>"
            });

            rplist.Add(new VmRpItem()
            {
                Source = "<rasd:ElementName>sataController0</rasd:ElementName>",
                Target = "<rasd:ElementName>SCSIController</rasd:ElementName>"
            });

            rplist.Add(new VmRpItem()
            {
                Source = "<rasd:ResourceSubType>AHCI</rasd:ResourceSubType>",
                Target = "<rasd:ResourceSubType>lsilogic</rasd:ResourceSubType>"
            });


            rplist.Add(new VmRpItem()
            {
                Source = "<rasd:ResourceType>20</rasd:ResourceType>",
                Target = "<rasd:ResourceType>6</rasd:ResourceType>"
            });
            return rplist;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            rpList = GenRplist();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();

            dialog.Filter = "ovf文件(*.ovf)|*.ovf";

            var ret = dialog.ShowDialog(this);
            if (ret == DialogResult.OK)
            {
                var filePath = dialog.FileName;
    
                ProcessOVF(filePath);

                ProcessMfSha1(filePath);
               
            }
        }
    }

    public class VmRpItem
    {
        public string Source { get; set; }
        public string Target { get; set; }


        public byte[] SourceBytes
        {
            get
            {
                if (!string.IsNullOrEmpty(Source))
                {
                    return Encoding.UTF8.GetBytes(Source);
                }

                return new byte[0];
            }
        }

        public byte[] TargetBytes
        {
            get
            {
                if (!string.IsNullOrEmpty(Target))
                {
                    return Encoding.UTF8.GetBytes(Target);
                }

                return new byte[0];
            }
        }
    }
}
