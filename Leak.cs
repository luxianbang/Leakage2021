using Leakage_Lib;
using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows;

namespace Leakage2021
{
    public partial class SYS_Set
    {
        /// <summary>自动扫描条码</summary>
        public static bool AutoScan { get; set; }
        /// <summary>扫描回车启动</summary>
        public static bool EnterStart { get; set; }
        /// <summary>系统设置密码</summary>
        public static string Set_Passwd { get; set; }
        /// <summary>显示语言，"CN","EN"</summary>
        public static string LANG { get; set; }

        /// <summary>工厂代码</summary>
        public static string Factory_Code { get; set; }

        /// <summary>条码枪接口方式</summary>
        public static string Barcode_Mode { get; set; }
    

    }

    class Leak : MainWindow
    {
        // public static Modbus_Conn_inf Conn_inf = new Modbus_Conn_inf();

        //public string LOT_NUMBER
        //{
        //    set { this.Display_box.Text = value; }
        //    get { return this.Display_box.Text; }
        //}

        //LW0 10000   9999
        //RWI 20000   45536


        public static bool ConnectOk = false;//连接状态

        public static bool Modbus_Conn()
        {
            try
            {
                if (ConnectOk)
                {
                    ModbusTcpM.Disconnect();//断开连接
                    Console.Beep(850, 500);
                    ConnectOk = false;
                }
                else
                {
                    ConnectOk = ModbusTcpM.Connect();//连接  
                    if (ConnectOk)
                    {
                        ModbusTcpM.R_Coils(Modbus_ID, 0, 1);
                        Write_conn_inf();//写连接配置信息
                        MC_Name_Num();
                        ConnectOk = false;

                        验证码窗口 WIN = new 验证码窗口();//验证码窗口
                        WIN.ShowDialog();//打开窗口 
                        ConnectOk = (bool)WIN.DialogResult;
                        ModbusTcpM.W_SingleCoil(Modbus_ID, 3, false);//M3
                        //Console.WriteLine(ConnectOk);
                        if (ConnectOk)
                        {
                            Console.Beep();
                        }
                        else
                        {
                            ModbusTcpM.Disconnect();
                            //Console.Beep(850, 500); //断开连接
                        }
                    }
                }

            }
            catch (Exception ex)//MessageBox.Show(ex.Message, "Message");
            {
                ConnectOk = false;
                Console.Beep(850, 500);
                MessageBox.Show(ex.Message);
                //var Conn_SET = new Conn_SET();
                //Conn_SET.ShowDialog();
            }
            return ConnectOk;
        }

        public static void 断开所有连接()
        {
            ModbusTcpM.Disconnect();//断开连接
        }


        public static void MC_Name_Num() //读设备名称OR编号
        {
            try
            {
                if (ConnectOk)
                {
                    ushort[] data = ModbusTcpM.R_Registers(Modbus_ID, 1000, 25);

                    //D1000 - D1001 = 版本号，D1002 - D1011 = 设备名，D1012 - D1021 = 设备编号 , D1022 - D1024 = 拉线号
                    byte[] BYTE = new byte[data.Length * 2 - 4];
                    Buffer.BlockCopy(data, 4, BYTE, 0, data.Length * 2 - 4);

                    MC_inf.PLC_Ver = data[1].ToString("X4") + data[0].ToString("X4");
                    MC_inf.MC_Name = Encoding.UTF8.GetString(BYTE.Skip(0).Take(20).ToArray()).Trim().Replace("\0", "");
                    MC_inf.MC_Num = Encoding.UTF8.GetString(BYTE.Skip(20).Take(20).ToArray()).Trim().Replace("\0", "");
                    MC_inf.MC_Line = Encoding.UTF8.GetString(BYTE.Skip(40).Take(6).ToArray()).Trim().Replace("\0", "");
                }
                else
                    throw new Exception("设备没连接!");

            }
            catch (Exception ex)
            {
                ConnectOk = false;
                throw ex; // MessageBox.Show(ex.Message);
                //throw;
            }
        }

