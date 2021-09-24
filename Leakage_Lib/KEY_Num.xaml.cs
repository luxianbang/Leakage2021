using System;
using System.Windows;
using System.Windows.Controls;

namespace Leakage_Lib
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class KEY_Num : Window
    {
        private bool 首次输入 = true;
        /// <summary>数字键盘
        /// </summary>
        public KEY_Num()
        {
            InitializeComponent();
            KEY_Esc.IsCancel = true;// IsCancel="True"
        }
        /// <summary>double 类型，键盘返回值和设置值
        /// </summary>
        public double Value
        {
            set {this.Display_box.Text = value.ToString();}
            get { return Convert_To_Double(this.Display_box.Text); }
        }
        /// <summary>string 类型，键盘返回值和设置值
        /// </summary>
        public string Str_Value //读/写字符串
        {
            set{this.Display_box.Text = value;}
            get{ return this.Display_box.Text;}
        }
        private double Convert_To_Double(string sKey)//返回小数
        {
            if (sKey == "")
                sKey = "0";
            return double.Parse(sKey);
        }
        private void KEY_BS_Click(object sender, RoutedEventArgs e)//回删
        {
            if (Display_box.SelectionLength > 0)//全选时->全删
                Display_box.Text = "";

            if (Display_box.Text.Length > 0)//回删1个字符
                Display_box.Text = Display_box.Text.Substring(0, Display_box.Text.Length - 1);
        }
        private void Number_Click(object sender, RoutedEventArgs e) //输入数值
        {
            //Console.Beep();
            //System.Console.WriteLine(Display_box.SelectionStart.ToString());
            if (首次输入 && Display_box.SelectionStart == 0)
                Display_box.Text = "";
            首次输入 = false;
            Display_box.Text = Display_box.Text + ((Button)sender).Tag;//输入数值
        }
        private void KEY_OK_Click(object sender, RoutedEventArgs e) //确认输入->窗口返回真
        { 
            //Console.Beep();
            if (Display_box.Text == "")
                Display_box.Text = "0";
            this.DialogResult = true;//确认输入->窗口返回真
           
        }
        private void KEY_Esc_Click(object sender, RoutedEventArgs e)//取消->窗口返回假 
        {
            //Console.Beep();
            this.DialogResult = false; //取消->窗口返回假 
        }
        private void KEY_Clr_Click(object sender, RoutedEventArgs e)//清除
        {
            //Console.Beep();
            Display_box.Text = "";
        }      
        private void BT_NEG_Click(object sender, RoutedEventArgs e)//转负数
        {
            //-
            //Console.Beep();
            if (Display_box.Text == ""){ return; }
            double NUM = double.Parse(Display_box.Text);
            Display_box.Text = (NUM * (-1)).ToString();
            //if (NUM > 0)
            //{
            //    Display_box.Text = (NUM * (-1)).ToString();
            //}
        }
        private void BT_Point_Click(object sender, RoutedEventArgs e) //小数点
        {
            //Console.Beep();
            if (Display_box.Text.IndexOf(".") < 0)
            { 
                Display_box.Text += "."; 
            } 
        }
        private void KEYNum_Loaded(object sender, RoutedEventArgs e)
        {
            Display_box.Focus();//取得焦点
            Display_box.SelectAll();//全选
        }

        private void 调用例子(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                string 数字 = System.Text.RegularExpressions.Regex.Replace(textBox.Uid, @"[^0-9]+", "");//取数字,要写入数据的地址
                ushort addr = (ushort)Convert.ToInt16(数字);//要读取的地址 

                KEY_Num 输入键盘 = new KEY_Num();//输入键盘
                输入键盘.Str_Value = textBox.Text;//取原来的数值
                textBox.Text = System.Text.RegularExpressions.Regex.Replace(textBox.Text, @"[^0-9]+", "");//取数字
                if (textBox.Text == "") { textBox.Text = "0"; }
                输入键盘.Value = double.Parse(textBox.Text);
                输入键盘.ShowDialog();//打开键盘窗口 

                if ((bool)输入键盘.DialogResult)//显示对话框
                {
                    var TEST = 输入键盘.Str_Value;

                }

            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Message"); }

        }

    }
}
