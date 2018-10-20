using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devart.Data.Oracle;
using System.Configuration;
using System.IO;

namespace IlufaSharedObjects
{
    public class OracleHandler
    {

        public OracleConnection ocl;
        public OracleDataReader odr;

        string sHostIp
        {
            get;
            set;
        }
        string sPort
        {
            get;
            set;
        }
        string sUserId
        {
            get;
            set;
        }
        string sPassword
        {
            get;
            set;
        }
        public string sConnect;
        public Exception error;

        public static string GetDBUserName()
        {
            return (ConfigurationManager.AppSettings["OracleUID"]);
        }

        public OracleHandler(string host = "192.168.1.3", string port = "1521", string id="KEITH", string pw="KEITH")
        {
            //Get the values from the app.config
            OracleConnectionStringBuilder oraCSB = new OracleConnectionStringBuilder();
            oraCSB.Direct = true;
            oraCSB.Server = ConfigurationManager.AppSettings["OracleIP"];
            oraCSB.Port = int.Parse(ConfigurationManager.AppSettings["OraclePort"]);
            oraCSB.Sid = "orcl";
            oraCSB.UserId = ConfigurationManager.AppSettings["OracleUID"]; 
            oraCSB.Password = ConfigurationManager.AppSettings["OraclePW"];
            oraCSB.ConnectionTimeout = 30;
            this.ocl = new OracleConnection(oraCSB.ConnectionString);
//            try
//            {
//                this.ocl.Open();
//            }

//            catch (Exception ex)
//            {
//                this.error = ex;
//                System.IO.File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\pos_error.txt", ex.ToString() + "(" + oraCSB.ToString() + ")");
                //                this.error.Message = this.error.Message + oradb;
//                return;
//            }
        }

        ~OracleHandler()
        {
            this.ocl.Close();
        }

        public static bool execNonReadQuery(OracleCommand cmd)
        {
            bool result = true;

            OracleConnection oc = static_connect();
            cmd.ParameterCheck = true;

            if (oc == null)
                return false;

            cmd.Connection = oc;
            try
            {
                cmd.ExecuteNonQuery();
            }

            catch (Exception ex)
            {
                string error = ex.ToString();
                oc.Close();
                return false;
            }
            oc.Close();
            return result;
        }

        public static OracleDataReader zexecReadQuery(OracleCommand cmd)
        {
            OracleDataReader result = null;

            OracleConnection oc = static_connect();

            if (oc == null)
                return result;

            cmd.Connection = oc;
            try
            {
                result = cmd.ExecuteReader();
            }

            catch (Exception ex)
            {
                string error = ex.ToString();
                oc.Close();
                return null;
            }


            oc.Close();
            return result;
        }

        public static List<System.Object[]> execRetrieveReadQuery(OracleCommand cmd)
        {
            OracleDataReader result = null;
            List<System.Object[]> all_data = new List<object[]>();

            OracleConnection oc = static_connect();

            if (oc == null)
                return all_data;

            cmd.Connection = oc;
            try
            {
                result = cmd.ExecuteReader();
            }

            catch (Exception ex)
            {
                string error = ex.ToString();
                oc.Close();
                return null;
            }

            //Load the data into a var to process later

            while (result.Read())
            {
                System.Object[] the_data = new System.Object[result.FieldCount];
                int aa = result.GetValues(the_data);
                all_data.Add(the_data);
            }
            oc.Close();
            return all_data;
        }
        public static OracleConnection static_connect()
        {
            OracleConnection oc = null;
            OracleConnectionStringBuilder oraCSB = new OracleConnectionStringBuilder();
            oraCSB.Direct = true;
            oraCSB.Server = ConfigurationManager.AppSettings["OracleIP"];
            oraCSB.Port = int.Parse(ConfigurationManager.AppSettings["OraclePort"]);
            oraCSB.Sid = "orcl";
            oraCSB.UserId = ConfigurationManager.AppSettings["OracleUID"];
            oraCSB.Password = ConfigurationManager.AppSettings["OraclePW"];
            oraCSB.ConnectionTimeout = 1000;
            oc = new OracleConnection(oraCSB.ConnectionString);
            try
            {
                oc.Open(); 
            }

            catch
            {
                //this.error = ex;
                return oc = null;
            }
            return oc;

        }

