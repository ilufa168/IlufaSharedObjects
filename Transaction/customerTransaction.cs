using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using IlufaSharedObjects;
using Devart.Data.Oracle;
using System.Data;
using System.Runtime.InteropServices;
using System.IO;
using IlufaSharedObjects.Payment;
//using System.Dr
using System.Configuration;
using System.Drawing.Printing;


namespace IlufaSharedObjects.Transaction
{
    public struct base_transaction
    {  //pretty much a straight copy from h_jual
        public string nobukti;
        public DateTime tgl; //date
        public DateTime jam; //time
        public string nmkas; //cashiers name
        public double total; //total sale amt
        public string jnkbyr; //payment type
        public double tvch; //total voucher
        public double tbyr; //total cash
        public double tdbt; //total debit
        public double tccd; //total credit
        public double kembali; //change?
        public string  nocard; // card #
        public string pemilik; //card owner
        public string nmcard; //name on card
        public string ket; //????
        public string lokasi; //location
        public string nm_update; //nm_update
        public DateTime? tg_update; //datetime upd
        public DateTime? tg_insert; //datetime add
        public double tpiutang; //store voucher
        public double tother; //Other amt
        public string memberid; //member id
        public string txt; //????
    }

    //Class to define the items purchased in a sale
    public class saleItem
    {
        public string item_code_kdbrg;
        public string category_kdgol;
        public string vendor_code_kdsupp;
        public double hbeli;
        public double item_price_hjual;
        public Int32 quantity_qty;
        public double disc;
        public double discrp;
        public string location_lokasi;
        public string t;
        public string description;


    }

    public class saleItemDisplay : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _item_code_kdbrg;
        public string _category_kdgol;
        public string _vendor_code_kdsupp;
        public double _item_price_hjual;
        public Int32 _quantity_qty;
        public string _t;

