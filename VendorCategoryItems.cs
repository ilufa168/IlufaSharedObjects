using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devart.Data.Oracle;

namespace IlufaSharedObjects
{

    public struct category_items
    {
        public string item_code;
        public string item_name;
        public string long_name
        {
            get { return "[" + item_code + "] - " + item_name; }
        }
    }

    public struct vendor_categories
    {
        public string category_code;
        public string category_description;
        public string category_title
        {
            get { return category_code + " - " + category_description; }
        }
        public List<category_items> items;

    }

    public class VendorCategoryItems
    {
        string vendor_code;
        string vendor_name;
        //string errors = "";
        List<vendor_categories> vendor_item_list = new List<vendor_categories>();

        public VendorCategoryItems()
        {
            this.vendor_code = "";
            this.vendor_name = "";

        }

        public string getSupplierCode()
        { return this.vendor_code; }
        
        public List<vendor_categories> getCategoryList()
        {
            return this.vendor_item_list;
        }

        public string getSupplierName()
        {
            return this.vendor_code + " - " + this.vendor_name;
        }
        public VendorCategoryItems(string v_code)
        {
            this.vendor_code = v_code;
            getVendorCategoryItems();
        }

        private bool getVendorCategoryItems()
        {
            bool rc = true;

            string sql = "SELECT        ERP.ITEM.ITEM_CODE, ERP.ITEM.ITEM_TYPE, ERP.ITEM.ITEM_NAME, ERP.ITEM.VENDOR_CODE, ERP.CODES.DESCRIPTION, ERP.CODES.CODE, ERP.VENDOR.VENDOR_CODE AS EXPR1, " +
                                       "ERP.VENDOR.VENDOR_NAME, ERP.CODES.KIND " +
                         "FROM          ERP.ITEM INNER JOIN " +
                                       "ERP.VENDOR ON ERP.ITEM.VENDOR_CODE = ERP.VENDOR.VENDOR_CODE INNER JOIN " +
                                       "ERP.CODES ON ERP.ITEM.CATEGORY = ERP.CODES.CODE " +
                         "WHERE        (ERP.VENDOR.VENDOR_CODE = :vc) AND (ERP.CODES.KIND = 'ITEM_CATEGORY') " +
                         "ORDER BY ERP.CODES.CODE, ERP.ITEM.ITEM_CODE";

            OracleCommand cmd = new OracleCommand(sql);
            cmd.Parameters.AddWithValue("vc", this.vendor_code);

            //OracleDataReader odr;

            //odr = OracleHandler.execReadQuery(cmd);
            //if (odr == null)
            //    rc=false;
            List<System.Object[]> raw = OracleHandler.execRetrieveReadQuery(cmd);
            if (raw.Count == 0)
                return false;

            //bool first = true;
            string last_cat = "";
            vendor_categories vc_read = new vendor_categories();
            vc_read.items = new List<category_items>();
            //while (odr.Read())
            //{

            //Get the first row and preload last_cat
            System.Object[] first_row = raw[0];
            last_cat = first_row[5].ToString();
            vendor_name = first_row[7].ToString();
            vc_read.category_code = first_row[5].ToString();
            vc_read.category_description = first_row[4].ToString();
            /*if (first)
            {
                this.vendor_name = odr.GetString(7);
                //last_cat = odr.GetString[]
                first = false;
            }*/
            foreach (System.Object[] a_row in raw)
            {
                string tmp_cat = a_row[5].ToString();
                if (!(tmp_cat == last_cat)) //cat changed do add to cat list
                {
                    this.vendor_item_list.Add(vc_read);
                    vc_read = new vendor_categories(); //reset the input list
                    vc_read.items = new List<category_items>();
                    last_cat = tmp_cat;
                    vc_read.category_code = tmp_cat;
                    vc_read.category_description = a_row[4].ToString();
                }
                //Add in the items
                category_items ci = new category_items();
                ci.item_code = a_row[0].ToString();
                ci.item_name = a_row[2].ToString();
                vc_read.items.Add(ci);
            }
            if (vc_read.items.Count > 0)
                this.vendor_item_list.Add(vc_read);
            /*
                //position 5 is the category code, see if we need a new object
                //string tmp_cat = odr.GetString(5);
                if (!(tmp_cat == last_cat)) //category changed
                {
                    if (last_cat != "")//Not the first time so write the category
                    {
                        this.vendor_item_list.Add(vc_read);
                        vc_read = new vendor_categories(); //reset the input list
                        vc_read.items = new List<category_items>();
                    }
                    last_cat = tmp_cat;
                    vc_read.category_code = tmp_cat;
                    vc_read.category_description = odr.GetString(4);
                }
                category_items ci = new category_items();
                ci.item_code = odr.GetString(0);
                ci.item_name = odr.GetString(2);
                vc_read.items.Add(ci);
                //Now fill in the item information
            //}*/


            return rc;
        }
    }
}
