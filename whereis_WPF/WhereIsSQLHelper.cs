using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace whereis_WPF
{
    class WhereIsSQLHelper
    {
        string connStr = "";
        MySqlConnection conn;
        public void Connect()
        {
            connStr = File.OpenText("str.txt").ReadToEnd();
            conn = new MySqlConnection(connStr);
            conn.Open();
        }

        public void Insert(string item_name, string item_description, string item_place, int item_class_id, string image, int owner_id, int item_count)
        {
            string InsertSql = $"INSERT into whereis_data values ('0','{item_name}','{item_description}','{item_place}','{item_class_id}','{image}','{owner_id}','{item_count}')";
            MySqlCommand com = new MySqlCommand(InsertSql, conn);
            com.ExecuteNonQuery();
        }
        public void Disconnect() =>conn.Close();
        public bool DeleteById(int itemId)
        {
            if (GetItemConut(itemId)>0)
            {
                string sql = "delete from whereis_data where item_id=@itemId";
                MySqlParameter p = new MySqlParameter("@itemId", itemId);
                return Delete(sql, p) > 0;
            }
            else
            {
                Console.WriteLine($"data数据库不存在id为{itemId}的项目");
                return false;
            }
        }

        public bool DeleteByName(string itemName)
        {
            if (GetItemConut(itemName) > 0)
            {
                string sql = "delete from whereis_data where item_name=@itemName";
                MySqlParameter p = new MySqlParameter("@itemName", itemName);
                return Delete(sql, p) > 0;
            }
            else
            {
                Console.WriteLine($"data数据库不存在id为{itemName}的项目");
                return false;
            }
        }

        public int Delete(string sql, params MySqlParameter[] ps)
        {
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddRange(ps);
            return cmd.ExecuteNonQuery();
        }

        public int GetItemConut(int id)
        {
            string str = $"select * from whereis_data where item_id={id}";
            MySqlCommand com = new MySqlCommand(str, conn);
            int intcont = Convert.ToInt32(com.ExecuteScalar());

            return intcont;
        }
        public int GetItemConut(string s)
        {
            string str = $"select * from whereis_data where item_name='{s}'";
            MySqlCommand com = new MySqlCommand(str, conn);
            int intcont = Convert.ToInt32(com.ExecuteScalar());

            return intcont;
        }
        public int GetClassConut(int id)
        {
            string str = $"select * from whereis_class where class_id={id}";
            MySqlCommand com = new MySqlCommand(str, conn);
            int intcont = Convert.ToInt32(com.ExecuteScalar());

            return intcont;
        }
        public int GetClassConut(string name)
        {
            string str = $"select * from whereis_class where class_name='{name}'";
            MySqlCommand com = new MySqlCommand(str, conn);
            int intcont = Convert.ToInt32(com.ExecuteScalar());

            return intcont;
        }
        public int GetConut(string item_name)
        {
            string str = $"select * from whereis_data where item_name={item_name}";
            MySqlCommand com = new MySqlCommand(str, conn);
            int intcont = Convert.ToInt32(com.ExecuteScalar());

            return intcont;
        }

        public List<(int,string,int)> GetClassString()
        {
            List<(int, string, int)> tmp = new List<(int, string, int)>();
            string sql = "SELECT * from whereis_class;";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                tmp.Add((int.Parse(rdr[0].ToString()), rdr[1].ToString(), int.Parse(rdr[2].ToString())));
            }
            rdr.Close();
            return tmp;
        }
        public void AddClass(string name,int parent)
        {
            string InsertSql = $"INSERT into whereis_class values ('0','{name}','{parent}')";
            MySqlCommand com = new MySqlCommand(InsertSql, conn);
            com.ExecuteNonQuery();
        }

        public bool RemoveClass(int classId)
        {
            if (GetClassConut(classId) > 0)
            {
                string sql = "delete from whereis_class where class_id=@class_id";
                MySqlParameter p = new MySqlParameter("@class_id", classId);
                return Delete(sql, p) > 0;
            }
            else
            {
                Console.WriteLine($"class数据库不存在id为{classId}的项目");
                return false;
            }
        }

        public bool RemoveClass(string className)
        {
            if (GetClassConut(className) > 0)
            {
                string sql = "delete from whereis_class where class_name=@class_name";
                MySqlParameter p = new MySqlParameter("@class_name", className);
                return Delete(sql, p) > 0;
            }
            else
            {
                Console.WriteLine($"class数据库不存在id为{className}的项目");
                return false;
            }
        }

        public Item[] GetItemByClassId(int id) {
            List<Item> items = new List<Item>();

            string sql = $"SELECT * from whereis_data where item_class={id};";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                items.Add(new Item(id,rdr[1].ToString(),rdr[2].ToString(), rdr[3].ToString(), int.Parse(rdr[4].ToString()),rdr[5].ToString(), int.Parse(rdr[6].ToString()), int.Parse(rdr[7].ToString())));
            }
            rdr.Close();

            return items.ToArray();
        }
    }
}