        public static void ModbusTcp_Read_Set_Para()// 读设置参数
        {
            try
            {
                if (ConnectOk)
                {
                    SetData.Comm_MODE = Comm_Mode.ACE_TCP;

                    Leak.MC_Name_Num();// 读设备名称OR编号
                    SetData.Work_MODE = (Work_Mode)ModbusTcpM.R_Registers(Modbus_ID, 1100, 1)[0];//读取气密机工作方式 bit0, bit1, bit2

                    ushort[] data = ModbusTcpM.R_Registers(Modbus_ID, 2000, 30);//D2000
                    byte[] BYTE = new byte[10];
                    Buffer.BlockCopy(data, 0, BYTE, 0, 10);

                    if (DB.Local_login)
                        REC_inf.Model = Encoding.UTF8.GetString(BYTE.Skip(0).Take(20).ToArray()).Trim().Replace("\0", "");

                    SetData.Press_Down_Time = (float)data[05] / 10;//JIG下压时间
                    SetData.Tank_Settling_Time = (float)data[06] / 10;//罐稳定时间
                    SetData.Fill_Time = (float)data[07] / 10;//充气时间
                    SetData.Settling_Time = (float)data[08] / 10;//稳定时间
                    SetData.Test_Time = (float)data[09] / 10;//检测时间
                    SetData.Exit_Time = (float)data[10] / 10;//排气时间

                    SetData.Tank_Press_Hi = UshortToFloat(data[14], data[15]);//罐充气上限

                    SetData.Tank_Press_Lo = UshortToFloat(data[16], data[17]);//罐充气下限

                    SetData.Press_Differe = UshortToFloat(data[18], data[19]);//大泄漏压差

                    SetData.Fill_Press_Hi = UshortToFloat(data[20], data[21]);//充气上限

                    SetData.Fill_Press_Lo = UshortToFloat(data[22], data[23]);//充气下限

                    SetData.Upper_Leak_Limit = UshortToFloat(data[24], data[25]);//泄漏值上限

                    SetData.Lower_Leak_Limit = UshortToFloat(data[26], data[27]);//泄漏值下限

                    SetData.Volume = UshortToFloat(data[28], data[29]);//容积

                    SetData.Fill_Unit = "Kpa";
                    if (data[13] == 0)
                        SetData.Leak_Unit = "Pa";
                    else
                        SetData.Leak_Unit = "CCM";

                    TestData.Leak_Pa = TestData.Leak_CCM = TestData.Fill_Time = 0;
                    TestData.Test_Time = TestData.Fill_Press = TestData.Result = 0;
                }
                else
                    MessageBox.Show("设备没连接!");
            }
            catch (Exception ex)
            {
                ConnectOk = false;
                throw ex; //MessageBox.Show(ex.Message);
            }

        }

        /// <summary>读测试数据t</summary>
        public static void ModbusTcp_R_TestData() //读测试数据
        {
            try
            {
                if (ConnectOk)
                {
                    ushort[] data = ModbusTcpM.R_Registers(Modbus_ID, 1, 20);//读测试数据

                    MC_STATE = data[0]; //设备启动结果
                    TestData.JIG_Down_Time = (float)data[1] / 10; //JIG下时间
                    TestData.Tank_Settling_Time = (float)data[2] / 10;

                    if (TestData.Tank_Settling_Time + (float)data[3] / 10 > 0)
                    {
                        TestData.Fill_Time = (float)data[3] / 10; //充气时间
                        TestData.Settling_Time = (float)data[4] / 10;//稳定时间
                        TestData.Test_Time = (float)data[5] / 10; //检测时间
                        TestData.Exit_Time = (float)data[6] / 10; //排气时间

                        TestData.Tank_Press = UshortToFloat(data[08], data[09]);//罐压力值
                        TestData.Tank_Residual_Press = UshortToFloat(data[10], data[11]);//罐剩余压力
                        TestData.Fill_Press = UshortToFloat(data[12], data[13]);//充气压力 
                        TestData.Leak_Pa = UshortToFloat(data[14], data[15]);   //泄漏压差
                        TestData.Leak_CCM = UshortToFloat(data[16], data[17]);  //泄漏 CCM
                    }
                    TestData.Result = data[18];
                }
                else
                    throw new Exception("设备没连接!");
            }
            catch (Exception ex)
            {
                ConnectOk = false;
                throw ex; //MessageBox.Show(ex.Message);
            }
        }

