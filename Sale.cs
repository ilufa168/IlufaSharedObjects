using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Devart.Data.Oracle;
using System.IO;

//using ilufapos;

namespace IlufaSharedObjects
{

    public struct sales_item
    {
        public string item_code;
        public string item_name;
        public string vendor_code;
        public double unit_price;
        public string unit_discount;

        public override string ToString()
        {
            return item_code + " -- " + item_name;
        }
    }

    public struct supplier_item
    {
        public string vendor_code;
        public List<string> item_list;

        public supplier_item(string supplier)
        {
            this.vendor_code = supplier;
            this.item_list = new List<string>();
        }
    }

    public class Sale
    {
        public static string db_prefix = "ERP";
        private int sale_id=-1;
        private DateTime start_date = DateTime.Today;
        private DateTime end_date = DateTime.Today;
        //XDocument sale_parameters;
        private int sale_type;
        private int sale_source_type;
        private List<string> errors = new List<string>();

        public void set_start_date(DateTime start)
        {
            this.start_date = start;
        }

        public int get_sale_id() { return sale_id; }
        public void set_sale_id(int id) { this.sale_id = id; }
        public DateTime get_start_date() { return this.start_date; }

        public virtual List<supplier_item> get_keys()
        {
            return null;
        }

        public void add_error(string msg)
        {
            this.errors.Add(msg);
        }

        public string get_errors()
        {
            return this.errors.ToString();
        }

        public  virtual void addSupplier(Vendor v) { }

        public virtual void set_x_list(List<sales_item> lst_x) {}

        public virtual bool all_items(string vendor_code) { return false; }

        public virtual void set_y_list(List<sales_item> lst_y) { }

        public void set_end_date(DateTime end) { this.end_date = end; }

        public DateTime get_end_date() { return this.end_date; }

        public void set_sale_type(int i) { this.sale_type = i; }

        public int get_sale_type() { return this.sale_type; }

        public void set_sale_source_type(int i) { this.sale_source_type = i; }

        public int get_sale_source_type() { return this.sale_source_type; }

        public virtual void set_discount_value(double disc) { }

        public virtual double get_discount_value() { return 0; }

        public virtual void add_discount_key(string key) { }

        public virtual void add_discount_key(List<supplier_item> key) { }

        public virtual bool validate() { return true; }

        public virtual int get_quantity() { return 0; }

        public virtual void add_sale_parameter(string key, string value) { }

        public virtual void add_sale_parameter(string value) { }

        public virtual void add_sale_parameter(sales_item value) { }

        public virtual bool save() { return true; }

        public virtual void parse_parameters(string parms) { }

        public virtual string display_parameters()
        {
            return "";
        }

        public virtual void recalulate_combo_sale(BindingList<SalesOrder> all_sales) { }

        public static Sale FetchASale(List<Sale> sales_to_search, int sale_id)
        {
            Sale the_sale = new Sale();

            for (int i=0;i<sales_to_search.Count;i++)
            {
                if (sales_to_search[i].sale_id == sale_id)
                {
                    the_sale = sales_to_search[i];
                    break;
                }
            }

            return the_sale;
        }
        public static List<Sale> loadAllSales()
        {
            List<Sale> the_list = new List<Sale>();
            string sql = "select * from pos_sale " +
                //                         "where to_date('" + today.ToString("dd/MM/yyyy") + "','DD/MM/YYYY') " +
                         " where 1=1 order by pos_sale_id asc";
            OracleCommand cmd = new OracleCommand();

            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = sql;

            //OracleDataReader odr;

            //odr = OracleHandler.execReadQuery(cmd);

            //if (odr == null)
            //    return null;

            List<System.Object[]> raw_sales = OracleHandler.execRetrieveReadQuery(cmd);

            foreach(System.Object[] sale_row in raw_sales)
            {
                //first get the sale type (parm 4) to determine which class to make
                Sale sale_tmp = new Sale();
                decimal tmp;
                tmp = (decimal) sale_row[4];
                string cTmp = tmp.ToString();
                switch (cTmp)
                {
                    case "1":
                        sale_tmp = new PercentDiscountSale();
                        sale_tmp.set_sale_id(int.Parse(sale_row[0].ToString()));
                        sale_tmp.set_start_date((DateTime)sale_row[1]);
                        //sale_tmp.set_start_date((DateTime)sale_row[1]);
                        sale_tmp.set_end_date((DateTime)sale_row[2]);
                        //sale_tmp.set_end_date(odr.GetDateTime(2));
                        sale_tmp.set_sale_source_type(int.Parse(sale_row[3].ToString()));
                        //sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                        sale_tmp.set_sale_type(int.Parse(sale_row[4].ToString()));
                        //sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                        sale_tmp.parse_parameters(sale_row[5].ToString());
                        //sale_tmp.parse_parameters(odr.GetString(5));
                        break;
                    case "2":
                        sale_tmp = new BuyXGetYFreeSale();
                        sale_tmp.set_sale_id(int.Parse(sale_row[0].ToString()));
                        sale_tmp.set_start_date((DateTime)sale_row[1]);
                        sale_tmp.set_end_date((DateTime)sale_row[2]);
                        sale_tmp.set_sale_source_type(int.Parse(sale_row[3].ToString()));
                        sale_tmp.set_sale_type(int.Parse(sale_row[4].ToString()));
                        sale_tmp.parse_parameters(sale_row[5].ToString());
                        //sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                        //sale_tmp.set_start_date(odr.GetDateTime(1));
                        //sale_tmp.set_end_date(odr.GetDateTime(2));
                        //sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                        //sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                        //sale_tmp.parse_parameters(odr.GetString(5));
                        break;
                    case "3":
                        sale_tmp = new BuyXForPrice();
                        sale_tmp.set_sale_id(int.Parse(sale_row[0].ToString()));
                        sale_tmp.set_start_date((DateTime)sale_row[1]);
                        sale_tmp.set_end_date((DateTime)sale_row[2]);
                        sale_tmp.set_sale_source_type(int.Parse(sale_row[3].ToString()));
                        sale_tmp.set_sale_type(int.Parse(sale_row[4].ToString()));
                        sale_tmp.parse_parameters(sale_row[5].ToString());
                        //sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                        //sale_tmp.set_start_date(odr.GetDateTime(1));
                        //sale_tmp.set_end_date(odr.GetDateTime(2));
                        //sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                        //sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                        //sale_tmp.parse_parameters(odr.GetString(5));
                        break;
                    case "4":
                        sale_tmp = new FixedAmountSale();
                        sale_tmp.set_sale_id(int.Parse(sale_row[0].ToString()));
                        sale_tmp.set_start_date((DateTime)sale_row[1]);
                        sale_tmp.set_end_date((DateTime)sale_row[2]);
                        sale_tmp.set_sale_source_type(int.Parse(sale_row[3].ToString()));
                        sale_tmp.set_sale_type(int.Parse(sale_row[4].ToString()));
                        sale_tmp.parse_parameters(sale_row[5].ToString());
                        break;
                    default:
                        
                        break;


                }
                the_list.Add(sale_tmp);
            }
/*
            while (odr.Read())
            {
                
                //first get the sale type (parm 4) to determine which class to make
                Sale sale_tmp = new Sale();
                decimal tmp;
                tmp = odr.GetDecimal(4);
                string cTmp = tmp.ToString();
                switch (cTmp)
                {
                    case "1":
                        sale_tmp = new PercentDiscountSale();
                        sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                        sale_tmp.set_start_date(odr.GetDateTime(1));
                        sale_tmp.set_end_date(odr.GetDateTime(2));
                        sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                        sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                        sale_tmp.parse_parameters(odr.GetString(5));
                        break;
                    case "2":
                        sale_tmp = new BuyXGetYFreeSale();
                        sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                        sale_tmp.set_start_date(odr.GetDateTime(1));
                        sale_tmp.set_end_date(odr.GetDateTime(2));
                        sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                        sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                        sale_tmp.parse_parameters(odr.GetString(5));
                        break;
                    case "3":
                        sale_tmp = new BuyXForPrice();
                        sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                        sale_tmp.set_start_date(odr.GetDateTime(1));
                        sale_tmp.set_end_date(odr.GetDateTime(2));
                        sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                        sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                        sale_tmp.parse_parameters(odr.GetString(5));
                        break;

                }
                the_list.Add(sale_tmp);
                 
            }
            odr.Dispose();
*/
            return the_list;
        }
        
