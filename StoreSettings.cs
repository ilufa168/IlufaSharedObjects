using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using ilufapos;
using Devart.Data.Oracle;
using IlufaSharedObjects;
using System.Data;

namespace IlufaSharedObjects
{

    class StoreSettings
    {
        private string store_location;//
        private string store_address1;
        private string store_address2;
        private string store_name;

        private OracleHandler db;

        public StoreSettings(OracleHandler db,string location)
        {
            this.db = db;
            this.store_location = location;
            loadSettings();
        }

        public StoreSettings(string location)
        {
            this.store_location = location;
            loadSettings_static();
        }

        public string get_store_name() { return store_name; }

        public string get_address1() { return store_address1; }
        public string get_address2() { return store_address2; }

        public void loadSettings_static()
        {
            string sql = "SELECT POS_STORE_NAME,POS_ADDRESS_LINE_1,POS_ADDRESS_LINE_2 " +
                         "FROM   POS_STORE_SETTINGS " +
                         "WHERE  POS_LOKASI = :loc";

            OracleCommand cmd = new OracleCommand();

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("loc", this.store_location);

            List<System.Object[]> raw = OracleHandler.execRetrieveReadQuery(cmd);
            //OracleDataReader odr;

            //odr = OracleHandler.execReadQuery(cmd);

            //if (odr==null)
            //     return;

            //while (odr.Read())  
            foreach(System.Object[] a_row in raw)
            {
                this.store_name = a_row[0].ToString();
                this.store_address1 = a_row[1].ToString();
                this.store_address2 = a_row[2].ToString();
            }

            //odr.Dispose();
        }
        public void loadSettings()
        {
            string sql = "SELECT POS_STORE_NAME,POS_ADDRESS_LINE_1,POS_ADDRESS_LINE_2 " +
                         "FROM   POS_STORE_SETTINGS " +
                         "WHERE  POS_LOKASI = '" + this.store_location + "'";

            if (!db.DoQuery(sql))
            {
                //error = db.error.ToString() + sql;
            }

            if (!db.hasRows())
            {
                //error = "Bad User ID / Password Combination ";
            }

            OracleDataReader odr;

            odr = db.odr;
            //
            //There should only be one row of data, so parse the info into our class
            // (Maybe verify there is only one row?)
            while (odr.Read())
            {
                this.store_name = odr.GetString(0);
                this.store_address1 = odr.GetString(1);
                this.store_address2 = odr.GetString(2);
            }

            odr.Dispose();
        }

    }
}