        public static bool SN_CMD()
        {
            try
            {
                if (ConnectOk)
                {
                    ModbusTcpM.W_SingleCoil(Modbus_ID, 2, true);//M2
                    return true;
                }
                else
                    throw new Exception("START 设备没连接!");

            }
            catch (Exception ex)
            {
                ConnectOk = false;
                throw ex; //MessageBox.Show(ex.Message);
            }
        }
        public static bool START_CMD()
        {
            try
            {
                if (ConnectOk)
                {
                    ModbusTcpM.W_SingleCoil(Modbus_ID, 0, true);//M0 停止
                    ModbusTcpM.W_SingleCoil(Modbus_ID, 1, true);//M1 启动
                    //System.Threading.Thread.Sleep(200);//延时
                    return true;
                }
                else
                    throw new Exception("START 设备没连接!");

            }
            catch (Exception ex)
            {
                ConnectOk = false;
                throw ex; //MessageBox.Show(ex.Message);
            }
        }
        public static bool STOP_CMD()
        {
            try
            {
                if (ConnectOk)
                {
                    ModbusTcpM.W_SingleCoil(Modbus_ID, 0, true);//M0 停止
                    return true;
                }
                else
                    throw new Exception("STOP 设备没连接!");

            }
            catch (Exception ex)
            {
                ConnectOk = false;
                throw ex; //MessageBox.Show(ex.Message);
            }
        }


        /// <summary>ushort array to Float</summary>
        private static float UshortToFloat(ushort Data1, ushort Data2)
        {
            ushort[] Udata = new ushort[2] { Data1, Data2 };
            float[] floatData = new float[Udata.Length / 2];
            Buffer.BlockCopy(Udata, 0, floatData, 0, Udata.Length * 2);
            return floatData[0];
        }

        public static void Read_conn_inf() //读连接配置信息 
        {
            string[] inf = ((string)DB.Get_Reg("TCP_Conn_inf", "1,192.168.19.100,8000")).Split(',');//id,ip,port

            if (inf[0] == "") { inf[0] = "1"; }
            TCP_inf.Slave_ID = byte.Parse(inf[0]);
            TCP_inf.IP = inf[1];
            if (inf[2] == "") { inf[2] = "8000"; }
            TCP_inf.Port = int.Parse(inf[2]);

            inf = ((string)DB.Get_Reg("RTU_Conn_inf", "COM1,9600,8,N,1")).Split(',');//COMx,9600,8,n,1
            COMx_inf.COMx = inf[0];
            COMx_inf.BaudRate = inf[1];
            COMx_inf.DataBits = inf[2];
            COMx_inf.Parity = inf[3];
            COMx_inf.StopBits = inf[4];

        }
        public static void Write_conn_inf() //写连接配置信息 
        {
            string inf = COMx_inf.Slave_ID.ToString() + "," + TCP_inf.IP + "," + TCP_inf.Port.ToString();
            DB.Set_Reg("TCP_Conn_inf", inf);//id,ip,port
            inf = COMx_inf.COMx + "," + COMx_inf.BaudRate + "," + COMx_inf.DataBits + ","
                                                       + COMx_inf.Parity + "," + COMx_inf.StopBits;
            DB.Set_Reg("RTU_Conn_inf", inf);//COMx,9600,8,n,1
        }

        #region 获取硬件ID

        /// <summary>
        /// 获取CPU ID
        /// </summary>
        /// <returns>CPU ID</returns>
        public static string GetCPU_ID()
        {
            try
            {
                string str = string.Empty;
                System.Management.ManagementClass mcCpu = new System.Management.ManagementClass("win32_Processor");
                System.Management.ManagementObjectCollection mocCpu = mcCpu.GetInstances();
                foreach (System.Management.ManagementObject m in mocCpu)
                {
                    str = m["Processorid"].ToString().Trim().Substring(0, 8);//
                }
                Console.WriteLine("CPU ID:" + str);
                //return str;

                System.Management.ManagementClass mc = new System.Management.ManagementClass("Win32_Processor");
                System.Management.ManagementObjectCollection moc = mc.GetInstances();
                string strID = null;
                foreach (System.Management.ManagementObject mo in moc)
                {
                    strID = mo.Properties["ProcessorId"].Value.ToString();
                    break;
                }
                Console.WriteLine("CPU ID:" + strID);

                return strID;


            }
            catch (Exception ex)
            { }

            return "";
        }

