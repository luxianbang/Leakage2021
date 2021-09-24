
using Leakage_Lib;
using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Leakage2021
{
    //App.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新 
    //{
    //    Thread.Sleep(500);
    //}));
    /*
        设定数据 地址21开始 30个
        0-4 MODEL, 5下压时间, 6罐稳定时间, 7充气时间,8稳定时间,9检测时间,10排气时间
        13泄漏值单位
        14 15罐充气上限,16 17罐充气下限,18 19大泄漏压差,20 21充压力上限,22 23充压力下限
        24 25泄漏值上限,26 27泄漏值下限,28 29容积

        地址0写入1=启动/0=停止,故障后写入2  

        故障代码: 
        20 = 请检查产品是否放到位? 感应器 X2=ON ?
        21 = 请检查安全光栅是否动作? 感应器 X3=OFF ?
        22 = 请检查电磁阀动作是否异常?

       运行数据 地址1开始 20个
       [0]=1运行，[1]=下压时间,[2]=罐稳定时间,[3]=充气时间,[4]=稳定时间,[5]=检测时间,[6]=排气时间
       [8][9]=罐压力值,[10][11]=罐剩余压力,[12][13]=充气压力,[14][15]=泄漏压力Pa,[16][17]=CCM
       18测试结果 
       “PASS”, “+NG”, “-NG”, “大泄漏”, “罐充压过高”, “罐充压过低”, “充压过高”, “充压过低”, “超量程”, “防水贴 NG”)

    */



    /// <summary>
    /// ASCII_KEY.xaml 的交互逻辑
    /// </summary>
    public partial class ParamSET : Page
    {

        private DispatcherTimer Timer;//定时器

        private ushort[] Write_Data;
        private ushort Write_Addr;
        private bool Write_Status = false;//写状态

        private ushort RWI_Base = 22000; //HMI RWI0R 的地址
        private ushort RWI_Index = 0; //HMI RWI 索引
        private ushort LW9000_addr = 12000 + 9000; //HMI RWI 索引

        private ushort PROG_NUM = 0; //HMI 程序编号

       // bool ConnectOk = false;

        //private ushort PLC_Fill_Uint = 0; //PLC充气单位 0=Kpa, 1=Bar
        //private ushort PLC_Leak_Uint = 0; //PLC泄漏单位 0=pa, 1=CCM

        private recipe Recipe = new recipe();
        /// <summary>参数设置
        /// </summary>
        public ParamSET()
        {
            InitializeComponent();
            //Comm_Thread = new Thread(new ThreadStart(CommThread));
            //Comm_Thread.Start();
            //Timer_Thread();

            Binding binding = new Binding();//创建Binding实例
            binding.Source = Recipe;//指定数据源
            binding.Path = new PropertyPath("PROG_Name");//指定访问路径 
            //使用Binding 连接数据源与Bingding目标
            BindingOperations.SetBinding(this.Recipe_MODEL_txt, TextBox.TextProperty, binding);//使用binding实例将数据源与目标关联起来
            binding = new Binding();//创建Binding实例
            binding.Source = Recipe;//指定数据源
            binding.Path = new PropertyPath("Fill_Press_H");//指定访问路径
            BindingOperations.SetBinding(this.Fill_Press_Hi_A_txt, TextBox.TextProperty, binding);//使用binding实例将数据源与目标关联起来
            //参数为 目标；目标的某个属性

        }
        private void Timer_Thread()//定时器
        {
            Timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };//周期时间
            Timer.Start();//启动定时器
            Timer.Tick += (s, b) =>
            {
                //DateTime dateTime = DateTime.Now;
                try
                {
                    //Console.WriteLine("开始:"+ DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff"));//
                    if (Leak.ConnectOk)//Index
                    {
                        ushort[] uintData = null;//要写入的数组
                        if (Write_Status)
                        {
                            //LeakMC.Master_TCP.WriteMultipleRegisters(COMx_inf.Slave_ID, Write_Addr, Write_Data);//写多个寄存器
                            Write_Status = false;//写状态
                        }

                        //uintData = LeakMC.Master_TCP.ReadHoldingRegisters(COMx_inf.Slave_ID, LW9000_addr, 1);
                        //RWI_Index = uintData[0];//LW9000

                        if (Recipe_TabControl.SelectedIndex == 0)//PLC现在的配方
                        {
                            //uintData = LeakMC.Master_TCP.ReadHoldingRegisters(COMx_inf.Slave_ID, 6000, 40);

                            byte[] BYTE = new byte[12];
                            Buffer.BlockCopy(uintData, 0, BYTE, 0, 12);
                            string Text = string.Empty;
                            Text += Encoding.UTF8.GetString(BYTE);
                            Text = Text.Replace("\0", "");//[0-5]=机种名
                            //Recipe_MODEL_txt.Text = Text;//机种名
                            Recipe.PROG_Name = Text;

                            //Console.ReadLine();
                            //Single.Parse("");
                            //Convert.ToSingle("");

                            float[] floatData = new float[1];
                            Buffer.BlockCopy(uintData, 28 * 2, floatData, 0, 4);//Convert ushort array to Float  

                            Recipe.Fill_Press_H = floatData[0];
                            //Fill_Press_Hi_A_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 30 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Fill_Press_Lo_A_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 32 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Upper_Leak_Limit_A_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 34 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Lower_Leak_Limit_A_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 36 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Cubage_A_txt.Text = floatData[0].ToString();

                            Buffer.BlockCopy(uintData, 20 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Tank_Press_Hi_A_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 22 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Tank_Press_Lo_A_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 26 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Ven_Cap_Press_Differe_A_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 24 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Tank_Press_Differe_A_txt.Text = floatData[0].ToString("F2");

                            JIG_Down_Time_A_txt.Text = ((double)uintData[11] / 10).ToString("F1");
                            Air_Fill_Time_A_txt.Text = ((double)uintData[13] / 10).ToString("F1");
                            Settling_Time_A_txt.Text = ((double)uintData[14] / 10).ToString("F1");
                            Test_Time_A_txt.Text = ((double)uintData[15] / 10).ToString("F1");
                            Exhaust_Time_A_xtx.Text = ((double)uintData[16] / 10).ToString("F1");
                            Tank_Settling_Time_A_txt.Text = ((double)uintData[12] / 10).ToString("F1");
                            Vent_Cap_Num_A_txt.Text = uintData[10].ToString("F0");


                        }
                        else if (Recipe_TabControl.SelectedIndex == 1)//选择HMI的配方
                        {
                            PROG_NUM = (ushort)(RWI_Index / 40 + 1);//显示程序号

                            //uintData = LeakMC.Master_TCP.ReadHoldingRegisters(COMx_inf.Slave_ID, RWI_Base, 40);

                            byte[] BYTE = new byte[12];
                            Buffer.BlockCopy(uintData, 0, BYTE, 0, 12);
                            string Text = string.Empty;
                            Text += Encoding.UTF8.GetString(BYTE);
                            Text = Text.Replace("\0", "");//[0-5]=机种名
                            PROG_NUM_txt.Text = "#" + PROG_NUM.ToString("000") + ":  ";//程序编号
                            Select_MODEL_txt.Text = Text;//显示程序名

                            float[] floatData = new float[1];
                            Buffer.BlockCopy(uintData, 28 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Fill_Press_Hi_B_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 30 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Fill_Press_Lo_B_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 32 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Upper_Leak_Limit_B_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 34 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Lower_Leak_Limit_B_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 36 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Cubage_B_txt.Text = floatData[0].ToString();

                            Buffer.BlockCopy(uintData, 20 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Tank_Press_Hi_B_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 22 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Tank_Press_Lo_B_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 26 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Ven_Cap_Press_Differe_B_txt.Text = floatData[0].ToString("F2");

                            Buffer.BlockCopy(uintData, 24 * 2, floatData, 0, 4);//Convert ushort array to Float
                            Tank_Press_Differe_B_txt.Text = floatData[0].ToString("F2");

                            JIG_Down_Time_B_txt.Text = ((double)uintData[11] / 10).ToString("F1");
                            Air_Fill_Time_B_txt.Text = ((double)uintData[13] / 10).ToString("F1");
                            Settling_Time_B_txt.Text = ((double)uintData[14] / 10).ToString("F1");
                            Test_Time_B_txt.Text = ((double)uintData[15] / 10).ToString("F1");
                            Exhaust_Time_B_xtx.Text = ((double)uintData[16] / 10).ToString("F1");
                            Tank_Settling_Time_B_txt.Text = ((double)uintData[12] / 10).ToString("F1");
                            Vent_Cap_Num_B_txt.Text = uintData[10].ToString("F0");

                        }


                    }
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message, "Message");
                    //string AA = Ex.Message;
                    Timer.Stop();
                }

                //LeakMC.Write_Log("结束:" + (DateTime.Now - dateTime).ToString("fff"));//记录日志
                //Console.WriteLine("结束:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff"));//
            };
        }

        private void NEXT_Click(object sender, RoutedEventArgs e)
        {
            if (RWI_Index < 4000)
            {
                RWI_Index += 40;
                Write_Addr = LW9000_addr;
                Write_Data = new ushort[] { RWI_Index };
                Write_Status = true;//写状态
                //LeakMC.Modbus_M.WriteMultipleRegisters(LeakMC.TCP.ID, LW9000_addr, Write_Data);//写多个寄存器
            }

        }

        private void PREV_Click(object sender, RoutedEventArgs e)
        {
            if (RWI_Index >= 40)
            {
                RWI_Index -= 40;
                Write_Addr = LW9000_addr;
                Write_Data = new ushort[] { RWI_Index };
                Write_Status = true;//写状态
                //LeakMC.Modbus_M.WriteMultipleRegisters(LeakMC.TCP.ID, LW9000_addr, Write_Data);//写多个寄存器
            }
        }




        /* 键盘输入数字 */
        private void Value_BOX_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                //Write_Status = true;//写状态

                if (Leak.ConnectOk)
                {
                    string 数字 = Regex.Replace(textBox.Uid, @"[^0-9]+", "");//取数字,要写入数据的地址
                    ushort addr = (ushort)Convert.ToInt16(数字);//要读取的地址 

                    if (Recipe_TabControl.SelectedIndex == 1)//选择HMI的配方
                    {
                        addr += RWI_Base;//RWI的基址=22000
                    }

                    KEY_Num in_Num = new KEY_Num();//输入键盘
                    in_Num.ShowDialog();//打开键盘窗口 

                    if ((bool)in_Num.DialogResult)//显示对话框
                    {
                        ushort[] W_Data = { (ushort)(in_Num.Value) }; // "U0" 要写入的数据
                        if ((string)textBox.Tag == "U1")
                        {
                            W_Data[0] = (ushort)(in_Num.Value * 10);// "U1"
                        }
                        else if ((string)textBox.Tag == "F2")
                        {
                            W_Data = new ushort[] { 0, 0 }; //"F2"
                            Buffer.BlockCopy(new float[1] { (float)in_Num.Value }, 0, W_Data, 0, 4);//Float To Ushort  
                        }
                        else if ((string)textBox.Tag == "PROG_NUM")

                        {
                            if (in_Num.Value > 100)
                            {
                                in_Num.Value = 100;
                            }
                            else if (in_Num.Value <= 0)
                            {
                                in_Num.Value = 1;
                            }
                            W_Data[0] = (ushort)((in_Num.Value - 1) * 40);// "PROG_NUM"

                            addr = (ushort)Convert.ToInt16(数字);//要读取的地址 
                        }

                        Write_Addr = addr;
                        Write_Data = W_Data;
                        Write_Status = true;//写状态
                        //LeakMC.Modbus_M.WriteMultipleRegisters(LeakMC.TCP.ID, addr, W_Data);//写多个寄存器
                    }
                }

            }
            catch (Exception exception) { MessageBox.Show(exception.Message, "Message"); }
            //Write_Status = false;//写状态
        }

        /* 字符串输入 */
        private void MODEL_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                //Write_Status = true;//写状态

                if (Leak.ConnectOk)
                {
                    string 数字 = Regex.Replace(textBox.Uid, @"[^0-9]+", "");//取数字
                    ushort addr = (ushort)Convert.ToInt16(数字);

                    KEY_ASSCII KEY_asscii = new KEY_ASSCII();//字符串输入键盘
                    KEY_asscii.ShowDialog();//打开键盘窗口 
                    if ((bool)KEY_asscii.DialogResult)//显示对话框
                    {
                        byte[] BYTE = Encoding.UTF8.GetBytes(KEY_asscii.TXT + "\0\0\0\0\0\0\0\0\0\0\0\0");
                        ushort[] W_Data = new ushort[6];
                        Buffer.BlockCopy(BYTE, 0, W_Data, 0, 12);

                        if (Recipe_TabControl.SelectedIndex == 1)//选择HMI的配方
                        {
                            addr += RWI_Base;//RWI的基址=22000
                            //addr += (ushort)(RWI_Base + RWI_Index);//RWI的基址=22000
                        }
                        Write_Addr = addr;
                        Write_Data = W_Data;
                        Write_Status = true;//写状态
                        //LeakMC.Modbus_M.WriteMultipleRegisters(LeakMC.TCP.ID, addr, W_Data);
                    }
                }

            }
            catch (Exception exception) { MessageBox.Show(exception.Message, "Message"); }
            //Write_Status = false;//写状态
        }

        private void DELETE_Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)//删除1个配方
        {
            Button Button = (Button)sender;

            Thread th = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(1000);
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    Write_Addr = RWI_Base;
                    Write_Data = new ushort[40]; //"F2"
                    Write_Status = true;//写状态
                    //Console.Beep(800,300);
                    Console.Beep();
                    //Application.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新                  
                    //{
                    //}));
                }
            }));
            th.Start();

        }
        private void SELECT_Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button Button = (Button)sender;

            Thread th = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(1000);

                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    //Write_Data = LeakMC.Master_TCP.ReadHoldingRegisters(COMx_inf.Slave_ID, RWI_Base, 40);//读出数据
                    Write_Addr = 6000;//PLC D6000
                    Write_Status = true;//写状态
                    Console.Beep();

                    Application.Current.Dispatcher.BeginInvoke(new Action(() => //委托//UI更新                  
                    {
                        Recipe_TabControl.SelectedIndex = 0;//显示PLC的配方
                    }));
                }
            }));
            th.Start();

        }

        private void SELECT_PROG_Button_Click(object sender, RoutedEventArgs e)
        {
            Recipe_TabControl.SelectedIndex = 1;
        }


    }




    //创建一个名为 recipe 的类，它具有    40个属性
    class recipe : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string Prog_Name { get; set; }
        public float fill_Press_H { get; set; }
        public float fill_Press_L { get; set; }
        public float Fill_Press_L { get => fill_Press_L; set => fill_Press_L = value; }
        public string PROG_Name
        {
            get { return Prog_Name; }
            set
            {
                Prog_Name = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("PROG_Name"));//当Name的属性值发生改变时，PropertyChanged事件触发
                }
            }
        }

        public float Fill_Press_H
        {
            get { return fill_Press_H; }
            set
            {
                fill_Press_H = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Fill_Press_H"));//当Name的属性值发生改变时，PropertyChanged事件触发
                }
            }
        }


        //private void button0_Click(object sender, EventArgs e)
        //{
        //    //TabPage Page = new TabPage();
        //    //Page.Name = "Page" + index.ToString();
        //    //Page.Text = "tabPage" + index.ToString();
        //    //Page.TabIndex = index;
        //    //this.tabControl1.Controls.Add(Page);
        //    //this.tabControl1.SelectedTab = Page;
        //    //index++;
        //}


    }
}
