using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace IlufaSharedObjects
{

    public class OposPrinter
    {
        //        private PosPrinter print_device;
        //  private List<string> errors;
        public static string CRLF = Convert.ToChar(13).ToString() + Convert.ToChar(10).ToString();
        public static string ESC = "\x1B";
        public static string LF = "\x0A";

        public static string open_drawer()
        {
            return ESC + "\x70\x00\x19\xff" + CRLF;  //code to open drawer
        }

        public static string align_center()
        {
            return ESC + "\x61\x31";
        }

        public static string align_left()
        {
            return ESC + "\x61\x30";
        }
        public static string bold()
        {
            return ESC + "\x45\x31";
        }

        public static string bold_off()
        {
            return ESC + "\x45\x30";
        }

        public static string double_size()
        {
            return ESC + "\x21\x10";
        }

        public static string font1()
        {
            return ESC + "\x21\x01";
        }

        public static string font0()
        {
            return ESC + "\x21\x00";
        }

        public static string underline()
        {
            return ESC + "\x2D\x31";
        }

        public static string underline_off()
        {
            return ESC + "\x2D\x30";
        }
        public static string header_format_1()
        {
            return ESC + "\x21\x18";
        }
        public static string cut_paper()
        {
            return ESC + "\x6d\xff";
        }
    }

}
