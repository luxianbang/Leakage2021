using Leakage_Lib;
using System;
using System.Windows;
using System.Windows.Input;


namespace Leakage2021
{
    /// <summary>Login_win.xaml 的交互逻辑 </summary>
    public partial class Login_win : Window
    {
        Login_inf login_inf = new Login_inf();//登陆信息="用户名，密码，语言，工厂，工序号，拉线号，工位号"

        public Login_win()
        {
            InitializeComponent();
            Local_login_CheckBox_Click(null, null);

            Initialize();

            //ImageBrush b3 = new ImageBrush() { ImageSource = new BitmapImage(new Uri("要运行目录路径Resources/Connect.ico", UriKind.RelativeOrAbsolute)) };   
            //b3.Stretch = Stretch.Fill;
            //this.Background = b3;


            //this.Login_win1.WindowState = WindowState.Maximized;
            //this.Topmost = true;//窗口最前端
        }

        //初始化
        private void Initialize()
        {
            try
            {
                Language_int(login_inf.language);//语言初始化

                User_TextBox.Text = login_inf.user;
                //passwd_Box.Password = login_inf.passwd;

                if (login_inf.language == "CN")
                    Language_ComboBox.SelectedIndex = 0;//显示CN语言
                else
                    Language_ComboBox.SelectedIndex = 1;//显示EN语言

                if (login_inf.Factory_Name == "DACE")
                {
                    Factory_ComboBox.SelectedIndex = 0;//显示DACE工厂
                    DB.ServerIP = Server_IP.Text = (string)DB.Get_Reg("Dace_ServerIP", "192.168.19.21");
                }
                else
                {
                    Factory_ComboBox.SelectedIndex = 1;//显示VACE工厂
                    DB.ServerIP = Server_IP.Text = (string)DB.Get_Reg("Vace_ServerIP", "192.168.105.20");
                }

                OPER_TextBox.Text = login_inf.OPER;
                Line_TextBox.Text = login_inf.Line_Num;
                //Seat_Num.Text = login_inf.Seat_Num;
            }
            catch (Exception)
            {
            }
        }
        //语言初始化
        private void Language_int(string language)
        {
            if (language == "CN")
            {
                Login_win1.Title = "登陆窗口";
                USER_LOGIN_txt.Content = "用户登陆";
                User_name_txt.Content = "用户名称";
                Passwowd_txt.Content = "用户密码";
                Language_txt.Content = "语    言";

                Factory_txt.Content = "工厂代码";
                OPER_txt.Content = "工序代码";
                Line_Num_txt.Content = "拉线号码";

                Login1.Content = "登 陆";
                Cancel1.Content = "取 消";

                Local_login_CheckBox.Content = "本地登陆";
            }
            else
            {
                Login_win1.Title = "Login_Window";
                USER_LOGIN_txt.Content = "USER_LOGIN";
                User_name_txt.Content = "User_name";
                Passwowd_txt.Content = "Passwowd";
                Language_txt.Content = "Language";//Chinese   English

                Factory_txt.Content = "Factory";
                OPER_txt.Content = "Process_No";
                Line_Num_txt.Content = "Line_Num";

                Login1.Content = "Login";
                Cancel1.Content = "Cancel";

                Local_login_CheckBox.Content = "Local login";
            }
        }

        //语言选择框_事件
        private void Language_select_DropDownClosed(object sender, EventArgs e)//选择语言
        {
            login_inf.language = Language_ComboBox.Text;
            Initialize();
        }

        //工厂选择框_事件
        private void Factory_ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            login_inf.Factory_Name = Factory_ComboBox.Text;
            if (login_inf.Factory_Name == "DACE")
                DB.ServerIP = Server_IP.Text = (string)DB.Get_Reg("Dace_ServerIP", "192.168.19.21");
            else
                DB.ServerIP = Server_IP.Text = (string)DB.Get_Reg("Vace_ServerIP", "192.168.105.20");
        }

