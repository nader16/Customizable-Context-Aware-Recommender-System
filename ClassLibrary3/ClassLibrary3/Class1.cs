using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using System.Diagnostics;
using System.Threading;
namespace ClassLibrary3
{
    public class DatabaseController
    {
        String ConString;
        public SqlConnection Connect;
        public string Path = @"C:\Users\Lenovo\Desktop\GP Files\";
        public DatabaseController()
        {
            ConString = "Data Source=(local);Initial Catalog=booking;Integrated Security=True";
            Connect = new SqlConnection(ConString);
        }
        public DatabaseController(String name)
        {
            ConString = "Data Source=(local);Initial Catalog=" + name + ";Integrated Security=True";
            Connect = new SqlConnection(ConString);
        }
        public void Request_to_access() { }
        public void Retrieve_data(String TableUserName, String TableItemName)
        {
            Connect.Open();
            SqlCommand usersCmd = new SqlCommand("Select * from " + TableUserName, Connect);
            SqlDataReader UsersReader = usersCmd.ExecuteReader();
            new Users().Set_Users(UsersReader);
            UsersReader.Close();
            SqlCommand itemsCmd = new SqlCommand("Select * from " + TableItemName, Connect);
            SqlDataReader ItemsReader = itemsCmd.ExecuteReader();
            new Items().Set_Items(ItemsReader);
            ItemsReader.Close();
            Connect.Close();
        }
        public void GetResults() { }
    }
    public class DB_Attributes
    {
        public String ID;
        public List<String> Data = new List<string>();
    }
    public class Items
    {
        static List<DB_Attributes> items_list = new List<DB_Attributes>();
        public void Set_Items(SqlDataReader reader)
        {
            while (reader.Read())
            {
                DB_Attributes newItem = new DB_Attributes();
                newItem.ID = Convert.ToString(reader.GetValue(0));
                for (int i = 1; i < reader.FieldCount; i++)
                {
                    newItem.Data.Add(Convert.ToString(reader.GetValue(i)));
                }
                items_list.Add(newItem);
            }
        }
        public List<DB_Attributes> Get_Items() { return items_list; }
    }
    public class Users
    {
        static List<DB_Attributes> users_list = new List<DB_Attributes>();
        public void Set_Users(SqlDataReader reader)
        {
            while (reader.Read())
            {
                DB_Attributes newUser = new DB_Attributes();
                newUser.ID = Convert.ToString(reader.GetValue(0));
                for (int i = 1; i < reader.FieldCount; i++)
                {
                    newUser.Data.Add(Convert.ToString(reader.GetValue(i)));
                }
                users_list.Add(newUser);
            }
        }
        public List<DB_Attributes> Get_Users() { return users_list; }
    }
    public class MatrixData
    {
        String PATH = new DatabaseController().Path;
        public void GenerateUsertemMatrix(String TableRating, String ColumnRating, String UserINRating, String ItemINRating)
        {
            FileStream rateFile;
            FileStream UnRatedFile;
            FileStream NoRatesFile;
            if (!File.Exists(PATH + "OriginalRates.txt"))
            { rateFile = new FileStream(PATH + "OriginalRates.txt", FileMode.OpenOrCreate); }
            else rateFile = new FileStream(PATH + "OriginalRates.txt", FileMode.Truncate);
            if (!File.Exists(PATH + "UnRated.txt"))
            { UnRatedFile = new FileStream(PATH + "UnRated.txt", FileMode.OpenOrCreate); }
            else UnRatedFile = new FileStream(PATH + "UnRated.txt", FileMode.Truncate);
            StreamWriter writeRate = new StreamWriter(rateFile);
            StreamWriter writeUnRate = new StreamWriter(UnRatedFile);
            List<DB_Attributes> items_list = new Items().Get_Items();
            if (!File.Exists(PATH + "NoRates.txt"))
            { NoRatesFile = new FileStream(PATH + "NoRates.txt", FileMode.OpenOrCreate); }
            else NoRatesFile = new FileStream(PATH + "NoRates.txt", FileMode.Truncate);
            StreamWriter writeNoRate = new StreamWriter(NoRatesFile);
            SqlConnection Con = new DatabaseController().Connect;
            Con.Open();
            SqlCommand cmd1 = new SqlCommand("Select " + ItemINRating + " from " + TableRating, Con);
            SqlDataReader rdr = cmd1.ExecuteReader();
            List<string> notRatedItems = new List<string>();
            while (rdr.Read()) notRatedItems.Add(rdr.GetValue(0).ToString());
            for (int i = 0; i < items_list.Count; i++)
            {
                if (!notRatedItems.Contains(items_list[i].ID)) writeNoRate.WriteLine(items_list[i].ID.ToString());
            }
            List<DB_Attributes> users_list = new Users().Get_Users();
            String rate;
            SqlConnection Connect = new DatabaseController().Connect;
            Connect.Open();
            for (int i = 0; i < users_list.Count; i++)
            {
                for (int j = 0; j < items_list.Count; j++)
                {
                    SqlCommand cmd = new SqlCommand("Select " + ColumnRating + " from " + TableRating + " Where  " + UserINRating + " = " + users_list[i].ID + " AND " + ItemINRating + " = '" + items_list[j].ID + "'", Connect);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        writeUnRate.WriteLine(users_list[i].ID + "\t" + items_list[j].ID + "\t" + "-1");
                    }
                    else
                    {
                        rate = Convert.ToString(reader.GetValue(0));
                        writeRate.WriteLine(users_list[i].ID + "\t" + items_list[j].ID + "\t" + rate);
                    }

                    reader.Close();
                }
            }
            writeNoRate.Close();
            writeRate.Close();
            writeUnRate.Close();
            NoRatesFile.Close();
            rateFile.Close();
            UnRatedFile.Close();
            Connect.Close();
        }
    }
    public class ContextsGeneration
    {
        List<String> DistinctStatement = new List<String>();
        List<String> CountStatement = new List<String>();
        SqlConnection connection = new DatabaseController().Connect;
        List<String> DataStatement = new List<String>();
        public List<String> TableName = new List<String>();
        public bool checkoftable()
        {
            connection.Open();
            SqlCommand cmd = new SqlCommand("SELECT  table_name FROM information_schema.columns", connection);
            SqlDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                if (read.GetString(0) == "ASSRR")
                {
                    return true;
                }
            }
            read.Close();
            connection.Close();
            return false;
        }
        public List<String> GetContexts()
        {
            List<String> ColumnName = new List<String>();
            connection.Open();
            SqlCommand cmd = new SqlCommand("SELECT table_schema, table_name, column_name FROM information_schema.columns WHERE table_name != 'ASSRR'", connection);
            SqlDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                String DataString = "select DISTINCT  ";
                String CountString = "select count( ";
                String DistinctString = "select count (DISTINCT  ";
                DistinctString += read.GetString(2) + ") FROM " + read.GetString(1);
                DistinctStatement.Add(DistinctString);
                ColumnName.Add(read.GetString(2));
                TableName.Add(read.GetString(1));
                CountString += read.GetString(2) + ") FROM " + read.GetString(1);
                CountStatement.Add(CountString);
                DataString += read.GetString(2) + " FROM " + read.GetString(1);
                DataStatement.Add(DataString);
            }
            read.Close();
            connection.Close();
            return ColumnName;
        }
        public List<float> calculatePercentage()
        {
            List<float> Perecentage = new List<float>();
            int DistinctNumbers;
            int CountNumbers;
            float DistinctNumber;
            float CountNumber;
            String first;
            String second;
            SqlConnection con1 = new DatabaseController().Connect;
            SqlConnection con2 = new DatabaseController().Connect;
            con1.Open();
            con2.Open();
            for (int z = 0; z < DistinctStatement.Count(); z++)
            {

                first = DistinctStatement[z];
                second = CountStatement[z];
                SqlCommand m = new SqlCommand(first, con1);
                SqlCommand h = new SqlCommand(second, con2);
                DistinctNumbers = (int)m.ExecuteScalar();
                CountNumbers = (int)h.ExecuteScalar();
                DistinctNumber = (float)DistinctNumbers;
                CountNumber = (float)CountNumbers;
                Perecentage.Add((DistinctNumber / CountNumber) * 100);
            }
            con1.Close();
            con2.Close();
            return Perecentage;
        }
        public void Get_Data() { }
        public void ContextualPostFiltering() { }
        public void GetPredictions() { }
    }
    public class Context
    {
        public SqlConnection Connection;
        Dictionary<String, List<String>> DBContexts = new Dictionary<String, List<String>>();
        public void StoreRecommendations() { }
        public void SetContextsValues(List<String> ContextType, List<String> ContextValues)
        {

            Connection.Open();
            for (int i = 0; i < ContextType.Count; i++)
            {
                SqlCommand cmd = new SqlCommand(ContextValues[i], Connection);
                SqlDataReader reader = cmd.ExecuteReader();
                List<String> conValues = new List<String>();
                while (reader.Read()) conValues.Add(Convert.ToString(reader.GetValue(0)));
                if (!DBContexts.ContainsKey(ContextType[i]))
                    DBContexts.Add(ContextType[i], conValues);
                reader.Close();
            }
            Connection.Close();
        }
    }
    public class AssociationRules
    {
        SqlConnection con = new DatabaseController().Connect;
        public List<String> listuser = new List<String>();
        static List<String> columnOfNewTable = new List<String>();
        private List<String> ListOfSubset = new List<String>();
        public List<String> listitem = new List<String>();
        public List<String> listRating = new List<String>();
        public List<String> numfile = new List<String>();
        String PATH = new DatabaseController().Path;
        public void CreateJoin(String TableItem, String TableUser, String TableRating, String userInRating, String ItemInRating, String UserInUser, String ItemInItem)
        {
            ContextsGeneration co = new ContextsGeneration();
            List<String> ListOFColName = co.GetContexts();
            List<String> ListOfTableName = co.TableName;
            bool checks = co.checkoftable();
            con.Open();
            if (checks)
            {
                SqlCommand command = new SqlCommand("Drop table ASSRR", con);
                command.ExecuteNonQuery();
            }
            int op = 0;
            String z = "select  ";
            for (int i = 0; i < ListOFColName.Count(); i++)
            {
                if (ListOfTableName[i] == "ASSRR" && op == 0)
                {
                    //    SqlCommand command1 = new SqlCommand("Drop table ASSRR", con);
                    //    command1.ExecuteNonQuery();
                    op = 1;
                }
                if (ListOFColName[i] != userInRating && ListOFColName[i] != UserInUser && ListOFColName[i] != ItemInRating && ListOFColName[i] != ItemInItem)
                {
                    if (ListOfTableName[i] == TableItem)
                    {
                        z += "B.";
                        listitem.Add(ListOFColName[i]);
                    }
                    else if (ListOfTableName[i] == TableRating)
                    {
                        z += "R.";
                        listRating.Add(ListOfTableName[i]);
                    }
                    else if (ListOfTableName[i] == TableUser)
                    {
                        z += "U.";
                        listuser.Add(ListOFColName[i]);
                    }
                    if (ListOfTableName[i] != "ASSRR")
                    {
                        z += ListOFColName[i];
                        columnOfNewTable.Add(ListOFColName[i]);
                        if (i != ListOFColName.Count() - 1)
                        {
                            z += " , ";
                        }
                    }
                }
            }
            int numcheck = ListOFColName.Count() - 1;
            if (ListOFColName[numcheck] == userInRating || ListOFColName[numcheck] == UserInUser || ListOFColName[numcheck] == ItemInRating || ListOFColName[numcheck] == ItemInItem)
            {
                z = z.Remove(z.Length - 2);
            }
            z += " INTO ASSRR from " + TableItem + " B JOIN " + TableRating + " R ON  B." + ItemInItem + " = R." + ItemInRating + " JOIN " + TableUser + " U  ON R." + userInRating + " = U." + UserInUser + " ";
            Console.WriteLine(z);
            SqlCommand cmd = new SqlCommand(z, con);
            cmd.ExecuteNonQuery();
            cmd.Cancel();
            con.Close();
        }
        public void FrequentItem(String FileName, int numberOfFile, float minSupport)
        {
            con.Open();
            String s = PATH + "frequent" + numberOfFile.ToString() + ".txt";
            var frequent = new StreamWriter(s);
            foreach (String line in File.ReadLines(FileName))
            {
                string help = "select count( * ) from ASSRR where " + line;
                SqlCommand cmdfre = new SqlCommand(help, con);
                int Support = (int)cmdfre.ExecuteScalar();
                if (Support >= minSupport) frequent.WriteLine(line);
            }
            frequent.Close();
            con.Close();
        }
        public void CreateCandaite1ANDFreq1(float minSupport)
        {
            con.Open();
            var write = new StreamWriter(PATH + "candiate1.txt");
            bool check = false;
            for (int i = 0; i < columnOfNewTable.Count(); i++)
            {
                String use = "Select distinct " + columnOfNewTable[i] + " from ASSRR";
                SqlCommand cmd = new SqlCommand(use, con);
                SqlDataReader read = cmd.ExecuteReader();
                check = false;
                while (read.Read())
                {
                    for (int j = 0; j < read.FieldCount; j++)
                    {
                        if ((read.GetDataTypeName(j) == "varchar" || read.GetDataTypeName(j) == "char") && !read.IsDBNull(j))
                        {
                            string s = read.GetString(j);
                            if (!s.Contains("'"))
                            { write.WriteLine(columnOfNewTable[i] + " = '" + read[j] + "'"); }
                        }
                        else if (read.GetDataTypeName(j) == "int" && !read.IsDBNull(j))
                        {
                            AssociationRules ass = new AssociationRules();
                            List<String> sdata = ass.bin(columnOfNewTable[i]);
                            for (int m = 0; m < sdata.Count(); m++)
                            {
                                write.WriteLine(sdata[m]);
                            }
                            check = true;
                            break;
                        }
                    }
                    if (check) { break; }
                }
                read.Close();
            }
            write.Close();
            con.Close();
            AssociationRules apriory = new AssociationRules();
            apriory.FrequentItem(PATH + "candiate1.txt", 1, minSupport);
            numfile.Add(PATH + "frequent1.txt");
        }
        public void CreateCandaite2ANDFreq2(float minSupport)
        {
            SqlConnection conn = new DatabaseController().Connect;
            conn.Open();
            var candiate = new StreamWriter(PATH + "candiate2.txt");
            int count1 = 0;
            int count3 = 0;
            foreach (String line in File.ReadLines(PATH + "frequent1.txt"))
            {
                count1++;
                String[] delimiterChars = new string[] { " = ", " between " };

                String[] s = line.Split(delimiterChars, StringSplitOptions.None);

                foreach (String lin in File.ReadLines(PATH + "frequent1.txt"))
                {
                    count3++;
                    if (count3 > count1)
                    {
                        if (!lin.Contains(s[0]))
                        {
                            candiate.WriteLine(line + " and " + lin);
                        }
                    }
                }
                count3 = 0;
            }
            candiate.Close();
            conn.Close();
            AssociationRules apriory = new AssociationRules();
            apriory.FrequentItem(PATH + "candiate2.txt", 2, minSupport);
            numfile.Add(PATH + "frequent2.txt");
        }
        public void SEEfile()
        {
            for (int i = 0; i < numfile.Count(); i++)
            {
                Console.WriteLine(numfile[i]);
            }
        }
        public void FilterByContext(List<String> context, List<String> Files)
        {
            var FinalRules = new StreamWriter(PATH + "FinalRules.txt");
            for (int i = 0; i < Files.Count(); i++)
            {
                foreach (String line in File.ReadLines(Files[i]))
                {
                    for (int j = 0; j < context.Count(); j++)
                    {
                        int check = 0;
                        if (line.Contains(context[j]))
                        {
                            for (int l = 0; l < listitem.Count(); l++)
                            {
                                if (line.Contains(listitem[l]))
                                {
                                    for (int y = 0; y < listuser.Count(); y++)
                                    {
                                        if (line.Contains(listuser[y]))
                                        {
                                            FinalRules.WriteLine(line);
                                            check = 1;
                                            break;
                                        }
                                    }
                                    if (check == 0)
                                    {
                                        for (int y = 0; y < listRating.Count(); y++)
                                        {
                                            if (line.Contains(listRating[y]))
                                            {
                                                check = 1;
                                                FinalRules.WriteLine(line);
                                                break;
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        if (check == 1) { break; }
                    }
                }
            }
            FinalRules.Close();
        }
        public List<String> bin(String NameOfColum)
        {
            List<String> data = new List<String>();
            List<int> numbers = new List<int>();
            SqlConnection conn = new DatabaseController().Connect;
            conn.Open();
            int precentage;
            SqlCommand cmd = new SqlCommand("Select " + NameOfColum + " from ASSRR", conn);
            SqlDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                if (!read.IsDBNull(0))
                    numbers.Add(read.GetInt32(0));
            }
            read.Close();
            numbers.Sort();
            cmd = new SqlCommand("Select count (*) from ASSRR", conn);
            int Count = (int)cmd.ExecuteScalar();
            precentage = (numbers.Count * 10) / 100;
            int count = 0;
            int next = 0;
            while (count != numbers.Count() && count < numbers.Count())
            {
                string z = NameOfColum + " between " + numbers[count].ToString() + " AND ";
                int newnum = numbers.Count - next;
                if (newnum > precentage)
                {
                    if (numbers[next + precentage] == numbers[next + precentage + 1])
                    {
                        int cal = next + precentage + 1;
                        if (cal < numbers.Count())
                        {
                            while (numbers[cal] == numbers[next + precentage])
                            {
                                cal++;
                                if (cal > numbers.Count() || cal == numbers.Count()) break;
                            }

                            z += numbers[next + precentage].ToString();
                            next = cal;
                            count = cal;
                        }
                    }
                    else if (numbers[next + precentage] == numbers[count])
                    {
                        int cal = next + precentage;
                        if (cal < numbers.Count())
                        {
                            while (numbers[cal] == numbers[count])
                            {
                                cal++;
                                if (cal > numbers.Count() || cal == numbers.Count()) break;
                            }

                            z += numbers[cal].ToString();
                            next = cal + 1;
                            count = cal + 1;
                        }
                    }
                    else
                    {
                        z += numbers[next + precentage].ToString();
                        next = next + precentage + 1;
                        count = next + 1;
                    }
                }
                else
                {
                    z += (numbers[numbers.Count() - 1] + 1).ToString();
                    count++;
                }
                data.Add(z);
            }

            return data;
        }
        public void trending(String ItemTable, String ItemName, float minsupport, String ItemID)
        {
            con.Open();
            String s = PATH + "TrendingItem.txt";
            var frequent = new StreamWriter(s);
            Dictionary<String, int> itemDICT = new Dictionary<String, int>();
            int count = 1;
            List<String> item = new List<String>();
            SqlCommand cmd = new SqlCommand("select distinct " + ItemName + " from ASSRR", con);
            SqlDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                item.Add(read.GetString(0));
            }
            read.Close();
            for (int i = 0; i < item.Count(); i++)
            {
                SqlCommand cmd2 = new SqlCommand("select count(*) from ASSRR where " + ItemName + " = '" + item[i] + "'", con);
                int Support = (int)cmd2.ExecuteScalar();
                if (Support >= minsupport) itemDICT.Add(item[i], Support);

            }
            itemDICT = itemDICT.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            foreach (KeyValuePair<String, int> DICT in itemDICT)
            {
                if (count < 11)
                {
                    SqlCommand cmd2 = new SqlCommand("select " + ItemID + " from " + ItemTable + " where " + ItemName + " = '" + DICT.Key + "'", con);
                    SqlDataReader read2 = cmd2.ExecuteReader();
                    while (read2.Read())
                    {
                        frequent.WriteLine(read2.GetString(0));
                    }
                    read2.Close();
                }
                else
                {
                    break;
                }
                count++;
            }
            frequent.Close();
            con.Close();
        }

    }
    public class ContextForm
    {
        List<string> ChoosenContext = new List<string>();
        public List<float> Perecentage = new List<float>();
        public List<String> contextNAME = new List<String>();
        public bool GiveAccess() { return true; }
        public List<string> ChooseContext(List<String> choose)
        {
            ChoosenContext = choose;
            return ChoosenContext;
        }
        public void ViewRecommendations() { }
    }
    public class Recommendations
    {
        String PATH = new DatabaseController().Path;
        public void CalculateSimilarity(string TABLERating, String RAtingColumn, String UserInRating, String ItemInRating)
        {
            FileStream simfile;
            if (!File.Exists(PATH + "ItemsSimilarity.txt"))
            { simfile = new FileStream(PATH + "ItemsSimilarity.txt", FileMode.OpenOrCreate); }
            else simfile = new FileStream(PATH + "ItemsSimilarity.txt", FileMode.Truncate);
            StreamWriter sw = new StreamWriter(simfile);
            List<DB_Attributes> items_list = new Items().Get_Items();
            SqlConnection Connect = new DatabaseController().Connect;
            Connect.Open();
            SqlConnection Connect1 = new DatabaseController().Connect;
            Connect1.Open();
            for (int i = 0; i < items_list.Count; i++)
            {
                for (int j = i + 1; j < items_list.Count; j++)
                {
                    double multirate = 0;
                    double fristite_rate = 0;
                    double sec_ite_rate = 0;
                    SqlCommand cmd = new SqlCommand("Select " + RAtingColumn + " , " + UserInRating + " from " + TABLERating + " Where " + ItemInRating + "='" + items_list[i].ID + "'", Connect);
                    SqlDataReader reader = cmd.ExecuteReader();
                    SqlCommand cmd1 = new SqlCommand("Select " + RAtingColumn + " , " + UserInRating + " from " + TABLERating + " Where " + ItemInRating + "='" + items_list[j].ID + "'", Connect1);
                    SqlDataReader reader1 = cmd1.ExecuteReader();
                    while (reader.Read())
                    {
                        double rate = Convert.ToDouble(reader[0]);
                        int id = (int)reader[1];
                        fristite_rate += rate * rate;
                        while (reader1.Read())
                        {
                            double rate1 = Convert.ToDouble(reader1[0]);
                            int id1 = (int)reader1[1];
                            sec_ite_rate += rate1 * rate1;
                            if (id == id1) multirate += rate * rate1;
                            else multirate += 0;
                        }
                    }
                    if (fristite_rate != 0 && sec_ite_rate != 0)
                    {
                        fristite_rate = Math.Sqrt((double)fristite_rate);
                        sec_ite_rate = Math.Sqrt((double)sec_ite_rate);
                        double finaltotal = multirate / (fristite_rate * sec_ite_rate);
                        if (finaltotal != 0)
                        {
                            String t = finaltotal.ToString();
                            sw.WriteLine(items_list[i].ID + "\t" + items_list[j].ID + "\t" + t);
                        }
                    }
                    reader.Close();
                    reader1.Close();
                }
            }
            sw.Close();
            simfile.Close();
            Connect.Close();
        }
        public void GeneratePredictions()
        {
            File.Delete(PATH + "top10.txt");
            string TmpFile = PATH + "tmp1.txt";
            var FS_userANDitem = new FileStream(PATH + "OriginalRates.txt", FileMode.Open);
            var SR_userANDitem = new StreamReader(FS_userANDitem);
            string prev_user = "";
            while (SR_userANDitem.Peek() != -1)
            {
                string[] tmp_line = SR_userANDitem.ReadLine().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                string curr_user = tmp_line[0], curr_item = tmp_line[1];
                //Console.WriteLine(curr_user);
                if (prev_user != curr_user && prev_user != "")
                {
                    //htsafeeeh w t7otlo el top10
                    var SW_TopTen = new StreamWriter(PATH + "top10.txt", true);
                    var SR_TmpFile = new StreamReader(TmpFile, true);
                    var mp = new SortedDictionary<string, double>();
                    while (SR_TmpFile.Peek() != -1)
                    {
                        tmp_line = SR_TmpFile.ReadLine().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                        if (mp.ContainsKey(tmp_line[0]))
                            mp[tmp_line[0]] = Math.Max(mp[tmp_line[0]], double.Parse(tmp_line[1]));
                        else
                        {
                            mp.Add(tmp_line[0], double.Parse(tmp_line[1]));
                            if (mp.Count > 10)
                                mp.Remove(mp.First(x => x.Value == mp.Values.Min()).Key);
                        }
                    }
                    SR_TmpFile.Close();
                    File.WriteAllText(TmpFile, string.Empty);
                    SW_TopTen.WriteLine(prev_user);
                    SW_TopTen.WriteLine(mp.Count);
                    while (mp.Count > 0)
                    {
                        string tmp_key = mp.First(x => x.Value == mp.Values.Max()).Key;
                        SW_TopTen.Write(tmp_key + "\t");
                        SW_TopTen.WriteLine(mp[tmp_key]);
                        mp.Remove(tmp_key);
                    }
                    SW_TopTen.WriteLine();//nice look

                    SW_TopTen.Close();
                }
                var SW_TmpFile = new StreamWriter(TmpFile, true);
                var FS_similarity = new FileStream(PATH + "ItemsSimilarity.txt", FileMode.Open);
                var SR_similarity = new StreamReader(FS_similarity);
                while (SR_similarity.Peek() != -1)
                {
                    tmp_line = SR_similarity.ReadLine().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    string item1 = tmp_line[0], item2 = tmp_line[1], cos = tmp_line[2];

                    //Console.WriteLine(item1 + "\t" + item2);

                    if (curr_item == item1)
                    {
                        //Console.WriteLine(item2 + "\t" + cos);
                        string TmpFile2 = PATH + "tmp2.txt";
                        File.Copy(PATH + "OriginalRates.txt", TmpFile2, true);
                        bool exist = false;
                        var FS_userANDitem2 = new FileStream(TmpFile2, FileMode.Open);
                        var SR_userANDitem2 = new StreamReader(FS_userANDitem2);
                        while (SR_userANDitem2.Peek() != -1)
                        {
                            tmp_line = SR_userANDitem2.ReadLine().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                            if (tmp_line[0] == curr_user && tmp_line[1] == item2)
                            {
                                exist = true;
                                break;
                            }
                        }
                        if (!exist) SW_TmpFile.WriteLine(item2 + "\t" + cos);
                        SR_userANDitem2.Close();
                        FS_userANDitem2.Close();
                    }
                    else if (curr_item == item2)
                    {
                        //Console.WriteLine(item1 + "\t" + cos);
                        string TmpFile2 = PATH + "tmp3.txt";
                        File.Copy(PATH + "OriginalRates.txt", TmpFile2, true);
                        bool exist = false;
                        var FS_userANDitem2 = new FileStream(TmpFile2, FileMode.Open);
                        var SR_userANDitem2 = new StreamReader(FS_userANDitem2);
                        while (SR_userANDitem2.Peek() != -1)
                        {
                            tmp_line = SR_userANDitem2.ReadLine().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                            if (tmp_line[0] == curr_user && tmp_line[1] == item1)
                            {
                                exist = true;
                                break;
                            }
                        }
                        SR_userANDitem2.Close();
                        FS_userANDitem2.Close();
                        if (!exist) SW_TmpFile.WriteLine(item1 + "\t" + cos);
                    }
                }
                SW_TmpFile.Close();
                if (SR_userANDitem.Peek() == -1)
                {
                    var SW_TopTen = new StreamWriter(PATH + "top10.txt", true);

                    var SR_TmpFile = new StreamReader(TmpFile, true);
                    var mp = new SortedDictionary<string, double>();
                    while (SR_TmpFile.Peek() != -1)
                    {
                        tmp_line = SR_TmpFile.ReadLine().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                        if (mp.ContainsKey(tmp_line[0]))
                            mp[tmp_line[0]] = Math.Max(mp[tmp_line[0]], double.Parse(tmp_line[1]));
                        else
                        {
                            mp.Add(tmp_line[0], double.Parse(tmp_line[1]));
                            if (mp.Count > 10)
                                mp.Remove(mp.First(x => x.Value == mp.Values.Min()).Key);
                        }
                    }
                    SR_TmpFile.Close();
                    File.WriteAllText(TmpFile, string.Empty);
                    SW_TopTen.WriteLine(prev_user);
                    SW_TopTen.WriteLine(mp.Count);
                    while (mp.Count > 0)
                    {
                        string tmp_key = mp.First(x => x.Value == mp.Values.Max()).Key;
                        SW_TopTen.Write(tmp_key + "\t");
                        SW_TopTen.WriteLine(mp[tmp_key]);
                        mp.Remove(tmp_key);
                    }
                    SW_TopTen.WriteLine();//nice look
                    SW_TopTen.Close();
                }
                prev_user = curr_user;
                SR_similarity.Close();
                FS_similarity.Close();
            }
            SR_userANDitem.Close();
            FS_userANDitem.Close();
        }
        public bool checkMatchedRules(string UserTable, string UserPK, string UserID_val, string ItemTable, string ItemPK, string ItemID_val)
        {
            SqlConnection con = new DatabaseController().Connect;
            SqlConnection con2 = new DatabaseController().Connect;
            SqlCommand cmd = new SqlCommand();
            SqlDataReader rdr;
            SqlDataReader rdr2;
            string checkString = "";
            string userColumnsQuery = "SELECT  table_name, column_name FROM information_schema.columns WHERE table_name = '" + UserTable + "'";
            con.Open();
            cmd = new SqlCommand(userColumnsQuery, con);
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                con2.Open();
                string userQuery = "select " + rdr.GetString(1) + " from " + UserTable + " Where " + UserPK + "='" + UserID_val + "'and " + rdr.GetString(1) + " Is Not null ";
                cmd = new SqlCommand(userQuery, con2);
                rdr2 = cmd.ExecuteReader();
                while (rdr2.Read())
                {
                    for (int j = 0; j < rdr2.FieldCount; j++)
                    {
                        if ((rdr2.GetDataTypeName(j) == "varchar" || rdr2.GetDataTypeName(j) == "char") && !rdr2.IsDBNull(j))
                        {
                            checkString += rdr.GetValue(1) + " = '" + rdr2.GetValue(0) + "'     ";
                        }
                        else if (rdr2.GetDataTypeName(j) == "int" && !rdr2.IsDBNull(j))
                        {
                            checkString += rdr.GetValue(1) + " = " + rdr2.GetValue(0) + "     ";
                        }
                    }
                }
                con2.Close();
            }
            con.Close();
            string itemColumnsQuery = "SELECT  table_name, column_name FROM information_schema.columns WHERE table_name = '" + ItemTable + "'";
            con.Open();
            cmd = new SqlCommand(itemColumnsQuery, con);
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                con2.Open();
                string itemQuery = "select " + rdr.GetString(1) + " from " + ItemTable + " Where " + ItemPK + "='" + ItemID_val + "'";
                cmd = new SqlCommand(itemQuery, con2);
                rdr2 = cmd.ExecuteReader();
                while (rdr2.Read())
                {
                    for (int j = 0; j < rdr2.FieldCount; j++)
                    {
                        if ((rdr2.GetDataTypeName(j) == "varchar" || rdr2.GetDataTypeName(j) == "char") && !rdr2.IsDBNull(j))
                        {
                            checkString += rdr.GetValue(1) + " = '" + rdr2.GetValue(0) + "'     ";
                        }
                        else if (rdr2.GetDataTypeName(j) == "int" && !rdr2.IsDBNull(j))
                        {
                            checkString += rdr.GetValue(1) + " = " + rdr2.GetValue(0) + "     ";
                        }
                    }
                }
                con2.Close();
            }
            con.Close();
            var finalRulesFile = new FileStream(PATH + "FinalRules.txt", FileMode.Open);
            int check = 0;
            bool checkASS = false;
            List<string> userItemlist = (checkString.Split(new string[] { "     " }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            var finalRulesSR = new StreamReader(finalRulesFile);
            while (finalRulesSR.Peek() != -1)
            {
                string[] source = finalRulesSR.ReadLine().Split(new string[] { " and " }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < source.Count(); i++)
                {
                    if (!source[i].Contains("between"))
                    {
                        for (int j = 0; j < userItemlist.Count(); j++)
                            if (userItemlist[j] == source[i])
                            {
                                check++;
                                break;
                            }
                    }
                    else if (source[i].Contains("between"))
                    {
                        string[] nameCol = source[i].Split(new string[] { " between " }, StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < userItemlist.Count(); j++)
                        {
                            if (userItemlist[j].Contains(nameCol[0]))
                            {
                                string[] numbers = nameCol[1].Split(new string[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
                                string[] number = userItemlist[j].Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                                try
                                {
                                    int NUmber1 = int.Parse(numbers[0]);
                                    int NUmber2 = int.Parse(numbers[1]);
                                    int NUmber3 = int.Parse(number[1]);
                                    if (NUmber3 <= NUmber2 && NUmber3 >= NUmber1)
                                    {
                                        check++;
                                        break;
                                    }
                                }
                                catch { continue; }
                            }
                        }
                    }
                    if (source.Count() == check)
                    {
                        checkASS = true;
                        break;
                    }
                }
                if (checkASS)
                {
                    con.Close();
                    finalRulesFile.Close();
                    finalRulesSR.Close();
                    return true;
                }
                else { check = 0; }
            }
            finalRulesFile.Close();
            finalRulesSR.Close();
            con.Close();
            return false;
        }
        public void FinalRecommendations(string UserTable, string UserPK, string ItemTable, string ItemPK)
        {
            Dictionary<String, List<String>> recS = new Dictionary<String, List<String>>();
            SqlConnection con = new DatabaseController().Connect;
            SqlCommand cmd = new SqlCommand();
            SqlDataReader rdr;
            string Query = "SELECT  " + UserPK + " FROM " + UserTable;
            con.Open();
            cmd = new SqlCommand(Query, con);
            rdr = cmd.ExecuteReader();
            while (rdr.Read()) recS.Add(rdr.GetValue(0).ToString(), new List<String>());
            var top10File = new FileStream(PATH + "top10.txt", FileMode.Open);
            var top10SR = new StreamReader(top10File);
            string Uid = "";
            int ItemCount = 0;
            for (int i = 0; top10SR.Peek() != -1; i++)
            {
                string newLine = top10SR.ReadLine();
                if (newLine == String.Empty)
                {
                    i = -1;
                    continue;
                }
                else if (i == 0)
                { Uid = newLine; continue; }
                else if (i == 1)
                {
                    ItemCount = Int32.Parse(newLine);
                    for (int j = 0; j < ItemCount; j++)
                    {
                        if (recS[Uid].Count >= 10) break;
                        string[] itemData = top10SR.ReadLine().Split('\t');
                        bool Matched = checkMatchedRules(UserTable, UserPK, Uid, ItemTable, ItemPK, itemData[0]);
                        if (Matched && !recS[Uid].Contains(itemData[0])) recS[Uid].Add(itemData[0]);
                    }
                }
            }
            top10SR.Close();
            top10File.Close();
            var TrendingItemFile = new FileStream(PATH + "TrendingItem.txt", FileMode.Open);
            var TrendingItemSR = new StreamReader(TrendingItemFile);
            List<String> trendingItems = new List<String>();
            while (TrendingItemSR.Peek() != -1)
            {
                string newLine = TrendingItemSR.ReadLine();
                trendingItems.Add(newLine);
                for (int i = 0; i < recS.Count; i++)
                {
                    if (recS[recS.ElementAt(i).Key].Count >= 10) break;
                    bool Matched = checkMatchedRules(UserTable, UserPK, recS.ElementAt(i).Key, ItemTable, ItemPK, newLine);
                    if (Matched && !recS[recS.ElementAt(i).Key].Contains(newLine)) recS[recS.ElementAt(i).Key].Add(newLine);
                }
            }
            TrendingItemSR.Close();
            TrendingItemFile.Close();
            var UnRatedFile = new FileStream(PATH + "UnRated.txt", FileMode.Open);
            var UnRatedSR = new StreamReader(UnRatedFile);
            string userID, itemID;
            while (UnRatedSR.Peek() != -1)
            {
                string[] newLine = UnRatedSR.ReadLine().Split('\t');
                userID = newLine[0];
                itemID = newLine[1];
                if (recS[userID].Count >= 10) break;
                bool Matched = checkMatchedRules(UserTable, UserPK, userID, ItemTable, ItemPK, itemID);
                if (Matched && recS[userID].Count < 10 && !recS[Uid].Contains(itemID)) recS[userID].Add(itemID);
            }
            UnRatedSR.Close();
            UnRatedFile.Close();
            var NoRatesFile = new FileStream(PATH + "NoRates.txt", FileMode.Open);
            var NoRatesSR = new StreamReader(NoRatesFile);
            while (NoRatesSR.Peek() != -1)
            {
                string newLine = NoRatesSR.ReadLine();
                for (int i = 0; i < recS.Count; i++)
                {
                    if (recS[recS.ElementAt(i).Key].Count >= 10) break;
                    bool Matched = checkMatchedRules(UserTable, UserPK, recS.ElementAt(i).Key, ItemTable, ItemPK, newLine);
                    if (Matched && recS[recS.ElementAt(i).Key].Count < 10 && !recS[recS.ElementAt(i).Key].Contains(newLine)) recS[recS.ElementAt(i).Key].Add(newLine);
                }
            }
            NoRatesSR.Close();
            NoRatesFile.Close();
            for (int i = 0; i < recS.Count; i++)
            {
                if (recS.ElementAt(i).Value.Count == 0)
                {
                    for (int j = 0; j < 10; j++)
                        recS.ElementAt(i).Value.Add(trendingItems[j]);
                }
            }
            FileStream FinalRecSFile;
            if (!File.Exists(PATH + "FinalRecommendations.txt"))
            { FinalRecSFile = new FileStream(PATH + "FinalRecommendations.txt", FileMode.OpenOrCreate); }
            else FinalRecSFile = new FileStream(PATH + "FinalRecommendations.txt", FileMode.Truncate);
            var FinalRecSSR = new StreamWriter(FinalRecSFile);
            for (int i = 0; i < recS.Count; i++)
            {
                FinalRecSSR.WriteLine(recS.ElementAt(i).Key);
                FinalRecSSR.WriteLine(recS.ElementAt(i).Value.Count.ToString());
                for (int j = 0; j < recS.ElementAt(i).Value.Count; j++)
                    FinalRecSSR.WriteLine(recS.ElementAt(i).Value[j]);
                FinalRecSSR.WriteLine();
            }
            FinalRecSSR.Close();
            FinalRecSFile.Close();
        }
    }
}