        public string item_code_kdbrg
        {
            get { return _item_code_kdbrg; }
            set
            {
                _item_code_kdbrg = value;
                this.NotifyPropertyChanged("item_code_kdbrg");
            }
        }
        public string category_kdgol
        {
            get { return _category_kdgol; }
            set
            {
                _category_kdgol = value;
                this.NotifyPropertyChanged("category_kdgol");
            }
        }
        public string vendor_code_kdsupp
        {
            get { return _vendor_code_kdsupp; }
            set
            {
                _vendor_code_kdsupp = value;
                this.NotifyPropertyChanged("vendor_code_kdsupp");
            }
        }
        public double item_price_hjual
        {
            get { return _item_price_hjual; }
            set
            {
                _item_price_hjual = value;
                this.NotifyPropertyChanged("item_price_hjual");
            }
        }
        public Int32 quantity_qty
        {
            get { return _quantity_qty; }
            set
            {
                _quantity_qty = value;
                this.NotifyPropertyChanged("quantity_qty");
            }
        }
        public string t
        {
            get { return _t; }
            set
            {
                _t = value;
                this.NotifyPropertyChanged("t");
            }
        }

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    public class customerTransaction
    {
        public List<String> errors;
        public base_transaction base_data;
        private List<saleItem> sales_items = new List<saleItem>();
        private List<PaymentItem> payment_items = new List<PaymentItem>();

        public customerTransaction(Object[] tmp_values)
        {
            if (tmp_values.Count() == 0)
                return;

            this.loadBaseRec(tmp_values);
        }

        public customerTransaction(string nobukti)
        {
            base_data = new base_transaction();
            sales_items = new List<saleItem>();

            base_data.nobukti = nobukti;
            this.load_base();
            this.load_items();
            this.load_payments();
        }

        public void addPayment(PaymentItem pi)
        {
            this.payment_items.Add(pi);
        }

        public int itemCount()
        {
            int cnt = 0;
            foreach (saleItem si in sales_items)
            {
                cnt += si.quantity_qty;
            }
            return cnt;
        }

        public decimal getYSales()
        {
            double total = 0;
            foreach (saleItem si in sales_items)
            {
                if (si.t == "Y")
                    total += si.quantity_qty * (si.item_price_hjual - si.discrp);
            }
            return (decimal) total;
        }

        public bool allYSales()
        {
            bool result = true;
            foreach (saleItem si in sales_items)
            {
                if (si.t != "Y")
                    result=false;
            }

            return result;
        }

        public bool createTmpSaleItem(decimal amt)
        {
//Not using this so comment out
/*
            //step 1, try and find an exact match on the amount
            string sql = "select dj.KDBRG, dj.KDGOL, dj.KDSUPP, dj.HBELI, dj.HJUAL, " +
                               " dj.QTY, dj.DISC, dj.DISCRP, dj.LOKASI, dj.NM_UPDATE, " +
                               " dj.TG_UPDATE, dj.TGL_INSERT, dj.SPID, dj.TXT, nvl(v.T,'N'), i.item_name " +
                          "from d_jual dj, vendor v, item i " +
                          "where dj.kdsupp = v.vendor_code " +
                          "  and dj.kdbrg = i.item_code " +
                          "  and dj.HJUAL between :amount1 and :amount2 and rownum < 100";

            OracleCommand cmd = new OracleCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("amount1", amt);
            cmd.Parameters.AddWithValue("amount2", amt + 1000);

            OracleDataReader reader;

            try
            {
                reader = OracleHandler.execReadQuery(cmd);
            }
            catch (Exception ex)
            {
                string tmp = ex.ToString();
                this.errors.Add(tmp);
                
                return false;
            }

            if (reader == null)
                return false;

            if (!reader.HasRows)
                return false;
            //pick an item and create the sales item
            List<saleItem> si_list = new List<saleItem>();
            while (reader.Read())
            {
                saleItem tmp_si = new saleItem();
                tmp_si.item_code_kdbrg = reader.GetString(0);
                tmp_si.category_kdgol = reader.GetString(1);
                tmp_si.vendor_code_kdsupp = reader.GetString(2);
                tmp_si.item_price_hjual = double.Parse(reader.GetString(4));
                tmp_si.quantity_qty = 1;
                tmp_si.t = reader.GetString(14);
                tmp_si.discrp = Math.Abs((double) amt - tmp_si.item_price_hjual);
                tmp_si.disc = tmp_si.discrp;
                tmp_si.description = reader.GetString(15);

                si_list.Add(tmp_si);

            }

            //now pick one
            Random rnd = new Random();
            int r_to_steal = rnd.Next(si_list.Count);
            this.sales_items.Add(si_list[r_to_steal]);
*/
            return true;
        }

        public decimal getNSales()
        {
            double total = 0;
            foreach (saleItem si in sales_items)
            {
                if (si.t == "N" || si.t == "")
                    total += si.quantity_qty * (si.item_price_hjual - si.discrp);
            }
            return (decimal)total;
        }

        public void loadItemRec(Object[] a_rec)
        {
            //the array of objects is set in stone.  Any changes will need changed everywhere
            saleItem si = new saleItem();
            si.item_code_kdbrg = a_rec[23].ToString();
            si.category_kdgol = a_rec[24].ToString();
            si.vendor_code_kdsupp = a_rec[25].ToString();
            si.hbeli = double.Parse(a_rec[26].ToString());
            si.item_price_hjual = double.Parse(a_rec[27].ToString());
            si.quantity_qty = int.Parse(a_rec[28].ToString());
            si.disc = double.Parse(a_rec[29].ToString());
            si.discrp = double.Parse(a_rec[30].ToString());
            si.location_lokasi = a_rec[31].ToString();
            si.t = a_rec[33].ToString();
            si.description = a_rec[34].ToString();
            this.sales_items.Add(si);
        }

        private void loadBaseRec(Object[] a_rec)
        {
            this.base_data.nobukti = a_rec[0].ToString();
            this.base_data.tgl = DateTime.Parse(a_rec[1].ToString());
            //this.base_data.jam = DateTime.Parse(a_rec[2].ToString(),"hh:mm:ss");
            this.base_data.nmkas = a_rec[3].ToString();
            this.base_data.total = double.Parse(a_rec[4].ToString());
            this.base_data.jnkbyr = a_rec[5].ToString();;
            this.base_data.tvch = double.Parse(a_rec[6].ToString());
            this.base_data.tbyr = double.Parse(a_rec[7].ToString());
            this.base_data.tdbt = double.Parse(a_rec[8].ToString());
            this.base_data.tccd = double.Parse(a_rec[9].ToString());
            this.base_data.kembali = double.Parse(a_rec[11].ToString());
            this.base_data.nocard = a_rec[13].ToString();
            this.base_data.pemilik = a_rec[14].ToString();
            this.base_data.nmcard = a_rec[15].ToString();
            this.base_data.ket = a_rec[16].ToString();
            this.base_data.lokasi = a_rec[17].ToString();
            this.base_data.nm_update = a_rec[18].ToString();
            if (!string.IsNullOrWhiteSpace(a_rec[19].ToString()))
                this.base_data.tg_update = DateTime.Parse(a_rec[19].ToString());
            if (!string.IsNullOrWhiteSpace(a_rec[20].ToString()))
                this.base_data.tg_insert = DateTime.Parse(a_rec[20].ToString());
            this.base_data.tpiutang = double.Parse(a_rec[10].ToString());
            this.base_data.tother = double.Parse(a_rec[12].ToString());
            this.base_data.memberid = a_rec[21].ToString();
            this.base_data.txt = a_rec[22].ToString();
        }

        public BindingList<saleItemDisplay> display_items()
        {
            BindingList<saleItemDisplay> the_items = new BindingList<saleItemDisplay>();
            foreach (saleItem si in this.sales_items)
            {
                saleItemDisplay tmp_sid = new saleItemDisplay();
                tmp_sid.item_code_kdbrg = si.item_code_kdbrg;
                tmp_sid.item_price_hjual = si.item_price_hjual;
                tmp_sid.category_kdgol = si.category_kdgol;
                tmp_sid.vendor_code_kdsupp = si.vendor_code_kdsupp;
                tmp_sid.quantity_qty = si.quantity_qty;
                tmp_sid.t = si.t;
                the_items.Add(tmp_sid);
            }

            return the_items;
        }

        private void load_items()
        {
            OracleHandler oh = new OracleHandler();
            string sql = "select dj.KDBRG, dj.KDGOL, dj.KDSUPP, dj.HBELI, dj.HJUAL, " +
                               " dj.QTY, dj.DISC, dj.DISCRP, dj.LOKASI, dj.NM_UPDATE, " +
                               " dj.TG_UPDATE, dj.TGL_INSERT, dj.SPID, dj.TXT, nvl(v.T,'N') " +
                          "from d_jual dj, vendor v " +
                          "where dj.kdsupp = v.vendor_code " +
                          "  and nobukti = '" + this.base_data.nobukti + "'";

            if (!oh.connect())
            {
                return;
            }

            if (!oh.DoQuery(sql))
                return;

            if (oh.getRowCount() >= 1)
            {
                OracleDataReader odr = oh.odr;
                while (odr.Read())
                {
                    saleItem tmp_si = new saleItem();
                    tmp_si.item_code_kdbrg = odr.GetString(0);
                    tmp_si.category_kdgol = odr.GetString(1);
                    tmp_si.vendor_code_kdsupp = odr.GetString(2);
                    tmp_si.item_price_hjual = double.Parse(odr.GetString(4));
                    tmp_si.quantity_qty = Int32.Parse(odr.GetString(5));
                    tmp_si.t = odr.GetString(14);

                    this.sales_items.Add(tmp_si);
//                    tmpTs.nmkas = odr.GetString(2);
//                    tmpTs.total = double.Parse(odr.GetString(3));
//                    tmpTs.jnsbyr = odr.GetString(4);
//                    tmpTs.tvch = double.Parse(odr.GetString(5));
//                    tmpTs.tbyr = double.Parse(odr.GetString(6));
//                    tmpTs.tdbt = double.Parse(odr.GetString(7));
//                    tmpTs.tccd = double.Parse(odr.GetString(8));
//                    tmpTs.tpiutang = double.Parse(odr.GetString(9));
//                    tmpTs.kembali = double.Parse(odr.GetString(10));
//                    tmpTs.tother = double.Parse(odr.GetString(11));
//                    tmpTs.total_quantity = Int32.Parse(odr.GetString(12));
                }
                odr.Dispose();
            }
            oh.disconnect();
        }

        public void load_payments()
        {
//I dont think i need this atm
/*
            string sql = "select jnsbyr, nocard, bayar, kembali from pay_jual where nobukti = :id";

            OracleCommand cmd = new OracleCommand();

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("id", this.base_data.nobukti);

            OracleDataReader odr;

            odr = OracleHandler.execReadQuery(cmd);

            if (odr == null)
                return;

            while (odr.Read())
            {
                PaymentItem pi = new PaymentItem();
                pi.pay_type = pi.determinePayType(odr.GetString(0));
                pi.account_number = odr.GetString(1);
                pi.amount = decimal.Parse(odr.GetString(2));
                pi.change = decimal.Parse(odr.GetString(3));
                this.payment_items.Add(pi);
            }

            odr.Dispose();
*/
        }

        private void load_base()
        {
            OracleHandler oh = new OracleHandler();
//            OracleDataReader odr;

            string sql = "select NOBUKTI, TGL, JAM, NMKAS, TOTAL, JNSBYR, TVCH, TBYR, " +
                                " TDBT, TCCD, KEMBALI, NOCARD, PEMILIK, NMCARD, KET, LOKASI, " +
                                " NM_UPDATE, TG_UPDATE, TGL_INSERT, TPIUTANG, TOTHER, MEMBERID, TXT " +
                         "from  ERP.H_JUAL " +
                         "where nobukti = '" + this.base_data.nobukti + "'";

            if (!oh.connect())
            {
                return;
            }

            if (!oh.DoQuery(sql))
                return;

            if (oh.getRowCount() >= 1)
            {
                OracleDataReader odr = oh.odr;
                while (odr.Read())
                {
//already have nobukti so skip that one
                    this.base_data.tgl = odr.GetDateTime(1);
                    this.base_data.jam = odr.GetDateTime(2);
                    this.base_data.nmkas = odr.GetString(3);
                    base_data.total = double.Parse(odr.GetString(4));
                }
                odr.Dispose();
            }
            oh.disconnect();
        }

        public bool printTransactionReceipt()
        {
            PrinterSettings ps = new PrinterSettings();
            ps.PrinterName = ConfigurationManager.AppSettings["OposPrinter"];

            //Load the payments if we dont have it
            if (this.payment_items.Count == 0)
                this.load_payments();

            string output = this.print_header();
            output += this.print_data_rows();
            output += this.print_footer();
            RawPrinterHelper.SendStringToPrinter(ps.PrinterName, output);
            RawPrinterHelper.SendStringToPrinter(ps.PrinterName, OposPrinter.cut_paper());     
            return true;
        }
        private decimal getPaymentTotal()
        {
            decimal result = 0;

            foreach (PaymentItem pi in payment_items)
            {
                result += pi.amount;
            }

            return result;
        }
        private string print_header()
        {
            //string outstuff = "";

            //Print the store name, centered bold double size
            //string outstuff = "\x1B\x70\x40" + CRLF; //Open drawer

            string outstuff = ""; // OposPrinter.open_drawer(); //Open drawer
            
            StoreSettings store_settings = new StoreSettings(this.base_data.lokasi);
            
            outstuff += OposPrinter.align_center() + store_settings.get_store_name() + OposPrinter.CRLF;

            //Print address lines centered
            outstuff += OposPrinter.font0()
                      + OposPrinter.bold_off()
                      + OposPrinter.align_center() + store_settings.get_address1() + OposPrinter.CRLF
                      + store_settings.get_address2() + OposPrinter.CRLF + OposPrinter.CRLF;

            //Cashier and date/time
            outstuff += OposPrinter.align_left() + OposPrinter.font1()
                + "Kasir:" + String.Format("{0,-21}", this.base_data.nmkas) + " " + String.Format("{0:dd MMM yyyy}", this.base_data.tgl) + OposPrinter.CRLF;
            //Transaction number
            outstuff += OposPrinter.underline() + "No: " + String.Format("{0,-35}", this.base_data.nobukti) + OposPrinter.CRLF;
            //Column headers
            outstuff += OposPrinter.underline_off() + "Qty " + "    Harga" + "     Disc           " + "Total" + OposPrinter.CRLF;
            //Thats all folks
            
            return outstuff;
        }

        private string print_data_rows()
        {
            string outstuff = "";
            int qty = 0;
            double discount_total = 0;
            foreach (saleItem an_item in this.sales_items)
            {
                qty += an_item.quantity_qty; //need this for later, why is it not part of the class!
                string price = String.Format("{0,10}", an_item.item_price_hjual.ToString("##,###,###"));
                string discount = String.Format("{0,9}", an_item.discrp.ToString("#,###,###"));
                discount_total += an_item.discrp;
                string total = ((an_item.quantity_qty * an_item.item_price_hjual) - an_item.discrp).ToString("N0");
      //come back to this          string total = String.Format("{0,14}", an_item..total.ToString("##,###,###,###,###"));
                outstuff += OposPrinter.align_left()
                          + OposPrinter.font1() + String.Format("{0,-4}", an_item.quantity_qty.ToString()) + price + " " + discount + " " + total + OposPrinter.CRLF;
                //line 2
                string desc;
                //Fix me toooooo
                if (an_item.description.Length > 17)
                    desc = an_item.description.Substring(0, 18);
                else
                    desc = String.Format("{0,-18}", an_item.description);

                outstuff += "     PLU: " + an_item.item_code_kdbrg + "  " + desc + OposPrinter.CRLF;

            }
            //Add a dummy row underlined
            outstuff += OposPrinter.underline() + String.Format("{0,39}", " ") + OposPrinter.underline_off() + OposPrinter.CRLF;
            //The totals
            //            sale_transaction.get
            outstuff += OposPrinter.bold() + String.Format("{0,-12}", qty.ToString())
                + "Total:" + String.Format("{0,21}", "Rp. "
                + (this.getNSales() + this.getYSales()).ToString("###,###,###,###,###")) + OposPrinter.bold_off() + OposPrinter.CRLF;
            //Now time for the payments
            decimal payment_total = 0;
            outstuff += OposPrinter.underline() + String.Format("{0,-39}", "Pembayaran") + OposPrinter.underline_off() + OposPrinter.CRLF;
            
            foreach (PaymentItem pi in this.payment_items)
            {
                string pay_type, account;

                switch (pi.pay_type)
                {
                    case PaymentType.Cash:
                        pay_type = "Tunai";
                        account = " ";
                        break;
                    case PaymentType.CreditCard:
                        pay_type = "K. Kredit";
                        account = "XXXX-" + pi.account_number.Substring(pi.account_number.Length - 4, 4);
                        break;
                    case PaymentType.DebitCard:
                        pay_type = "K. Debit";
                        account = "XXXX-" + pi.account_number.Substring(pi.account_number.Length - 4, 4);
                        break;
                    case PaymentType.Voucher:
                        pay_type = "Voucher";
                        account = pi.account_number;
                        break;
                    case PaymentType.StoreCredit:
                        pay_type = "Piutang";
                        account = pi.account_number;
                        break;
                    default:
                        pay_type = "Other";
                        account = pi.account_number;
                        break;
                }
                payment_total += pi.amount;
                outstuff += String.Format("{0,-10}", pay_type) + String.Format("{0,-14}", account)
                    + String.Format("{0,15}", pi.amount.ToString("###,###,###,###")) + OposPrinter.CRLF;
            }
            //Payment Total

            outstuff += OposPrinter.underline() + String.Format("{0,39}", " ") + OposPrinter.underline_off() + OposPrinter.CRLF;
            outstuff += OposPrinter.bold() + "Total Bayar:" + String.Format("{0,27}", "Rp. " + payment_total.ToString("###,###,###,###")) + OposPrinter.bold_off() + OposPrinter.CRLF;
            //The change
            decimal change_due = this.getPaymentTotal() - (this.getNSales() + this.getYSales());
            outstuff += OposPrinter.bold() + "    Kembali:" + String.Format("{0,27}", "Rp. " + change_due.ToString("###,###,###,##0")) + OposPrinter.bold_off() + OposPrinter.CRLF;
            //discount message
            if (discount_total > 0)
            {
                outstuff += OposPrinter.align_center() + "Anda Hemat Rp." + discount_total.ToString("###,###,###,###") + OposPrinter.CRLF
                         + OposPrinter.align_left() + OposPrinter.CRLF;
            }
          
            return outstuff;
        }

        private string print_footer()
        {
            string outstuff;



            outstuff = OposPrinter.align_center() + "Terima Kasih atas Kunjungan Anda." + OposPrinter.CRLF
                        + "Barang yang sudah dibeli tidak dapat" + OposPrinter.CRLF
                        + "ditukar atau dikembalikan." + OposPrinter.align_left() + OposPrinter.CRLF;

            outstuff += OposPrinter.CRLF + OposPrinter.CRLF + OposPrinter.CRLF + OposPrinter.CRLF + OposPrinter.CRLF + OposPrinter.CRLF + OposPrinter.CRLF;
            return outstuff;
        }

    }

