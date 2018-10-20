using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devart.Data.Oracle;

namespace IlufaSharedObjects
{
    public class Vendor
    {
        public string vendor_name;

        public string vendor_code;


        public Vendor()
        {

        }

        public Vendor (string vc)
        {
            //try and load the vendor.  If failed, set code to 0
            this.vendor_code = vc;
            this.loadVendor();
        }

        private void loadVendor()
        {
                        string sql = "SELECT    VENDOR_CODE, VENDOR_NAME " +
                                     "FROM      VENDOR " +
                                     "WHERE     VENDOR_CODE = :vc";

            OracleCommand cmd = new OracleCommand(sql);
            cmd.Parameters.AddWithValue("vc", this.vendor_code);
            List<System.Object[]> raw = OracleHandler.execRetrieveReadQuery(cmd);
            //OracleDataReader odr;

            //odr = OracleHandler.execReadQuery(cmd);
            //if (odr == null || !odr.HasRows)
            if(raw.Count == 0)
            {
                this.vendor_code = "0";
                this.vendor_name = "VENDOR NOT FOUND";
                return;
            }

            //while (odr.Read())
            foreach (System.Object[] a_row in raw)
            {
                this.vendor_name = a_row[1].ToString();
            }
        }
    }
}
