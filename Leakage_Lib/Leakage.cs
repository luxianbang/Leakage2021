using System;
using System.IO.Ports;

using System.Net.Sockets;
using Modbus.Device;    //for modbus master 主
using System.Net;       //for tcp client
using System.Net.NetworkInformation;
using System.IO;
using System.Windows;


namespace Leakage_Lib
{
    //public sealed class GG : Window //密封的 别的地方不能 NEW
    //{
    //}
    /// <summary>ACE Leakage 连接协议</summary>
    public partial class LeakMC : Curved
    {
        static bool ConnectOk = false;

        /// <summary>设置气密机的连接</summary>
        public static void SET_Conn() //设置连接
        {
            Conn_SET conn_SET = new Conn_SET();
            conn_SET.ShowDialog();// 打开窗口            
            if ((bool)conn_SET.DialogResult)//显示对话框
            {
                //MessageBox.Show("通信连接成功!", "提示");
            }
            else
            {
                if (!ConnectOk)
                {
                    MessageBox.Show("通信连接失败!", "提示");

                }
            }
        }
        /// <summary>读通讯方式</summary>
        public static string Read_comm_mode()
        {
            return "ACE_TCP";
        }

        #region 连接
        /// <summary>气密机置连接</summary>
        public static bool Connect()
        {
            try
            {
                if (SetData.Comm_MODE == Comm_Mode.ACE_TCP | SetData.Comm_MODE == Comm_Mode.lnterTech_TCP)
                {

                    return false;
                }
                else if (SetData.Comm_MODE == Comm_Mode.lnterTech_TCP)
                {
                    return false;//TCP连接
                }
                else if (SetData.Comm_MODE == Comm_Mode.ACE_COM)
                {
                    return false;//串口连接
                }
                else if (SetData.Comm_MODE == Comm_Mode.lnterTech_COM)
                {
                    return false;//串口连接
                }
                else if (SetData.Comm_MODE == Comm_Mode.WaYeal_COM)
                {
                    return false;//串口连接
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message, "Message");
            }
            return false;//串口连接
        }
        /// <summary>测试连接</summary>



        /// <summary>测试连接 </summary>
        public static bool TEST_Conn() //测试连接
        {
            if (ConnectOk)
            {
                switch (SetData.Comm_MODE)
                {
                    case Comm_Mode.ACE_TCP:


                    case Comm_Mode.ACE_COM:
                    //uintData = Master_TCP.ReadHoldingRegisters(TCP.ID, 0, 1);
                    //return uintData.Length == 1;
                    case Comm_Mode.lnterTech_TCP:
                    //uintData = Master_TCP.ReadHoldingRegisters(TCP.ID, 0, 1);
                    //return uintData.Length == 1;
                    case Comm_Mode.lnterTech_COM:
                    //uintData = Master_TCP.ReadHoldingRegisters(TCP.ID, 0, 1);
                    //return uintData.Length == 1;
                    case Comm_Mode.WaYeal_COM:
                    //uintData = Master_TCP.ReadHoldingRegisters(TCP.ID, 0, 1);
                    //return uintData.Length == 1;
                    default:
                        return false;
                }

            }
            return false;
        }

        #endregion
        #region Ping命令检测网络是否畅通
        private static bool MyPing(string ip)
        {
            Ping ping = new Ping();
            try
            {
                PingReply pr;
                pr = ping.Send(ip);
                //Console.WriteLine("Ping " + pr.Status.ToString());
                return (pr.Status == IPStatus.Success);
            }
            catch
            {
                return false;
            }
        }
        #endregion


