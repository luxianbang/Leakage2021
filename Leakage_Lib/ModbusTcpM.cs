using System;
using System.Net.Sockets;
using Modbus.Device;//modbus master


namespace Leakage_Lib
{
    /// <summary>Modbus_Master_TCP</summary>
    public class ModbusTcpM
    {
        //私有的
        private static TcpClient tcpClient;   //TCP连接
        private static ModbusIpMaster master; //TCP主机


        //private static Timer LinkTimer;
        //LinkTimer = new Timer(2000);//5S                  
        //LinkTimer.Elapsed += LinkTimer_Elapsed; //时间到达事件
        //LinkTimer.AutoReset = true;//一次事件=假，重复=真
        //LinkTimer.Start();//启动定时器 
        //LinkTimer_Elapsed(null, null);
        //private static int Count = 0;


        /// <summary>是否已连接,获取一个值，该值指示 System.Net.Sockets.TcpClient 的基础 System.Net.Sockets.Socket 是否已连接到远程主机</summary>
        public static bool IsConnect()//连接
        {
            return tcpClient.Connected;
        }

        /// <summary>连接</summary>
        public static bool Connect()//连接
        {
            bool ConnectOk = false;
            try
            {
                if (master != null)
                    master.Dispose();
                if (tcpClient != null)
                    tcpClient.Close();
                tcpClient = new TcpClient();

                IAsyncResult asyncResult = tcpClient.BeginConnect(TCP_inf.IP, TCP_inf.Port, null, null);//请求异步连接结果
                asyncResult.AsyncWaitHandle.WaitOne(2000, true); //等待2秒

                if (!asyncResult.IsCompleted) //检查是否连接
                {
                    tcpClient.Close();
                    throw new Exception("Connection failed!");
                }
                master = ModbusIpMaster.CreateIp(tcpClient);
                master.Transport.Retries = 0;   //不需要重试
                master.Transport.WaitToRetryMilliseconds = 500;//等待重试毫秒
                master.Transport.ReadTimeout = 1000;//传输.读取超时毫秒
                master.Transport.WriteTimeout = 1000;//传输.写入超时毫秒
               
                //ushort[] jj = master.ReadHoldingRegisters(0, 1);
                ConnectOk = true;
            }
            catch (Exception ex) { throw ex;/*返回错误*/}
            return ConnectOk;
        }