        public static List<Sale> load_current()
        {
            List<Sale> the_list = new List<Sale>();

            DateTime today = DateTime.Now;

            string sql = "select * from " + db_prefix + ".pos_sale " +
                //                         "where to_date('" + today.ToString("dd/MM/yyyy") + "','DD/MM/YYYY') " +
                         " where trunc(sysdate) " +
                         "between pos_sale_start_date and pos_sale_end_date order by pos_sale_id asc";

            OracleHandler oh = new OracleHandler();
            OracleDataReader odr;

            if (!oh.connect())
            {
                return the_list;
            }

            if (!oh.DoQuery(sql))
                return the_list;

            if (oh.getRowCount() >= 1)
            {
                odr = oh.odr;
                while (odr.Read())
                {
                    //first get the sale type (parm 4) to determine which class to make
                    Sale sale_tmp = new Sale();
                    decimal tmp;
                    tmp = odr.GetDecimal(4);
                    string cTmp = tmp.ToString();
                    switch (cTmp)
                    {
                        case "1":
                            sale_tmp = new PercentDiscountSale();
                            sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                            sale_tmp.set_start_date(odr.GetDateTime(1));
                            sale_tmp.set_end_date(odr.GetDateTime(2));
                            sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                            sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                            sale_tmp.parse_parameters(odr.GetString(5));
                            break;
                        case "2":
                            sale_tmp = new BuyXGetYFreeSale();
                            sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                            sale_tmp.set_start_date(odr.GetDateTime(1));
                            sale_tmp.set_end_date(odr.GetDateTime(2));
                            sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                            sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                            sale_tmp.parse_parameters(odr.GetString(5));
                            break;
                        case "3":
                            sale_tmp = new BuyXForPrice();
                            sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                            sale_tmp.set_start_date(odr.GetDateTime(1));
                            sale_tmp.set_end_date(odr.GetDateTime(2));
                            sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                            sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                            sale_tmp.parse_parameters(odr.GetString(5));
                            break;
                        case "4":
                            sale_tmp = new FixedAmountSale();
                            sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                            sale_tmp.set_start_date(odr.GetDateTime(1));
                            sale_tmp.set_end_date(odr.GetDateTime(2));
                            sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                            sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                            sale_tmp.parse_parameters(odr.GetString(5));
                            break;
                    }
                    the_list.Add(sale_tmp);
                }
                odr.Dispose();
            }
            oh.disconnect();

            return the_list;
            //            return true;
        }

        public static List<Sale> load_expired()
        {
            List<Sale> the_list = new List<Sale>();

            DateTime today = DateTime.Now;

            string sql = "select * from " + db_prefix + ".pos_sale " +
                //                         "where to_date('" + today.ToString("dd/MM/yyyy") + "','DD/MM/YYYY') " +
                         " where trunc(sysdate) " +
                         "> pos_sale_end_date order by pos_sale_id asc";

            OracleHandler oh = new OracleHandler();
            OracleDataReader odr;

            if (!oh.connect())
                return the_list;

            if (!oh.DoQuery(sql))
                return the_list;

            if (oh.getRowCount() >= 1)
            {
                odr = oh.odr;
                while (odr.Read())
                {
                    //first get the sale type (parm 4) to determine which class to make
                    Sale sale_tmp = new Sale();
                    decimal tmp;
                    tmp = odr.GetDecimal(4);
                    string cTmp = tmp.ToString();
                    switch (cTmp)
                    {
                        case "1":
                            sale_tmp = new PercentDiscountSale();
                            sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                            sale_tmp.set_start_date(odr.GetDateTime(1));
                            sale_tmp.set_end_date(odr.GetDateTime(2));
                            sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                            sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                            sale_tmp.parse_parameters(odr.GetString(5));
                            break;
                        case "2":
                            sale_tmp = new BuyXGetYFreeSale();
                            sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                            sale_tmp.set_start_date(odr.GetDateTime(1));
                            sale_tmp.set_end_date(odr.GetDateTime(2));
                            sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                            sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                            sale_tmp.parse_parameters(odr.GetString(5));
                            break;

                    }
                    the_list.Add(sale_tmp);
                    //                    ss = new sale_type();
                    //                    decimal tmp;
                    //                    tmp = odr.GetDecimal(0);
                    //                    ss.sale_type_id = (int)tmp;
                    //                    ss.sale_type_description = odr.GetString(1);
                    //                    st_id.Add(ss);
                }
                odr.Dispose();
            }
            oh.disconnect();

            return the_list;
            //            return true;
        }

        public static List<Sale> load_future()
        {
            List<Sale> the_list = new List<Sale>();

            DateTime today = DateTime.Now;

            string sql = "select * from " + db_prefix + ".pos_sale " +
                //                         "where to_date('" + today.ToString("dd/MM/yyyy") + "','DD/MM/YYYY') " +
                         " where trunc(sysdate) " +
                         "< pos_sale_start_date order by pos_sale_id asc";

            OracleHandler oh = new OracleHandler();
            OracleDataReader odr;

            if (!oh.connect())
                return the_list;

            if (!oh.DoQuery(sql))
                return the_list;

            if (oh.getRowCount() >= 1)
            {
                odr = oh.odr;
                while (odr.Read())
                {
                    //first get the sale type (parm 4) to determine which class to make
                    Sale sale_tmp = new Sale();
                    decimal tmp;
                    tmp = odr.GetDecimal(4);
                    string cTmp = tmp.ToString();
                    switch (cTmp)
                    {
                        case "1":
                            sale_tmp = new PercentDiscountSale();
                            sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                            sale_tmp.set_start_date(odr.GetDateTime(1));
                            sale_tmp.set_end_date(odr.GetDateTime(2));
                            sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                            sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                            sale_tmp.parse_parameters(odr.GetString(5));
                            break;
                        case "2":
                            sale_tmp = new BuyXGetYFreeSale();
                            sale_tmp.set_sale_id((int)odr.GetDecimal(0));
                            sale_tmp.set_start_date(odr.GetDateTime(1));
                            sale_tmp.set_end_date(odr.GetDateTime(2));
                            sale_tmp.set_sale_source_type((int)odr.GetDecimal(3));
                            sale_tmp.set_sale_type((int)odr.GetDecimal(4));
                            sale_tmp.parse_parameters(odr.GetString(5));
                            break;

                    }
                    the_list.Add(sale_tmp);
                    //                    ss = new sale_type();
                    //                    decimal tmp;
                    //                    tmp = odr.GetDecimal(0);
                    //                    ss.sale_type_id = (int)tmp;
                    //                    ss.sale_type_description = odr.GetString(1);
                    //                    st_id.Add(ss);
                }
                odr.Dispose();
            }
            oh.disconnect();

            return the_list;
            //            return true;
        }

        public static sales_item load_an_item(string item_code)
        {
            sales_item ret_item = new sales_item();

            ret_item.item_code = "NOT FOUND";

            string sql = "select item_code, item_name, vendor_code " +
                          "from erp.item " +
                          "where item_code = '" + item_code + "' ";

            OracleHandler oh = new OracleHandler();
            OracleDataReader odr;

            if (!oh.connect())
                return ret_item;

            if (!oh.DoQuery(sql))
                return ret_item;

            if (oh.getRowCount() >= 1)
            {
                odr = oh.odr;
                while (odr.Read())
                {
                    ret_item.item_code = odr.GetString(0);
                    ret_item.item_name = odr.GetString(1);
                    ret_item.vendor_code = odr.GetString(2);
                }
                odr.Dispose();
            }
            oh.disconnect();
            return ret_item;
        }

        public static bool delete_sale(int sale_id)
        {
            OracleHandler oh = new OracleHandler();
            string sql = "delete from " + db_prefix + ".pos_sale where pos_sale_id = " + sale_id.ToString();

            if (!oh.connect())
                return false;

            if (!oh.DoNonQuery(sql))
            {
                oh.disconnect();
                return false;
            }

            oh.disconnect();

            return true;
        }

        public virtual void reset_combo_sale(BindingList<SalesOrder> pos_items)
        {
        }

        public virtual void check_for_sale(item scanned, SalesOrder the_sale, BindingList<SalesOrder> other_sales)
        {
        }

        public virtual bool check_supplier_exists(Vendor v)
        {
            return false;
        }
        public virtual void check_for_sale(item scanned, int idx, BindingList<SalesOrder> other_sales)
        {

        }

        public virtual bool check_for_sale(string vendor, string category, string item)
        {
            return false;
        }

        public _Sale MakeBindingListItem()
        {
            _Sale bl_sale = new _Sale(this.sale_id,this.start_date,this.end_date,this.sale_type.ToString(),this.sale_source_type.ToString(),this.display_parameters());

            return bl_sale;

        }

        public virtual string getSupplierString()
        {
            return "";
        }

        public virtual string getItemString()
        {
            return "";
        }

        static public List<_Sale_Type> loadSalesTypes()
        {
            List<_Sale_Type> all_types = new List<_Sale_Type>();
            string sql = "select * from pos_sale_type " +
                         "order by pos_sale_type_id asc";
            OracleCommand cmd = new OracleCommand();

            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = sql;

            //OracleDataReader odr;

            //odr = OracleHandler.execReadQuery(cmd);
            List<System.Object[]> raw = OracleHandler.execRetrieveReadQuery(cmd);

            foreach (System.Object[] a_row in raw)
            {
                _Sale_Type a_type = new _Sale_Type();
                double tmp = double.Parse(a_row[0].ToString());
                a_type.setSaleType(tmp);
                //a_type.pos_sale_type_id = tmp;
                string s_tmp = a_row[1].ToString();
                a_type.setSaleTypeDesc(s_tmp);
                all_types.Add(a_type);
            }

/*            while (odr.Read())
            {

                _Sale_Type a_type = new _Sale_Type();
                double tmp = double.Parse(odr.GetString(0));
                a_type.setSaleType(tmp);
                //a_type.pos_sale_type_id = tmp;
                string s_tmp = odr.GetString(1);
                a_type.setSaleTypeDesc(s_tmp);
                all_types.Add(a_type);
            }
            odr.Dispose();
            */
            return all_types;
        }
    }

