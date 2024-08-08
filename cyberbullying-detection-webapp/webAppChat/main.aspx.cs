using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using webAppChat.classes;

namespace webAppChat
{
    public partial class main : util
    {
        const decimal cntToxLevelHigh = 80;
        const decimal cntToxLevelMild = 50;
     // const string cntMsgTox0 = " is trying to send a toxic message!";
        const string cntMsgTox1 = "You sent a toxic message!";
        const string cntMsgWarn = "Some messages may offend people's sensibilities!";

        decimal toxLevelGlobal = 0;
        decimal toxLevelMessage = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["nickname"] == null)
            {
                Response.Redirect("index.aspx");
            }
            else
            {
                setDataSource_Users();
                setDataSource_Chat();
            }
        }

        protected void lnkButSignOff_Click(object sender, EventArgs e)
        {
            Session.Abandon();
            Response.Redirect("index.aspx");
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (DataBinder.Eval(e.Row.DataItem, "nickname").ToString() == Session["nickname"].ToString())
                {
                    e.Row.Font.Bold = true;
                }

                if ((Decimal)DataBinder.Eval(e.Row.DataItem, "toxLevel") >= cntToxLevelHigh)
                {
                    e.Row.Cells[3].CssClass = "text-danger";
                }
                else if ((Decimal)DataBinder.Eval(e.Row.DataItem, "toxLevel") >= cntToxLevelMild)
                {
                    e.Row.Cells[3].CssClass = "text-warning";
                }
                else
                {
                    e.Row.Cells[3].CssClass = "text-success";
                }
            }
        }
        protected void gvChat_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if ((Decimal)DataBinder.Eval(e.Row.DataItem, "value") >= cntToxLevelHigh)
                {
                    // e.Row.Font.Bold = true;
                    // e.Row.ForeColor = Color.Red;
                    // e.Row.ControlStyle.ForeColor = Color.Red;
                    // e.Row.BackColor = ColorTranslator.FromHtml("#FFFF00");
                    e.Row.CssClass = "table-danger";
                }
                else if ((Decimal)DataBinder.Eval(e.Row.DataItem, "value") >= cntToxLevelMild)
                {
                    e.Row.CssClass = "table-warning";
                }
                else
                {
                    e.Row.CssClass = "table-success";
                }
            }
        }
        
        protected void btnSend_Click(object sender, EventArgs e)
        {
            string strMessage;

            lblStatus.Text = "";
            lblStatus.CssClass = "";

            if (Page.IsValid)
            {
                strMessage = txtMsg.Text.Trim();
                toxicityMessageLevel(strMessage);

                if (toxLevelMessage >= cntToxLevelHigh)
                {
                    // strMessage = Session["nickname"].ToString() + cntMsgTox0;
                    lblStatus.Text = cntMsgTox1;
                    lblStatus.CssClass = "lblStatus_banned";
                }
                else if (toxLevelMessage >= cntToxLevelMild)
                {
                    lblStatus.Text = cntMsgWarn;
                    lblStatus.CssClass = "lblStatus_warning";
                }
 
                // adding message to chatroom
                addMessage(Session["nickname"].ToString() + " says:", strMessage, toxLevelMessage);
                setDataSource_Chat();
                scrollDown("divgv");

                // updating toxicity level of user
                foreach (DataRow dtRow in ((DataTable)Application["dtUsers"]).Rows)
                {
                    if (dtRow["sessionId"].ToString() == Session.SessionID)
                    {
                        dtRow["toxLevel"] = toxLevelGlobal;
                        dtRow["toxLevelStr"] = levelBar(toxLevelGlobal);  // toxLevelGlobal.ToString("0.00\\%");
                        break;
                    }
                }

                setDataSource_Users();

                txtMsg.Text = "";
                txtMsg.Focus();
            }
            else
            {
                showMessage("Message is required");
            }
        }

        private void addMessage(string nickname, string message, decimal value)
        {
            DataRow rowMsg = ((DataTable)Application["dtChat"]).NewRow();
            rowMsg["nickname"] = nickname;
            rowMsg["message"]  = message;
            rowMsg["value"] = value;
            ((DataTable)Application["dtChat"]).Rows.Add(rowMsg);
        }

        private void setDataSource_Users() 
        {
            gvUsers.DataSource = (DataTable)Application["dtUsers"];
            gvUsers.DataBind();
        }

        private void setDataSource_Chat()
        {
            gvChat.DataSource = (DataTable)Application["dtChat"];
            gvChat.DataBind();
        }

        private void toxicityMessageLevel(string pMessage)
        {
            
            if (pMessage.Contains("mierda"))
            {
                toxLevelMessage = 0.95m;
            }
            else if (pMessage.Contains("bobo"))
            {
                toxLevelMessage = 0.60m;
            }

            toxLevelGlobal  = 0.8m;

            toxLevelGlobal  *= 100;
            toxLevelMessage *= 100;
        }

        private string levelBar(Decimal pLevel)
        {            
            return new string('|', (int)(20 * (pLevel / 100)));
        }

    }
}