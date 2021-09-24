using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Leakage_Lib
{
    //App.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新 
    //{
    //    Thread.Sleep(500);
    //}));

    /// <summary>
    /// ASCII_KEY.xaml 的交互逻辑
    /// </summary>
    public partial class KEY_ASSCII : Window
    {
        /// <summary>字符键盘
        /// </summary>
        public KEY_ASSCII()
        {
            InitializeComponent();
            KEY_Esc.IsCancel = true;// IsCancel="True"
        }
        /// <summary>返回输入字符
        /// </summary>
        public string TXT        {
            set { this.Display_box.Text = value; }
            get { return this.Display_box.Text; }
        }
        private void KEY_OK_Click(object sender, RoutedEventArgs e) 
        {
            Console.Beep();
            this.DialogResult = true;//确认输入->窗口返回真
        }
        private void KEY_Esc_Click(object sender, RoutedEventArgs e) 
        { 
            this.DialogResult = false;
            Console.Beep();
        }//取消->窗口返回假 
        private void KEY_Clr_Click(object sender, RoutedEventArgs e) 
        {
            Console.Beep();
            Display_box.Text = ""; 
        }//清除 
        private void ASCII_Click(object sender, RoutedEventArgs e)//输入字符
        {
            Button Button = (Button)sender;
            Console.Beep();
            Display_box.Text += Button.Content;//输入字符
        }
        private void 键_删除_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button Button = (Button)sender;
            Console.Beep();
            if (Display_box.Text.Length > 0)
            {
                Display_box.Text = Display_box.Text.Substring(0, Display_box.Text.Length - 1);
            }

            Thread th = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(1500);

                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新                  
                    {
                        Display_box.Text = "";//MessageBox.Show("长按了1秒");
                    }));
                }
            }));
            th.Start();
        }
        private void 键_切换_Click(object sender, RoutedEventArgs e)//大小写转换
        {
            Button Button = (Button)sender;
            Console.Beep();
            if ((string)Button.Tag != "A")
            {
                Button.Tag = "A";

                键_Q.Content = "q";
                键_W.Content = "w";
                键_E.Content = "e";
                键_R.Content = "r";
                键_T.Content = "t"; 
                键_Y.Content = "y";
                键_U.Content = "u";
                键_I.Content = "i";
                键_O.Content = "o";
                键_P.Content = "p";

                键_A.Content = "a";
                键_S.Content = "s";
                键_D.Content = "d";
                键_F.Content = "f";
                键_G.Content = "g";
                键_H.Content = "h";
                键_J.Content = "j";
                键_K.Content = "k";
                键_L.Content = "l";

                键_Z.Content = "z";
                键_X.Content = "x";
                键_C.Content = "c";
                键_V.Content = "v";
                键_B.Content = "b";
                键_N.Content = "n";
                键_M.Content = "m";
            }
            else
            {
                Button.Tag = "a";

                键_Q.Content = "Q";
                键_W.Content = "W";
                键_E.Content = "E";
                键_R.Content = "R";
                键_T.Content = "T";
                键_Y.Content = "Y";
                键_U.Content = "U";
                键_I.Content = "I";
                键_O.Content = "O";
                键_P.Content = "P";

                键_A.Content = "A";
                键_S.Content = "S";
                键_D.Content = "D";
                键_F.Content = "F";
                键_G.Content = "G";
                键_H.Content = "H";
                键_J.Content = "J";
                键_K.Content = "K";
                键_L.Content = "L";

                键_Z.Content = "Z";
                键_X.Content = "X";
                键_C.Content = "C";
                键_V.Content = "V";
                键_B.Content = "B";
                键_N.Content = "N";
                键_M.Content = "M";
            }
        }

       
    }
}