        /// <summary>断开连接</summary>
        public static void Disconnect()//断开连接
        {
            try
            {
                if (master != null)
                    master.Dispose();
                if (tcpClient != null)
                    tcpClient.Close();
            }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }


        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        #region TCP_IP读写函数
        /// <summary>读线圈,功能码 01H</summary>
        /// <param name="slaveAddress">从机设备ID</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取的点数</param>
        /// <returns>bool[]数组</returns>/
        public static bool[] R_Coils(byte slaveAddress, ushort startAddress, ushort numberOfPoints)//读线圈
        {
            try { return master.ReadCoils(slaveAddress, startAddress, numberOfPoints); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }
        /// <summary>读线圈,功能码 01H</summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取的点数</param>
        /// <returns>bool[]数组</returns>/
        public static bool[] R_Coils(ushort startAddress, ushort numberOfPoints)//读线圈
        {
            try { return master.ReadCoils(startAddress, numberOfPoints); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }


        /// <summary>读保持寄存器,功能码 03H</summary>
        /// <param name="slaveAddress">从机设备ID</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取的点数</param>
        /// <returns>ushort[]数组</returns>/ 
        public static ushort[] R_Registers(byte slaveAddress, ushort startAddress, ushort numberOfPoints)//读寄存器
        {
            try { return master.ReadHoldingRegisters(slaveAddress, startAddress, numberOfPoints); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }
        /// <summary>读保持寄存器,功能码 03H</summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取的点数</param>
        /// <returns>ushort[]数组</returns>/ 
        public static ushort[] R_Registers(ushort startAddress, ushort numberOfPoints)//读寄存器
        {
            try { return master.ReadHoldingRegisters(startAddress, numberOfPoints); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }


        /// <summary>读输入寄存器,功能码 04H</summary>
        /// <param name="slaveAddress">从机设备ID</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取的点数</param>
        /// <returns>ushort[]数组</returns>/ 
        public static ushort[] R_InputRegisters(byte slaveAddress, ushort startAddress, ushort numberOfPoints)//读输入寄存器
        {
            try { return master.ReadInputRegisters(slaveAddress, startAddress, numberOfPoints); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }
        /// <summary>读输入寄存器,功能码 04H</summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取的点数</param>
        /// <returns>ushort[]数组</returns>/ 
        public static ushort[] R_InputRegisters(ushort startAddress, ushort numberOfPoints)//读输入寄存器
        {
            try { return master.ReadInputRegisters(startAddress, numberOfPoints); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }

 
        /// <summary>读输入,功能码 02H</summary>
        /// <param name="slaveAddress">从机设备ID</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取的点数</param>
        /// <returns>bool[]数组</returns>/ 
        public static bool[] R_Input(byte slaveAddress, ushort startAddress, ushort numberOfPoints)//读输入
        {
            try { return master.ReadInputs(slaveAddress, startAddress, numberOfPoints); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }
        /// <summary>读输入,功能码 02H</summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取的点数</param>
        /// <returns>bool[]数组</returns>/ 
        public static bool[] R_Input(ushort startAddress, ushort numberOfPoints)//读输入
        {
            try { return master.ReadInputs(startAddress, numberOfPoints); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }


        /// <summary>读写多寄存器,读功能码 03H,读功能码10H</summary>
        /// <param name="slaveAddress">从机设备ID</param>
        /// <param name="startReadAddress">读寄存器开始地址</param>
        /// <param name="numberOfPointsToRead">读寄存器点数</param>
        /// <param name="startWriteAddress">写寄存器开始地址</param>
        /// <param name="writeData">写入数据ushort数组</param>
        /// <returns>ushort[]数组</returns>/ 
        public static ushort[] R_W_MultipleRegisters(byte slaveAddress, ushort startReadAddress,
            ushort numberOfPointsToRead, ushort startWriteAddress, ushort[] writeData)//读写多寄存器
        {
            try { return master.ReadWriteMultipleRegisters(slaveAddress, startReadAddress, numberOfPointsToRead, startWriteAddress, writeData); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }
        /// <summary>读写多寄存器,读功能码 03H,读功能码10H</summary>
        /// <param name="startReadAddress">读寄存器开始地址</param>
        /// <param name="numberOfPointsToRead">读寄存器点数</param>
        /// <param name="startWriteAddress">写寄存器开始地址</param>
        /// <param name="writeData">写入数据ushort数组</param>
        /// <returns>ushort[]数组</returns>/ 
        public static ushort[] R_W_MultipleRegisters(ushort startReadAddress,
            ushort numberOfPointsToRead, ushort startWriteAddress, ushort[] writeData)//读写多寄存器
        {
            try { return master.ReadWriteMultipleRegisters(startReadAddress, numberOfPointsToRead, startWriteAddress, writeData); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }


        /// <summary>写多个线圈,功能码 0FH</summary>
        /// <param name="slaveAddress">从机设备ID</param>
        /// <param name="startAddress">线圈开始地址</param>
        /// <param name="data">bool[]数组</param>
        /// <returns></returns>/ 
        public static void W_MultipleCoils(byte slaveAddress, ushort startAddress, bool[] data)//写多个线圈
        {
            try { master.WriteMultipleCoils(slaveAddress, startAddress, data); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }

        /// <summary>写多个线圈,功能码 0FH</summary>
        /// <param name="startAddress">线圈开始地址</param>
        /// <param name="data">bool[]数组</param>
        /// <returns></returns>/ 
        public static void W_MultipleCoils(ushort startAddress, bool[] data)//写多个线圈
        {
            try { master.WriteMultipleCoils(startAddress, data); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }


        /// <summary>写多个寄存器,功能码 10H</summary>
        /// <param name="slaveAddress">从机设备ID</param>
        /// <param name="startAddress">寄存器开始地址</param>
        /// <param name="data">ushort[]数组</param>
        /// <returns></returns>/ 
        public static void W_MultipleRegisters(byte slaveAddress, ushort startAddress, ushort[] data)//写多个寄存器
        {
            try { master.WriteMultipleRegisters(slaveAddress, startAddress, data); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }
        /// <summary>写多个寄存器,功能码 10H</summary>
        /// <param name="startAddress">寄存器开始地址</param>
        /// <param name="data">ushort[]数组</param>
        /// <returns></returns>/ 
        public static void W_MultipleRegisters(ushort startAddress, ushort[] data)//写多个寄存器
        {
            try{master.WriteMultipleRegisters(startAddress, data);}
            catch (Exception ex) { throw ex;/*返回错误*/}
        }


        /// <summary>写单个线圈,功能码 05H</summary>
        /// <param name="slaveAddress">从机设备ID</param>
        /// <param name="coilAddress">线圈地址</param>
        /// <param name="value">bool值</param>
        /// <returns></returns>/ 
        public static void W_SingleCoil(byte slaveAddress, ushort coilAddress, bool value)
        {
            try{master.WriteSingleCoil(slaveAddress, coilAddress, value);}
            catch (Exception ex) { throw ex;/*返回错误*/}
        }
        /// <summary>写单个线圈,功能码 05H</summary>
        /// <param name="coilAddress">线圈地址</param>
        /// <param name="value">bool值</param>
        /// <returns></returns>/ 
        public static void W_SingleCoil(ushort coilAddress, bool value)
        {
            try{master.WriteSingleCoil(coilAddress, value); }
            catch (Exception ex) { throw ex;/*返回错误*/}
        }


        /// <summary>写单个寄存器,功能码 06H</summary>
        /// <param name="slaveAddress">从机设备ID</param>
        /// <param name="registerAddress">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <returns></returns>/ 
        public static void WriteSingleRegister(byte slaveAddress, ushort registerAddress, ushort value)
        {
            try
            {
                master.WriteSingleRegister(slaveAddress, registerAddress, value);
            }
            catch (Exception ex)
            {
                throw ex;//返回错误
            }
        }
        /// <summary>写单个寄存器,功能码 06H</summary>
        /// <param name="registerAddress">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <returns></returns>/ 
        public static void WriteSingleRegister(ushort registerAddress, ushort value)
        {
            try{master.WriteSingleRegister(registerAddress, value);}
            catch (Exception ex){throw ex;/*返回错误*/}
        }
        #endregion
        //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    }


}