        #region 记录日志,逐行写入   
        /// <summary>逐行写入日志</summary>
        public static void Write_Log(string Log)
        {

            //判断文件夹的存在、创建、删除文件夹            
            string path = System.Windows.Forms.Application.StartupPath + "\\Log";//路径的正确写法
            if (!Directory.Exists(path))//如果不存在就创建file文件夹
                Directory.CreateDirectory(path);//创建该文件夹


            string fileName = "Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".Log";
            Log = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff  ") + Log;//  Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff"));//
            //Console.WriteLine("写："+ModbusTcp.LogText);
            try
            {
                //if (!File.Exists(fileName))//判断文件是否存，如果不存在则创建文件 
                //{
                //    FileStream fs = new FileStream(fileName, FileMode.Append); //创建文件  
                //    File.Delete(fileName);// 删除文件
                //}
                //1.创建文件流
                FileStream fs = new FileStream(fileName, FileMode.Append); //创建文件                                                                    
                StreamWriter sw = new StreamWriter(fs); //2.创建写入器               
                sw.WriteLine(Log);//3.以流的方式写入              
                sw.Close(); //4.关闭写入器              
                fs.Close(); //5.关闭文件流
            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message, "Message");
            }
        } //写日志
        #endregion

        #region 变量


        #endregion
    }
    /// <summary>Leak 参数</summary>
    public partial class SetData
    {
        /// <summary>设备的通信方式</summary>
        public static Comm_Mode Comm_MODE { get; set; }
        /// <summary>设备的工作模式</summary>
        public static Work_Mode Work_MODE { get; set; } //设备工作模式

        /// <summary>设置参数->罐充气上限</summary>
        public static float Tank_Press_Hi { get; set; }
        /// <summary>设置参数->罐充气下限</summary>
        public static float Tank_Press_Lo { get; set; }
        /// <summary>设置参数->大泄漏压差</summary>
        public static float Press_Differe { get; set; }
        /// <summary>设置参数->充气上限</summary>
        public static float Fill_Press_Hi { get; set; }
        /// <summary>设置参数->充气下限</summary>
        public static float Fill_Press_Lo { get; set; }
        /// <summary>设置参数->泄漏值上限</summary>
        public static float Upper_Leak_Limit { get; set; }
        /// <summary>设置参数->泄漏值下限
        /// </summary>
        public static float Lower_Leak_Limit { get; set; }
        /// <summary>设置参数->JIG下压时间</summary>
        public static float Press_Down_Time { get; set; }
        /// <summary>设置参数->罐稳定时间</summary>
        public static float Tank_Settling_Time { get; set; }
        /// <summary>设置参数->充气时间</summary>
        public static float Fill_Time { get; set; }
        /// <summary>设置参数->稳定时间</summary>
        public static float Settling_Time { get; set; }
        /// <summary>设置参数->检测时间</summary>
        public static float Test_Time { get; set; }
        /// <summary>设置参数->排气时间</summary>
        public static float Exit_Time { get; set; }
        /// <summary>设置参数->容积</summary>
        public static float Volume { get; set; }
        /// <summary>设备的充气值单位</summary>
        public static string Fill_Unit { get; set; }
        /// <summary>设备的泄漏值单位</summary>
        public static string Leak_Unit { get; set; }
    }

    /// <summary>Leak 测试数据</summary>
    public partial class TestData
    {
        /// <summary>JIG下压时间</summary>
        public static float JIG_Down_Time { get; set; }

        /// <summary>罐稳定时间</summary>
        public static float Tank_Settling_Time { get; set; }

        /// <summary>充气时间</summary>
        public static float Fill_Time { get; set; }

        /// <summary>稳定时间</summary>
        public static float Settling_Time { get; set; }

        /// <summary>检测时间</summary>
        public static float Test_Time { get; set; }

        /// <summary>排气时间</summary>
        public static float Exit_Time { get; set; }

        /// <summary>罐压力值</summary>
        public static float Tank_Press { get; set; }

        /// <summary>罐剩余压力 </summary>
        public static float Tank_Residual_Press { get; set; }

        /// <summary>充气压力</summary>
        public static float Fill_Press { get; set; }

        /// <summary>泄漏值 Pa</summary>
        public static float Leak_Pa { get; set; }

        /// <summary>泄漏值 CCM</summary>
        public static float Leak_CCM { get; set; }

        /// <summary>过程时间</summary>
        public static float Process_Time { get; set; }

        /// <summary>测试结果，1=OK,2=+NG,3=-NG,4=大泄漏，5= 罐充压过高，6=罐充压过低，7=充压过高，8=充压过低，9=超量程,10=Vent Cap NG</summary>
        public static ushort Result { get; set; }

        /// <summary>测试结果文本</summary>
        public static string Result_Txt { get; set; }

    }
    
    /// <summary>Leak 设备信息</summary>
    public partial class MC_inf
    {
        /// <summary>设备版本号</summary>
        public static string PLC_Ver { get; set; }
        /// <summary>设备名称</summary>
        public static string MC_Name { get; set; }

        /// <summary>设备编号</summary>
        public static string MC_Num { get; set; }

        /// <summary>设备拉线号</summary>
        public static string MC_Line { get; set; }

        /// <summary>设备状态,(0=没回原点，1=工作中，2=待机中，3=故障，4=紧急停止)</summary>
        public static string MC_State { get; set; }
    }
    /// <summary>产品记录信息</summary>
    public partial class REC_inf
    {
        /// <summary>测试机种</summary>
        public static string Model { get; set; }

        /// <summary>测试条码</summary>
        public static string SN { get; set; }

        /// <summary>测试者ID</summary>
        public static string User_ID { get; set; }

        /// <summary>测试者名字</summary>
        public static string User_Name { get; set; }

        /// <summary>测试拉线号</summary>
        public static string Line_Num { get; set; }

        /// <summary>测试工序号</summary>
        public static string OPER { get; set; }

        /// <summary>测试时日期</summary>
        public static string Test_Date { get; set; }

        /// <summary>测试时时间</summary>
        public static string Test_HHMM { get; set; }
     
    }

    /// <summary>TCP 连接信息</summary>
    public partial class TCP_inf////连接信息
    {
        /// <summary>从机ID </summary>
        public static byte Slave_ID { get; set; }
        /// <summary>TCP连接的 IP</summary>
        public static string IP { get; set; }

        /// <summary>TCP连接的 Port</summary>
        public static int Port { get; set; }
    }

    /// <summary>串口 连接信息</summary>
    public partial class COMx_inf
    {
        //COMx,9600,8,n,1

        /// <summary>从机ID </summary>
        public static byte Slave_ID { get; set; }
        /// <summary>串口号</summary>
        public static string COMx { get; set; } //串口号
        /// <summary>波特率</summary>
        public static string BaudRate { get; set; } //波特率
        /// <summary>数据位</summary>
        public static string DataBits { get; set; } //数据位
        /// <summary>校验位</summary>
        public static string Parity { get; set; } //校验位
        /// <summary>停止位</summary>
        public static string StopBits { get; set; } //停止位
    }

    /// <summary>设备工作模式,常规/全封闭/防水贴 </summary>
    public enum Work_Mode //设备工作模式
    {
        /// <summary>设备工作模式 = 常规</summary>
        Normal = 1, //常规
        /// <summary>设备工作模式 = 全封闭</summary>
        Fully_Sealed = 2, //全封闭 Fully_Sealed
        /// <summary>设备工作模式 = 防水贴</summary>
        Vent_Cap = 4, //防水贴 Vent_Cap
    }
    /// <summary>设备通信方式,ACE_TCP,lnterTech_TCP,ACE_COM,lnterTech_COMW,aYeal_COM</summary>
    public enum Comm_Mode //通信方式
    {
        /// <summary>ACE Modbus TCP</summary>
        ACE_TCP = 0,
        /// <summary>lnterTech TCP</summary>
        lnterTech_TCP = 1,
        /// <summary>ACE Modbus COM</summary>
        ACE_COM = 2,
        /// <summary>lnterTech COM</summary>
        lnterTech_COM = 3,
        /// <summary>WaYeal COM</summary>
        WaYeal_COM = 4
    }

}
