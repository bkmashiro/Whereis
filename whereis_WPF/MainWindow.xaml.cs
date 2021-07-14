using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace whereis_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        WhereIsSQLHelper sql = new WhereIsSQLHelper();
        List<(int, string, int)> tmpClassString = new List<(int, string, int)>();
        List<(int, string, int)> ClassString = new List<(int, string, int)>();
        public MainWindow()
        {
            InitializeComponent();

            sql.Connect();
            //sql.DeleteById(2);
            tmpClassString = sql.GetClassString();

            Analyze(tmpClassString);
            UpdateTreeView();
            //UpdateUi = UpdateSelection();
        }

        public static int ClassIdSelected = -1;
        public static string ClassNameSelected = "";
        public static readonly object Lock = new object();
        ClassItem RootItem;
        private void Analyze(List<(int, string, int)> ps2)
        {
            List<(int, string, int)> ps = new List<(int, string, int)>();
            ps.AddRange(ps2);
            RootItem = new ClassItem(1, "root", null);
            ps.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            while (ps.Count > 1)
            {
                var node = RootItem.GetItemByItemId(ps[1].Item3);
                if (node != null)
                {
                    node.Add(new ClassItem(ps[1].Item1, ps[1].Item2, ps[1].Item3));
                    ps.RemoveAt(1);
                }
                else
                {
                    RootItem.Add(new ClassItem(ps[1].Item1, ps[1].Item2, ps[1].Item3));
                    Console.WriteLine("Wrong!");
                    break;
                }
            }
        }
        private int GetIdByName(string name)
        {
            var p = RootItem.GetItemByItemName(name);

            if (p == null)
            {
                return -1;
            }
            else
            {
                return p.itemId;
            }
        }
        private void AddClass(string child, string parent)
        {
            sql.AddClass(child, GetIdByName(parent));
        }
        private void AddClass(string child, int classId)
        {
            sql.AddClass(child, classId);
        }
        private void RemoveClass(string name) => sql.RemoveClass(name);
        private void AddItem(string item_name, string item_description, string item_place, int item_class_id, string image, int count = 1, int owner_id = 0)
        {
            sql.Insert(item_name, item_description, item_place, item_class_id, image, owner_id, count);
        }
        private void RemoveItem(int id) => sql.DeleteById(id);
        private void RemoveItem(string name) => sql.DeleteByName(name);

        private void UpdateTreeView()
        {
            List<PropertyNodeItem> listItem = new List<PropertyNodeItem>();
            listItem.Add(RootItem.GetNode());
            tv.ItemsSource = listItem;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ClassIdSelected != -1 && tb1.Text != "")
            {
                AddClass(tb1.Text, ClassIdSelected);
                tmpClassString.Add((tmpClassString[tmpClassString.Count - 1].Item1 + 1, tb1.Text, ClassIdSelected));
                Analyze(tmpClassString);
                tb1.Text = "";
                UpdateTreeView();

            }
        }

        public void UpdateSelection()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SelectionInfo.Text = $"id:{ClassIdSelected},name{ClassNameSelected}";
            }));
        }

        private void tv_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateSelection();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lock (Lock)
            {
                classItems.Items.Clear();

                StackPanel sp2 = new StackPanel();
                sp2.FlowDirection = FlowDirection.LeftToRight;
                sp2.Orientation = Orientation.Horizontal;
                TextBlock tb2 = new TextBlock();
                tb2.Text = $"{"名称",20}{"描述",20}{"数量",5}{"位置",20}";
                Button btn2 = new Button();
                btn2.Content = "刷新";
                sp2.Children.Add(btn2);
                sp2.Children.Add(tb2);
                classItems.Items.Add(sp2);
                ClassNameSelected = (sender as TextBlock).Text;
                ClassIdSelected = GetIdByName(ClassNameSelected);

                foreach (var item in sql.GetItemByClassId(ClassIdSelected))
                {
                    StackPanel sp = new StackPanel();
                    sp.FlowDirection = FlowDirection.LeftToRight;
                    sp.Orientation = Orientation.Horizontal;
                    TextBlock tb = new TextBlock();
                    tb.Text = $"\n{item.itemName,20}{item.itemDes,20}{item.itemCount,5}{item.itemPlace,20}";
                    Button btn = new Button();
                    btn.Content = "修改";
                    btn.Click += Btn_Click;
                    btn.Tag = item;
                    sp.Children.Add(btn);
                    sp.Children.Add(tb);
                    classItems.Items.Add(sp);
                }
                UpdateSelection();
            }
        }

        int SelectedItemId = -1;
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Item item = (Item)(sender as Button).Tag;
            CopyConfig(item);
        }

        private void CopyConfig(Item item)
        {
            itemName.Text = item.itemName;
            itemDes.Text = item.itemDes;
            itemCount.Text = item.itemCount.ToString();
            itemPos.Text = item.itemPlace;
            SelectedItemId = item.itemId;
            selected_id_txt.Text = $"已选择物品{SelectedItemId}：{item.itemName}";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sql.Disconnect();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int res = -1;
            if (int.TryParse(itemCount.Text, out res))
            {
                AddItem(itemName.Text, itemDes.Text, itemPos.Text.ToUpper(), ClassIdSelected, "null", res, 0);
            }
            ClearInput();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            RemoveItem(itemName.Text);
        }

        private void ClearInput()
        {
            itemName.Text = "";
            itemDes.Text = "";
            itemCount.Text = "";
            itemPos.Text = "";
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ClearInput();
        }
    }
    //delegate void UpdateUi();
    public class PropertyNodeItem
    {
        public string Icon { get; set; }
        public string DisplayName { get; set; }
        public int id { get; set; }

        public MouseEventHandler OnMouseClick;
        public List<PropertyNodeItem> Children { get; set; }
        public PropertyNodeItem()
        {
            Children = new List<PropertyNodeItem>();
        }
    }
    public class Item
    {
        public int itemId;
        public string itemName;
        public string itemDes;
        public string itemPlace;
        public int itemClassId;
        public string itemUri;
        public int itemOwnerId;
        public int itemCount;

        public Item(int id, string name, string des, string place, int class_id, string uri, int owner_id, int cnt)
        {
            itemId = id;
            itemName = name;
            itemDes = des;
            itemPlace = place;
            itemClassId = class_id;
            itemUri = uri;
            itemOwnerId = owner_id;
            itemCount = cnt;
        }
    }

    public class ClassItem
    {
        public int itemId;
        public string itemName;
        public int? paerntId;
        public List<ClassItem> children = new List<ClassItem>();
        public ClassItem(int itemId, string itemName, int? paerntId)
        {
            if (itemId < 1)
            {
                itemId = 2;
            }
            this.itemId = itemId;
            this.itemName = itemName;
            this.paerntId = paerntId;
        }

        public void Add(ClassItem item) => children.Add(item);
        public void Add(IEnumerable<ClassItem> items) => children.AddRange(items);
        public void Remove(ClassItem item)
        {
            if (children.Contains(item))
            {
                children.Remove(item);
            }
        }
        public void Remove(int itemId)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].itemId == itemId)
                {
                    children.RemoveAt(i);
                    break;
                }
            }
        }
        public ClassItem GetItemByItemId(int id)
        {
            if (this.itemId == id)
            {
                return this;
            }
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].itemId == id)
                    {
                        return children[i];
                    }
                }
                for (int i = 0; i < children.Count; i++)
                {
                    var res = children[i].GetItemByItemId(id);
                    if (res != null)
                    {
                        return res;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }
        public ClassItem GetItemByItemName(string name)
        {
            if (this.itemName == name)
            {
                return this;
            }
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].itemName == name)
                    {
                        return children[i];
                    }
                }
                for (int i = 0; i < children.Count; i++)
                {
                    var res = children[i].GetItemByItemName(name);
                    if (res != null)
                    {
                        return res;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }
        public PropertyNodeItem GetNode()
        {
            PropertyNodeItem propertyNodeItem = new PropertyNodeItem();
            propertyNodeItem.DisplayName = itemName;
            propertyNodeItem.id = itemId;
            foreach (var item in children)
            {
                propertyNodeItem.Children.Add(item.GetNode());
            }
            return propertyNodeItem;
        }

    }
}
