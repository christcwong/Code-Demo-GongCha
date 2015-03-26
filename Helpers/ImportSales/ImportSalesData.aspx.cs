using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GongChaWebApplication.Models;

namespace GongChaWebApplication.Helpers.ImportSales
{
    public partial class ImportSalesData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ddlStore.Items.Clear();
            ddlMember.Items.Clear();
            GongChaDbContext db = new GongChaDbContext();
            foreach (var store in db.Stores.ToList())
            {
                ddlStore.Items.Add(new ListItem(store.Name, store.Id.ToString()));
            }
            foreach (var member in db.Members.ToList())
            {
                ddlMember.Items.Add(new ListItem(member.Name, member.Id.ToString()));
            }
        }

        protected void btnImport_Click(object sender, EventArgs e)
        {
            lblOutput.Text += "<p>Program start ...</p>";

            if (fuSalesData.HasFile && System.IO.Path.GetExtension(fuSalesData.PostedFile.FileName) == ".csv")
            {
                string filePath = Server.MapPath("~/Helpers/ImportSales/" + fuSalesData.PostedFile.FileName);
                fuSalesData.SaveAs(filePath);
                lblOutput.Text += "<p>" + "You uploaded : " + fuSalesData.PostedFile.FileName + "</p>";
                lblOutput.Text += "<p>Data ready to import...</p>";
                ImportSalesDataWebService ws = new ImportSalesDataWebService();
                lblOutput.Text += "<p>" + ws.ImportSales(filePath, int.Parse(ddlMember.SelectedValue), int.Parse(ddlStore.SelectedValue)) + "</p>";
            }
            else
            {
                lblOutput.Text += "<p>Error of uploading file, must be xlsx and certian format, please ask administrator for details.</p>";
            }

        }
    }
}