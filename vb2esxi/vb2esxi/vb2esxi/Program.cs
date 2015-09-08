using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using vb2esxi_gui;

namespace vb2esxi
{
    class Program
    {
        private const string EndString = "</Envelope>";
        static void Main(string[] args)
        {


            var ovfFile = @"D:\vms\template\AirFASE_B2_150907-vbm.ovf";

            RemoveSoundNodes(ovfFile);

            var ovfSha1 = FileHashHelper.SHA1File(ovfFile);

            Console.WriteLine(ovfSha1);


            

            Console.ReadKey();
            return;
            
            var file = @"F:\vm-share\airfase-b2\vb-airfase-b2\vb-airfase-b2.ovf";

            var targetFile = file.Replace(".ovf", "-vmx.ovf");


            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }

            var rplist = new List<VmRpItem>();

            //rplist.Add(new VmRpItem()
            //{
            //    Source = "",
            //    Target = ""
            //});

            //var allLines = File.ReadAllLines(file);

            //Console.WriteLine("Line Count: {0}",allLines.Length);

            //Console.ReadKey();
            //return;


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



            bool isxmlend = false;

            using (var fs = File.OpenRead(file))
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
                            foreach (var rp in rplist)
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
                            var recent = ((decimal)wcount / count) * 100;

                            Console.WriteLine("Now processed: {0}.", recent);
                        }
                    }

                    if (waitWrite != null && waitWrite.Count > 0)
                    {
                        File.AppendAllLines(targetFile, waitWrite,Encoding.ASCII);
                        waitWrite.Clear();
                        var recent = 100;//((decimal)wcount / count) * 100;

                        Console.WriteLine("Now processed: {0}.", recent);
                    }
                }
            }

            Console.WriteLine("Press Enter Exit.");
            Console.ReadLine();
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

        static int WriteToFile(string file, List<string> countents)
        {
            var wbcount = 0;

            foreach (var c in countents)
            {
                wbcount += Encoding.UTF8.GetBytes(c).Length;
            }

            File.AppendAllLines(file, countents, Encoding.UTF8);

            return wbcount;
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
