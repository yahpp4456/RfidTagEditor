using Newtonsoft.Json;
using RfidTagEditor.Model;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace RfidTagEditor
{
    public partial class Form1 : Form
    {
        //資料來源
        string sourceFilePath = System.Configuration.ConfigurationManager.AppSettings["sourceFilePath"];
        //目的位置
        string targetFilePath = System.Configuration.ConfigurationManager.AppSettings["targetFilePath"];

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load CSV To Show
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
                List<string> list = GetDataFromCsv();

                List<TagInfoModel> tags = new List<TagInfoModel>();
                foreach (var item in list)
                {
                    var tag = new TagInfoModel();
                    tag.TagCode = GetTransferData(item);
                    tags.Add(tag);
                }
                DataConver conver = new DataConver();
                dataGridView1.DataSource = conver.ToDataTable(tags);            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var tags = ConvertDataTable<TagInfoModel>((DataTable)dataGridView1.DataSource);

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON文件 (*.json)|*.json";
                saveFileDialog.Title = "選擇保存JSON文件的位置";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    var jsonString = JsonConvert.SerializeObject(tags);

                    try
                    {
                        File.WriteAllText(fileName, jsonString);
                        Console.WriteLine("JSON文件已保存成功！");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"發生錯誤: {ex.Message}");
                    }
                }
            }
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {

            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName].ToString(), null);
                    else
                        continue;
                }
            }
            return obj;
        }

        private List<string> GetDataFromCsv()
        {
            var result = new List<string>();

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV文件 (*.csv)|*.csv";
                openFileDialog.Title = "選擇CSV文件";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                        {
                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine().Substring(18, 44);
                                result.Add(line);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"發生錯誤: {ex.Message}");
                    }
                }
            }

            return result;
        }

        private string GetTransferData(string str)
        {
            var transList = new List<string>();
            var data = str.Split(' ').ToList();
            foreach (var item in data)
            {
                transList.Add(item);
            }
            var result = $"\\x{string.Join("\\x", transList)}";
            return result;
        }
    }
}