        /// <summary>
        /// 获取主板ID
        /// </summary>
        /// <returns>主板ID</returns>
        public static string GetMainboard_ID()
        {
            try
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_baseboard");
                string serNumber = string.Empty;
                string manufacturer = string.Empty;
                string product = string.Empty;

                foreach (ManagementObject m in mos.Get())
                {
                    serNumber = m["SerialNumber"].ToString();//序列号
                    manufacturer = m["Manufacturer"].ToString();//制造商
                    product = m["Product"].ToString();//型号
                }
                return serNumber + " " + manufacturer + " " + product;
            }
            catch
            {
                return "likeshan";
            }

            //System.Management.ManagementClass mc = new System.Management.ManagementClass("Win32_BaseBoard");
            //System.Management.ManagementObjectCollection moc = mc.GetInstances();
            //string strID = null;
            //foreach (System.Management.ManagementObject mo in moc)
            //{
            //    strID = mo.Properties["SerialNumber"].Value.ToString();
            //    break;
            //}
            //Console.WriteLine("主板 ID:" + strID);

            //return strID;
        }

        /// <summary>
        /// 获取硬盘ID
        /// </summary>
        /// <returns>主板ID</returns>
        public static string 获取硬盘ID()
        {
            try
            {
                string hdId = string.Empty;
                ManagementClass hardDisk = new ManagementClass("win32_DiskDrive");
                ManagementObjectCollection hardDiskC = hardDisk.GetInstances();
                foreach (ManagementObject m in hardDiskC)
                {
                    //hdId = m["Model"].ToString().Trim();
                    hdId = m.Properties["Model"].Value.ToString();//
                }
                return hdId;
            }
            catch
            { return "likeshan"; }

        }
        /// <summary>
        /// 获取网卡地址
        /// </summary>
        /// <returns></returns>
        public string GetNetwordAdapter()
        {
            try
            {
                string MoAddress = string.Empty;
                ManagementClass networkAdapter = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection adapterC = networkAdapter.GetInstances();
                foreach (ManagementObject m in adapterC)
                {
                    if ((bool)m["IPEnabled"] == true)
                    {
                        MoAddress = m["MacAddress"].ToString().Trim();
                        m.Dispose();
                    }
                }
                return MoAddress;
            }
            catch
            {
                return "likeshan";
            }
        }
        /// <summary>
        /// 加密算法（利用到了cpuid）
        /// </summary>
        /// <param name="data">要加密的字符串</param>
        /// <returns></returns>
        public string Encode(string data)
        {
            byte[] akey = ASCIIEncoding.ASCII.GetBytes(GetCPU_ID());
            byte[] aIV = ASCIIEncoding.ASCII.GetBytes(GetCPU_ID());
            using (System.Security.Cryptography.DESCryptoServiceProvider CP = new System.Security.Cryptography.DESCryptoServiceProvider())
            {
                MemoryStream ms = new MemoryStream();
                System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, CP.CreateEncryptor(akey, aIV), System.Security.Cryptography.CryptoStreamMode.Write);
                StreamWriter sw = new StreamWriter(cs);
                sw.Write(data);
                sw.Flush();
                cs.FlushFinalBlock();
                sw.Flush();
                return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }
        /// <summary>
        /// 加密算法（利用cpuid）
        /// </summary>
        /// <param name="data">需要解密的字符串</param>
        /// <returns></returns>
        public string Decode(string data)
        {
            byte[] akey = ASCIIEncoding.ASCII.GetBytes(GetCPU_ID());
            byte[] aIV = ASCIIEncoding.ASCII.GetBytes(GetCPU_ID());
            byte[] Enc = null;
            try
            {
                Enc = Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }

            System.Security.Cryptography.DESCryptoServiceProvider cp = new System.Security.Cryptography.DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(Enc);
            System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, cp.CreateDecryptor(akey, aIV), System.Security.Cryptography.CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }
        #endregion
    }



}