    public class PercentDiscountSale : Sale
    //This will always be a x-1 relationship, with one or more keys of the same type 
    //(supplier, category, item, etc) and a single discount
    {


        private double discount_pct;
        public List<supplier_item> discount_key = new List<supplier_item>();

        public static PercentDiscountSale loadASale(int sale_id)
        {
            PercentDiscountSale a_sale = new PercentDiscountSale();
            a_sale.set_sale_id(-1);

            string sql = "select * from pos_sale " +
                        " where pos_sale_id = :id ";

            OracleCommand cmd = new OracleCommand(sql);
            cmd.Parameters.AddWithValue("id", sale_id);

            List<System.Object[]> raw = OracleHandler.execRetrieveReadQuery(cmd);

            //OracleDataReader odr;

            //odr = OracleHandler.execReadQuery(cmd);
            //if (odr == null)
            //    return a_sale;

            //odr.Read();
            System.Object[] the_one_row = raw[0];
            a_sale.set_sale_id(int.Parse(the_one_row[0].ToString()));
            a_sale.set_start_date(DateTime.Parse(the_one_row[1].ToString()));
            a_sale.set_end_date(DateTime.Parse(the_one_row[2].ToString()));
            a_sale.set_sale_source_type(int.Parse(the_one_row[3].ToString()));
            a_sale.set_sale_type(int.Parse(the_one_row[4].ToString()));
            a_sale.parse_parameters(the_one_row[5].ToString());

            return a_sale;
                         
        }

        public override bool check_supplier_exists(Vendor v)
        {
            bool response = false;
            foreach (supplier_item si in discount_key)
            {
                if (si.vendor_code == v.vendor_code)
                    return true;
            }
            return response;
        }

        public override void addSupplier(Vendor v)
        {
            supplier_item si = new supplier_item();
            si.item_list = new List<string>();
            si.vendor_code = v.vendor_code;

            this.discount_key.Add(si);
        }

        public override string getItemString()
        {
            string the_items = "";
            foreach (supplier_item tmp in discount_key)
            {
                foreach (string item_code in tmp.item_list)
                {
                    the_items += "|" + item_code;
                }
            }

            return the_items;
        }

        public override void add_discount_key(List<supplier_item> key)
        {
            this.discount_key = key;
        }

        public override string getSupplierString()
        {
            string suppliers = "";

            foreach (supplier_item tmp in discount_key)
            {
                suppliers += "|" + tmp.vendor_code;
            }
            return suppliers;
        }

        public override bool check_for_sale(string vendor,string category, string item)
        {
            bool result = false;
            //Loop through the vendors in the discount key
            foreach (supplier_item sale_key in discount_key)
            {
                if (sale_key.vendor_code == vendor)
                //We have a match on supplier, look for the specific item id
                {
                    bool found = false;
                    int cnt = 0;
                    while (!found && cnt < sale_key.item_list.Count)
                    {
                        if (sale_key.item_list[cnt] == item || sale_key.item_list[cnt] == "ALL")
                        {
                            result = true;
                            break;
                        }
                        cnt++;
                    }
                }

            }
            return result;
        }
        //
        //Added for Ilufapos
        //
        public override void check_for_sale(item scanned, SalesOrder the_sale, BindingList<SalesOrder> other_sales)
        {
            //Loop through the vendors in the discount key
            foreach (supplier_item sale_key in discount_key)
            {
                if (sale_key.vendor_code == scanned.supplier)
                //We have a match on supplier, look for the specific item id
                {
                    bool found = false;
                    int cnt = 0;
                    while (!found && cnt < sale_key.item_list.Count)
                    {
                        if (sale_key.item_list[cnt] == scanned.item_id || sale_key.item_list[cnt] == "ALL")
                        {
                            found = true;
                            the_sale.discount = Math.Round(the_sale.unit_price * this.discount_pct / 100);
                            the_sale.discount_desc = "Sale Item (" + this.discount_pct.ToString() + "%)";
                        }
                        cnt++;
                    }
                }
            }
        }

        public override List<supplier_item> get_keys()
        {
            return this.discount_key;
        }

        public override double get_discount_value()
        {
            return this.discount_pct;
        }

        public override bool all_items(string vendor_code)
        {
            bool result = false;
       
            foreach (supplier_item item in this.discount_key)
            {
                if (item.item_list.Count > 0)
                {
                    if ((item.vendor_code == vendor_code) && (item.item_list[0] == "ALL"))
                        result = true;
                }
            }

            return result;
        }

        public override void set_discount_value(double discount)
        {
            this.discount_pct = discount;
        }

        public override bool validate()
        {
            return base.validate();
        }

        public void build_sale_parameters_from_tree()
        {

        }

        public override void add_sale_parameter(sales_item an_item)
        {
            //Look and see if the supplier exists, if so add the specific item
            //otherwise, create a new supplier_item and add it
            //discount_key.Add(value);
            supplier_item match;
            match = discount_key.Find(
                    delegate(supplier_item tmp)
                    {
                        return tmp.vendor_code == an_item.vendor_code;
                    }
                    );
            if (match.vendor_code == null)
            {
                match = new supplier_item(an_item.vendor_code);
                match.item_list.Add(an_item.item_code);
                discount_key.Add(match);
                return;
            }
            //Match contains the vendor, so add the item if its not there
            List<string> tmp_list = match.item_list;

            if (match.item_list.Exists(
                delegate(string str_tmp)
                {
                    return str_tmp == an_item.item_code;
                }))
                return;

            match.item_list.Add(an_item.item_code);

        }

        private string get_sale_parameters()
        {
            XDocument parms = new XDocument();

            XElement root = new XElement("Parameters");

            root.Add(new XElement("amount", this.discount_pct));

            foreach (supplier_item si in this.discount_key)
            {
                XElement sub_vendor = new XElement("vendor", new XElement("vendor_code", si.vendor_code));
                //Now a sub element for each item
                foreach (string an_item in si.item_list)
                {
                    XElement sub_item = new XElement("item", an_item);
                    sub_vendor.Add(sub_item);
                }

                root.Add(sub_vendor);
            }
            //            discount_key.ForEach(delegate (string key)
            //            {
            //                XElement sub = new XElement("key", key);
            //                root.Add(sub);
            //            });



            parms.Add(root);
            return parms.ToString();
        }

        private void parse_old_parms(XDocument xparms)
        {
            XElement top = xparms.Element("Parameters");
            this.set_discount_value(double.Parse(top.Element("amount").Value));
            string vendor_code = top.Element("key").Value;
            supplier_item si = new supplier_item(vendor_code);
            si.item_list.Add("ALL");
            this.discount_key.Add(si);
        }

        public override void parse_parameters(string parms)
        {
            XDocument xparms = new XDocument();
            xparms = XDocument.Parse(parms);

            //Hack to handle legacy sales
            if (this.get_sale_source_type() == 1)
            {
                parse_old_parms(xparms);
                return;
            }

            XElement top = xparms.Element("Parameters");
            //Get the keys from the list
            IEnumerable<XElement> elements =
            from el in top.Elements("vendor")
            select el;

            foreach (XElement el in elements)
            {
                string key = el.Element("vendor_code").Value.ToString();
                supplier_item si = new supplier_item(key);

                IEnumerable<XElement> item_elements =
                from subel in el.Elements("item")
                select subel;

                foreach (XElement subel in item_elements)
                {
                    si.item_list.Add(subel.Value.ToString());
                }

                this.discount_key.Add(si);
            }
            //                Console.WriteLine(el);
            string tmp_amt = top.Element("amount").Value.ToString();
            tmp_amt = tmp_amt.Replace(",", ".");
            //decimal amount = decimal.Parse(top.Element("amount").Value);
            System.Globalization.CultureInfo culInfo = new System.Globalization.CultureInfo("en-US");
            decimal amount = decimal.Parse(tmp_amt, System.Globalization.NumberStyles.Number, culInfo);
            //this.set_discount_value (double) (decimal.Parse(top.Element("amount").Value));
            this.set_discount_value((double) amount);

        }

        public override string display_parameters()
        {
            string value, keylist;

            keylist = "[Suppliers](items): ";
            value = this.discount_pct.ToString();
            foreach (supplier_item supplier in discount_key)
            {
                keylist = keylist + "|[" + supplier.vendor_code + "]";
                foreach (string item in supplier.item_list)
                {
                    keylist = keylist + "(" + item + ")";
                }
                keylist = keylist + "|";
            }
            return value + "% for " + keylist;
        }

        public override bool save()
        {
            OracleHandler oh = new OracleHandler();
            bool ret = true;

            //If we dont have a sale id, do an insert, otherwise, update
            if (this.get_sale_id() > 0)
                return this.updateSale();

            int sale_id = -1;
            //Try and connect to the database
            if (!oh.connect())
            {
                ret = false;
                this.add_error(oh.error.ToString());
                oh.disconnect();
            }

            //Now insert the record into the database.
            //step 1 get the next sequence for the sale id
            OracleDataReader odr;
            string sql = "select " + db_prefix + ".seq_pos_sale_id.nextval from dual";

            if (!oh.DoQuery(sql))
            {
                this.add_error(oh.error.ToString());
                oh.disconnect();
                return false;
            }

            if (oh.getRowCount() > 1)
            {
                odr = oh.odr;
                odr.Read();
                sale_id = ((int)odr.GetDecimal(0));

            }

            sql = "insert into " + db_prefix + "." + "pos_sale (pos_sale_id, pos_sale_start_date,pos_sale_end_date," +
                                               "pos_sale_source_type,pos_sale_type,pos_sale_parameters) values(" +
                                               sale_id.ToString() + "," +
                //                                               "seq_pos_sale_id.nextval, " +
                                               "to_date('" + this.get_start_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY')," +
                                               "to_date('" + this.get_end_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY')," +
                                               this.get_sale_source_type().ToString() + "," +
                                               this.get_sale_type().ToString() + ",:parms)";
            //                                                   +
            //                                              "'" + this.get_sale_parameters() + "')";

            oh.InsertClob(sql, this.get_sale_parameters());


            return ret;
        }

        public bool updateSale()
        {
            bool ret = true;

            string sql = "UPDATE        POS_SALE " +
                         "SET           POS_SALE_START_DATE = to_date('" + this.get_start_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY'), " +
                                       "POS_SALE_END_DATE = to_date('" + this.get_end_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY'), " +
                                      " POS_SALE_SOURCE_TYPE = :source" +
                                      ",POS_SALE_TYPE = :type " +
                                      ", POS_SALE_PARAMETERS  = :parms " +
                         "WHERE         POS_SALE_ID = :id ";

            OracleCommand cmd = new OracleCommand(sql);
            //cmd.Parameters.AddWithValue("start", this.get_start_date());
            //cmd.Parameters.AddWithValue("end", this.get_end_date());

            cmd.Parameters.AddWithValue("source", this.get_sale_source_type());
            cmd.Parameters.AddWithValue("type", this.get_sale_type());
//            cmd.Parameters.AddWithValue("parms", this.get_sale_parameters());

            cmd.Parameters.AddWithValue("id", this.get_sale_id());

            //add in the parameters, clob so a little more complicated
            
                    //                    byte[] byte_parms = System.Text.Encoding.Unicode.GetBytes(text);
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(this.get_sale_parameters());
            MemoryStream stream = new MemoryStream(bytes);
            BinaryReader r = new BinaryReader(stream);
            //Transfer data to parm
            int streamLength = (int)bytes.Length;
            OracleLob myLob = new OracleLob(OracleDbType.Clob);
            myLob.Write(r.ReadBytes(streamLength), 0, streamLength);

            OracleParameter myParam = cmd.Parameters.Add("parms", OracleDbType.Clob);
                    //                    OracleParameter myParam = myCommand.Parameters.Add("parms", text);
            myParam.OracleValue = myLob;

            OracleHandler.execNonReadQuery(cmd);

            return ret;
        }
    }

