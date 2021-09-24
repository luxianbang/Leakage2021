using Leakage_Lib;
using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace Leakage2021
{

    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class 验证码窗口 : Window
    {
        ushort 验证码 = 0;
        public 验证码窗口()
        {
            InitializeComponent();
            //KEY_Esc.IsCancel = true;// IsCancel="True"
            this.Topmost = true;//窗口最前端

            发送按键_Click(null, null);
            Language_int(SYS_Set.LANG);//语言初始化
            Inputy_box.Focus();
        }



        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Topmost = false;//窗口最前端

                ushort[] data = ModbusTcpM.R_Registers(Leak.Modbus_ID, 1012, 10);
                byte[] BYTE = new byte[data.Length * 2 ];
                Buffer.BlockCopy(data, 0, BYTE, 0, data.Length * 2);
                MC_inf.MC_Num = Encoding.UTF8.GetString(BYTE.Skip(0).Take(20).ToArray()).Trim().Replace("\0", "");

                if (Inputy_box.Text == 验证码.ToString() || (MC_inf.MC_Num.Length > 3 && Inputy_box.Text == MC_inf.MC_Num))
                {
                    ModbusTcpM.W_SingleCoil(Leak.Modbus_ID, 3, false);//M3
                    this.DialogResult = true;//窗口返回真 
                }
                else
                {
                    //Console.Beep(850, 500);
                    MessageBox.Show("输入错误!请重新输入");
                    Inputy_box.Clear();
                    this.Topmost = true;//窗口最前端
                }

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void Inputy_box_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新 
                {
                    Enter_Click(null, null);

                }));
            }
        }
        private void 发送按键_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                验证码 = (ushort)new Random().Next(2234, 9999);
                Console.WriteLine("验证码={0}", 验证码.ToString());
                ModbusTcpM.WriteSingleRegister(Leak.Modbus_ID, 200, 验证码);//D200
                ModbusTcpM.W_SingleCoil(Leak.Modbus_ID, 3, true);//M3
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void Language_int(string language)//语言初始化
        {
            if (language == "CN")
            {
                提示_Label.Content = "请输入气密机上的验证码!";
                KEY_重发.Content = "更 新";
            }
            else
            {
                提示_Label.Content = "Please input the verification \n           code on the device!";
                KEY_重发.Content = "Update";
            }
        }





        //private void 调用例子(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    try
        //    {
        //        TextBox textBox = (TextBox)sender;
        //        string 数字 = System.Text.RegularExpressions.Regex.Replace(textBox.Uid, @"[^0-9]+", "");//取数字,要写入数据的地址
        //        ushort addr = (ushort)Convert.ToInt16(数字);//要读取的地址 

        //        验证码窗口 输入键盘 = new 验证码窗口();//输入键盘
        //        输入键盘.Str_Value = textBox.Text;//取原来的数值
        //        textBox.Text = System.Text.RegularExpressions.Regex.Replace(textBox.Text, @"[^0-9]+", "");//取数字
        //        if (textBox.Text == "") { textBox.Text = "0"; }
        //        输入键盘.Value = double.Parse(textBox.Text);
        //        输入键盘.ShowDialog();//打开键盘窗口 

        //        if ((bool)输入键盘.DialogResult)//显示对话框
        //        {
        //            var TEST = 输入键盘.Str_Value;

        //        }

        //    }
        //    catch (Exception ex) { MessageBox.Show(ex.Message, "Message"); }

        //}

    }
}
