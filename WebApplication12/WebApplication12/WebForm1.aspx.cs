using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using System.Diagnostics;
using System.Threading;
using ClassLibrary3;
using System.Reflection;
namespace WebApplication12
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        static String tableuser = "";
        static String tableitem = "";
        static String tableRating = "";
        static String columuser = "";
        static String columitem = "";
        static String columuserinrating = "";
        static String columiteminrating = "";
        static float minSupport = 6;
        bool check = false;

        protected void dataname_TextChanged(object sender, EventArgs e)
        {
            DatabaseController db = new DatabaseController(dataname.Text);
            userTable_combo.Items.Add("");
            itemTable_combo.Items.Add("");
            ratingTable_combo.Items.Add("");
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=" + dataname.Text + ";Integrated Security=True");
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT table_schema, table_name FROM information_schema.tables WHERE table_name != 'ASSRR'", con);
            SqlDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                userTable_combo.Items.Add(read.GetString(1));
                itemTable_combo.Items.Add(read.GetString(1));
                ratingTable_combo.Items.Add(read.GetString(1));
            }
            read.Close();
            con.Close();
            ContextsGeneration context = new ContextsGeneration();
            List<String> colum = context.GetContexts();
            List<float> percentage = context.calculatePercentage();
            for (int i = 0; i < colum.Count(); i++)
            {
                dbContexts_List.Items.Add(colum[i] + "," + percentage[i].ToString() + "%");
            }
            ContextForm form = new ContextForm();
            con.Close();
            check = true;
        }

        protected void userTable_combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            tableuser = userTable_combo.SelectedItem.Value;
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=" + dataname.Text + ";Integrated Security=True");
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT table_schema, table_name, column_name FROM information_schema.columns WHERE table_name = " + "'" + userTable_combo.SelectedItem + "'", con);
            SqlDataReader read = cmd.ExecuteReader();
            UsersPK_drop.Items.Clear();
            UsersPK_drop.Items.Add("");
            while (read.Read()) UsersPK_drop.Items.Add(read.GetString(2));
            read.Close();
        }

        protected void ratingTable_combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            tableRating = ratingTable_combo.SelectedItem.Value;
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=" + dataname.Text + ";Integrated Security=True");
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT table_schema, table_name, column_name FROM information_schema.columns WHERE table_name = " + "'" + ratingTable_combo.SelectedItem + "'", con);
            SqlDataReader read = cmd.ExecuteReader();
            ItemsFK_drop.Items.Clear();
            UsersFK_drop.Items.Clear();
            ItemsFK_drop.Items.Add("");
            UsersFK_drop.Items.Add("");
            rateN_drop.Items.Add("");
            while (read.Read())
            {
                ItemsFK_drop.Items.Add(read.GetString(2));
                UsersFK_drop.Items.Add(read.GetString(2));
                rateN_drop.Items.Add(read.GetString(2));
            }
            read.Close();
            con.Close();
        }

        protected void itemTable_combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            tableitem = itemTable_combo.SelectedItem.Value;
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=" + dataname.Text + ";Integrated Security=True");
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT table_schema, table_name, column_name FROM information_schema.columns WHERE table_name = " + "'" + itemTable_combo.SelectedItem + "'", con);
            SqlDataReader read = cmd.ExecuteReader();
            ItemsPK_drop.Items.Clear();
            ItemsPK_drop.Items.Add("");
            while (read.Read()) ItemsPK_drop.Items.Add(read.GetString(2));
            read.Close();
            con.Close();
        }

        protected void dbContexts_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < dbContexts_List.Items.Count; i++)
            {
                if (dbContexts_List.Items[i].Selected)
                {
                    String s = dbContexts_List.Items[i].Value;
                    string[] m = s.Split(',');
                    chosenContexts_List.Items.Add(m[0]);
                }
            }
        }

        protected void clrBtn_OnClick(object sender, EventArgs e)
        { chosenContexts_List.Items.Clear(); }

        protected void ItemsPK_drop_SelectedIndexChanged(object sender, EventArgs e)
        {
            columitem = ItemsPK_drop.SelectedItem.Value;
        }

        protected void ItemsFK_drop_SelectedIndexChanged(object sender, EventArgs e)
        {
            columiteminrating = ItemsFK_drop.SelectedItem.Value;
        }

        protected void UsersFK_drop_SelectedIndexChanged(object sender, EventArgs e)
        {
            columuserinrating = UsersFK_drop.SelectedItem.Value;
        }

        protected void UsersPK_drop_SelectedIndexChanged(object sender, EventArgs e)
        {
            columuser = UsersPK_drop.SelectedItem.Value;
        }

        protected void MinSupport_TextChanged(object sender, EventArgs e)
        {
            minSupport = float.Parse(MinSupport_txt.Text);
        }

        protected void btnStart_OnClick(object sender, EventArgs e)
        {
            new DatabaseController().Retrieve_data(userTable_combo.SelectedItem.Value, itemTable_combo.SelectedItem.Value);
            AssociationRules Apriory = new AssociationRules();
            Apriory.CreateJoin(tableitem, tableuser, tableRating, columuserinrating, columiteminrating, columuser, columitem);
            Apriory.CreateCandaite1ANDFreq1(minSupport);
            Apriory.CreateCandaite2ANDFreq2(minSupport);
            List<String> m = Apriory.numfile;
            List<String> ContextChoose = new List<string>();
            for (int i = 0; i < chosenContexts_List.Items.Count; i++) ContextChoose.Add(chosenContexts_List.Items[i].Value);
            Apriory.FilterByContext(ContextChoose, m);
            Apriory.trending(tableitem, "BookTitle", minSupport, columitem);
            MatrixData matrix = new MatrixData();
            matrix.GenerateUsertemMatrix(tableRating, rateN_drop.SelectedItem.Value, columuserinrating, columiteminrating);
            Recommendations recommed = new Recommendations();
            recommed.CalculateSimilarity(tableRating, rateN_drop.SelectedItem.Value, columuserinrating, columiteminrating);
            recommed.GeneratePredictions();
            recommed.FinalRecommendations(tableuser, columuser, tableitem, columitem);
            Response.Write("<script>window.alert('Done')</script>");
        }
    }
}