    public class BuyXGetYFreeSale : Sale
    {
        private List<sales_item> lst_x_items = new List<sales_item>();
        private List<sales_item> lst_y_items = new List<sales_item>();
        private int x_count;
        private int y_count;
        private double discount_pct;

        public override void set_discount_value(double discount)
        {
            this.discount_pct = discount;
        }

        public override double get_discount_value() { return this.discount_pct; }
        public override void set_x_list(List<sales_item> lst_x)
        {
            this.lst_x_items = lst_x;
        }

        public override void set_y_list(List<sales_item> lst_y)
        {
            this.lst_y_items = lst_y;
        }

        public void set_xy(int x, int y)
        {
            this.x_count = x;
            this.y_count = y;
        }

        private string get_sale_parameters()
        {
            XDocument parms = new XDocument();

            XElement root = new XElement("Parameters");

            root.Add(new XElement("amount", this.discount_pct));

            root.Add(new XElement("x_val", this.x_count));
            root.Add(new XElement("y_val", this.y_count));

            XElement xlist = new XElement("x_list");
            foreach (sales_item si in this.lst_x_items)
            {
                XElement sales_item = new XElement("sales_item", new XElement("item_code", si.item_code),
                                                                 new XElement("item_name", si.item_name),
                                                                 new XElement("vendor_code", si.vendor_code));
                xlist.Add(sales_item);
            }

            root.Add(xlist);

            //Y list
            XElement ylist = new XElement("y_list");
            foreach (sales_item si in this.lst_y_items)
            {
                XElement sales_item = new XElement("sales_item", new XElement("item_code", si.item_code),
                                                                 new XElement("item_name", si.item_name),
                                                                 new XElement("vendor_code", si.vendor_code));
                ylist.Add(sales_item);
            }

            root.Add(ylist);
            //            discount_key.ForEach(delegate (string key)
            //            {
            //                XElement sub = new XElement("key", key);
            //                root.Add(sub);
            //            });



            parms.Add(root);
            return parms.ToString();
        }

        public override string display_parameters()
        {
            string value, keylist;

            keylist = " x:";
            value = "buy " + this.x_count + " get " + this.y_count + "@" + this.discount_pct.ToString() + "%disc";

            foreach (sales_item si in this.lst_x_items)
            {
                keylist = keylist + "(" + si.item_code + ")";
            }
            keylist = keylist + " y:";
            foreach (sales_item si in this.lst_y_items)
            {
                keylist = keylist + "(" + si.item_code + ")";
            }

            return value + keylist;
        }

        public override void parse_parameters(string parms)
        {
            XDocument xparms = new XDocument();
            xparms = XDocument.Parse(parms);

            XElement top = xparms.Element("Parameters");
            this.set_discount_value(double.Parse(top.Element("amount").Value));
            this.set_xy(int.Parse(top.Element("x_val").Value), int.Parse(top.Element("y_val").Value));

            XElement xEleXlist = top.Element("x_list");
            //Get the xlist from the xml
            IEnumerable<XElement> elements =
            from el in xEleXlist.Elements("sales_item")
            select el;


            foreach (XElement el in elements)
            {
                sales_item si = new sales_item();
                si.item_code = el.Element("item_code").Value;
                si.item_name = el.Element("item_name").Value;
                si.vendor_code = el.Element("vendor_code").Value;

                this.lst_x_items.Add(si);
            }
            //Get the ylist from the xml
            XElement yEleXlist = top.Element("y_list");

            elements =
            from el in yEleXlist.Elements("sales_item")
            select el;


            foreach (XElement el in elements)
            {
                sales_item si = new sales_item();
                si.item_code = el.Element("item_code").Value;
                si.item_name = el.Element("item_name").Value;
                si.vendor_code = el.Element("vendor_code").Value;

                this.lst_y_items.Add(si);
            }
            Console.WriteLine("BLAH");

            //            this.set_discount_value(double.Parse(top.Element("amount").Value));

        }

        public bool updateSale()
        {
            bool ret = true;

            string sql = "UPDATE        POS_SALE " +
                         "SET           POS_SALE_START_DATE = :start" +
                                      " POS_SALE_END_DATE = :end" +
                                      " POS_SALE_SOURCE_TYPE = :source" +
                                      " POS_SALE_TYPE = :type" +
                                      " POS_SALE_PARAMETERS  = :parms" +
                         "WHERE         POS_SALE_ID = :id ";

            OracleCommand cmd = new OracleCommand(sql);
            cmd.Parameters.AddWithValue("start", this.get_start_date());
            cmd.Parameters.AddWithValue("end", this.get_end_date());

            cmd.Parameters.AddWithValue("source", this.get_sale_source_type());
            cmd.Parameters.AddWithValue("type", this.get_sale_type());
            cmd.Parameters.AddWithValue("parms", this.get_sale_parameters());

            cmd.Parameters.AddWithValue("id", this.get_sale_id());

            OracleHandler.execNonReadQuery(cmd);

            return ret;
        }

        public override bool save()
        {
            OracleHandler oh = new OracleHandler();
            bool ret = true;

            //If we dont have a sale id, do an insert, otherwise, update
            if (this.get_sale_id() > 0)
                return this.updateSale();

            int sale_id = -1;
            //Try and connect to the database
            if (!oh.connect())
            {
                ret = false;
                this.add_error(oh.error.ToString());
                oh.disconnect();
            }

            //Now insert the record into the database.
            //step 1 get the next sequence for the sale id
            OracleDataReader odr;
            string sql = "select seq_pos_sale_id.nextval from dual";

            if (!oh.DoQuery(sql))
            {
                this.add_error(oh.error.ToString());
                oh.disconnect();
                return false;
            }

            if (oh.getRowCount() > 0)
            {
                odr = oh.odr;
                odr.Read();
                sale_id = ((int)odr.GetDecimal(0));

            }

            sql = "insert into " + db_prefix + "." + "pos_sale (pos_sale_id, pos_sale_start_date,pos_sale_end_date," +
                                               "pos_sale_source_type,pos_sale_type,pos_sale_parameters) values(" +
                                               sale_id.ToString() + "," +
                //                                               "seq_pos_sale_id.nextval, " +
                                               "to_date('" + this.get_start_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY')," +
                                               "to_date('" + this.get_end_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY')," +
                                               this.get_sale_source_type().ToString() + "," +
                                               this.get_sale_type().ToString() + ",:parms)";
            //                                                   +
            //                                              "'" + this.get_sale_parameters() + "')";

            oh.InsertClob(sql, this.get_sale_parameters());
            //            if (!oh.DoNonQuery(sql))
            //            {
            //                ret = false;
            //                this.add_error(oh.error.ToString());
            //                oh.disconnect();
            //            }
            ////                sql="select news_text from table_nm +
            //    "WHERE field_nm'" + field_id + "' FOR UPDATE";
            //            sql = "select pos_sale_parameters " +
            //                  "from " + db_prefix + "." + "pos_sale " +
            //                  "where pos_sale_id = " + sale_id.ToString() + " FOR UPDATE";

            //            if (!oh.InsertClob(sql,this.get_sale_parameters()))
            //            {
            //                ret = false;
            //                this.add_error(oh.error.ToString());
            //                oh.disconnect();
            //            }

            return ret;
        }

        private bool check_for_x(string item_id)
        {
            foreach (sales_item si in this.lst_x_items)
            {
                if (item_id == si.item_code)
                {
                    return true;
                }
            }
            return false;
        }

        private bool check_for_y(string item_id)
        {
            foreach (sales_item si in this.lst_y_items)
            {
                if (item_id == si.item_code)
                {
                    //Its a Y, we need a total x count
//                    MessageBox.Show("We Have an Y:" + item_id);
                    return true;
                }
            }
            return false;
        }
        private int count_x(BindingList<SalesOrder> so)
        {
            int cnt = 0;
            foreach (SalesOrder a_so in so)
            {
                if (check_for_x(a_so.item_id))
                    cnt = cnt + a_so.quantity;
            }
            return cnt;
        }