    //Raw printer helper class for printing stuff
    public class RawPrinterHelper
    {
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        // SendBytesToPrinter()
        // When the function is given a printer name and an unmanaged array
        // of bytes, the function sends those bytes to the print queue.
        // Returns true on success, false on failure.
        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            di.pDocName = "My C#.NET RAW Document";
            di.pDataType = "RAW";

            // Open the printer.
            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write your bytes.
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (bSuccess == false)
            {
                dwError = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }

        public static bool SendFileToPrinter(string szPrinterName, string szFileName)
        {
            // Open the file.
            FileStream fs = new FileStream(szFileName, FileMode.Open);
            // Create a BinaryReader on the file.
            BinaryReader br = new BinaryReader(fs);
            // Dim an array of bytes big enough to hold the file's contents.
            Byte[] bytes = new Byte[fs.Length];
            bool bSuccess = false;
            // Your unmanaged pointer.
            IntPtr pUnmanagedBytes = new IntPtr(0);
            int nLength;

            nLength = Convert.ToInt32(fs.Length);
            // Read the contents of the file into the array.
            bytes = br.ReadBytes(nLength);
            // Allocate some unmanaged memory for those bytes.
            pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
            // Send the unmanaged bytes to the printer.
            bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
            // Free the unmanaged memory that you allocated earlier.
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return bSuccess;
        }
        public static bool SendStringToPrinter(string szPrinterName, string szString)
        {
            IntPtr pBytes;
            Int32 dwCount;
            // How many characters are in the string?
            dwCount = szString.Length;
            // Assume that the printer is expecting ANSI text, and then convert
            // the string to ANSI text.
            pBytes = Marshal.StringToCoTaskMemAnsi(szString);
            // Send the converted ANSI string to the printer.
            SendBytesToPrinter(szPrinterName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return true;
        }
    }
}
