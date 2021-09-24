
using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Leakage_Lib
{
    /// <summary>
    /// 连接设置.xaml 的交互逻辑
    /// </summary>
    public partial class Conn_SET : Window
    {
        string conn_inf = "";//

        SerialPort TEST_SerialPort = new SerialPort();


        /// <summary>
        /// </summary>
        public Conn_SET()
        {
            InitializeComponent();
            INIT();
        }




        //Conn_SET conn_set = new Conn_SET();//
        //conn_set.ShowDialog();//打开窗口 
        //if ((bool) conn_set.DialogResult)//显示对话框
        //{
        //  double value = conn_set.Value;
        //}

        private void INIT()
        {
            Cancel_Button.IsCancel = true;// IsCancel="True"

            switch (SetData.Comm_MODE)
            {
                case Comm_Mode.ACE_TCP:
                    Protocol_select.SelectedIndex = 0;
                    TCP_init();
                    break;
                case Comm_Mode.lnterTech_TCP:
                    Protocol_select.SelectedIndex = 1;
                    TCP_init();
                    break;
                case Comm_Mode.ACE_COM:
                    Protocol_select.SelectedIndex = 2;
                    Port_init();
                    break;
                case Comm_Mode.lnterTech_COM:
                    Protocol_select.SelectedIndex = 3;
                    Port_init();
                    break;
                case Comm_Mode.WaYeal_COM:
                    Protocol_select.SelectedIndex = 4;
                    Port_init();
                    break;
                default:
                    Protocol_select.SelectedIndex = 0;
                    COMM_TabControl.SelectedIndex = 0;
                    break;
            }
            //Protocol.Header = "协议选择";//"Protocol select"
            //Protocol_select.Items.Clear();
            //Protocol_select.Items.Add("ACE_TCP");
            //Protocol_select.Items.Add("ACE_COM");
            //Protocol_select.Items.Add("lnterTech_TCP");
            //Protocol_select.Items.Add("lnterTech_COM");
            //Protocol_select.Items.Add("WaYeal_COM");
            //Serial_set.Header = "&#xA;串口设置";//"&#xA;Serial_set"

        }


        private void TCP_init()
        {
            COMM_TabControl.SelectedIndex = 0;//选择 TCP

            Slave_id.Text = COMx_inf.Slave_ID.ToString();
            if (TCP_inf.IP != null)
            {
                IP_Addr.Text = TCP_inf.IP;
            }
            if (TCP_inf.Port != 0)
            {
                port.Text = TCP_inf.Port.ToString();
            }
        }

        //串口初始化
        private void Port_init()
        {
            COMM_TabControl.SelectedIndex = 1;//选择 COM
            UP_Port_Num();//刷新串口号数据
            Port_Num.SelectedIndex = 0;//
            //设置串口波特率 
            switch (COMx_inf.BaudRate)
            {
                case "9600":
                    Baud.SelectedIndex = 0;
                    break;
                case "19200":
                    Baud.SelectedIndex = 1;
                    break;
                case "38400":
                    Baud.SelectedIndex = 2;
                    break;
                case "57600":
                    Baud.SelectedIndex = 3;
                    break;
                case "115200":
                    Baud.SelectedIndex = 4;
                    break;
                default:
                    Baud.SelectedIndex = 0;
                    break;
            }
            //设置串口数据位 7/8 
            switch (COMx_inf.DataBits)
            {
                case "7":
                    Data_Bits.SelectedIndex = 0;
                    break;
                case "8":
                    Data_Bits.SelectedIndex = 1;
                    break;
                default:
                    Data_Bits.SelectedIndex = 0;
                    break;
            }
            //设置串口停止位 1 / 2 / 1.5
            switch (COMx_inf.StopBits)
            {
                case "1"://1 StopBits.One
                    Data_Bits.SelectedIndex = 0; 
                    break;
                case "2"://2 StopBits.Two
                    Data_Bits.SelectedIndex = 1;
                    break;
                //case StopBits.OnePointFive://3
                //    Data_Bits.SelectedIndex = 2;
                //    break;
                default:
                    Data_Bits.SelectedIndex = 0;
                    break;
            }
            //设置串口校验位
            switch (COMx_inf.Parity)
            {
                case "0"://0 Parity.None
                    Parity_Bit.SelectedIndex = 0;
                    break;
                case "1"://1 Parity.Odd
                    Parity_Bit.SelectedIndex = 1;
                    break;
                case "2"://2 Parity.Even
                    Parity_Bit.SelectedIndex = 2;
                    break;
                default:
                    Parity_Bit.SelectedIndex = 0;
                    break;
            }
        }

        //刷新串口号
        private void UP_Port_Num()
        {
            string[] PortNames = SerialPort.GetPortNames();//获取当前计算机的串行端口名的数组。
            Port_Num.Items.Clear();//清除串口数组
            foreach (string str in PortNames)
            { Port_Num.Items.Add(str); }//添加item
        }

        //串口框鼠标左键按下事件->刷新串口号
        private void Port_Num_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { UP_Port_Num(); }//刷新串口号 

        //失去焦点事件->检查IP格式
        private void IP_Addr_LostFocus(object sender, RoutedEventArgs e)//检查IP格式
        {
            if (!(Regex.IsMatch(IP_Addr.Text, @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])(\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])){3}$")))
            {
                IP_Addr.Text = "192.168.1.1";
                MessageBox.Show("IP format setting error!", "Message");//"IP格式设置错误!"              
            }
        }
       
        
        #region //打开测试串口

        //TEST
        private void TEST_Click(object sender, RoutedEventArgs e)//测试连接
        {
            try
            {
                SetData.Comm_MODE = (Comm_Mode)Protocol_select.SelectedIndex;
                if (SetData.Comm_MODE == Comm_Mode.ACE_COM | SetData.Comm_MODE == Comm_Mode.lnterTech_COM | SetData.Comm_MODE == Comm_Mode.WaYeal_COM)
                {
                    if (Port_Num.SelectedItem == null)
                    {
                        MessageBox.Show("no serial port!", "Message");
                        return;
                    }
                    //检查串口是否打开
                    if (TEST_SerialPort.IsOpen)
                    {
                        TEST_SerialPort.Close(); //关闭串口
                    }
                    TEST_SerialPort.PortName = Port_Num.SelectedItem.ToString();//设置串口号                                                                               
                    TEST_SerialPort.BaudRate = Convert.ToInt32(Baud.Text.ToString());//设置串口波特率 
                    TEST_SerialPort.DataBits = Convert.ToInt32(Data_Bits.Text.ToString());//设置串口数据位 7/8 
                    TEST_SerialPort.StopBits = (StopBits)(Stop_Bit.SelectedIndex + 1);//设置串口停止位 1/2    
                    TEST_SerialPort.Parity = (Parity)Parity_Bit.SelectedIndex;//设置串口校验位
                                                                              //Console.WriteLine("{0},{1}", Stop_Bbit.SelectedIndex, Parity_Bit.SelectedIndex);

                    TEST_SerialPort.Open();
                    TEST_SerialPort.WriteTimeout = 3000;
                    TEST_SerialPort.ReadTimeout = 3000;
                    TEST_SerialPort.ReceivedBytesThreshold = 1;//一个字节接收

                    //Console.WriteLine("{0},{1},{2},{3},{4}", MainWindow.SerialPort.PortName,
                    //    MainWindow.SerialPort.BaudRate.ToString(), MainWindow.SerialPort.DataBits.ToString(),
                    //    MainWindow.SerialPort.Parity.ToString(), MainWindow.SerialPort.StopBits.ToString());

                    if (LeakMC.Connect() && LeakMC.TEST_Conn())
                    {
                        COMx_inf.COMx = Port_Num.SelectedItem.ToString();//设置串口号 
                        COMx_inf.BaudRate = Baud.Text.ToString();//设置串口波特率 
                        COMx_inf.DataBits = Data_Bits.Text.ToString();//设置串口数据位 7/8 
                        COMx_inf.StopBits = Stop_Bit.Text.ToString();//设置串口停止位 1/2 
                        COMx_inf.Parity = Parity_Bit.Text.ToString();//设置串口校验位

                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        //检查串口是否打开
                        if (TEST_SerialPort.IsOpen)
                        {
                            TEST_SerialPort.Close(); //关闭串口
                        }
                        MessageBox.Show("Connection Failed!", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                    }


                }
                else if(SetData.Comm_MODE == Comm_Mode.ACE_TCP | SetData.Comm_MODE == Comm_Mode.lnterTech_TCP)
                {
                    COMx_inf.Slave_ID = Convert.ToByte(Slave_id.Text);//byte.Parse(Slave_id.Text);
                    TCP_inf.IP = IP_Addr.Text;
                    TCP_inf.Port = Convert.ToInt32(port.Text);//int.Parse(port.Text);

                    if (LeakMC.Connect() && LeakMC.TEST_Conn())
                    {
                        this.DialogResult = true;
                       // this.Close();
                    }
                    else
                    {

                        MessageBox.Show("Connection Failed!", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    //Console.WriteLine(Cancel_Button.IsCancel);
                    //
                    //
                    //
                    //Leakage.StopConnect();
                }

            }
            catch (Exception EX) { MessageBox.Show(EX.Message, "Message"); }

        }
        #endregion

        //确认按钮事件
        private void ENTER_Click(object sender, RoutedEventArgs e)
        {
            if (COMM_TabControl.SelectedIndex == 0)//TCP="ACE_TCP,id,ip,prot"
            {
                conn_inf = Protocol_select.Text + "," + TCP_ID.Text + "," + port.Text + "," + IP_Addr.Text;
            }
            else if (COMM_TabControl.SelectedIndex == 1)//COMx="ACE_COM,id,COM1,9600,8,N,1"    COM1,9600,8,N,1
            {
                conn_inf = Protocol_select.Text + "," + TCP_ID.Text + "," + port.Text + "," + IP_Addr.Text;
            }
            
            this.DialogResult = true;//
        }

        //取消按钮事件
        private void CANCEL_Click(object sender, RoutedEventArgs e) { this.Close();}

        //输入事件->限制只能输入<31数据
        private void id_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //限制只能输入2位数字
            TextBox textBox = (TextBox)sender;
            Regex re = new Regex("^[^0-9]+");//只能输入数字和
            e.Handled = re.IsMatch(e.Text);//=真时,丢弃当前输入的1个文本   e.Text只是键入时的1位
            if (Convert.ToInt16(textBox.Text) > 31)//只能输入<31数据
            {
                textBox.Text = "31";
            }
        }

        //协议选择框下拉事件
        private void Protocol_select_DropDownClosed(object sender, EventArgs e)
        {
            SetData.Comm_MODE = (Comm_Mode)Protocol_select.SelectedIndex;
            INIT();
            //if (MainWindow.通信方式 == Comm_MODE.ACE_TCP | MainWindow.通信方式 == Comm_MODE.lnterTech_TCP)
            //{
            //    CONN_TabControl.SelectedIndex = 0;
            //}
            //else
            //{
            //    CONN_TabControl.SelectedIndex = 1;
            //}

        }

        
    }

   
}