        public bool isConnected()
        {
//            System.Data.
            if (ocl.State == System.Data.ConnectionState.Open)
            {
                return true;
            }
            return false;
        }

        public bool connect()
        {
            ocl.Close();
            OracleConnectionStringBuilder oraCSB = new OracleConnectionStringBuilder();
            oraCSB.Direct = true;
            oraCSB.Server = ConfigurationManager.AppSettings["OracleIP"];
            oraCSB.Port = int.Parse(ConfigurationManager.AppSettings["OraclePort"]);
            oraCSB.Sid = "orcl";
            oraCSB.UserId = ConfigurationManager.AppSettings["OracleUID"];
            oraCSB.Password = ConfigurationManager.AppSettings["OraclePW"];
            oraCSB.ConnectionTimeout = 1000;
            this.ocl = new OracleConnection(oraCSB.ConnectionString);
            try
            {
                this.ocl.Open();
            }

            catch (Exception ex)
            {
                this.error = ex;
                System.IO.File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\"+ System.AppDomain.CurrentDomain.FriendlyName +  "_error.txt", ex.ToString() + "(" + oraCSB.ToString() + ")");
                //                this.error.Message = this.error.Message + oradb;
                return false;
            }


            return true;

        }

        public bool disconnect()
        {
            this.ocl.Close();
            return true;
        }

        public string getRow(int i)
        {

//            odr.Re
            string ret = null;

            try
            {
                this.ocl.Open();
            }

            catch (Exception ex)
            {
                this.error = ex;
                return ret;
            }


            return ret;
        }

        public Boolean DoQuery(string query)
        {

            OracleCommand cmd = new OracleCommand();
//            OracleDataReader dr;
            cmd.Connection = this.ocl;

            cmd.CommandText = query;

            cmd.CommandType = System.Data.CommandType.Text;

            try
            {
                odr = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                this.error = ex;
                return (false);
            }

            return (true);

        }

        public Boolean DoNonQuery(string query,OracleCommand cmd)
        {

            //OracleCommand cmd = new OracleCommand();
            //            OracleDataReader dr;
            cmd.Connection = this.ocl;

            cmd.CommandText = query;

            cmd.CommandType = System.Data.CommandType.Text;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                this.error = ex;
                return (false);
            }

            cmd.Dispose();

            return (true);

        }

        public Boolean DoNonQuery(string query)
        {

            OracleCommand cmd = new OracleCommand();
            //            OracleDataReader dr;
            cmd.Connection = this.ocl;

            cmd.CommandText = query;

            cmd.CommandType = System.Data.CommandType.Text;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                this.error = ex;
                return (false);
            }

            cmd.Dispose();

            return (true);

        }

        public void release()
        {
            odr.Dispose();

            return;
        }

        public bool hasRows()
        {
            return odr.HasRows;
        }

        public Boolean InsertClob(string sql, string text)
        {
            if (sql.Length > 0 && text.Length > 0)
            {
                if (ocl.State.ToString().Equals("Open"))
                {
                    //                    byte[] byte_parms = System.Text.Encoding.Unicode.GetBytes(text);
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(text);
                    MemoryStream stream = new MemoryStream(bytes);
                    BinaryReader r = new BinaryReader(stream);
                    //Transfer data to server
                    int streamLength = (int)bytes.Length;
                    OracleLob myLob = new OracleLob(ocl, OracleDbType.Clob);
                    myLob.Write(r.ReadBytes(streamLength), 0, streamLength);

                    //Perform INSERT
                    OracleCommand myCommand = new OracleCommand(sql, ocl);
                    OracleParameter myParam = myCommand.Parameters.Add("parms", OracleDbType.Clob);
                    //                    OracleParameter myParam = myCommand.Parameters.Add("parms", text);
                    myParam.OracleValue = myLob;

                    try
                    {
                        Console.WriteLine(myCommand.ExecuteNonQuery() + " rows affected.");
                    }
                    finally
                    {
                        ocl.Close();
                    }

                }
            }
            return true;

        }

        public long getRowCount()
        {
            return 99;
//            return odr.RecordsAffected;
//            return odr.RowSize;
//            return odr.RecordsAffected;
        }
    }

}
