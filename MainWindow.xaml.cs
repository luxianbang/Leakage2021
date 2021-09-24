
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xceed.Wpf.AvalonDock.Layout;
using Leakage_Lib;
using System.Windows.Threading;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.IO.Ports;





namespace Leakage2021 //Console.WriteLine("");  Brushes.Wheat;// new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑   5200355103707
    /// </summary>
    public partial class MainWindow : Window
    {
        //ACE_SN = LeakTestData.SN  

        //public static DataTable dataTable = new DataTable();

        public static ushort MC_STATE = 0/*设备启动结果*/, STL = 0;//
        bool STOP_Mark = false/*停止标志*/;

        public static byte Modbus_ID = 0;

        //***** 定义线程与定时器 *********************************************************************
        public Thread CoonThread;
        //public bool Thread_Run = false;/*线程运行*/

        

        public DispatcherTimer UI_Timer;//状态定时器  可以直接更新UI Timer在非UI线程跑的，DispatcherTimer是在UI线程跑的
        Stopwatch stopWatch = new Stopwatch();//准确地测量运行时间

        public static double RunTime = 0;//过程运行时间
        public double ScanTime = 1; //自动扫描等待时间
        byte CNT = 0;  //通信失败重试次数
        int Run_Count = 0, Usage_CNT = 360; //运行次数  

       

        public MainWindow()
        {
            #region
            //REC_inf.OPER = "";
            //string E_OPER = (REC_inf.OPER.Length > 0) ? (REC_inf.OPER.Substring(0, 1) + "0") : ("");
            //Console.WriteLine("结果=" + E_OPER);

            //string str = "12345678";
            //int N = 3;
            //string left = str.Substring(0, N);//取字符串左边N个字符
            //Console.WriteLine("取左边N" + left);
            //string right = str.Substring(str.Length - N);//取字符串右边N个字符
            //Console.WriteLine("取右边N" + right);
            //right = str.Substring(1); // 去掉左边N个字符
            //Console.WriteLine("去掉左边N" + right);
            //SN_text.IsReadOnly = true ;
            //SN_text.Text = "1224324535436";

            try
            {
                //Convert.ToInt32("");
                //int.Parse("");
                //throw new Exception("请正确输入条码!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            //finally { } //finally 块始终都会运行
            #endregion END


            InitializeComponent();
            Initialize();
        }
        private void Initialize()
        {
            连接窗口.Hide(true);//启动时隐藏连接窗口
            OUT1.Hide(true);//启动时隐藏OUT1窗口
            State_txt.Visibility = Visibility.Hidden;//条码输入框隐藏显示
            Reg();//检查运行次数
            SN_text.Text = Run_Count.ToString();


            DB.Check_Access("Data\\LeakageDB.mdb", "ASIS");//检查Access数据库是否存在
            登陆窗口();
            DB.DB_INT(DB.Factory_Name);

            //读连接配置信息 
            Leak.Read_conn_inf();
            Modbus_ID = TCP_inf.Slave_ID;
            Slave_ID.Text = TCP_inf.Slave_ID.ToString();
            TCP_IP.Text = TCP_inf.IP;
            TCP_Port.Text = TCP_inf.Port.ToString();

            User_ID_text.Content = REC_inf.User_ID + " (" + REC_inf.User_Name + ")";
            Line_text.Content = REC_inf.Line_Num;
            OPER_text.Content = REC_inf.OPER;


            if (DB.Factory_Name == "VACE") //越南工厂
                SYS_Set.Factory_Code = "6000";  //越南工厂代码
            else
                SYS_Set.Factory_Code = "3000";  //东莞工厂代码


            //ScanTime = Properties.Settings.Default.ScanTime;  //扫描时间

            Licence();//检查许可证
            Language_int(SYS_Set.LANG);//语言初始化
            stopWatch.Start();  //启动计时器
            联机_Click(null, null);

            //线程 = new Thread(new ParameterizedThreadStart(RUN_线程));//有参数
            CoonThread = new Thread(new ThreadStart(Thread_Coon));
            CoonThread.IsBackground = true; //后台线程
            CoonThread.Start();  //开启线程
            UI_UP_Timer();//加载定时器

            SN_Port_init();//SN串口初始化
        }


        #region 条码枪串口
        SerialPort SN_SerialPort = new SerialPort();//条码枪串口
        private void SN_Port_init()
        {
            Port_Num.Items.Clear();//清除串口数组
            Port_Num.Items.Add(((string)DB.Get_Reg("SN_Conn_inf", "COM1,9600,8,N,1")).Split(',')[0]);
            Port_Num.SelectedIndex = 0;//
            Port_OPEN();
        }
        private void Port_OPEN()
        {
            try
            {
                //if (Port_Num.SelectedItem == null) { MessageBox.Show("没有可用的串口"); return; }
                if (Port_Num.SelectedItem == null || Port_Num.SelectedItem.ToString() != "USB")
                {
                    //检查串口是否打开
                    if (SN_SerialPort.IsOpen) 
                    {
                        SN_SerialPort.DataReceived -= new SerialDataReceivedEventHandler(SN_Received);
                        SN_SerialPort.Close(); //关闭串口
                    }
                    SN_SerialPort = new SerialPort(Port_Num.SelectedItem.ToString(), 9600, Parity.None, 8, StopBits.One);//Port_Num.SelectedItem.ToString()
                                                                                                                         //SN_SerialPort.ReadTimeout = 1000;
                    SN_SerialPort.DataReceived += new SerialDataReceivedEventHandler(SN_Received);
                    //SN_SerialPort.ErrorReceived += new SerialErrorReceivedEventHandler(Received_Err);
                    SN_SerialPort.Open();//打开串口
                    SN_TEST_BT.IsEnabled = false;
                    stopWatch.Restart();//计时器复位后开始
                    Barcode_ON();
                }
                else
                {
                    //检查串口是否打开
                    if (SN_SerialPort.IsOpen)
                    {
                        SN_SerialPort.DataReceived -= new SerialDataReceivedEventHandler(SN_Received);
                        SN_SerialPort.Close(); //关闭串口
                    }
                    SN_TEST_BT.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void SN_TEST_Click(object sender, RoutedEventArgs e)
        {
            if (STL != 0 || Port_Num.SelectedItem == null || Port_Num.SelectedItem.ToString() == "USB")
                return;
            SN_TEST_BT.IsEnabled = false;
            stopWatch.Restart();//计时器复位后开始
            Barcode_ON();
        }
        private void Barcode_ON()
        {
            try
            {
                if (Port_Num.SelectedItem == null || Port_Num.SelectedItem.ToString() == "USB")
                    return;
                if (!SN_SerialPort.IsOpen) { SN_SerialPort.Open(); }//检查串口硬件是否拔出
                byte[] buff = { 22, 84, 13 };
                SN_SerialPort.Write(buff, 0, 3);//发送 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message");
            }
        }
        private void Barcode_OFF()
        {
            try
            {
                if (Port_Num.SelectedItem == null || Port_Num.SelectedItem.ToString() == "USB")
                    return;
                if (!SN_SerialPort.IsOpen) { SN_SerialPort.Open(); }//检查串口硬件是否拔出
                byte[] buff = { 22, 85, 13 };
                SN_SerialPort.Write(buff, 0, 3);//发送 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message");
            }
        }  
        private void SN_Received(Object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(100);
                int len = SN_SerialPort.BytesToRead;
           
                if (len != 0)
                {
                    byte[] buff = new byte[len];
                    SN_SerialPort.Read(buff, 0, len);
                    if (buff[len-1] ==13)
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(buff,0, len-1));
                        App.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新 
                        {
                            Barcode_OFF();
                            if (SN_TEST_BT.IsEnabled == true)
                            {
                                SN_text.Text = Encoding.UTF8.GetString(buff, 0, len - 1);
                                SN框键按下事件(null, null);
                            }
                            SN_TEST_BT.IsEnabled = true;
                           
                        }));
                        
                    }
                }
                SN_SerialPort.DiscardInBuffer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //刷新串口号
        private void UP_Port_Num()
        {
            if (SN_SerialPort.IsOpen) { SN_SerialPort.Close(); }//关闭串口
            string[] PortNames = SerialPort.GetPortNames();//获取当前计算机的串行端口名的数组       
            Port_Num.Items.Clear();//清除串口数组
            Port_Num.Items.Add("USB");
            foreach (string str in PortNames){Port_Num.Items.Add(str); }//添加item
        }
        private void Port_Num_DropDownClosed(object sender, EventArgs e)
        {
            if (Port_Num.SelectedItem == null)
                Port_Num.SelectedItem = "USB";
            Port_OPEN();
            DB.Set_Reg("SN_Conn_inf", Port_Num.SelectedItem.ToString() + ",9600,8,N,1");// 
        }
        //串口框鼠标左键按下事件->刷新串口号
        private void Port_Num_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { UP_Port_Num(); }//刷新串口号 
        #endregion END

        #region 选择框
        private void AutoScan_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Passwd_Box passwd_Box = new Passwd_Box(false);
            if (!(bool)passwd_Box.ShowDialog())
            {
                AutoScan_CheckBox.IsChecked = !AutoScan_CheckBox.IsChecked; 
                return;
            }

            SYS_Set.AutoScan = (bool)AutoScan_CheckBox.IsChecked;  //自动扫描启动
            DB.Set_Reg("AutoScan", SYS_Set.AutoScan);
            EnterStart_CheckBox.Visibility = (SYS_Set.AutoScan) ? (Visibility.Hidden) : (Visibility.Visible); 
        }
        private void EnterStart_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Passwd_Box passwd_Box = new Passwd_Box(false);
            if (!(bool)passwd_Box.ShowDialog())
            {
                EnterStart_CheckBox.IsChecked = !EnterStart_CheckBox.IsChecked;
                return;
            }
            SYS_Set.EnterStart = (bool)EnterStart_CheckBox.IsChecked; ;//回车启动
            DB.Set_Reg("EnterStart", SYS_Set.EnterStart);
        }
        private void AutoScan_CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            AutoScan_CheckBox.IsChecked = SYS_Set.AutoScan;
        }
        private void EnterStart_CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            EnterStart_CheckBox.IsChecked = SYS_Set.EnterStart;
        }
        #endregion END
        
        
      


        private void Thread_Coon()
        {
            while (true)
            {
                try
                {
                    if (Leak.ConnectOk)
                    {
                        Leak_Modbus();//
                    }  
                    else
                    {
                        Thread.Sleep(50);//延时 
                    }
                }
                catch (Exception ex)
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新 
                    {
                        OUT1_Log(ex.Message);
                        this.Activate();
                        MessageBox.Show(ex.Message);
                    }));
                }
            }
        }

        //气密机 Modbus_TCP通信
        private void Leak_Modbus()
        {
            try
            {
                //启动命令->读设置参数
                if (STL == 1)
                {
                    Leak.ModbusTcp_Read_Set_Para();// 读设置参数
                    if (int.Parse(MC_inf.PLC_Ver) < 20210601)
                    {
                        if (SYS_Set.LANG == "CN")
                            throw new Exception("PLC 版本过低，请更新(2021-06-10 Ver)!");
                        else
                            throw new Exception("PLC Version low, Please Update(2021-06-10 Ver)!");
                    }
                    canvas.Dispatcher.Invoke(new Action(() => //委托//UI更新
                    {
                        State_txt.Text = "";
                        State_txt.Visibility = Visibility.Hidden;//不显示元素，测试结果
                        model_text.Content = REC_inf.Model; //显示型号
                        display_model.Text = REC_inf.Model; //显示型号
                        Curved.Draw_Line_Label(canvas); //绘制线与标签
                        Curve_Click(null, null);
                    }));
                    Leak.START_CMD();//启动命令
                    stopWatch.Restart();//计时器复位
                    STL = 2;
                }
                //启动中
                else if (STL == 2)
                {
                    Leak.ModbusTcp_R_TestData(); //读测试数据
                    //if (TestData.Result > 19)//有故障
                    //    ModbusTcpM.WriteSingleRegister(Modbus_ID, 0, 2);//因故障取消后会继续启动，所以增加

                    if (MC_STATE == 1) //设备已运行
                    {
                        TestData.Fill_Time = TestData.Settling_Time = TestData.Test_Time = 0;
                        TestData.Fill_Press = TestData.Leak_Pa = TestData.Leak_CCM = 0;
                        stopWatch.Restart();//
                        STL = 3;
                    }
                    else if (TestData.Result == 20)
                    {
                        if (SYS_Set.LANG == "CN")
                            throw new Exception("请检查产品是否放到位? 感应器 X2=ON ?");
                        else
                            throw new Exception("Check if the product is in place? ? I/O X2=ON ?");
                    }
                    else if (TestData.Result == 21)
                    {
                        if (SYS_Set.LANG == "CN")
                            throw new Exception("请检查安全光栅是否动作? 感应器 X3=?");
                        else
                            throw new Exception("Check safety grating ? I/O X3=?");
                    }
                    else if (TestData.Result == 22)
                    {
                        if (SYS_Set.LANG == "CN")
                            throw new Exception("请检查电磁阀动作是否异常?");
                        else
                            throw new Exception("Whether the action of solenoid valve is abnormal ?");
                    }
                    else if (stopWatch.Elapsed.TotalSeconds > 2)
                    {
                        //Console.WriteLine("启动失败");
                        STL = 1;
                        CNT++;
                        if (CNT > 4)
                        {
                            CNT = 0;
                            if (SYS_Set.LANG == "CN")
                                throw new Exception("启动失败 !");
                            else
                                throw new Exception("Failed to start !");
                        }
                    }
                }
                //设备运行中
                else if (STL == 3)
                {
                    Leak.ModbusTcp_R_TestData(); //读测试数据

                    //RunTime = stopWatch.Elapsed.TotalSeconds - 0.3;
                    //if (RunTime < 0) { RunTime = 0; }
                    RunTime = TestData.Fill_Time + TestData.Settling_Time + TestData.Test_Time;
                    if (SetData.Work_MODE == Work_Mode.Fully_Sealed)
                        RunTime += TestData.Tank_Settling_Time;
                    else if (SetData.Work_MODE == Work_Mode.Vent_Cap) { }


                    App.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新 
                    {
                        display_Time.Text = RunTime.ToString("0.0S");//显示测试过程时间

                        display_press.Text = TestData.Fill_Press.ToString("F1") + SetData.Fill_Unit;//显示测试压力
                        //if (SetData.Leak_Unit == "Pa")
                        display_leak.Text = TestData.Leak_Pa.ToString("F2") + " Pa"; //显示泄漏Pa
                        display_CCM.Text = TestData.Leak_CCM.ToString("F2") + " CCM";//显示泄漏CCM

                        Curved.Add_Curve_Data(RunTime);//添加曲线数据
                    }));

                    //测试结果
                    if (TestData.Result > 0 && RunTime > 2)//测试完成
                        STL = 4;
                    else if (MC_STATE == 0)//设备停止
                        STL = 100;

                }
                //处理测试结果
                else if (STL == 4)
                {
                    Leak.ModbusTcp_R_TestData(); //读测试数据
                    App.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新 
                    {
                        State_txt.Visibility = Visibility.Visible;  //显示测试结果文本
                        TestData.Result_Txt = ResulTXT(TestData.Result);
                        State_txt.Text = TestData.Result_Txt;
                       
                        if (TestData.Result > 1)
                        {
                            State_txt.Background = Brushes.Red;   //背景红色
                            State_txt.Foreground = Brushes.Wheat;// new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));
                        }
                        else
                        {
                            State_txt.Background = Brushes.Lime;   //背景绿色
                            State_txt.Foreground = Brushes.Black;
                        }
                    }));
                    STL = 5;
                }//保存结果
                else if (STL == 5)
                {
                    TestData.Result_Txt = ResulTXT(TestData.Result);
                    string[] LeackData = { TestData.Fill_Time.ToString(), TestData.Test_Time.ToString(), TestData.Fill_Press.ToString("0.00"),
                                TestData.Leak_Pa.ToString("0.00"), TestData .Leak_CCM.ToString("0.00"), TestData.Result_Txt};
                    //Console.WriteLine(TestData.Result_Txt);

                    if (TestData.Result > 0 && TestData.Result < 4)
                    {
                        if (!DB.Leak_Data_FACRST(LeackData, DB.Factory_Name, DB.Local_login))
                        {
                            if (SYS_Set.LANG == "CN")
                                throw new Exception("保存数据失败!");
                            else
                                throw new Exception("Failed to Save Data!");
                        }
                        if (!DB.Local_login && REC_inf.SN.ToUpper() != "TEST")
                        {
                            if (!DB.Leak_Data_WIPWTM(DB.Factory_Name))
                            {
                                if (SYS_Set.LANG == "CN")
                                    throw new Exception("保存数据失败!");
                                else
                                    throw new Exception("Failed to Save Data!");
                            }
                        }

                        OUT1_Log(REC_inf.SN + " = " + TestData.Result_Txt);
                        STL = 100;
                        App.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新 
                        {
                            this.Activate();
                            if (SYS_Set.LANG == "CN")
                                MessageBox.Show("保存数据成功!");
                            else
                                MessageBox.Show("Data Save Success!");
                            SN_text.Focus(); //条码框获得焦点
                        }));
                    }
                }//其它处理
                else
                {

                    if (stopWatch.Elapsed.TotalSeconds >= ScanTime) //扫描时间
                    {
                        //stopWatch.Stop();
                        stopWatch.Restart();//计时器复位后开始
                       
                            App.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新
                            {
                                if (SYS_Set.AutoScan)
                                {
                                    if (SN_text.IsReadOnly == false)
                                    {
                                        SN_text.Clear();
                                        SN_text.IsReadOnly = true;
                                        SN_text.Background = Brushes.Gray;//灰色
                                        SN_text.Focus(); //条码框获得焦点
                                    }
                                }
                                if (SN_TEST_BT.IsEnabled == false)
                                {
                                    Barcode_OFF();
                                    if (Port_Num.SelectedItem != null && Port_Num.SelectedItem.ToString() != "USB")
                                        SN_TEST_BT.IsEnabled = true;
                                }
                            }));
                    }

                   

                }




                //停止指令
                if (STOP_Mark)
                {
                    STL = 100;
                    STOP_Mark = false;
                    Leak.STOP_CMD();//停止命令
                }
                //
                if (STL == 100)
                {
                    STL = 0;
                    App.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新
                    {
                        SN_text.Clear(); //清除条码
                        if ((bool)ShowData_CheckBox.IsChecked)
                            Test_Data_Click(null, null);

                        if (SYS_Set.AutoScan)
                        {
                            START_BT.IsEnabled = SN_text.IsReadOnly = true;
                            SN_text.Background = Brushes.Gray;//灰色
                            START_BT.Background = Brushes.Green;//绿
                        }
                        else
                        {
                            START_BT.IsEnabled = SN_text.IsReadOnly = false;
                            START_BT.Background = Brushes.Gray;//灰色
                            SN_text.IsReadOnly = false;
                            SN_text.Background = Brushes.Yellow;//条码框黄
                            SN_text.Focus(); //条码框获得焦点
                        }
                    }));
                }

            }
            catch (Exception ex)
            {
                STL = 100;
                throw ex; //MessageBox.Show(ex.Message);
            }
        }
        //**********************************************************************************************
        private void START_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GC.Collect();//释放内存 
                Licence();//检查许可证

                if (SYS_Set.AutoScan && SN_text.IsReadOnly == true) //按钮启动->自动扫描->设备启动
                {
                    stopWatch.Restart();//计时器复位开始
                    SN_text.Clear(); //清除条码
                    //SN_text.Background = Brushes.Yellow;//条码框黄
                    SN_text.IsReadOnly = false;    //条码框使能
                    //SN_text.Focus(); //条码框获得焦点
                    //Leak.SN_CMD(); //扫描条码命令
                    Barcode_ON();
                }
                else if (!SYS_Set.AutoScan)//手动扫描->按钮启动->设备启动
                {
                    START_BT.IsEnabled = false;
                    START_BT.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));
                    if (STL == 0) { STL = 1; }//启动
                    display_Time.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //SN输入框-按键按下->事件
        private void SN框键按下事件(object sender, KeyEventArgs e)
        {
            try
            {
                stopWatch.Restart();//计时器复位后开始
                if ((e == null||e.Key == Key.Enter) && SN_text.IsReadOnly == false)
                {
                    
                    if (SN_text.Text.Length > 3)
                    {

                        if (DB.Check_SN(SN_text.Text, DB.Factory_Name))//检查条码
                        {
                            SN_text.IsReadOnly = true;
                            SN_text.Background = Brushes.Green;//绿

                            if (SYS_Set.AutoScan || SYS_Set.EnterStart)
                            {
                                REC_inf.SN = SN_text.Text;
                                if (STL == 0) { STL = 1; }//启动
                                START_BT.IsEnabled = false;
                                START_BT.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));
                                display_Time.Focus();
                            }
                            else
                            {
                                START_BT.Background = Brushes.Green;//绿
                                START_BT.IsEnabled = true;
                            }
                        }
                    }
                    else
                    {
                        if (SYS_Set.LANG == "CN")
                            MessageBox.Show("输入条码不符合规定!");
                        else
                            MessageBox.Show("输入条码不符合规定!");
                    }
                }
                //Console.WriteLine(e.Key); 5701125604296
            }
            catch (Exception ex)
            {
                SN_text.Background = Brushes.Red;//红
                SN_text.IsReadOnly = true;
                MessageBox.Show(ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Warning);
                if (SYS_Set.AutoScan)
                    SN_text.Background = Brushes.Gray;//灰色
                else
                {
                    SN_text.IsReadOnly = false;
                    SN_text.Background = Brushes.Yellow;//条码框黄
                }
                //SN_text.Clear();               
                SN_text.Focus(); //条码框获得焦点
                SN_text.SelectAll();
            }
        }

        private void STOP_Click(object sender, RoutedEventArgs e)
        {
            STOP_Mark = true;
            if (SYS_Set.AutoScan)
            {
                START_BT.IsEnabled = SN_text.IsReadOnly = true;
                SN_text.Background = Brushes.Gray;//灰色
                START_BT.Background = Brushes.Green;//绿
            }
            else
            {
                START_BT.IsEnabled = SN_text.IsReadOnly = false;
                START_BT.Background = Brushes.Gray;//灰色
                SN_text.Background = Brushes.Yellow;//条码框黄
            }
        }
        //**********************************************************************************************

        private void UI_UP_Timer()
        {
            UI_Timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };//周期时间  FromMilliseconds   FromSeconds
            UI_Timer.Tick += (s, b) =>
            {
                更新状态();
                if (STL == 0)
                {
                    //this.Activate();
                    //stopWatch.Restart();//计时器复位开始
                    //SN_text.Clear(); //清除条码
                    //SN_text.Background = Brushes.Yellow;//条码框黄
                    //SN_text.IsReadOnly = false;   //条码框使能
                    //SN_text.Focus(); //条码框获得焦点
                    //Leak.SN_CMD();//扫描条码命令
                    //SN_text.Focus(); //条码框获得焦点
                }

            };
            UI_Timer.Start();
        }

        private string ResulTXT(ushort Resultw)
        {
            if (SYS_Set.LANG == "CN")
            {
                switch (Resultw)
                {
                    case 1:
                        return "PASS";
                    case 2:
                        return "+NG";
                    case 3:
                        return "-NG";
                    case 4:
                        return "大泄漏";
                    case 5:
                        return "罐充压过高";
                    case 6:
                        return "罐充压过低";
                    case 7:
                        return "充压过高";
                    case 8:
                        return "充压过低";
                    case 9:
                        return "超量程";
                    case 10:
                        return "防水贴NG";
                    default:
                        return "Fail";
                }
            }
            else
            {
                switch (Resultw)
                {
                    case 1:
                        return "PASS";
                    case 2:
                        return "+NG";
                    case 3:
                        return "-NG";
                    case 4:
                        return "Big_Leak";
                    case 5:
                        return "Tank_Pressure H";
                    case 6:
                        return "Tank_Pressure L";
                    case 7:
                        return "Pressure_H";
                    case 8:
                        return "Pressure_L";
                    case 9:
                        return "Out_Of_Range";
                    case 10:
                        return "Vent_Cap_NG";
                    default:
                        return "Fail";
                }
            }
        }

        //**********************************************************************************************
        #region 读写配置
        private void 读配置()
        {
            //IP_Addr.Text = Properties.Settings.Default.TCP_IP;
            //Port.Text = Properties.Settings.Default.TCP_Port;

        }
        private void 保存配置()
        {
            //Properties.Settings.Default.TCP_IP = IP_Addr.Text;
            //Properties.Settings.Default.TCP_Port = Port.Text;

            //Properties.Settings.Default.Save();
        }
        #endregion
        //**********************************************************************************************
        #region 保存布局
        private void Save_Layout()
        {
            try
            {
                var serializer = new XmlLayoutSerializer(dockManager);
                serializer.Serialize(@".\Layout.config");

                //string fileName = "Layout";// (sender as MenuItem).Header.ToString();
                //var serializer = new XmlLayoutSerializer(dockManager);
                //using (var stream = new StreamWriter(string.Format(@".\AvalonDock_{0}.config", fileName)))//AvalonDock_Layout_1.config
                //    serializer.Serialize(stream);

            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Message"); }

        }
        #endregion

        #region 恢复布局
        private void Restore_Layout()
        {
            try
            {
                var Layout_default = new XmlLayoutSerializer(dockManager);
                Layout_default.Serialize(@".\Layout_default.config"); //保存默认布局


                //if (MessageBox.Show("要恢复布局吗？", "AvalonDock Sample", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                //{
                var serializer = new XmlLayoutSerializer(dockManager);
                using (var stream = new StreamReader(@".\Layout.config"))
                    serializer.Deserialize(stream);
                //}


                var win = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "Connection");
                if (win != null)
                    win.Hidden += 连接窗口_Hidden;

                win = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "OUT1");
                if (win != null)
                    win.Hidden += 输出窗口_Hidden;

                //Curve_Click(null, null);

            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                //MessageBox.Show(ex.Message, "Message");
            }

        }
        #endregion

        //**********************************************************************************************
        #region 菜单 Click

        #region 菜单_文件
        private void 打开文件_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //OpenFileDialog openFile = new OpenFileDialog
                //{
                //    //Title="打开Dxf或csv文件",//标题文本
                //    //AddExtension = true,//文件名添加扩展名
                //    FileName = string.Empty,
                //    FilterIndex = 1,
                //    Multiselect = false,
                //    RestoreDirectory = true,//还原目录
                //    DefaultExt = "dxf",//默认扩展名字符串
                //                       //InitialDirectory = path,//初始目录
                //    Filter = "配方文件|*.xlsx;*.xls|所有文件|*.*",//
                //};
                //if (openFile.ShowDialog() == false)//打开对话框
                //    return;




            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void 保存_Click(object sender, RoutedEventArgs e)
        {

        }
        private void EXIT_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion //*************************************************

        #region 菜单_设置

        // 参数设置
        private void 系统参数_Click(object sender, RoutedEventArgs e)
        {
            Passwd_Box passwd_Box = new Passwd_Box(false);
            //passwd_Box.Value = false;
            if (!(bool)passwd_Box.ShowDialog())
                return;

            环境设定 SET_Win = new 环境设定();
            SET_Win.ShowDialog();

            //Window w = new Window();
            //w.Content = SET_Win;
            //w.ShowDialog();
            //Passwd_Box passwd_Box = new Passwd_Box();//密码输入窗口
            //if ((bool)passwd_Box.ShowDialog())
            //    MessageBox.Show("密码OK");
        }

        private void 菜单_设备参数_Click(object sender, RoutedEventArgs e)
        {

        }
        // 添加用户
        private void User_Add_Click(object sender, RoutedEventArgs e)
        {
            Passwd_Box passwd_Box = new Passwd_Box(false);
            if (!(bool)passwd_Box.ShowDialog())
            {
                return;
            }
          

        }
        private void 修改系统密码_Click(object sender, RoutedEventArgs e)
        {
            Passwd_Box passwd_Box = new Passwd_Box(true);
            passwd_Box.ShowDialog();
        }


        #endregion //*************************************************

        #region 菜单_查看
        private void 默认布局_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var serializer = new XmlLayoutSerializer(dockManager);
                using (var stream = new StreamReader(@".\Layout_default.config"))
                    serializer.Deserialize(stream);//恢复默认布局

                菜单_输出窗口.IsChecked = false;
                菜单_连接窗口.IsChecked = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message");
            }
        }
        private void 输出窗口_Click(object sender, RoutedEventArgs e)
        {
            var win = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "OUT1");
            if (win.IsHidden)
                win.Show();//显示
            else
                win.Hide();//隐藏
            菜单_输出窗口.IsChecked = !win.IsHidden;

            if (win.IsVisible)
                win.IsActive = true;
        }
        private void 输出窗口_Hidden(object sender, EventArgs e)
        {
            菜单_输出窗口.IsChecked = !dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "OUT1").IsHidden;
        }
        private void 状态栏_Click(object sender, RoutedEventArgs e)
        {
            菜单_状态栏.IsChecked = !菜单_状态栏.IsChecked;
        }
        //曲线图视图
        private void Curve_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var Layout_DocumentPane = dockManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
                if (Layout_DocumentPane != null)
                {
                    var Layout_Document = Layout_DocumentPane.Children.FirstOrDefault(m => (string)m.Title == "Curve");
                    if (Layout_Document != null)
                    {
                        Layout_Document.IsSelected = true;//选中
                        Layout_Document.Title = "Curve";
                        SN_text.Focus(); //条码框获得焦点
                    }
                    else
                    {
                        //canvas = new Canvas();
                        //canvas.Name = "canvas";
                        //canvas.Background = Brushes.Black;//黑色

                        //grid = new Grid() { Name = "border", ClipToBounds = true };//画布边界
                        //grid.PreviewMouseMove += 网格鼠标移动事件; //鼠标移动事件
                        //grid.PreviewMouseWheel += 网格鼠标滚轮事件; //鼠标滚轮事件
                        //grid.Margin = new Thickness() { Left = 0, Top = -4, Right = 0, Bottom = -4 };//4边位置
                        //grid.Background = Brushes.Black;//黑色
                        //grid.DataContext = canvas;

                        //var New_doc = new LayoutDocument() { Title = title, ContentId = "Curve_Document", ToolTip = "Curve_Document" };
                        //New_doc.IconSource = new BitmapImage(new Uri("Resources/Chartico.png", UriKind.RelativeOrAbsolute));//图标                 
                        //New_doc.Content = grid;
                        //New_doc.CanClose = false;//可否关闭
                        //New_doc.CanMove = true;//可否移动
                        //New_doc.IsActive = true;
                        //New_doc.IsSelected = true;//选中
                        //Layout_DocumentPane.Children.Add(New_doc);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message");
            }
        }
        //测试数据视图
        private void Test_Data_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = "Test Data";
                if (SYS_Set.LANG == "CN")
                    title = "测试数据";

                //布局_文档窗格
                var Layout_DocumentPane = dockManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();

                if (Layout_DocumentPane != null)
                {
                    //var Layout_Doc = Layout_DocumentPane.Children.FirstOrDefault(m => m.ContentId == "Layout_Doc");
                    //布局_文档
                    var Layout_Doc = Layout_DocumentPane.Children.FirstOrDefault(m => (string)m.ToolTip == "Test Data");//ToolTip==Layout_Doc

                    if (Layout_Doc == null)
                        goto NEW_Doc;
                    //else if (Layout_Doc.Title != title)
                    //{
                    //    Layout_Doc.Close();
                    //    goto NEW_Doc;
                    //}
                    else
                    {
                        Layout_Doc.IsSelected = true;//选中
                        Layout_Doc.Title = title;
                        return;
                    }

                NEW_Doc:
                    var New_doc = new LayoutDocument() { Title = title, ToolTip = "Test Data" };
                    New_doc.IconSource = new BitmapImage(new Uri("Resources/data.ico", UriKind.RelativeOrAbsolute));//图标

                    Test_Data test_Data = new Test_Data();//数据显示页
                    //Frame Frame = new Frame();
                    //Frame.Content = data_Win;
                    //doc.Content = Frame;

                    New_doc.Content = new Frame().Content = test_Data;//载入数据显示页

                    //dataGrid.IsReadOnly = true;//只读
                    //doc.Content = dataGrid;

                    New_doc.CanClose = true;//可否关闭
                    New_doc.CanMove = true;//可否移动
                    New_doc.IsActive = true;
                    New_doc.IsSelected = true;//选中
                    Layout_DocumentPane.Children.Add(New_doc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message");
            }

        }
        //产品追踪视图
        private void 产品追踪_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = "Product Track";
                if (SYS_Set.LANG == "CN")
                    title = "产品追踪";

                //布局_文档窗格
                var Layout_DocumentPane = dockManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();

                if (Layout_DocumentPane != null)
                {
                    //var Layout_Doc = Layout_DocumentPane.Children.FirstOrDefault(m => m.ContentId == "Layout_Doc");
                    //布局_文档
                    var Layout_Doc = Layout_DocumentPane.Children.FirstOrDefault(m => (string)m.ToolTip == "Product Track");

                    if (Layout_Doc == null)
                        goto NEW_Doc;
                    else if (Layout_Doc.Title != title)
                    {
                        Layout_Doc.Close();
                        goto NEW_Doc;
                    }
                    else
                    {
                        Layout_Doc.IsSelected = true;//选中
                        Layout_Doc.Title = title;
                        return;
                    }

                NEW_Doc:
                    var New_doc = new LayoutDocument() { Title = title, ToolTip = "Product Track" };
                    New_doc.IconSource = new BitmapImage(new Uri("Resources/STATE.png", UriKind.RelativeOrAbsolute));//图标

                    追踪信息页 Page = new 追踪信息页();//产品追踪信息页

                    New_doc.Content = new Frame().Content = Page;//载入数据显示页

                    //dataGrid.IsReadOnly = true;//只读
                    //doc.Content = dataGrid;

                    New_doc.CanClose = true;//可否关闭
                    New_doc.CanMove = true;//可否移动
                    New_doc.IsActive = true;
                    New_doc.IsSelected = true;//选中
                    Layout_DocumentPane.Children.Add(New_doc);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message");
            }
        }
        //气密参数视图
        private void 气密参数_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = "Leak Param";
                if (SYS_Set.LANG == "CN")
                    title = "设备参数";

                var Layout_DocumentPane = dockManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
                if (Layout_DocumentPane != null)
                {
                    //布局_文档
                    var Layout_Doc = Layout_DocumentPane.Children.FirstOrDefault(m => (string)m.ToolTip == "Leak Param");

                    if (Layout_Doc == null)
                        goto NEW_Doc;
                    else if (Layout_Doc.Title != title)
                    {
                        Layout_Doc.Close();
                        goto NEW_Doc;
                    }
                    else
                    {
                        Layout_Doc.IsSelected = true;//选中
                        Layout_Doc.Title = title;
                        return;
                    }


                NEW_Doc:
                    var New_doc = new LayoutDocument() { Title = title, ToolTip = "Leak Param" };
                    New_doc.IconSource = new BitmapImage(new Uri("Resources/参数.png", UriKind.RelativeOrAbsolute));//图标

                    //ParamSET Param = new ParamSET();//
                    //New_doc.Content = new Frame().Content = Param;//载入数据显示页

                    TextBox textBox = new TextBox();
                    textBox.Text = "\n\n\n\n\n\n           正在努力开发中......";
                    textBox.FontSize = 24;
                    New_doc.Content = textBox;



                    New_doc.CanClose = true;//可否关闭
                    New_doc.CanMove = true;//可否移动
                    New_doc.IsActive = true;
                    New_doc.IsSelected = true;//选中
                    Layout_DocumentPane.Children.Add(New_doc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message");
            }
        }
        #endregion //*************************************************  

        #region 在线菜单
        private void 连接窗口_Click(object sender, RoutedEventArgs e)
        {
            var win = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "Connection");
            if (win.IsHidden)
                win.Show();//显示
            else
                win.Hide();//隐藏
            菜单_连接窗口.IsChecked = !win.IsHidden;

            if (win.IsVisible)
                win.IsActive = true;
        }
        private void 连接窗口_Hidden(object sender, EventArgs e)
        {
            菜单_连接窗口.IsChecked = !dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "Connection").IsHidden;
        }

        private void 更新状态()
        {
            if (Leak.ConnectOk)
            {
                if (SYS_Set.AutoScan && (string)START_BT.Tag != "1")
                {
                    START_BT.Tag = "1";
                    SN_text.IsReadOnly = true;   //条码框使能
                    SN_text.Background = Brushes.Gray;//灰色
                    
                    START_BT.IsEnabled = true;
                    STOP_BT.IsEnabled = true;
                    START_BT.Background = Brushes.Green;//绿
                    EnterStart_CheckBox.Visibility = Visibility.Hidden;
                }
                else if (!SYS_Set.AutoScan && (string)START_BT.Tag != "2")
                {
                    START_BT.Tag = "2";
                    SN_text.IsReadOnly = false;   //条码框使能
                    SN_text.Background = Brushes.Yellow;//条码框黄
       
                    START_BT.IsEnabled = false;
                    STOP_BT.IsEnabled = true;
                    START_BT.Background = Brushes.Gray;//灰色
                    EnterStart_CheckBox.Visibility = Visibility.Visible;
                }



               





                if ((string)菜单_联机.Header != "断开联机")
                {
                    START_BT.Tag = "0";
                    SN_text.IsEnabled = true;
                    STOP_BT.Background = Brushes.Red;//红色
                   
                    菜单_联机.Header = "断开联机";
                    菜单_联机.Icon = new Image() { Source = new BitmapImage(new Uri("Resources/Connect.ico", UriKind.RelativeOrAbsolute)) };
                    连接按钮.Content = new Image() { Source = new BitmapImage(new Uri("Resources/Connect.ico", UriKind.RelativeOrAbsolute)) };
                    工具栏_联机.Content = new Image() { Source = new BitmapImage(new Uri("Resources/Connect.ico", UriKind.RelativeOrAbsolute)) };
                    状态栏.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF339933"));//状态栏在线颜色

                    菜单_上载.IsEnabled = true;
                    菜单_下载.IsEnabled = true;
                    菜单_上载.Icon = new Image() { Source = new BitmapImage(new Uri("Resources/上载ON.png", UriKind.RelativeOrAbsolute)) };
                    菜单_下载.Icon = new Image() { Source = new BitmapImage(new Uri("Resources/下载ON.png", UriKind.RelativeOrAbsolute)) };
                }
            }
            else
            {
                START_BT.IsEnabled = STOP_BT.IsEnabled = SN_text.IsEnabled = false;
                START_BT.Background = STOP_BT.Background = Brushes.Gray;//灰色

                if ((string)菜单_联机.Header == "断开联机")
                {
                    菜单_联机.Header = "联机";
                    菜单_联机.Icon = new Image() { Source = new BitmapImage(new Uri("Resources/Disconnect.ico", UriKind.RelativeOrAbsolute)) };
                    连接按钮.Content = new Image() { Source = new BitmapImage(new Uri("Resources/Disconnect.ico", UriKind.RelativeOrAbsolute)) };
                    工具栏_联机.Content = new Image() { Source = new BitmapImage(new Uri("Resources/Disconnect.ico", UriKind.RelativeOrAbsolute)) };
                    状态栏.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));//状态栏离线颜色

                    菜单_上载.IsEnabled = false;
                    菜单_下载.IsEnabled = false;
                    菜单_上载.Icon = new Image() { Source = new BitmapImage(new Uri("Resources/上载OFF.png", UriKind.RelativeOrAbsolute)) };
                    菜单_下载.Icon = new Image() { Source = new BitmapImage(new Uri("Resources/下载OFF.png", UriKind.RelativeOrAbsolute)) };
                }
            }



            time_text.Content = DateTime.Now.ToString("HH:mm:ss  yyyy年-MM月-dd日 ");//显示时间
        }



        private void 联机_Click(object sender, RoutedEventArgs e)
        {
            TCP_inf.IP = TCP_IP.Text;
            if (TCP_Port.Text == "") { TCP_Port.Text = "8000"; }
            TCP_inf.Port = int.Parse(TCP_Port.Text);
            if (Slave_ID.Text == "") { Slave_ID.Text = "1"; }
            COMx_inf.Slave_ID = byte.Parse(Slave_ID.Text);

            Leak.ConnectOk = Leak.Modbus_Conn();
            更新状态();
            OUT1_Log("连接设备_" + TCP_inf.IP + ":" + TCP_inf.Port + "\r\r");
            OUT1_Log((Leak.ConnectOk ? "连接成功!" : "断开连接!") + "\r\r");

            Licence();//检查许可证
        }
        private void 联机设定_Click(object sender, RoutedEventArgs e)
        {
            //LeakMC.SET_Conn();




            Conn_SET conn_set = new Conn_SET();//
            conn_set.ShowDialog();//打开窗口 
            if ((bool)conn_set.DialogResult)//显示对话框
            {
                //var value = conn_set.Conn_inf;
                //Console.WriteLine(value);
            }
        }
        private void 下载_Click(object sender, RoutedEventArgs e)
        {

        }
        private void 上载_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion //*************************************************

        #region 在工具菜单


        #endregion //*************************************************

        #region 帮助菜单
        private void 关于_Click(object sender, RoutedEventArgs e)
        {
            try
            {


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void 说明_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion //*************************************************
        #endregion 菜单 END
        //**********************************************************************************************

        #region 窗口关闭事件
        private void 窗口关闭事件(object sender, EventArgs e)
        {
            Save_Layout();//保存布局
            保存配置();     //保存配置
            Leak.断开所有连接();
            if (SN_SerialPort.IsOpen) { SN_SerialPort.Close(); }//关闭串口
            //Environment.Exit(0);// 强制退出，即使有其他的线程没有结束
            Process.GetCurrentProcess().Kill();//停止关联进程
            Environment.Exit(0);
        }
        #endregion


        //恢复布局
        private void 停靠容器加载完成(object sender, RoutedEventArgs e)
        {
            Restore_Layout();//恢复布局
        }

        //查看本机的IP
        private void 本机IP_Click(object sender, RoutedEventArgs e)
        {
            //查看本机IP           
            IPAddress[] IP = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            string TXT = "";
            foreach (IPAddress ip in IP)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    TXT = TXT + ip.ToString() + "\n";
                }
            }
            MessageBox.Show(TXT, "Local IP address");
        }

        private void 网格鼠标移动事件(object sender, MouseEventArgs e)
        {

        }

        private void 网格鼠标滚轮事件(object sender, MouseWheelEventArgs e)
        {

        }
        //IP框失去焦点-事件  检查IP格式
        private void TCP_IP_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DB.Format_IP(TCP_IP.Text))
                return;
            MessageBox.Show("IP格式设置错误!", "Message", MessageBoxButton.OK, MessageBoxImage.Error);//"IP格式设置错误!" 
            TCP_IP.Text = "192.168.1.1";

        }
        //IP框输入_事件
        private void TCP_IP_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (TCP_IP.SelectionLength > 0)
                TCP_IP.Clear();

            int N = TCP_IP.Text.Length - TCP_IP.Text.LastIndexOf('.');
            Console.WriteLine(N.ToString());


            if (N == 1)
            {
                e.Handled = Regex.IsMatch(e.Text, @"[^[^1-2]+");//=真时,丢弃当前输入的1个文本
            }
            else if (N == 2)
            {
                Regex.IsMatch(TCP_IP.Text, @"");
            }
            else if (N == 3)
            {

            }

            //if (re.IsMatch(e.Text))
            //    e.Handled = true;

        }

        private void TCP_Port_PreviewTextInput(object sender, TextCompositionEventArgs e)//只能输入数字
        {
            TextBox textBox = (TextBox)sender;

            Regex re = new Regex("[^[^0-9]+");//只能输入数字    Regex("[^[^0-9.]+");//数字和 .
            e.Handled = re.IsMatch(e.Text);//=真时,丢弃当前输入的1个文本
        }

        private void TCP_ID_PreviewTextInput(object sender, TextCompositionEventArgs e)//最大输入31
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Length > 1 && int.Parse(textBox.Text) == 31)
                textBox.Text = "";

            if (textBox.Text.Length > 0 && int.Parse(textBox.Text + e.Text) > 31)
            {
                e.Handled = true;
                textBox.Text = "31";
                textBox.SelectAll();
            }

            //Regex re = new Regex("[^[^0-3]+");
            //e.Handled = re.IsMatch(e.Text);//=真时,丢弃当前输入的1个文本
            //if (re.IsMatch(e.Text))
            //    e.Handled = true;
            //Console.WriteLine(Slave_ID.Text);
        }

        private void 窗口大小更改事件(object sender, SizeChangedEventArgs e)
        {

        }


        private void Main_Window_Loaded(object sender, RoutedEventArgs e)
        {
          
        }


        //写 Log 到显示
        public void OUT1_Log(string Text)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新 
            {
                if (OUT1_TextBox.LineCount > 100)
                    OUT1_TextBox.Clear();
                OUT1_TextBox.Text = OUT1_TextBox.Text + "--> " + Text + "\n";
                if (OUT1_TextBox.LineCount > 0)
                    this.OUT1_TextBox.ScrollToLine(OUT1_TextBox.LineCount - 1);//显示最后一行

            }));
        }

        //语言选择
        private void Language_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            SYS_Set.LANG = (string)menuItem.Header;
            Language_int(SYS_Set.LANG);
            //Console.WriteLine(menuItem.Header);

            string[] inf;
            if (DB.Local_login)
                inf = ((string)DB.Get_Reg("Login_inf1", ",,,,,,")).Split(',');//本机登陆
            else
                inf = ((string)DB.Get_Reg("Login_inf2", ",,,,,,")).Split(',');
            //读取登陆信息
            string login_inf = inf[0] + "," + inf[1] + "," + SYS_Set.LANG + "," + inf[3] + "," + inf[4] + "," + inf[5] + "," + inf[6];
            if (DB.Local_login)
                DB.Set_Reg("Login_inf1", login_inf);//保存本地登陆信息
            else
                DB.Set_Reg("Login_inf2", login_inf);//保存本地登陆信息
        }


        //主题选择
        private void Theme_Click(object sender, RoutedEventArgs e)
        {
            MenuItem MenuItem = (MenuItem)sender;
            if ((string)MenuItem.Header == "Dock")
                dockManager.Theme = new Xceed.Wpf.AvalonDock.Themes.GenericTheme();
            else if ((string)MenuItem.Header == "Aero")
                dockManager.Theme = new Xceed.Wpf.AvalonDock.Themes.AeroTheme();
            else if ((string)MenuItem.Header == "Metro")
                dockManager.Theme = new Xceed.Wpf.AvalonDock.Themes.MetroTheme();
            else if ((string)MenuItem.Header == "VS2010")
                dockManager.Theme = new Xceed.Wpf.AvalonDock.Themes.VS2010Theme();
        }

        

        private void Slave_ID_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Slave_ID.Text.Length > 0)
            {
                Modbus_ID = byte.Parse(Slave_ID.Text);
            }
        }

        private void 登陆窗口()
        {
            Login_win Login = new Login_win();//登陆窗口实例化
            if (!(bool)Login.ShowDialog()) //打开登陆窗口 
                this.Close(); //登陆失败! 关闭程序
        }







        //语言初始化
        private void Language_int(string language)
        {
            if (language == "CN")
            {
             
                //OUT1.Title = "输出窗口";
                //连接窗口.Title = "连接窗口";

                //菜单_文件
                MenuItem_文件.Header = "文件";
                MenuItem_打开文件.Header = "打开文件";
                菜单_保存.Header = "保存";
                MenuItem_退出.Header = "退出";
                //菜单_设置
                菜单_设置.Header = "设置";
                菜单_系统参数.Header = "系统参数";
                菜单_语言.Header = "语言";
                菜单_添加用户.Header = "添加用户";
                菜单_修改系统密码.Header = "修改系统密码";
                //菜单_视图
                菜单_视图.Header = "视图";
                菜单_默认布局.Header = "默认布局";
                菜单_主题.Header = "主题";
                菜单_输出窗口.Header = "输出窗口";
                菜单_状态栏.Header = "状态栏";
                菜单_曲线图.Header = "曲线图";
                菜单_测试数据.Header = "测试数据";
                菜单_气密参数.Header = "气密参数";
                菜单_产品追踪.Header = "产品追踪";

                //菜单_在线
                菜单_在线.Header = "在线";
                菜单_连接窗口.Header = "连接窗口";
                菜单_联机.Header = "联机";
                菜单_联机设定.Header = "联机设定";
                菜单_下载.Header = "下载";
                菜单_上载.Header = "上载";
                //菜单_工具
                菜单_工具.Header = "工具";
                //菜单_帮助
                菜单_帮助.Header = "帮助";
                菜单_关于.Header = "关于";
                菜单_说明.Header = "说明";
                // 工具栏
                曲线_Label.Content = "曲线";//
                数据_Label.Content = "数据";//
                追踪_Label.Content = "追踪";//
                参数_Label.Content = "参数";//
                START_BT.Content = "启 动";
                STOP_BT.Content = "停 止";
                //画布下工具条
                MODEL_Label.Content = "型号 :";
                InAir_Label.Content = "气压 :";
                Pa_Label.Content = "压差 :";
                CCM_Label.Content = "泄漏值 :";
                Time_Label.Content = "时间 :";


                //下状态栏
                User_Label.Content = "操作者 :";
                Line_Label.Content = "拉线 :";
                oper_Label.Content = "工序 :";
                model_Label.Content = "型号 :";
            }
            else
            {
                //OUT1.Title = "OUT1";
                //连接窗口.Title = "Connection";

                //菜单_文件
                MenuItem_文件.Header = "File";
                MenuItem_打开文件.Header = "Open File";
                菜单_保存.Header = "Save";
                MenuItem_退出.Header = "Exit";
                //菜单_设置
                菜单_设置.Header = "Set Up";
                菜单_系统参数.Header = "System Param";
                菜单_语言.Header = "Language";
                菜单_添加用户.Header = "Add User";
                菜单_修改系统密码.Header = "Change SYS Passwd";
                //菜单_视图
                菜单_视图.Header = "View";
                菜单_默认布局.Header = "Default Layout";
                菜单_主题.Header = "Theme";
                菜单_输出窗口.Header = "Out Log";
                菜单_状态栏.Header = "Statu Bar";
                菜单_曲线图.Header = "Curve";
                菜单_测试数据.Header = "Test Data";
                菜单_气密参数.Header = "Leak Param";
                菜单_产品追踪.Header = "Product Track";
                //菜单_在线
                菜单_在线.Header = "On Line";
                菜单_连接窗口.Header = "Conn Win";
                菜单_联机.Header = "Conn";
                菜单_联机设定.Header = "Conn Set";
                菜单_下载.Header = "Down";
                菜单_上载.Header = "Upload";
                //菜单_工具
                菜单_工具.Header = "Tool";
                //菜单_帮助
                菜单_帮助.Header = "Help";
                菜单_关于.Header = "About";
                菜单_说明.Header = "Explain";
                // 工具栏
                曲线_Label.Content = "Curve";//
                数据_Label.Content = "Data";//
                追踪_Label.Content = "Track";//
                参数_Label.Content = "Param";//
                START_BT.Content = "START";
                STOP_BT.Content = "STOP";
                //画布下工具条
                MODEL_Label.Content = "MODEL :";
                InAir_Label.Content = "InAir :";
                Pa_Label.Content = "Pa :";
                CCM_Label.Content = "Leak :";
                Time_Label.Content = "Time :";
                //下状态栏
                User_Label.Content = "USER :";
                Line_Label.Content = "Line :";
                oper_Label.Content = "OPER :";
                model_Label.Content = "MODEL :";

            }

            var Layout_DocumentPane = dockManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            var Layout_Doc = Layout_DocumentPane.Children.FirstOrDefault(m => (string)m.ToolTip == "Test Data");//
            if (Layout_Doc != null)
                Test_Data_Click(null, null);

            Layout_Doc = Layout_DocumentPane.Children.FirstOrDefault(m => (string)m.ToolTip == "Product Track");//
            if (Layout_Doc != null)
                产品追踪_Click(null, null);

            Layout_Doc = Layout_DocumentPane.Children.FirstOrDefault(m => (string)m.ToolTip == "Leak Param");//
            if (Layout_Doc != null)
                气密参数_Click(null, null);

            //Layout_Doc = Layout_DocumentPane.Children.FirstOrDefault(m => (string)m.ToolTip == "Curve Document");//
            //if (Layout_Doc != null)
            //    Curve_Click(null, null);
        }
     
        private void Reg()
        {
            try
            {

                DB.Get_Reg("install", DateTime.Now);//软件安装时间

                DB.Get_Reg("TCP_Conn_inf", "1,192.168.19.100,8000");//Modbus_TCP连接设置
                DB.Get_Reg("RTU_Conn_inf", "COM1,9600,8,N,1");//Modbus_RTU连接设置
                DB.Get_Reg("SN_Conn_inf", "COM1,9600,8,N,1");//条码串设置
                DB.ServerIP = (string)DB.Get_Reg("Dace_ServerIP", "192.168.19.21");//DACE 服务器IP
                DB.Get_Reg("Vace_ServerIP", "192.168.105.20");//VACE 服务器IP
                DB.Get_Reg("Login_inf1", "USER,USER,CN,DACE,3520,202,202_8");//本机用户登陆信息
                DB.Get_Reg("Login_inf2", "D130282,D130282,CN,DACE,3520,202,202_8");//服务器I用户登陆信息

                SYS_Set.AutoScan   = (string)DB.Get_Reg("AutoScan", "False") != "False"; //自动扫描开关
                SYS_Set.EnterStart = (string)DB.Get_Reg("EnterStart", "False") != "False";//回车启动开关
                ScanTime   = double.Parse((string)DB.Get_Reg("ScanTime", "2"));//自动扫描延时
                Run_Count  = int.Parse((string)DB.Get_Reg("Run_Count", "1")); //软件运行计数
                SYS_Set.Set_Passwd = (string)DB.Get_Reg("Passwd", "8888"); //系统设置密码

                DB.Set_Reg("Run_Count", (Run_Count+1).ToString());//写运行次数
                //Console.WriteLine(Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void Licence()
        {
            if (Run_Count >= Usage_CNT)
            {
                if (SYS_Set.LANG == "CN")
                    MessageBox.Show("软件运行出错!，请与开发者联系!");
                else
                    MessageBox.Show("Software running error!, Please contact the developer!");
                Process.GetCurrentProcess().Kill();//停止关联进程
                Environment.Exit(0);
            }
        }


    }
}
