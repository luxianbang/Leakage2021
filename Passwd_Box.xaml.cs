using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Leakage2021
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Passwd_Box : Window
    {
        /// <summary>输入密码窗口,密码 OK true,返回否则返回 false </summary>
        public Passwd_Box(bool IsSelected)
        {
            InitializeComponent();
            var Reg = Registry.CurrentUser.CreateSubKey("software\\Leakage", true);
            SYS_Set.Set_Passwd = (string)Reg.GetValue("Passwd");
            修改密码.IsSelected = IsSelected;
            输入密码.IsSelected = !IsSelected;
            //if (SET_Password == null)
            //{
            //    Reg.SetValue("Passwd", "8888");
            //    SET_Password = (string)Reg.GetValue("Passwd");
            //}
            this.Topmost = true;//窗口最前端
            //KEY_Esc.IsCancel = true;// this.DialogResult = true;//确认输入->窗口返回真
        }

        //修改密码
        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            if (旧密码_Box.Password == "13827636866")
            {
                显示密码_Label.Content = SYS_Set.Set_Passwd;              
                return;
            }
            if (旧密码_Box.Password != SYS_Set.Set_Passwd)
            {
                if (SYS_Set.LANG == "CN")
                    TXT_Block1.Text = "密码错误，请重新输入!";
                //MessageBox.Show("密码错误，请重新输入!", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    TXT_Block1.Text = "Password error please re-enter!";
                //MessageBox.Show("Password error please re-enter!", "Message", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            else if (新密码1_Box.Password == "")
            {
                if (SYS_Set.LANG == "CN")
                    TXT_Block1.Text = "新密码不能为空!";
                //MessageBox.Show("新密码不能为空!");
                else
                    TXT_Block1.Text = "NEW Password cannot be empty!";
                //MessageBox.Show("NEW Password cannot be empty!");
            }
            else if (新密码1_Box.Password == 新密码2_Box.Password)
            {
                var Reg = Registry.CurrentUser.CreateSubKey("software\\Leakage",true);
                SYS_Set.Set_Passwd = 新密码1_Box.Password;
                Reg.SetValue("Passwd", SYS_Set.Set_Passwd);
                if (SYS_Set.LANG == "CN")
                    MessageBox.Show("密码修改成功!");
                else
                    MessageBox.Show("Password Modified Successfully!");
                this.Close();
            }
            else 
            {
                if (SYS_Set.LANG == "CN")
                    TXT_Block1.Text = "两次输入的密码不一样，请重新输入!";
                //MessageBox.Show("两次输入的密码不一样，请重新输入!");
                else
                    TXT_Block1.Text = "The two passwords are not the same, please re-enter!";
                //MessageBox.Show("The two passwords are not the same, please re-enter!");
            }
           
        }
       

        //密码框_键按下
        private void Password_Box_txt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            PasswordBox textBox = (PasswordBox)sender;
            if (e.Key == Key.Return)//回车键
            {
                if (textBox.Password == SYS_Set.Set_Passwd || SYS_Set.Set_Passwd == "")
                {
                    this.DialogResult = true;//密码OK->窗口返回真
                }
                else
                {
                    if (SYS_Set.LANG == "CN")
                        TXT_Block.Text = "密码错误，请重新输入!";
                    //MessageBox.Show("密码错误，请重新输入!", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        TXT_Block.Text = "Password error, please re-enter!";
                    //MessageBox.Show("Password error, please re-enter!", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    textBox.SelectAll();
                }
            }
        }
        ////密码框_鼠标按下
        //private void Password_Box_txt_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    PasswordBox tb = e.Source as PasswordBox;
        //    tb.Focus();
        //    e.Handled = true;
        //}

        ////密码框_读到焦点
        //private void Password_Box_txt_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    PasswordBox tb = e.Source as PasswordBox;
        //    tb.SelectAll();
        //    tb.PreviewMouseDown -= new MouseButtonEventHandler(Password_Box_txt_PreviewMouseDown);
        //}

        ////密码框_失去焦点
        //private void Password_Box_txt_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    PasswordBox tb = e.Source as PasswordBox;
        //    tb.PreviewMouseDown += new MouseButtonEventHandler(Password_Box_txt_PreviewMouseDown);
        //}

        // 窗口创建完成
        private void Passbox_Loaded(object sender, RoutedEventArgs e)
        {
            
           
        }
        private void PasswdBox_ContentRendered(object sender, System.EventArgs e)
        {
            Language_int(SYS_Set.LANG);//语言初始化

            if (输入密码.IsSelected)
                Password_Box_txt.Focus();
            else
                旧密码_Box.Focus();
        }
        private void Language_int(string language)
        {
            if (language == "CN")
            {
                PasswdBox.Title       = "请输入密码!";
                旧密码_Label.Content  = "输入旧密码:";
                新密码1_Label.Content = "输入新密码:";
                新密码2_Label.Content = "确认新密码:";
                TXT_Block.Text = "请重新输入!";
                TXT_Block1.Text = "请重新输入!";
            }
            else
            {
                PasswdBox.Title = "Please input password!";
                旧密码_Label.Content  = "Enter old password:";
                新密码1_Label.Content = "Enter new password:";
                新密码2_Label.Content = "Confirm new password:";
                TXT_Block.Text = "Please input password!";
                TXT_Block1.Text = "Please input password!";
                //输入密码.IsSelected = true;
                //密码.SelectedIndex = 0;
                //PasswdBox.Title = "Please input a Password";
            }
        }

        private void 旧密码_Box_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                新密码1_Box.Focus();

        }

        private void 新密码1_Box_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                新密码2_Box.Focus();
        }

        private void 新密码2_Box_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Enter_Click(null ,null);
        }

       
    }

}