        public override void check_for_sale(item scanned, SalesOrder the_sale, BindingList<SalesOrder> other_sales)
        {
            //Loop through the x list in the sale, if found in the list, check the other sales for a count
            bool found = false;
            if (check_for_x(scanned.item_id))
            {
                found = true;
            }
            else
            {
                found = check_for_y(scanned.item_id);
            }

            if (!found) //Nothing to do, not part of this sale
                return;

            //Ok, we have a new X or Y item, need to recalculate this sale
            //reset any existing sales that match this id
            //Create an ordered list for Y items, ordered by price
            List<sales_item> x = new List<sales_item>();
            List<sales_item> y = new List<sales_item>();
            
            this.reset_sale(other_sales);

            if (check_for_x(the_sale.item_id)) // New sale on an X item, add it to the list
            {
                this.add_to_xy_list(the_sale, x);
            }
            if (check_for_y(the_sale.item_id))
            {
                this.add_to_xy_list(the_sale, y);
            }
            //Now add the other sales already posted
            foreach (SalesOrder so in other_sales)
            {
                if (check_for_x(so.item_id)) // New sale on an X item, add it to the list
                {
                    this.add_to_xy_list(so, x);
                }
                if (check_for_y(so.item_id))
                {
                    this.add_to_xy_list(so, y);
                }

            }
            //Now check if the x count is > than the # needed for a discount
            //if xlistcount > x count
            // get most expensive x
            // remove it from y list if it exists
            // remove from x list
            // repeat until we get x_count # of items
            // If a y list item is left, find the least expensive and discount it.  If 
            // existing discount > new discount, look for another y
            // Found a y, apply the discount to that item on the sales order. if multiple rows on 
            // Sales order, will have to split one off and discount the new one
            if (x.Count < x_count)
                return; //not enough x yet

            int sale_x_cnt = 0;
            while (x.Count() > 0)
            {
                int idx = -1;

                idx = find_highest(x);
                if (idx >= 0)
                {
                    sale_x_cnt++;
                    sales_item si = x[idx];
                    x.RemoveAt(idx);
                    //Look for this x in the y list, if its there, remove it
                    if (check_for_y(si.item_code))
                    {
                        idx = find_index(si.item_code,y);
                        if (idx > -1)
                            y.RemoveAt(idx);
                    }
                    if (sale_x_cnt == x_count) // We have enough for a sale, so time to get a y
                    {
                        idx = find_lowest(y);
                        if (idx < 0) //No useable Y items, might as well quit the loops
                            break;
                        si = y[idx];
                        
                        //
                        //Look for a matching sales order item 
                        //first check the sale
                        bool sale_item_found = false;
                        double disc;
                        while (!sale_item_found && y.Count() > 0)
                        {
                            y.RemoveAt(idx);
                            if (the_sale.item_id == si.item_code)
                            {
                                disc = the_sale.unit_price * this.get_discount_value() / 100;
                                if (disc > the_sale.discount)
                                {
                                    sale_item_found = true;
                                    the_sale.discount = disc;
                                    the_sale.discount_desc = "Buy " + x_count.ToString() + " Get 1 @ " + this.get_discount_value().ToString() + "% off";
                                    the_sale.discount_id = this.get_sale_id().ToString();
                                    sale_x_cnt = 0;
                                }
                            }
                            //its not the new item, have to check all the scanned ones
                            if (!sale_item_found)
                            {
                                //look through the items already scanned for a match
                                //If there is none, have to go to the next y
                                for (int i = 0; i < other_sales.Count(); i++)
                                {
                                    if (other_sales[i].item_id == si.item_code && other_sales[i].discount_id != this.get_sale_id().ToString())
                                    {
                                        //matching item, check the discount, remember to account for the quantity
                                        disc = the_sale.unit_price * this.get_discount_value() / 100;
                                        if (disc > (other_sales[i].discount))
                                        {
                                            sale_item_found = true;
                                            other_sales[i].discount = disc;
                                            other_sales[i].discount_desc = "Buy " + x_count.ToString() + " Get 1 @ " + this.get_discount_value().ToString() + "% off";
                                            other_sales[i].discount_id = this.get_sale_id().ToString();
                                            sale_x_cnt = 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return;
        }

        private int find_index(string item, List<sales_item> lst)
        {
            int ret = -1;
            for (int i = 0; i < lst.Count(); i++)
            {
                if (item == lst[i].item_code)
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }

        private int find_lowest(List<sales_item> lst)
        {
            int ret = -1;
            double max = 999999999;
            //Quick pass through the list to find the one with the highest price
            for (int i = 0; i < lst.Count(); i++)
            {
                if (lst[i].unit_price < max)
                {
                    ret = i;
                    max = lst[i].unit_price;
                }
            }
            return ret;
        }


        private int find_highest(List<sales_item> lst)
        {
            int ret = -1;
            double max = -1;
            //Quick pass through the list to find the one with the highest price
            for (int i = 0; i < lst.Count(); i++)
            {
                if (lst[i].unit_price > max)
                {
                    ret = i;
                    max = lst[i].unit_price;
                }
            }
            return ret;
        }

        private void add_to_xy_list(SalesOrder the_sale,List<sales_item> x)
        {
            for (int i = 0; i < the_sale.quantity; i++)
            {
                sales_item si = new sales_item();
                si.item_code = the_sale.item_id;
                si.unit_price = the_sale.unit_price;
                x.Add(si);
            }
        }

        private void add_a_y(SalesOrder the_sale, List<sales_item> y)
        {
            for (int i = 0; i < the_sale.quantity; i++)
            {
                sales_item si = new sales_item();
                si.item_code = the_sale.item_id;
                si.unit_price = the_sale.unit_price;
                y.Add(si);
            }
        }

        private void reset_sale(BindingList<SalesOrder> sales)
        {
            foreach (SalesOrder so in sales)
            {
                if (so.discount_id == this.get_sale_id().ToString())
                {
                    so.discount_id = "";
                    so.discount = 0;
                    so.discount_desc = "";
                }
            }
        }


    }
    //
    // This sale is to buy a quantity of items for a fixed price
    // For example buy 3 shirts for rp 50,000
    // Has fields quantity and price, and the key is a list of item codes
    //
    public class BuyXForPrice:Sale
    {
        private double fixed_amt = 0;
        private int required_qty=0;

        public List<supplier_item> discount_key = new List<supplier_item>();
        public override List<supplier_item> get_keys()
        {
            return this.discount_key;
        }

        private bool is_item_for_sale(item scanned)
        {
            bool on_sale = false;
            //Loop through the vendors in the discount key
            foreach (supplier_item sale_key in discount_key)
            {
                if (sale_key.vendor_code == scanned.supplier)
                //We have a match on supplier, look for the specific item id
                {

                    int cnt = 0;
                    while (!on_sale && cnt < sale_key.item_list.Count)
                    {
                        if (sale_key.item_list[cnt] == scanned.item_id || sale_key.item_list[cnt] == "ALL")
                        {
                            on_sale = true;
                        }
                        cnt++;
                    }
                }
            }
            return on_sale;
        }
        public override void reset_combo_sale(BindingList<SalesOrder> pos_items)
        {
            foreach (SalesOrder so in pos_items)
            {
                if ((so.discount_id == null) || (!int.TryParse(so.discount_id, out int rslt)))
                    continue;

                if (int.Parse(so.discount_id) == get_sale_id())
                {
                    so.skip_notify = true;
                    so.discount_id = null;
                    so.discount_desc = "";
                    so.discount = 0;
                    so.skip_notify = false;
                }
            }
        }

        public override void recalulate_combo_sale(BindingList<SalesOrder> all_sales)
        {
            BindingList<SalesOrder> discount_items = new BindingList<SalesOrder>();
            List<supplier_item> keys = this.get_keys();
            //check to see if the item is in the sales order
            foreach(SalesOrder so in all_sales)
            {
                foreach(supplier_item si in keys)
                {
                    if (si.item_list.Contains(so.item_id) && so.discount_id == null)
                        discount_items.Add(so);
                }

            }
            //get the total count of items qualifying for the discount
            int disc_qty = discount_items.Sum(s =>
                {
                    return s.quantity;

                }
            );
            while (disc_qty >= required_qty)
            //we have enough for a sale, make it so
            {
                int count = 0;
                double cumulative_cost = 0;
                //foreach (SalesOrder so in discount_items)
                for (int i= 0;i<discount_items.Count();i++)
                {
                    //count += so.quantity;
                    if (disc_qty < required_qty)
                        break;

                    if (count + discount_items[i].quantity > required_qty)
                    {
                        int cnt_short = required_qty  - count;
                        int cnt_extra = discount_items[i].quantity - cnt_short;
                        //We have a sale with a count of > required quantity.  Change the number in this row to
                        //how many we need and add a new row with whats left
                        SalesOrder new_so = new SalesOrder();
                        new_so.CopyMeWithNewQty(discount_items[i], cnt_extra);
                        discount_items.Add(new_so);
                        all_sales.Add(new_so);
                        discount_items[i].quantity = cnt_short;
                        //Add in the cost
                        cumulative_cost += discount_items[i].quantity * discount_items[i].unit_price;
                        discount_items[i].discount_id = get_sale_id().ToString();
                        discount_items[i].discount_desc = "Beli Banyak Hemat";
                        discount_items[i].discount = ZeroCheck(((double)(cumulative_cost - this.getFixedPrice())/ discount_items[i].quantity));
                        cumulative_cost = 0;
                        disc_qty -= required_qty;
                        count = 0;
                    }
                    else
                    if (count + discount_items[i].quantity == required_qty)  //this row is exactly what we need!!!!
                    {
                        cumulative_cost += discount_items[i].quantity * discount_items[i].unit_price;
                        discount_items[i].discount_id = get_sale_id().ToString();
                        discount_items[i].discount_desc = "Beli Banyak Hemat";
                        discount_items[i].discount = ZeroCheck(((double)(cumulative_cost - this.getFixedPrice()) / discount_items[i].quantity));
                        disc_qty -= required_qty;
                        cumulative_cost = 0;
                        count = 0;
                    }
                    else
                    {   //not enough items yet, go on to the next but mark this one as part of the sale
                        count += discount_items[i].quantity;
                        cumulative_cost += discount_items[i].quantity * discount_items[i].unit_price;
                        discount_items[i].discount_id = get_sale_id().ToString();
                        discount_items[i].discount_desc = "Beli Banyak Hemat";
                        discount_items[i].discount = 0;
                    }
                }
            }
                  
        }

        private double ZeroCheck(double discount)
        {
            if (discount > 0)
                return discount;

            return 0;
        }

        public override void check_for_sale(item scanned, int idx, BindingList<SalesOrder> other_sales)
        {
            /*
            //idx contains the row that was changed, its all we need to focus on for checking the sale

            //Is the item even on sale?
            if (!is_item_for_sale(scanned))
                return;
            //Ok the item is for sale, we have the record changed, idx, and all the sales orders.
            //since the quantity changed, we will have to completely recalculate this sale
            //Get all the rows with an id in the sales key
            int endit = other_sales.Count();
            int this_sale = get_sale_id();
            BindingList<SalesOrder> matching_sales = new BindingList<SalesOrder>();
            int item_count = 0;
            for (int i = 0; i < endit; i++)
            {
                foreach(supplier_item sale_key in discount_key)
                {
                    int lcnt = 0;
                    bool found = false;
                    while (!found && lcnt < sale_key.item_list.Count)
                    {
                        if (sale_key.item_list[lcnt] == other_sales[i].item_id)
                        {
                            found = true;
                            item_count += other_sales[i].quantity;
                            if (other_sales[i].quantity > 1)
                            {
                                //Break this saleorder into multiple SalesOrder Records
                                while (other_sales[i].quantity > 0)
                                {
                                    SalesOrder tmp_so = new SalesOrder();
                                    tmp_so.CopyMeWithNewQty(other_sales[i], 1);
                                    other_sales[i].quantity--;
                                    matching_sales.Add(tmp_so);
                                }
                                //Reset the other_sales[i] qty to 1
                                other_sales[i].quantity = 1;
                            }

                            //matching_sales.Add(other_sales[i]);
                        }
                        lcnt++;
                    }
                }
               
            }

            if (item_count < get_quantity()) 
                return;
            //Calculate any sales that shoud occur now
            //make sure we account for other_sales[i] on the first occurance of a sale
            //otherwise we will create an extra row
            //Last add the matching sales into other sales
            bool first_sale = true;
            int qty_needed = get_quantity();
            int cnt = 0;
            //item_count contains the remaining number of items

            decimal cumulative_cost = 0;
            foreach(SalesOrder so in matching_sales)
            {
                //if there is not enough left no need to continue iterating
                if (item_count < qty_needed)
                {
                    other_sales.Add(so);
                    continue;
                }

                cnt++;

                if (cnt==qty_needed) //time to make a sale
                {
                    if (first_sale)  //use the original row for the first occurance of the sale
                    {
                        cumulative_cost += (decimal)other_sales[idx].unit_price;
                        decimal total_cost_to_be = (decimal)(this.getFixedPrice());
                        other_sales[idx].discount_id = get_sale_id().ToString();
                        other_sales[idx].discount_desc = "Here is the discount";
                        other_sales[idx].discount = (double)(cumulative_cost - total_cost_to_be);
                        first_sale = false;
                    }
                    else
                    {
                        cumulative_cost += (decimal)so.unit_price;
                        decimal total_cost_to_be = (decimal)(this.getFixedPrice());
                        so.discount_id = get_sale_id().ToString();
                        so.discount_desc = "Here is the discount";
                        so.discount = (double)(cumulative_cost - total_cost_to_be);
                        other_sales.Add(so);
                    }
                    item_count -= qty_needed;
                    cumulative_cost = 0;
                    cnt = 0;
                } else
                {
                    //mark it as modified by the sale and give it an id
                    so.discount_id = this.get_sale_id().ToString();
                    so.discount_desc = "Put something here";
                    cumulative_cost += (decimal)so.unit_price;
                    other_sales.Add(so);
                }
                //All the 
                
            }
//
//            int matching_item_qty = 0;
//            BindingList<SalesOrder> current_sale_list = new BindingList<SalesOrder>();
//            foreach(SalesOrder a_sale in matching_sales)
//            {
                //check to see if we have enough to make a sale item
//                matching_item_qty += a_sale.quantity;
//                current_sale_list.Add(a_sale);
//                while (matching_item_qty >= this.required_qty)
//                {
//                    if (current_sale_list.Count > 1) //We took more than 1 row to make a sale
//                    {
//
//                   }
//                    else //we have enough in this row to handle at least one sale
//                    {
//
//                    }
 //               }
//            }
*/
        }

        public override void check_for_sale(item scanned, SalesOrder the_sale, BindingList<SalesOrder> other_sales)
        {
            /*
            bool found = false;
            //Loop through the vendors in the discount key
            foreach (supplier_item sale_key in discount_key)
            {
                if (sale_key.vendor_code == scanned.supplier)
                //We have a match on supplier, look for the specific item id
                {
                    
                    int cnt = 0;
                    while (!found && cnt < sale_key.item_list.Count)
                    {
                        if (sale_key.item_list[cnt] == scanned.item_id || sale_key.item_list[cnt] == "ALL")
                        {
                            found = true;
                        }
                        cnt++;
                    }
                }

                if (!found)
                    return;  //No Sale

                //this was a sale item, so not we need to search other_sales to see of there are more
                BindingList<SalesOrder> sales_matches = new BindingList<SalesOrder>();
                foreach (SalesOrder a_sale in other_sales)
                {
                    bool match = false;
                    int cnt = 0;
                    int matching_item_count = 0;
                    
                    while (!match && cnt < sale_key.item_list.Count())
                    {
                        if (a_sale.item_id == sale_key.item_list[cnt] && a_sale.discount_id == null)  //Match on sale id, but not in any other sale yet
                        {
                            sales_matches.Add(a_sale);
                            matching_item_count += a_sale.quantity;
                            match = true;
                        }
                        cnt++;
                    }
                }
                if (sales_matches.Count >= get_quantity() -1) // If we have enough sale items, time to discount
                {
                    //Mark the first n-1 items with this sale id.  Then calculate the discount
                    decimal cumulative_cost = 0;
                    for (int i = 0; i < get_quantity() - 1; i++)
                    {
                        sales_matches[i].discount_id = this.get_sale_id().ToString();
                        sales_matches[i].discount_desc = "Put something here";
                        cumulative_cost += (decimal)sales_matches[i].unit_price;
                    }

                    //now calculate the discout for the current item
                    cumulative_cost += (decimal) the_sale.unit_price;
                    decimal total_cost_to_be = (decimal)(this.getFixedPrice());
                    the_sale.discount_id = get_sale_id().ToString();
                    the_sale.discount_desc = "Here is the discount";
                    the_sale.discount = (double)(  cumulative_cost - total_cost_to_be );
                    
                }
            }
            */
        }

        public override void addSupplier(Vendor v)
        {
            supplier_item si = new supplier_item();
            si.item_list = new List<string>();
            si.vendor_code = v.vendor_code;

            this.discount_key.Add(si);
        }

        public override bool check_supplier_exists(Vendor v)
        {
            bool response = false;
            foreach (supplier_item si in discount_key)
            {
                if (si.vendor_code == v.vendor_code)
                    return true;
            }
            return response;
        }
        public void setQuantity(int qty)
        {
            required_qty = qty;
        }

        public override int get_quantity() { return required_qty; }

        public double getFixedPrice() { return fixed_amt; }

        public void setFixedPrice(double price)
        {
            this.fixed_amt = price;
        }

        public override void add_discount_key(List<supplier_item> key)
        {
            this.discount_key = key;
        }

        public override string display_parameters()
        {
            string value, keylist;

            keylist = "[Suppliers](items): ";
            value = this.fixed_amt.ToString();
            foreach (supplier_item supplier in discount_key)
            {
                keylist = keylist + "|[" + supplier.vendor_code + "]";
                foreach (string item in supplier.item_list)
                {
                    keylist = keylist + "(" + item + ")";
                }
                keylist = keylist + "|";
            }
            return "Buy " + get_quantity() + " for " + getFixedPrice() + " Rp. " + keylist;
        }

        public override void parse_parameters(string parms)
        {
            XDocument xparms = new XDocument();
            xparms = XDocument.Parse(parms);

            XElement top = xparms.Element("Parameters");
            //Get the keys from the list
            IEnumerable<XElement> elements =
            from el in top.Elements("vendor")
            select el;

            foreach (XElement el in elements)
            {
                string key = el.Element("vendor_code").Value.ToString();
                supplier_item si = new supplier_item(key);

                IEnumerable<XElement> item_elements =
                from subel in el.Elements("item")
                select subel;

                foreach (XElement subel in item_elements)
                {
                    si.item_list.Add(subel.Value.ToString());
                }

                this.discount_key.Add(si);
            }
            //                Console.WriteLine(el);
            this.setFixedPrice(double.Parse(top.Element("fixed_amount").Value));
            this.setQuantity(int.Parse(top.Element("quantity").Value));
        }

        public bool updateSale()
        {
            bool ret = true;

            string sql = "UPDATE        POS_SALE " +
                         "SET           POS_SALE_START_DATE = to_date('" + this.get_start_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY'), " +
                                       "POS_SALE_END_DATE = to_date('" + this.get_end_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY'), " +
                                      " POS_SALE_SOURCE_TYPE = :source" +
                                      ",POS_SALE_TYPE = :type " +
                                      ", POS_SALE_PARAMETERS  = :parms " +
                         "WHERE         POS_SALE_ID = :id ";

            OracleCommand cmd = new OracleCommand(sql);
            //cmd.Parameters.AddWithValue("start", this.get_start_date());
            //cmd.Parameters.AddWithValue("end", this.get_end_date());

            cmd.Parameters.AddWithValue("source", this.get_sale_source_type());
            cmd.Parameters.AddWithValue("type", this.get_sale_type());
            //            cmd.Parameters.AddWithValue("parms", this.get_sale_parameters());

            cmd.Parameters.AddWithValue("id", this.get_sale_id());

            //add in the parameters, clob so a little more complicated

            //                    byte[] byte_parms = System.Text.Encoding.Unicode.GetBytes(text);
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(this.get_sale_parameters());
            MemoryStream stream = new MemoryStream(bytes);
            BinaryReader r = new BinaryReader(stream);
            //Transfer data to parm
            int streamLength = (int)bytes.Length;
            OracleLob myLob = new OracleLob(OracleDbType.Clob);
            myLob.Write(r.ReadBytes(streamLength), 0, streamLength);

            OracleParameter myParam = cmd.Parameters.Add("parms", OracleDbType.Clob);
            //                    OracleParameter myParam = myCommand.Parameters.Add("parms", text);
            myParam.OracleValue = myLob;

            OracleHandler.execNonReadQuery(cmd);

            return ret;
        }

        public override bool save()
        {
            string parms = this.get_sale_parameters();

            OracleHandler oh = new OracleHandler();
            bool ret = true;

            //If we dont have a sale id, do an insert, otherwise, update
            if (this.get_sale_id() > 0)
                return this.updateSale();

            int sale_id = -1;
            //Try and connect to the database
            if (!oh.connect())
            {
                ret = false;
                this.add_error(oh.error.ToString());
                oh.disconnect();
                return ret;
            }

            //Now insert the record into the database.
            //step 1 get the next sequence for the sale id
            OracleDataReader odr;
            string sql = "select " + db_prefix + ".seq_pos_sale_id.nextval from dual";

            if (!oh.DoQuery(sql))
            {
                this.add_error(oh.error.ToString());
                oh.disconnect();
                return false;
            }

            if (oh.getRowCount() > 1)
            {
                odr = oh.odr;
                odr.Read();
                sale_id = ((int)odr.GetDecimal(0));

            }

            sql = "insert into pos_sale (pos_sale_id, pos_sale_start_date,pos_sale_end_date," +
                                               "pos_sale_source_type,pos_sale_type,pos_sale_parameters) values(" +
                                               sale_id.ToString() + "," +
                                               "to_date('" + this.get_start_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY')," +
                                               "to_date('" + this.get_end_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY')," +
                                               this.get_sale_source_type().ToString() + "," +
                                               this.get_sale_type().ToString() + ",:parms)";
            //                                                   +
            //                                              "'" + this.get_sale_parameters() + "')";

            return (oh.InsertClob(sql, get_sale_parameters()));

        }

        private string get_sale_parameters()
        {
            XDocument parms = new XDocument();

            XElement root = new XElement("Parameters");

            root.Add(new XElement("fixed_amount", this.fixed_amt));
            root.Add(new XElement("quantity", this.required_qty));

            foreach (supplier_item si in this.discount_key)
            {
                XElement sub_vendor = new XElement("vendor", new XElement("vendor_code", si.vendor_code));
                //Now a sub element for each item
                foreach (string an_item in si.item_list)
                {
                    XElement sub_item = new XElement("item", an_item);
                    sub_vendor.Add(sub_item);
                }

                root.Add(sub_vendor);
            }
            //            discount_key.ForEach(delegate (string key)
            //            {
            //                XElement sub = new XElement("key", key);
            //                root.Add(sub);
            //            });



            parms.Add(root);
            return parms.ToString();
        }

        public override bool check_for_sale(string vendor, string category, string item)
        {
            bool result = false;
            //Loop through the vendors in the discount key
            foreach (supplier_item sale_key in discount_key)
            {
                if (sale_key.vendor_code == vendor)
                //We have a match on supplier, look for the specific item id
                {
                    bool found = false;
                    int cnt = 0;
                    while (!found && cnt < sale_key.item_list.Count)
                    {
                        if (sale_key.item_list[cnt] == item || sale_key.item_list[cnt] == "ALL")
                        {
                            result = true;
                            break;
                        }
                        cnt++;
                    }
                }

            }
            return result;
        }

    }
    /// <summary>
    /// Class to handle fixed amount sales
    /// 
    /// </summary>
    public class FixedAmountSale : Sale
    {
        
        private double discount_amount;
        public List<supplier_item> discount_key = new List<supplier_item>();

        public static FixedAmountSale loadASale(int sale_id)
        {
            FixedAmountSale a_sale = new FixedAmountSale();
            a_sale.set_sale_id(-1);

            string sql = "select * from pos_sale " +
                        " where pos_sale_id = :id ";

            OracleCommand cmd = new OracleCommand(sql);
            cmd.Parameters.AddWithValue("id", sale_id);
            List<System.Object[]> raw = OracleHandler.execRetrieveReadQuery(cmd);
            //OracleDataReader odr;

            //odr = OracleHandler.execReadQuery(cmd);
            //if (odr == null)
            //return a_sale;

            //odr.Read();
            System.Object[] the_one_row = raw[0];
            a_sale.set_sale_id(int.Parse(the_one_row[0].ToString()));
            a_sale.set_start_date(DateTime.Parse(the_one_row[1].ToString()));
            a_sale.set_end_date(DateTime.Parse(the_one_row[2].ToString()));
            a_sale.set_sale_source_type(int.Parse(the_one_row[3].ToString()));
            a_sale.set_sale_type(int.Parse(the_one_row[4].ToString()));
            a_sale.parse_parameters(the_one_row[5].ToString());
            //a_sale.set_sale_id((int)odr.GetDecimal(0));
            //a_sale.set_start_date(odr.GetDateTime(1));
            //a_sale.set_end_date(odr.GetDateTime(2));
            //a_sale.set_sale_source_type((int)odr.GetDecimal(3));
            //a_sale.set_sale_type((int)odr.GetDecimal(4));
            //a_sale.parse_parameters(odr.GetString(5));

            return a_sale;

        }

        public override void addSupplier(Vendor v)
        {
            supplier_item si = new supplier_item();
            si.item_list = new List<string>();
            si.vendor_code = v.vendor_code;

            this.discount_key.Add(si);
        }
        public override bool check_supplier_exists(Vendor v)
        {
            bool response = false;
            foreach (supplier_item si in discount_key)
            {
                if (si.vendor_code == v.vendor_code)
                    return true;
            }
            return response;
        }
        public override string getItemString()
        {
            string the_items = "";
            foreach (supplier_item tmp in discount_key)
            {
                foreach (string item_code in tmp.item_list)
                {
                    the_items += "|" + item_code;
                }
            }

            return the_items;
        }

        public override void add_discount_key(List<supplier_item> key)
        {
            this.discount_key = key;
        }

        public override string getSupplierString()
        {
            string suppliers = "";

            foreach (supplier_item tmp in discount_key)
            {
                suppliers += "|" + tmp.vendor_code;
            }
            return suppliers;
        }

        public override bool check_for_sale(string vendor, string category, string item)
        {
            bool result = false;
            //Loop through the vendors in the discount key
            foreach (supplier_item sale_key in discount_key)
            {
                if (sale_key.vendor_code == vendor)
                //We have a match on supplier, look for the specific item id
                {
                    bool found = false;
                    int cnt = 0;
                    while (!found && cnt < sale_key.item_list.Count)
                    {
                        if (sale_key.item_list[cnt] == item || sale_key.item_list[cnt] == "ALL")
                        {
                            result = true;
                            break;
                        }
                        cnt++;
                    }
                }

            }
            return result;
        }
        //
        //Added for Ilufapos
        //
        public override void check_for_sale(item scanned, SalesOrder the_sale, BindingList<SalesOrder> other_sales)
        {
            //Loop through the vendors in the discount key
            foreach (supplier_item sale_key in discount_key)
            {
                if (sale_key.vendor_code == scanned.supplier)
                //We have a match on supplier, look for the specific item id
                {
                    bool found = false;
                    int cnt = 0;
                    while (!found && cnt < sale_key.item_list.Count)
                    {
                        if (sale_key.item_list[cnt] == scanned.item_id || sale_key.item_list[cnt] == "ALL")
                        {
                            found = true;
                            the_sale.discount = this.discount_amount;
                            the_sale.discount_desc = "Sale Item (" + discount_amount.ToString() + " Rp.)";
                        }
                        cnt++;
                    }
                }
            }
        }

        public override List<supplier_item> get_keys()
        {
            return this.discount_key;
        }
        public override double get_discount_value()
        {
            return this.discount_amount;
        }

        public override bool all_items(string vendor_code)
        {
            bool result = false;

            foreach (supplier_item item in this.discount_key)
            {
                if (item.item_list.Count > 0)
                {
                    if ((item.vendor_code == vendor_code) && (item.item_list[0] == "ALL"))
                        result = true;
                }
            }

            return result;
        }
        public override void set_discount_value(double discount)
        {
            this.discount_amount = discount;
        }

        public override bool validate()
        {
            return base.validate();
        }

        public void build_sale_parameters_from_tree()
        {

        }

        public override void add_sale_parameter(sales_item an_item)
        {
            //Look and see if the supplier exists, if so add the specific item
            //otherwise, create a new supplier_item and add it
            //discount_key.Add(value);
            supplier_item match;
            match = discount_key.Find(
                    delegate(supplier_item tmp)
                    {
                        return tmp.vendor_code == an_item.vendor_code;
                    }
                    );
            if (match.vendor_code == null)
            {
                match = new supplier_item(an_item.vendor_code);
                match.item_list.Add(an_item.item_code);
                discount_key.Add(match);
                return;
            }
            //Match contains the vendor, so add the item if its not there
            List<string> tmp_list = match.item_list;

            if (match.item_list.Exists(
                delegate(string str_tmp)
                {
                    return str_tmp == an_item.item_code;
                }))
                return;

            match.item_list.Add(an_item.item_code);

        }

        private string get_sale_parameters()
        {
            XDocument parms = new XDocument();

            XElement root = new XElement("Parameters");

            root.Add(new XElement("amount", this.discount_amount));

            foreach (supplier_item si in this.discount_key)
            {
                XElement sub_vendor = new XElement("vendor", new XElement("vendor_code", si.vendor_code));
                //Now a sub element for each item
                foreach (string an_item in si.item_list)
                {
                    XElement sub_item = new XElement("item", an_item);
                    sub_vendor.Add(sub_item);
                }

                root.Add(sub_vendor);
            }

            parms.Add(root);
            return parms.ToString();
        }

        private void parse_old_parms(XDocument xparms)
        {
            XElement top = xparms.Element("Parameters");
            this.set_discount_value(double.Parse(top.Element("amount").Value));
            string vendor_code = top.Element("key").Value;
            supplier_item si = new supplier_item(vendor_code);
            si.item_list.Add("ALL");
            this.discount_key.Add(si);
        }

        public override void parse_parameters(string parms)
        {
            XDocument xparms = new XDocument();
            xparms = XDocument.Parse(parms);

            //Hack to handle legacy sales
            if (this.get_sale_source_type() == 1)
            {
                parse_old_parms(xparms);
                return;
            }

            XElement top = xparms.Element("Parameters");
            //Get the keys from the list
            IEnumerable<XElement> elements =
            from el in top.Elements("vendor")
            select el;

            foreach (XElement el in elements)
            {
                string key = el.Element("vendor_code").Value.ToString();
                supplier_item si = new supplier_item(key);

                IEnumerable<XElement> item_elements =
                from subel in el.Elements("item")
                select subel;

                foreach (XElement subel in item_elements)
                {
                    si.item_list.Add(subel.Value.ToString());
                }

                this.discount_key.Add(si);
            }
            //                Console.WriteLine(el);

            this.set_discount_value(double.Parse(top.Element("amount").Value));

        }

        public override string display_parameters()
        {
            string value, keylist;

            keylist = "[Suppliers](items): ";
            value = this.discount_amount.ToString();
            foreach (supplier_item supplier in discount_key)
            {
                keylist = keylist + "|[" + supplier.vendor_code + "]";
                foreach (string item in supplier.item_list)
                {
                    keylist = keylist + "(" + item + ")";
                }
                keylist = keylist + "|";
            }
            return value + " Rp. for " + keylist;
        }

        public override bool save()
        {
            OracleHandler oh = new OracleHandler();
            bool ret = true;

            //If we dont have a sale id, do an insert, otherwise, update
            if (this.get_sale_id() > 0)
                return this.updateSale();

            int sale_id = -1;
            //Try and connect to the database
            if (!oh.connect())
            {
                ret = false;
                this.add_error(oh.error.ToString());
                oh.disconnect();
            }

            //Now insert the record into the database.
            //step 1 get the next sequence for the sale id
            OracleDataReader odr;
            string sql = "select seq_pos_sale_id.nextval from dual";

            if (!oh.DoQuery(sql))
            {
                this.add_error(oh.error.ToString());
                oh.disconnect();
                return false;
            }

            if (oh.getRowCount() > 1)
            {
                odr = oh.odr;
                odr.Read();
                sale_id = ((int)odr.GetDecimal(0));

            }

            sql = "insert into " + db_prefix + "." + "pos_sale (pos_sale_id, pos_sale_start_date,pos_sale_end_date," +
                                               "pos_sale_source_type,pos_sale_type,pos_sale_parameters) values(" +
                                               sale_id.ToString() + "," +
                                               "to_date('" + this.get_start_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY')," +
                                               "to_date('" + this.get_end_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY')," +
                                               this.get_sale_source_type().ToString() + "," +
                                               this.get_sale_type().ToString() + ",:parms)";
            //                                                   +
            //                                              "'" + this.get_sale_parameters() + "')";

            oh.InsertClob(sql, this.get_sale_parameters());


            return ret;
        }

        public bool updateSale()
        {
            bool ret = true;

            string sql = "UPDATE        POS_SALE " +
                         "SET           POS_SALE_START_DATE = to_date('" + this.get_start_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY'), " +
                                       "POS_SALE_END_DATE = to_date('" + this.get_end_date().ToString("dd/MM/yyyy") + "','DD/MM/YYYY'), " +
                                      " POS_SALE_SOURCE_TYPE = :source" +
                                      ",POS_SALE_TYPE = :type " +
                                      ", POS_SALE_PARAMETERS  = :parms " +
                         "WHERE         POS_SALE_ID = :id ";

            OracleCommand cmd = new OracleCommand(sql);
            //cmd.Parameters.AddWithValue("start", this.get_start_date());
            //cmd.Parameters.AddWithValue("end", this.get_end_date());

            cmd.Parameters.AddWithValue("source", this.get_sale_source_type());
            cmd.Parameters.AddWithValue("type", this.get_sale_type());
            //            cmd.Parameters.AddWithValue("parms", this.get_sale_parameters());

            cmd.Parameters.AddWithValue("id", this.get_sale_id());

            //add in the parameters, clob so a little more complicated

            //                    byte[] byte_parms = System.Text.Encoding.Unicode.GetBytes(text);
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(this.get_sale_parameters());
            MemoryStream stream = new MemoryStream(bytes);
            BinaryReader r = new BinaryReader(stream);
            //Transfer data to parm
            int streamLength = (int)bytes.Length;
            OracleLob myLob = new OracleLob(OracleDbType.Clob);
            myLob.Write(r.ReadBytes(streamLength), 0, streamLength);

            OracleParameter myParam = cmd.Parameters.Add("parms", OracleDbType.Clob);
            //                    OracleParameter myParam = myCommand.Parameters.Add("parms", text);
            myParam.OracleValue = myLob;

            OracleHandler.execNonReadQuery(cmd);

            return ret;
        }
    }

    public class _Sale_Type
    {
        private double _pos_sale_type_id;
        private string _pos_sale_type_desc;

        public double pos_sale_type_id
        {
            get { return _pos_sale_type_id; }
            set { _pos_sale_type_id = pos_sale_type_id; }
        }

        public string pos_sale_type_desc
        {
            get { return _pos_sale_type_desc; }
            set { _pos_sale_type_desc = pos_sale_type_desc; }
        }

        public void setSaleType(double type){
            this._pos_sale_type_id = type;
        }

        public void setSaleTypeDesc(string desc)
        {
            this._pos_sale_type_desc = desc;
        }

    }

    //Display class for sale to make data grids work pretty
    public class _Sale
    {
        private int _sale_id;
        private DateTime _start_date;
        private DateTime _end_date;
        private string _sale_type;
        private string _sale_source_type;
        private string _parameters;

        public _Sale(int id,
                     DateTime start,
                     DateTime end,
                     string sale_type,
                     string source_type,
                     string desc
            )
        {
            _sale_id = id;
            _start_date = start;
            _end_date = end;
            _sale_type = sale_type;
            _sale_source_type = source_type;
            _parameters = desc;
        }

        public int sale_id
        {
            get { return _sale_id; }
            set { _sale_id = sale_id; }
        }

        public DateTime start_date
        {
            get { return _start_date; }
            set { _start_date = start_date; }
        }

        public DateTime end_date
        {
            get { return _end_date; }
            set { _end_date = end_date; }
        }

        public string parameters
        {
            get { return _parameters; }
            set { _parameters = parameters; }
        }

        public string sale_type
        {
            get
            {
                switch (_sale_type)
                {
                    case "1":
                        return ("% Discount");
                    case "2":
                        return ("Buy X Get Y Free");
                    case "3":
                        return ("Buy X for Rp. Y");
                    case "4":
                        return ("Flat Rp. Discount");
                }
                return "Test";
            }
            set { _sale_type = sale_type; }
        }

        public string sale_source_type
        {
            get
            {
                switch (_sale_source_type)
                {
                    case "1":
                        return ("By Supplier");
                }
                return "Unknown";
            }
            set { _sale_source_type = sale_source_type; }
        }
    }

}