        //本地登陆选择_事件
        private void Local_login_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            string[] inf;
            if ((bool)Local_login_CheckBox.IsChecked)
                inf = ((string)DB.Get_Reg("Login_inf1", ",,,,,,")).Split(',');
            else
                inf = ((string)DB.Get_Reg("Login_inf2", ",,,,,,")).Split(',');
            //读取登陆信息
            login_inf.user = inf[0];
            login_inf.passwd = inf[1];
            login_inf.language = inf[2];
            login_inf.Factory_Name = inf[3];
            login_inf.OPER = inf[4];
            login_inf.Line_Num = inf[5];
            login_inf.Seat_Num = inf[6];
            Initialize();
        }

        //用户框_失去焦点事件
        private void User_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            login_inf.user = User_TextBox.Text;

        }
        //工序框_失去焦点事件
        private void OPER_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            login_inf.OPER = OPER_TextBox.Text;
        }
        //拉线框_失去焦点事件
        private void Line_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            login_inf.Line_Num = Line_TextBox.Text;
        }

        // 登陆键按下
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            bool STATE = false;
            string[] TXT = new string[3];
            if (login_inf.language == "CN")
            {
                TXT[0] = "用户名不能为空,请输入!";
                TXT[1] = "工序号不能为空,请输入!";
                TXT[2] = "拉线号不能为空,请输入!";
            }
            else
            {
                TXT[0] = "The user name cannot be empty";
                TXT[1] = "Process number cannot be empty";
                TXT[2] = "Line cannot be empty";
            }

            try
            {
                if (User_TextBox.Text == "")
                    throw new Exception(TXT[0]);//用户名不能为空
                else if (OPER_TextBox.Text == "")
                    throw new Exception(TXT[1]);//工序号不能为空
                else if (Line_TextBox.Text == "")
                    throw new Exception(TXT[2]);//拉线号不能为空
                else
                {
                    this.Cursor = Cursors.Wait;//等待鼠标
                    if ((bool)Local_login_CheckBox.IsChecked)
                        STATE = Local_login();
                    else
                        STATE = Oracle_Login();
                    this.Cursor = Cursors.Arrow;//箭头鼠标

                    if (STATE)
                    {
                        REC_inf.User_ID = User_TextBox.Text;//用户ID
                        REC_inf.OPER = OPER_TextBox.Text;//工序号
                        REC_inf.Line_Num = Line_TextBox.Text;

                        SYS_Set.LANG = Language_ComboBox.Text;
                        DB.Factory_Name = Factory_ComboBox.Text;
                        DB.Local_login = (bool)Local_login_CheckBox.IsChecked;

                        string TXT1 = User_TextBox.Text + "," + passwd_Box.Password + "," + Language_ComboBox.Text + "," +
                                      Factory_ComboBox.Text + "," + OPER_TextBox.Text + "," + Line_TextBox.Text + "," + null;//保存登陆信息

                        if ((bool)Local_login_CheckBox.IsChecked)
                            DB.Set_Reg("Login_inf1", TXT1);//保存本地登陆信息
                        else
                            DB.Set_Reg("Login_inf2", TXT1);//保存服务器登陆信息

                        this.DialogResult = true;//窗口返回真  
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //数据库登陆
        private bool Oracle_Login()
        {
            string[] TXT = new string[10];
            if (login_inf.language == "CN")
            {
                TXT[0] = "没有此用户!";
                TXT[1] = "密码错误,请重新输入!";
                TXT[2] = "此用户只能操作工序: ";
                TXT[3] = "你没有打开 < " + OPER_TextBox.Text + " > 的权限";
            }
            else
            {
                TXT[0] = "No such user!";
                TXT[1] = "Password error, please re-enter!";
                TXT[2] = "This user can only operate operations: ";
                TXT[3] = "You don't have permission to open  < " + OPER_TextBox.Text + " >";
            }

            bool STATE = false;
            string sql = "";

            try
            {
                DB.DB_INT(login_inf.Factory_Name);
                if (login_inf.Factory_Name == "DACE")//DACE
                {
                    sql = "SELECT COMPANYCD,USERID,PASSWD,USERNM,DEPTCD,USEYN \n" //工厂号，用户ID，密码，名字，工序号，权限
                                + "        FROM AERP.USERINFO \n"
                                + "                  WHERE USERID = '" + User_TextBox.Text + "'\n";
                    //+ "                  AND COMPANYCD = '" + DB.Factory_Code + "'\n";

                }
                else
                {
                    sql = "SELECT FACILITY, USERID,PASSWD,USERNM,DEPTCD,USEFG \n"
                                  + "    FROM AMES.SYSUSR \n"
                                  + "                  WHERE USERID = '" + User_TextBox.Text + "'\n";
                    //+ "                  AND FACILITY = '" + DB.Factory_Code + "'\n";
                    // sql = "SELECT * FROM AMES.SYSUSR WHERE ROWNUM <= 50 \n";
                }

                var table = DB.Oracle_Data_Table(sql);

                if (table.Rows.Count < 1)
                    throw new Exception(TXT[0]);//没有此用户
                else if (table.Rows[0]["PASSWD"].ToString() != passwd_Box.Password)
                    throw new Exception(TXT[1]);//密码错误
                //else if (table.Rows[0]["DEPTCD"].ToString() != OPER_TextBox.Text)
                //    throw new Exception(TXT[2] + table.Rows[0]["DEPTCD"].ToString());//工序号错误
                else
                {
                    if (login_inf.Factory_Name == "DACE")//DACE
                    {
                        DB.Set_Reg("Dace_ServerIP", DB.ServerIP);
                        REC_inf.User_Name = DB.Ksc_GB18030(table.Rows[0]["USERNM"].ToString());//用户名
                    }
                    else
                    {
                        DB.Set_Reg("Vace_ServerIP", DB.ServerIP);
                        REC_inf.User_Name = table.Rows[0]["USERNM"].ToString();//用户名
                    }
                    STATE = true;//D130282  3310 D130179 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return STATE;
        }

        //本地登陆
        private bool Local_login()
        {
            string[] TXT = new string[10];
            if (login_inf.language == "CN")
            {
                TXT[0] = "没有此用户!";
                TXT[1] = "密码错误,请重新输入!";
                TXT[2] = "此用户只能操作工序: ";
                TXT[3] = "你没有打开 < " + OPER_TextBox.Text + " > 的权限";
            }
            else
            {
                TXT[0] = "No such user!";
                TXT[1] = "Password error, please re-enter!";
                TXT[2] = "This user can only operate operations: ";
                TXT[3] = "You don't have permission to open  < " + OPER_TextBox.Text + " >";
            }

            bool STATE = false;
            string sql = "";

            try
            {
                sql = "SELECT [USERID],[PASSWD],[USERNM],[OPER],[USEYN]  \n"
                    + "                 FROM USERINFO \n"
                    + "                     WHERE(USERID = '" + User_TextBox.Text + "'\n"
                    + ")\n"
                    ;
                var table = DB.Access_Data_Table(sql);


                if (table.Rows.Count < 1)
                    throw new Exception(TXT[0]);//没有此用户
                else if (table.Rows[0]["PASSWD"].ToString() != passwd_Box.Password)
                    throw new Exception(TXT[1]);//密码错误
                else if (table.Rows[0]["USERID"].ToString().ToUpper() == "ADMIN")
                    Console.WriteLine("ADMIN");//管理员操作  *******************************************************
                //else if (table.Rows[0]["OPER"].ToString() != OPER_TextBox.Text)
                //    throw new Exception(TXT[2] + table.Rows[0]["OPER"].ToString());//工序号错误
                //else if (table.Rows[0]["USEYN"].ToString() == "NN")//Y,N
                //    throw new Exception(TXT[3]);//没有权限
                else
                {
                    REC_inf.User_Name = table.Rows[0]["USERNM"].ToString();//用户名
                    DB.Local_login = true;
                    STATE = true;//D130282  3310 D130179 
                }


                //if (table.Rows.Count < 1)
                //    MessageBox.Show(messageBoxText[0], "Message", MessageBoxButton.OK, MessageBoxImage.Warning);//没有此用户
                //else if (table.Rows[0]["PASSWD"].ToString() != passwd_Box.Password)
                //    MessageBox.Show(messageBoxText[1], "Message", MessageBoxButton.OK, MessageBoxImage.Warning);//密码错误
                //else if (table.Rows[0]["USERID"].ToString().ToUpper() == "ADMIN")
                //    Console.WriteLine("ADMIN");//管理员操作
                //else if (table.Rows[0]["DEPTCD"].ToString() != OPER_TextBox.Text)
                //    MessageBox.Show(messageBoxText[2] + table.Rows[0]["DEPTCD"].ToString(), "Message", MessageBoxButton.OK, MessageBoxImage.Warning);//工序号错误
                //else if (table.Rows[0]["USEYN"].ToString() == "NN")//Y,N
                //    MessageBox.Show(messageBoxText[3], "Message", MessageBoxButton.OK, MessageBoxImage.Warning);//没有权限
                //else
                //{
                //    DB.USERNM = table.Rows[0]["USERNM"].ToString();//用户名
                //    STATE = true;//D130282  3310 D130179 
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return STATE;
        }

        //密码框按下回车键
        private void passwd_Box_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Cursor = Cursors.Wait;//等待鼠标
                Login_Click(null, null);
                this.Cursor = Cursors.Arrow;//箭头鼠标
            }
        }

        //取消按键
        private void Cancel_Click(object sender, RoutedEventArgs e) { this.DialogResult = false; }//窗口返回假


        private void Server_IP_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DB.Format_IP(Server_IP.Text))
                DB.ServerIP = Server_IP.Text;
            else
            {
                MessageBox.Show("IP格式设置错误!", "Message", MessageBoxButton.OK, MessageBoxImage.Error);//"IP格式设置错误!"
                Server_IP.Clear();
            }
        }
    }

    public class Login_inf //登陆信息
    {
        //登陆信息="用户名，密码，语言，工厂，工序号，拉线号，工位号"
        public string user { get; set; }    //用户名
        public string passwd { get; set; }//密码
        public string language { get; set; }//语言
        public string Factory_Name { get; set; }//工厂
        public string OPER { get; set; }//工序号
        public string Line_Num { get; set; }//拉线号
        public string Seat_Num { get; set; }//工位号
    }

}
