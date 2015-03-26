using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Data;
using System.Data.OleDb;
using GongChaWebApplication.Models;

namespace GongChaWebApplication.Helpers.ImportSales
{
    /// <summary>
    /// Summary description for ImportSalesDataWebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ImportSalesDataWebService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public string OurServerOutput()
        {
            return "The Server Date and Time is : " + GongChaDateTimeWrapper.Now.ToString();
        }

        [WebMethod]
        public string ImportSales(string filePath, int memberId, int storeId)
        {
            if (File.Exists(filePath))
            {
                GongChaDbContext db = new GongChaDbContext();
                
                StreamReader sr = new StreamReader(filePath);
                using (sr)
                {
                    string inputTitle = sr.ReadLine();
                    string inputHeader = sr.ReadLine();

                    string inputLine = "";
                    inputLine = sr.ReadLine();
                    while (inputLine != null)
                    {
                        Sales sales = new Sales();
                        string[] csv = inputLine.Split(',');
                        sales.Date = Convert.ToDateTime(csv[0]);
                        sales.SalesAmount = double.Parse(csv[1]);
                        sales.Payout = double.Parse(csv[2]);
                        sales.ActualCash = double.Parse(csv[4]);

                        sales.Store = db.Stores.Find(storeId);
                        sales.Member = db.Members.Find(memberId);
                        sales.ExpectCash = sales.SalesAmount - sales.Payout;
                        sales.Difference = sales.ActualCash - sales.ExpectCash;

                        db.SalesItems.Add(sales);
                        inputLine = sr.ReadLine();
                    }

                    db.SaveChanges();
                    return "Data imported.";
                }
            }
            else
            {
                return "File not exists.";
            }
        }


        //NO USE 
        private DataTable ReadExcelToTable(string path)
        {
            //Connection String
            string xlsxConnString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=Excel 12.0 Xml;HDR=YES;IMEX=1;";
            using (OleDbConnection conn = new OleDbConnection(xlsxConnString))
            {
                conn.Open();
                //DataTable sheetsName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });  //Get All Sheets Name
                //string firstSheetName = sheetsName.Rows[0][2].ToString();   //Get the First Sheet Name
                //string sql = string.Format("SELECT * FROM [{0}],firstSheetName");  //Query String

                OleDbCommand sql = new OleDbCommand("SELECT * FROM [Sheet1$]", conn);

                OleDbDataAdapter ada = new OleDbDataAdapter();

                ada.SelectCommand = sql;

                DataSet set = new DataSet();
                ada.Fill(set);
                return set.Tables[0];
            }
        }

        private void Log(string strLogMsg)
        {
            const int maxFileSize = 4194304; // 4096*1024 (4MB)
            string savePath = Server.MapPath("~/Helpers/ImportSales/ImportLog.log");
            if (!File.Exists(savePath))
            {
                FileStream fileStream = File.Create(savePath);
                fileStream.Close();
            }
            FileInfo logInfo = new FileInfo(savePath);
            if (logInfo.Length > maxFileSize)
            {
                File.Move(savePath, savePath + DateTime.Today.ToString("yyyymmdd"));
            }
            File.AppendAllLines(savePath, new string[] { DateTime.UtcNow.ToString() + " " + strLogMsg });
        }


    }
}
