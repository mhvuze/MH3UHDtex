using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MH3UHDtex
{
    class Program
    {
        // Big Endian methods
        public static int ToInt16BigEndian(byte[] buf, int i)
        {
            return (buf[i] << 8) | (buf[i + 1]);
        }
        public static int ToInt32BigEndian(byte[] buf, int i)
        {
            return (buf[i] << 24) | (buf[i + 1] << 16) | (buf[i + 2] << 8) | buf[i + 3];
        }

        static void Main(string[] args)
        {
            string input;
            string output;
            string mode;

            Console.WriteLine("\nMH3UHDtex by Vuze\n");

            // Handle argument warnings
            if (args.Length > 3)
            {
                Console.WriteLine("ERROR: Too many arguments, use MH3UHDtex <mode> <input> <output|base>");
                Console.WriteLine("Available modes: -e for export, -i for import\n");
                return;
            }
            else if (args.Length < 3)
            {
                Console.WriteLine("ERROR: Too few arguments, use MH3UHDtex <mode> <input> <output|base>");
                Console.WriteLine("Available modes: -e for export, -i for import\n");
                return;
            }

            // Assign arguments
            mode = args[0];
            input = args[1];
            output = args[2];

            // Handle input file
            if (!File.Exists(input))
            {
                Console.Write("ERROR: Input cannot be processed.\n");
                return;
            }

            // Handle texture export
            if (mode == "-e")
            {
                byte[] TexArray = File.ReadAllBytes(input);
                Stream data = new MemoryStream(TexArray);
                BinaryReader rinput = new BinaryReader(data);

                // Handle invalid input files
                if (rinput.ReadInt32() != 0x54455800)
                {
                    Console.WriteLine("ERROR: Invalid input file specified.\n");
                    return;
                }
                // Convert input file
                else
                {
                    // Process header
                    int version = rinput.ReadUInt16();
                    byte tex_type = rinput.ReadByte();
                    byte cmflag1 = rinput.ReadByte();
                    byte[] width_b = rinput.ReadBytes(2);
                    int width = (ToInt16BigEndian(width_b, 0) & 0x1FF) * 8;
                    byte[] height_b = rinput.ReadBytes(2);
                    int height = ToInt16BigEndian(height_b, 0);
                    byte cmflag2 = rinput.ReadByte();
                    byte px_type = rinput.ReadByte();

                    Console.WriteLine("INFO: " + width + "x" + height + "px, Type: " + px_type + "\n");

                    // Save image data to new array
                    rinput.BaseStream.Seek(0x10, SeekOrigin.Begin);
                    byte[] px_data = new byte[data.Length - 0x10];
                    data.Read(px_data, 0, px_data.Length);

                    data.Close();
                    rinput.Close();

                    // Create gtx file from tex data
                    var gtx_header = new byte[] { 0x47, 0x66, 0x78, 0x32, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x4C, 0x4B, 0x7B, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x9C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
                    var gtx_insert1 = new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01 };
                    var gtx_insert2 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
                    var gtx_insert3 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x0D, 0x00, 0x00 };
                    var gtx_insert4 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x02, 0x03, 0x1F, 0xF8, 0x7F, 0x21 };
                    var gtx_insert5 = new byte[] { 0x06, 0x88, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x10, 0x42, 0x4C, 0x4B, 0x7B, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0B };
                    var gtx_insert6 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    var gtx_footer = new byte[] { 0x42, 0x4C, 0x4B, 0x7B, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    int tmp_type;
                    uint tmp_fmt;
                    int tmp_pxinfo1;
                    int tmp_pxinfo2;
                    int tmp_size;
                    System.IO.FileStream temp_gtx = new System.IO.FileStream(input + ".gtx", System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    BinaryWriter wgtx = new BinaryWriter(temp_gtx);

                    temp_gtx.Write(gtx_header, 0, gtx_header.Length);
                    wgtx.BaseStream.Seek(gtx_header.Length, SeekOrigin.Begin);
                    wgtx.Write(ToInt32BigEndian(BitConverter.GetBytes(width), 0));
                    wgtx.Write(ToInt32BigEndian(BitConverter.GetBytes(height), 0));
                    wgtx.Write(gtx_insert1);

                    if (px_type == 0x0C)        // GX2_SURFACE_FORMAT_T_BC3_UNORM
                    {
                        tmp_type = 51;
                        tmp_fmt = 0xCC0003FF;
                        tmp_pxinfo1 = 8192;
                        tmp_pxinfo2 = 256;
                        tmp_size = (width * height * 8) / 8;
                    }
                    else if (px_type == 0x0B)   // GX2_SURFACE_FORMAT_T_BC1_UNORM
                    {
                        tmp_type = 49;
                        tmp_fmt = 0xC40003FF;
                        tmp_pxinfo1 = 4096;
                        tmp_pxinfo2 = 256;
                        tmp_size = (width * height * 4) / 8;
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Pixel data type not implemented yet, sorry.\n");
                        return;
                    }

                    wgtx.Write(ToInt32BigEndian(BitConverter.GetBytes(tmp_type), 0));
                    wgtx.Write(gtx_insert2);
                    wgtx.Write(ToInt32BigEndian(BitConverter.GetBytes(tmp_size), 0));
                    wgtx.Write(gtx_insert3);
                    wgtx.Write(ToInt32BigEndian(BitConverter.GetBytes(tmp_pxinfo1), 0));
                    wgtx.Write(ToInt32BigEndian(BitConverter.GetBytes(tmp_pxinfo2), 0));
                    wgtx.Write(gtx_insert4);
                    wgtx.Write(ToInt32BigEndian(BitConverter.GetBytes(tmp_fmt), 0));
                    wgtx.Write(gtx_insert5);
                    wgtx.Write(ToInt32BigEndian(BitConverter.GetBytes(tmp_size), 0));
                    wgtx.Write(gtx_insert6);
                    temp_gtx.Write(px_data, 0, tmp_size);
                    wgtx.Write(gtx_footer);

                    wgtx.Close();
                    temp_gtx.Close();

                    // Convert gtx to dds
                    String arguments = @"-i " + input + ".gtx " + "-f GX2_SURFACE_FORMAT_TCS_R8_G8_B8_A8_UNORM -o " + output;
                    ProcessStartInfo texconv2_si = new ProcessStartInfo("TexConv2.exe");
                    texconv2_si.Arguments = arguments;
                    Process texconv2 = Process.Start(texconv2_si);
                    texconv2.WaitForExit();
                    
                    File.Delete(input + ".gtx");
                    Console.WriteLine("INFO: Conversion done.\n");
                }


            }

            // Handle texture import
            else if (mode == "-i")
            {
                Console.WriteLine("ERROR: Not implemented yet, sorry.\n");
                return;
            }

            // Handle invalid arguments
            else
            {
                Console.WriteLine("ERROR: Invalid mode specified.");
                Console.WriteLine("Available modes: -e for export, -i for import\n");
                return;
            }
        }
    }
}
