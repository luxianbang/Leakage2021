using ADOX;
using System;
using System.IO;
using Leakage_Lib;
using System.Data;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.Data.OleDb;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Windows.Data;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;//Oracle数据库 DLL






namespace Leakage2021
{
    class DB : MainWindow  //protected  受保护的   public 公开的   static 静态的 private 私有的  ref out
    {
        #region 用户变量
        public static string ServerIP = "192.168.11.20";
        public static string USER = "Aerp";
        public static string PASS = "Aerp2014";

        //public static string Dace_Conn = "Data Source = (DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.11.21)" +
        //   "(PORT = 2009)))(CONNECT_DATA =(SID = ora10g)(SERVER = DEDICATED)));User Id = Aerp; Password=Aerp2014;";

        //public static string Vace_Conn = "Data Source = (DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.105.20)" +
        //    "(PORT = 2016)))(CONNECT_DATA =(SID = ora10g)(SERVER = DEDICATED)));User Id = ASIS; Password=ASIS;";

        #endregion

        /// <summary>工厂名</summary>
        public static string Factory_Name = "DACE"; //VACE
        /// <summary>本地登陆</summary>
        public static bool Local_login = false; //


        public static string ACE_Conn;
        private static string Access_Conn;
        public static void DB_INT(string Factory)
        {
          
            if (Factory == "DACE")
                ACE_Conn = "Data Source = (DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST =" + ServerIP + ")(PORT =2009) ))"
                            + "(CONNECT_DATA =(SID = ora10g)(SERVER = DEDICATED)));User Id =" + USER + "; Password=" + PASS + ";";
            else
                ACE_Conn = "Data Source = (DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST =" + ServerIP + ")(PORT =2016"
                          + ")))(CONNECT_DATA =(SID = ora10g)(SERVER = DEDICATED)));User Id =" + "AMES" + "; Password=" + "AMES" + ";";
        }

        #region 检查Access数据库
        /// <summary>检查Access数据库是否存在，没有新建,Pwd=null不加密</summary>
        /// <param name="fileName">文件名,如 "Data\\LeakageDB.mdb"</param>
        /// <param name="Pwd">密码,null为不加密</param>
        /// <returns></returns>
        public static void Check_Access(string fileName, string Pwd)
        {
            try
            {
                string conn;
                if (Pwd == null)
                {
                    conn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName;//创建无密码数据库连接 ADODB
                    //Access_Conn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Persist Security Info=False";//无密码  OleDb
                }
                else
                {    //Jet OLEDB:Engine Type(DBPROP_JETOLEDB_ENGINE) 指示用于访问当前数据存储的存储引擎
                     //Jet OLEDB:Database Password(DBPROP_JETOLEDB_DATABASEPASSWORD)指示数据库密码
                    conn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Jet OLEDB:Database Password=" + Pwd +
                               ";Jet OLEDB:Engine Type=5";//创建有密码数据库连接 ADODB
                    //Access_Conn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Jet OLEDB:Database Password=" + Pwd;//有密码  OleDb
                }
                Access_Conn = conn;
                //conn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName;//无密码
                //conn =        "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Jet OLEDB:Database Password=" + Pwd + ";Jet OLEDB:Engine Type=5";//有密码

                //判断文件夹是否存在、不在创建
                //string path = AppDomain.CurrentDomain.BaseDirectory + "Data";//F:\\notebook\\haha\\";//路径的正确写法
                string Folder = fileName.Substring(0, fileName.LastIndexOf('\\'));
                if (!Directory.Exists(Folder))//如果不存在就创建文件夹
                    Directory.CreateDirectory(Folder);  //创建文件夹

                if (File.Exists(fileName)) //文件是否存在?
                {
                    return;
                    //File.Delete(fileName); //删除原文件
                }

                //创建数据库
                Catalog catalog = new Catalog();
                catalog.Create(conn); //创建mdb文件
                //catalog.Create("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Jet OLEDB:Engine Type=5"); //创建mdb文件

                //连接数据库
                ADODB.Connection Conn = new ADODB.Connection();
                Conn.Open(conn, null, null, -1);
                //Conn.Open("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName, null, null, -1);
                catalog.ActiveConnection = Conn;

                //新建表
                ADOX.Table table = new ADOX.Table();
                table.Name = "FACRST";

                Column column = new Column();
                column.ParentCatalog = catalog;
                column.Type = DataTypeEnum.adInteger;
                column.Name = "ID";
                column.DefinedSize = 9;
                column.Properties["AutoIncrement"].Value = true;
                table.Columns.Append(column, DataTypeEnum.adInteger, 9);//数字
                //设置主键
                //table.Keys.Append("PrimaryKey",           KeyTypeEnum.adKeyPrimary,   "ID",   "",   "");
                table.Keys.Append("FirstTablePrimaryKey", KeyTypeEnum.adKeyPrimary, column, null, null);

                column = new Column();
                column.Name = "MODEL";//列名
                column.DefinedSize = 20;//字段长度
                column.Attributes = ColumnAttributesEnum.adColNullable;//允许空值
                table.Columns.Append(column, DataTypeEnum.adVarWChar, 20);//文本

                table.Columns.Append("SN", DataTypeEnum.adVarWChar, 20);
                table.Columns.Append("USER", DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("LINE", DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("OPER", DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("DATE", DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("TIME", DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("InAirTime", DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("CheckTime", DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("InAir", DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("Leak", DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("CCM", DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("Result", DataTypeEnum.adVarWChar, 10);
                catalog.Tables.Append(table);//添加表 FACRST

                //********************************************************************************
                //********************************************************************************
                table = new ADOX.Table();
                table.ParentCatalog = catalog;
                table.Name = "USERINFO";

                column = new Column();
                column.ParentCatalog = catalog;
                column.Name = "ID";
                column.Type = DataTypeEnum.adInteger;
                column.DefinedSize = 9;
                column.Properties["AutoIncrement"].Value = true;
                table.Columns.Append(column, DataTypeEnum.adInteger, 9);//数字
                table.Keys.Append("FirstTablePrimaryKey", KeyTypeEnum.adKeyPrimary, column, null, null);

                //table.Columns.Append("ID", DataTypeEnum.adInteger, 9);//数字
                table.Columns.Append("USERID", DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("PASSWD", DataTypeEnum.adVarWChar, 10);

                column = new Column();
                column.Name = "USERNM";//列名
                column.DefinedSize = 10;//字段长度
                column.Attributes = ColumnAttributesEnum.adColNullable;//允许空值
                table.Columns.Append(column, DataTypeEnum.adVarWChar, 10);
                table.Columns.Append("OPER", DataTypeEnum.adVarWChar, 10);

                column = new Column();
                column.Name = "USEYN";//列名
                column.DefinedSize = 10;//字段长度
                column.Attributes = ColumnAttributesEnum.adColNullable;//允许空值
                table.Columns.Append(column, DataTypeEnum.adVarWChar, 10);

                catalog.Tables.Append(table);//添加表 USERINFO

                //********************************************************************************
                //********************************************************************************
                table = null;
                catalog = null;
                Conn.Close(); ////此处一定要关闭连接，否则添加数据时候会出错

                AddUser();//添加 admin 用户
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>添加默认用户 admin </summary>
        private static void AddUser()
        {
            try
            {
                string sql = " INSERT INTO USERINFO  ([USERID], [PASSWD], [USERNM], [OPER], [USEYN])\n";
                sql += "                VALUES ('admin', '1648618', 'ADMIN', 'TEST', 'Y') \n";

                ExecSql(sql, null, true);//ADD admin, 1648618, ADMIN, TEST, Y

                sql = " INSERT INTO USERINFO  ([USERID], [PASSWD], [USERNM], [OPER], [USEYN])\n";
                sql += "                VALUES ('USER', 'USER', 'USER', '0', 'N') \n";

                ExecSql(sql, null, true);//ADD USER, USER, USER, 0, N
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion END

        //%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK 
        #region Oracle数据库读数据
        /// <summary>读 Oracle 数据,factory="DACE", "VACE"</summary>
        /// <param name="Sql">SQL语句</param>
        /// <param name="factory">"DACE","VACE"</param>
        /// <returns> DataTable </returns>
        public static DataTable Oracle_Data_Table(string SQL)
        {
            OracleConnection Conn;
            try
            {
                DataTable DT = new DataTable();//定义数据集;
                Conn = new OracleConnection(ACE_Conn);
                Conn.Open();//打开数据库连接

                string sql = "alter session set nls_language='AMERICAN'";//可以更改会话字符集环境  AMERICAN  SIMPLIFIED CHINESE  
                var Cmd = new OracleCommand(sql, Conn);
                Cmd.ExecuteNonQuery();

                var cmd = new OracleCommand(SQL, Conn);
                OracleDataAdapter Sql_DA = new OracleDataAdapter(cmd);
                Sql_DA.Fill(DT);
                Sql_DA.Dispose();
                Conn.Close();
                Conn.Dispose();
                cmd.Dispose();
                return DT;

                //var SessionInfo = Conn.GetSessionInfo();
                //SessionInfo.Language = "AMERICAN";
                //Conn.SetSessionInfo(SessionInfo);

                //using (OracleDataReader reader = cmd.ExecuteReader())
                //{
                //    DT.Load(reader);//填充datatable 
                //}

            }
            catch (Exception ex) {

                //Encoding KSC = Encoding.GetEncoding("EUC-KR");// 25"ks_c_5601-1987"  ko_KR.EUC-KR   EUC-KR
                //Encoding GBK = Encoding.GetEncoding("GB18030");//GB18030  gb2312
                //Byte[] gb = GBK.GetBytes(text);
                //gb = Encoding.Convert(GBK, KSC, gb);
                //return GBK.GetString(gb);
                throw ex;
            }
        }

        /// <summary>读 Oracle 数据,factory="DACE", "VACE"</summary>
        /// <param name="Sql">SQL语句</param>
        /// <param name="factory">"DACE","VACE"</param>
        /// <returns> DataSet </returns>
        public DataSet Data_Set(string Sql)
        {
            DataSet ds = new DataSet();
            OracleConnection Conn = new OracleConnection(ACE_Conn);
            Conn.Open();
            OracleCommand cmd = new OracleCommand(Sql, Conn);
            OracleDataAdapter Sql_DA = new OracleDataAdapter(cmd);
            Sql_DA.Fill(ds);
            Conn.Close();
            return ds;
        }
        #endregion END
        //%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK 
        #region Access数据库读数据
        /// <summary>读 Access 数据</summary>
        /// <param name="Sql">SQL语句</param>
        /// <returns> DataTable </returns>
        public static DataTable Access_Data_Table(string Sql)
        {
            try
            {
                DataTable DT = new DataTable();//定义数据集;
                OleDbConnection Conn = new OleDbConnection(Access_Conn);
                Conn.Open();//打开数据库连接
                OleDbCommand cmd = new OleDbCommand(Sql, Conn);

                OleDbDataAdapter Sql_DA = new OleDbDataAdapter(cmd);
                Sql_DA.Fill(DT);
                Sql_DA.Dispose();
                Conn.Close();
                return DT;
                //cmd.ExecuteNonQuery();
                //DT.Load(cmd.ExecuteReader());//填充datatable 
            }
            catch (Exception ex) { throw ex; }

        }
        #endregion END
        //%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK 
        #region 获取服务器时间
        /// <summary>获取服务器时间</summary>
        /// <param name="format"> 时间格式 "yyyyMMdd,HHmm", "yyyyMMdd,HHmmss", "yyyy-MM-dd HH:mm:ss"</param>
        /// <param name="factory">"DACE","VACE"</param>
        /// <returns>(日期,时间)string</returns>
        public static string GetServerDateTime(string format, string factory)
        {
            string sql = @"select sysdate from dual", ss = ","; //取时间
            //sql = "SELECT SYSDATE, TO_CHAR(SYSDATE,'YYYYMMDD') YYYYMMDD, TO_CHAR(SYSDATE, 'HH24:MI:SS') FROM DUAL";// PRODDT = "20210522"，HHMM= "16:41"
            var table = Oracle_Data_Table(sql);

            if (table.Rows.Count > 0)
            {
                ss = table.Rows[0][0].ToString();//2021/5/11 12:13:21
                DateTime D_DateTime = Convert.ToDateTime(ss);
                ss = D_DateTime.ToString(format);// "yyyyMMdd,HHmmss"  2021-05-11 12:43:56 "yyyy-MM-dd HH:mm:ss"
            }
            return ss;
        }
        #endregion END

        //%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK
        #region 检查条码
        /// <summary>检查条码->数据库</summary>
        /// <param name="SN">条码</param>
        /// <param name="factory">"DACE","VACE"</param>
        /// <returns>有=true, 没有=false</returns>
        public static bool Check_SN(string SN, string factory)
        {
            if (SN.ToUpper() == "TEST")//气密机测试
            {
                REC_inf.Model = "TEST";
                return true;
            }
            else if (DB.Local_login)//本机登陆不检查条码
                return true;

            //string FACILITY = SYS_Set.Factory_Code;
            string sql, TXT;
            bool STATE = false;
            DataTable table;
            REC_inf.Model = "";
            try
            {
                if (factory == "DACE")
                {
                    sql = "SELECT LOT_NUMBER as ACESN, OPER as OPER, ORDER_NO as ORDERNO, RWRK_FG as RWRKFG, RWRK_OPR as RWRKOPR  \n"
                                   + "     FROM AERP.WIPLOT \n"
                                   + "         WHERE (LOT_NUMBER   = '" + SN + "' \n"
                                   + "         OR ALIAS_LOT_NUMBER = '" + SN + "') \n"
                                   ;

                    table = Oracle_Data_Table(sql);
                    if (table.Rows.Count < 1)
                    {
                        sql = "SELECT LOT_NUMBER as ACESN, OPER as OREP, ORDER_NO as ORDERNO, RWRK_FG as RWRKFG, RWRK_OPR as RWRKOPR \n"
                            + "     FROM AERP.WIPLOT \n"
                            + "         WHERE LOT_NUMBER = (SELECT ACEID FROM AERP.HISBAR \n"
                            + "             WHERE (SERIALNUMBER = '" + SN + "' OR SN_2 = '" + SN + "')) \n"
                            ;

                        table = Oracle_Data_Table(sql);
                        if (table.Rows.Count < 1)
                            if (SYS_Set.LANG == "CN")
                                throw new Exception("没找到条码信息!");
                            else
                                throw new Exception("No Barcode information Found!");
                    }
                }
                else//VACE
                {
                    sql = "SELECT ACESN, OPER, ORDERNO, RWRKFG, RWRKOPR \n"
                       + " FROM AMES.WIPLOT \n"
                       + "     WHERE     ACESN = '" + SN + "' \n"
                       + "           OR CUSTSN = '" + SN + "' \n";

                    table = Oracle_Data_Table(sql);

                    if (table.Rows.Count < 1)//没找到
                    {
                        sql = "SELECT ACESN, OPER, ORDERNO, RWRKFG, RWRKOPR \n"
                            + "     FROM AMES.WIPLOT \n"
                            + "         WHERE ACESN = (SELECT ACEID FROM AMES.BARHIS \n"
                            + "             WHERE (SERIALNUMBER = '" + SN + "' OR SN_2 = '" + SN + "'))\n"
                            ;

                        table = Oracle_Data_Table(sql);
                        if (table.Rows.Count < 1)
                            if (SYS_Set.LANG == "CN")
                                throw new Exception("没找到条码信息!");
                            else
                                throw new Exception("No Barcode information Found!");
                    }
                }

                if (table.Rows[0]["RWRKFG"].ToString() == "Y")
                {
                    if (table.Rows[0]["RWRKOPR"].ToString() != REC_inf.OPER)
                    {
                        TXT = table.Rows[0]["RWRKOPR"].ToString(); //返修工序
                        if (SYS_Set.LANG == "CN")
                            TXT = "已到返修工序: " + TXT;
                        else
                            TXT = "To Repair Process: " + TXT;
                        throw new Exception(TXT);
                    }
                }
                else if (table.Rows[0]["OPER"].ToString() != REC_inf.OPER)//检查工序号
                {
                    TXT = table.Rows[0]["OPER"].ToString();//工序号
                    if (SYS_Set.LANG == "CN")
                        TXT = "此产品已到工序: " + TXT;
                    else
                        TXT = "Product to Process: " + TXT;
                    throw new Exception(TXT);

                }

                //查找机种名
                string ORDERNO = table.Rows[0]["ORDERNO"].ToString();//条码段
                REC_inf.SN = table.Rows[0]["ACESN"].ToString();//ACE条码号

                if (factory == "DACE")
                {
                    sql = "SELECT ORD_DESC as MODEL,ORD_UDF_6 as CLOSE FROM AERP.WIPORD \n"
                      + "     WHERE ORDER_NO = '" + ORDERNO + "' \n"
                      ;
                }
                else
                {
                    sql = "SELECT ORD_DESC as MODEL,ORD_UDF_6 as CLOSE FROM AMES.PRDORD \n"
                         + "     WHERE ORDER_NO = '" + ORDERNO + "' \n"
                         ;
                }
                table = Oracle_Data_Table(sql);

                if (table.Rows.Count > 0)
                {
                    if (table.Rows[0]["CLOSE"].ToString() == "X")
                    {
                        if (SYS_Set.LANG == "CN")
                            throw new Exception("此订单号已经关闭!");
                        else
                            throw new Exception("The order has been closed!");
                    }
                    else
                    {
                        REC_inf.Model = table.Rows[0]["MODEL"].ToString();//机种型号 
                    }
                }
                if (REC_inf.Model == "")
                {
                    if (SYS_Set.LANG == "CN")
                        throw new Exception("没找到产品型号!");
                    else
                        throw new Exception("No product model found!");
                }
                STATE = true;
                //Console.WriteLine();
            }
            catch (Exception ex) { throw ex; }
            return STATE;
        }
        #endregion NED
        //%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK
        //%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK

        #region 结果写入数据库
        /// <summary>测试数据写入FACRST表里,LeackData=InAirTime,CheckTime,InAirPressu,Leak_Pa,Leak_CCM,Result</summary>
        /// <param name="LeackData"></param> 
        /// <param name="factory">"DACE","VACE"</param>
        ///  /// <param name="Local">true 时不连接服务器</param>
        /// <returns>成功返回:true, 失败返回:false</returns>
        public static bool Leak_Data_FACRST(string[] LeackData, string factory, bool Local)
        {
            //工厂号  条码号 工序号 记录计数  in time  check time  in air   leakage   ccm                                             model    id     line    date    time    result        
            //FACILITY ACESN   OPER  SEONUM   NUMVAL1   NUMVAL2    NUMVAL3  NUMVAL4 NUMVAL5 NUMVAL6 NUMVAL7 NUMVAL8 NUMVAL9 NUMVAL10 TXTVAL1 TXTVAL2 TXTVAL3 TXTVAL4 TXTVAL5 TXTVAL6 TXTVAL7 TXTVAL8 TXTVAL9 TXTVAL1

            //FACILITY LOT_NUMBER  OPER   SEQ_NUM  NUMVAL1  NUMVAL2  NUMVAL3  NUMVAL4  NUMVAL5 TXTVAL1 TXTVAL2 TXTVAL3 TXTVAL4  TXTVAL5   TXTVAL6
            //工厂代码    条码号  工序号  记录次数 充气时间 检测时间 充气压力 泄漏压差 泄漏量    机种   用户ID   拉线  测试日期 测试时间  测试结果

            try
            {
                string FACILITY = SYS_Set.Factory_Code;
                string Date, Time, sql, ACESN;
                int SEQNUM = 1;
                DataTable table;

                if (Local)//取取本机时间
                {   //*******************　 取本机时间　 *****************************
                    DateTime Datetime = DateTime.Now;
                    Date = Datetime.ToString("yyyyMMdd");
                    Time = Datetime.ToString("HH:mm:ss");
                }
                else//取数据库的系统时间
                {
                    //*******************　 取数据库的系统时间　 *****************************
                    sql = "SELECT TO_CHAR(SYSDATE,'YYYYMMDD') YYYYMMDD, TO_CHAR(SYSDATE,'HH24:MI:SS') HHMM FROM DUAL";// PRODDT = "20210522"，'HH24MI' HHMM= "16:41"
                    table = Oracle_Data_Table(sql);
                    Date = table.Rows[0]["YYYYMMDD"].ToString();
                    Time = table.Rows[0]["HHMM"].ToString();
                }

                //*******************　 取最大记录数　 *****************************
                //int NO = 1;//编号
                //sql = "SELECT MAX(ID) FROM FACRST\n";
                //table = Access_Data_Table(sql);
                //if (table.Rows.Count > 0 && table.Rows[0][0].ToString() != "")
                //    NO = int.Parse(table.Rows[0][0].ToString())+1;//取最大记录数                     

                //sql = "SELECT MAX(SEQNUM) FROM FACRST\n";
                //sql += "          WHERE SN = '" + ACE_SN + "'\n";

                //table = Access_Data_Table(sql);

                //if (table.Rows.Count > 0 && table.Rows[0][0].ToString() != "")
                //    SEQNUM = int.Parse(table.Rows[0][0].ToString())+1;//取最大记录次数

                //*******************　 保存气密数据到 AERP.FACRST　 *****************************
                sql = " INSERT INTO FACRST       ([MODEL], [SN], [USER], [LINE],\n";
                sql += "                          [OPER], [DATE], [TIME], [InAirTime], \n";
                sql += "                          [CheckTime],[InAir], [Leak], [CCM], [Result]) \n";
                sql += " VALUES ('" + REC_inf.Model + "', '" + REC_inf.SN + "', '" + REC_inf.User_ID + "', '" + REC_inf.Line_Num + "', \n";
                sql += "         '" + REC_inf.OPER + "', '" + Date + "', '" + Time + "', '" + LeackData[0] + "', \n";
                sql += "         '" + LeackData[1] + "', '" + LeackData[2] + "', '" + LeackData[3] + "', '" + LeackData[4] + "', '" + LeackData[5] + "' )\n";

                if (!ExecSql(sql, factory, true))
                {
                    if (SYS_Set.LANG == "CN")
                        throw new Exception("本机保存数据失败!");
                    else
                        throw new Exception("Failed to save data locally!");
                }

                if (!Local)//Oracle
                {
                    if (factory == "DACE")
                    {
                        //*******************　 取最大记录数　 *****************************
                        sql = "SELECT NVL(MAX(SEQ_NUM),0)+1 MAXSEQ  FROM AERP.FACRST\n"
                           + "      WHERE FACILITY = '" + FACILITY + "'\n"
                           + "          AND LOT_NUMBER = '" + REC_inf.SN + "'\n";
                        table = Oracle_Data_Table(sql);

                        if (table.Rows.Count > 0 && table.Rows[0][0].ToString() != "")
                            SEQNUM = int.Parse(table.Rows[0][0].ToString());//取最大记次数

                        sql = "SELECT LOT_NUMBER as ACESN \n";
                        sql += "    FROM AERP.WIPLOT \n";
                        sql += "        WHERE LOT_NUMBER = '" + REC_inf.SN + "' \n";
                        sql += "            OR ALIAS_LOT_NUMBER = '" + REC_inf.SN + "' \n";

                        table = Oracle_Data_Table(sql);
                        if (table.Rows.Count > 0)
                            ACESN = table.Rows[0]["ACESN"].ToString();  //ACE条码
                        else
                            ACESN = REC_inf.SN;  //ACE条码


                        //*******************　 保存气密数据到 AERP.FACRST　 *****************************
                        sql = " INSERT INTO AERP.FACRST ( \n";
                        sql += "                          FACILITY, LOT_NUMBER, OPER, SEQ_NUM, NUMVAL1,\n";
                        sql += "                          NUMVAL2, NUMVAL3, NUMVAL4, NUMVAL5, TXTVAL1, \n";
                        sql += "                          TXTVAL2, TXTVAL3, TXTVAL4, TXTVAL5, TXTVAL6) \n";
                        sql += " VALUES ('" + FACILITY + "', '" + ACESN + "', '" + REC_inf.OPER + "', '" + SEQNUM.ToString() + "', '" + LeackData[0] + "',\n";
                        sql += "         '" + LeackData[1] + "', '" + LeackData[2] + "', '" + LeackData[3] + "', '" + LeackData[4] + "', '" + REC_inf.Model + "',\n";
                        sql += "         '" + REC_inf.User_ID + "', '" + REC_inf.Line_Num + "', '" + Date + "', '" + Time + "', '" + LeackData[5] + "')\n";
                    }
                    else //VACE
                    {
                        //*******************　 取最大记录数　 *****************************
                        sql = "SELECT NVL(MAX(SEQNUM),0)+1 MAXSEQ  FROM AMES.FACRST\n"
                           + "      WHERE FACILITY = '" + FACILITY + "'\n"
                           + "          AND ACESN = '" + REC_inf.SN + "'\n";
                        table = Oracle_Data_Table(sql);

                        if (table.Rows.Count > 0 && table.Rows[0][0].ToString() != "")
                            SEQNUM = int.Parse(table.Rows[0][0].ToString());//取最大记次数

                        sql = "SELECT ACESN as ACESN \n";
                        sql += "    FROM AMES.WIPLOT \n";
                        sql += "        WHERE ACESN = '" + REC_inf.SN + "' \n";
                        sql += "            OR CUSTSN = '" + REC_inf.SN + "' \n";

                        table = Oracle_Data_Table(sql);
                        if (table.Rows.Count > 0)
                            ACESN = table.Rows[0]["ACESN"].ToString();  //ACE条码
                        else
                            ACESN = REC_inf.SN;  //ACE条码

                        //*******************　 保存气密数据到 AERP.FACRST　 *****************************
                        sql = " INSERT INTO AMES.FACRST ( \n";
                        sql += "                          FACILITY, ACESN, OPER, SEQNUM, NUMVAL1,\n";
                        sql += "                          NUMVAL2, NUMVAL3, NUMVAL4, NUMVAL5, TXTVAL1, \n";
                        sql += "                          TXTVAL2, TXTVAL3, TXTVAL4, TXTVAL5, TXTVAL6) \n";
                        sql += " VALUES ('" + FACILITY + "', '" + ACESN + "', '" + REC_inf.OPER + "', '" + SEQNUM.ToString() + "', '" + LeackData[0] + "',\n";
                        sql += "         '" + LeackData[1] + "', '" + LeackData[2] + "', '" + LeackData[3] + "', '" + LeackData[4] + "', '" + REC_inf.Model + "',\n";
                        sql += "         '" + REC_inf.User_ID + "', '" + REC_inf.Line_Num + "', '" + Date + "', '" + Time + "', '" + LeackData[5] + "')\n";

                    }

                    if (!ExecSql(sql, factory, false))
                    {
                        if (SYS_Set.LANG == "CN")
                            throw new Exception("服务器保存数据失败!");
                        else
                            throw new Exception("The server failed to save the data!");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>气密测试结果写入数据库WIPWTM表里</summary>
        /// <param name="LeackData">"DACE","VACE"</param>
        /// <param name="factory">"DACE","VACE"</param>
        /// <returns>成功返回:true, 失败返回:false</returns>
        public static bool Leak_Data_WIPWTM(string factory)
        {
            string sql, Date/*日期*/, Time/*时间*/, ACESN, CUSTSN,/*客户条码*/ORDERNO/*条码段*/, PROD/*产品代码*/, RWRKFG/*返修标志*/;
            string STATUS = "E"/*状态*/, SAPOPER/*SAP工序*/, WORKSHIFTCD/*白夜班 D=白班, N=夜班*/, Result/*结果 FAIL = S, PASS = P*/;
            int SEQNUM = 1;//记录最大次数
            string FACILITY = SYS_Set.Factory_Code;
            DataTable table;

            try
            {
                //*******************　 取数据库的系统时间　 *****************************
                sql = "SELECT TO_CHAR(SYSDATE,'YYYYMMDD') YYYYMMDD, TO_CHAR(SYSDATE,'HH24MI') HHMM FROM DUAL";// PRODDT = "20210522"，HHMM= "1641"
                table = Oracle_Data_Table(sql);
                Date = table.Rows[0]["YYYYMMDD"].ToString();
                Time = table.Rows[0]["HHMM"].ToString();

                if (factory == "DACE")
                {
                    //*******************　 取最大记录数　 *****************************
                    sql = "SELECT NVL(MAX(SEQ_NUM),0)+1 MAXSEQ  FROM AERP.WIPWTM\n";
                    sql += "      WHERE FACILITY = '" + FACILITY + "'\n";
                    sql += "          AND LOT_NUMBER = '" + REC_inf.SN   + "'\n";
                    sql += "               AND OPER  = '" + REC_inf.OPER + "'\n";

                    table = Oracle_Data_Table(sql);

                    if (table.Rows.Count > 0 && table.Rows[0][0].ToString() != "")
                        SEQNUM = int.Parse(table.Rows[0][0].ToString());//取最大记次数

                    //*******************　 取产品信息　 *****************************
                    sql = "SELECT LOT_NUMBER as ACESN, ALIAS_LOT_NUMBER as CUSTSN, ORDER_NO as ORDERNO,PROD as PROD, RWRK_FG as RWRKFG \n";
                    sql += "    FROM AERP.WIPLOT \n";
                    sql += "        WHERE LOT_NUMBER = '" + REC_inf.SN + "' \n";
                    sql += "            OR ALIAS_LOT_NUMBER = '" + REC_inf.SN + "' \n";

                    table = Oracle_Data_Table(sql);
                    if (table.Rows.Count < 1)
                    {
                        if (SYS_Set.LANG == "CN")
                            throw new Exception("服务器保存数据失败!");
                        else
                            throw new Exception("The server failed to save the data!");
                    }

                    ACESN   = table.Rows[0]["ACESN"].ToString();  //ACE条码
                    PROD    = table.Rows[0]["PROD"].ToString();   //产品代码
                    RWRKFG  = table.Rows[0]["RWRKFG"].ToString(); //返修标志
                    CUSTSN  = table.Rows[0]["CUSTSN"].ToString(); //客户条码
                    ORDERNO = table.Rows[0]["ORDERNO"].ToString();//条码段

                    Result  = (TestData.Result == 1) ? ("P") : ("S");//结果 FAIL = S, PASS = P
                    SAPOPER = (REC_inf.OPER.Length > 0) ? (REC_inf.OPER.Substring(0, 1) + "0") : ("");//工序段 3520->30
                    WORKSHIFTCD = (int.Parse(Time) >= 2130 || int.Parse(Time) < 0830) ? ("N") : ("D");//白夜班 D=白班, N=夜班

                    //*******************　 保存气密数据到 AERP.FACRST　 *****************************
                    sql = " INSERT INTO AERP.WIPWTM ( FACILITY,  LOT_NUMBER,  SEQ_NUM, ALIAS_LOT_NUMBER,   OPER,\n";//工厂代码, ACE条码, 记录次数, 客户码, 工序
                    sql += "                            PRODDT,      ENTITY, ORDER_NO,             PROD, E_OPER,\n";//日期, 线号, 条码段, 产品代码, SAP工序
                    sql += "                            STATUS,        FLAG, EQUIP_ID,         DUE_TIME, USERID,\n";//状态, 返修标志, 设备编号, 1,用户ID 
                    sql += "                            RESULT, WORKSHIFTCD, DEFECTGB, DEFECTCD) \n";//结果, 白夜班标志, 03,01
                    sql += " VALUES ('" + FACILITY + "', '" + ACESN + "', '" + SEQNUM.ToString() + "', '" + CUSTSN + "', '" + REC_inf.OPER + "',\n";
                    sql += "         '" + Date + "', '" + REC_inf.Line_Num + "', '" + ORDERNO + "', '" + PROD + "', '" + SAPOPER + "',\n";
                    sql += "         '" + STATUS + "', '" + RWRKFG + "', '" + MC_inf.MC_Num + "', '" + "1" + "', '" + REC_inf.User_ID + "',\n";
                    sql += "         '" + Result + "', '" + WORKSHIFTCD + "', '03', '01' )\n";
                }
                else
                {
                    //*******************　 取最大记录数　 *****************************
                    sql = "SELECT NVL(MAX(SEQNUM),0)+1 MAXSEQ  FROM AMES.WIPWTM\n";
                    sql += "      WHERE FACILITY = '" + FACILITY + "'\n";
                    sql += "           AND ACESN = '" + REC_inf.SN + "'\n";
                    sql += "           AND OPER  = '" + REC_inf.OPER + "'\n";

                    table = Oracle_Data_Table(sql);

                    if (table.Rows.Count > 0 && table.Rows[0][0].ToString() != "")
                        SEQNUM = int.Parse(table.Rows[0][0].ToString());//取最大记次数

                    sql = "SELECT ACESN as ACESN, CUSTSN as CUSTSN, ORDERNO as ORDERNO,PROD as PROD, RWRKFG as RWRKFG \n";
                    sql += "    FROM AMES.WIPLOT \n";
                    sql += "        WHERE ACESN = '" + REC_inf.SN + "' \n";
                    sql += "            OR CUSTSN = '" + REC_inf.SN + "' \n";

                    table = Oracle_Data_Table(sql);
                    if (table.Rows.Count < 1)
                    {
                        if (SYS_Set.LANG == "CN")
                            throw new Exception("服务器保存数据失败!");
                        else
                            throw new Exception("The server failed to save the data!");
                    }

                    ACESN = table.Rows[0]["ACESN"].ToString();    //ACE条码
                    PROD = table.Rows[0]["PROD"].ToString();      //产品代码
                    RWRKFG = table.Rows[0]["RWRKFG"].ToString();  //返修标志
                    CUSTSN = table.Rows[0]["CUSTSN"].ToString();  //客户条码
                    ORDERNO = table.Rows[0]["ORDERNO"].ToString();//条码段

                    Result = (TestData.Result == 1) ? ("P") : ("S");//结果 FAIL = S, PASS = P
                    SAPOPER = (REC_inf.OPER.Length > 0) ? (REC_inf.OPER.Substring(0, 1) + "0") : ("");//SAP工序 3520->30
                    WORKSHIFTCD = (int.Parse(Time) >= 2130 || int.Parse(Time) < 0830) ? ("N") : ("D");//白夜班 D=白班, N=夜班

                    //*******************　 保存气密数据到 AERP.FACRST　 *****************************
                    sql = " INSERT INTO AMES.WIPWTM    (FACILITY, ACESN,    SEQNUM,   CUSTSN,   OPER,   \n";//工厂代码, ACE条码, 记录次数, 客户码, 工序
                    sql += "                               PRODDT, ENTITY,   ORDERNO,  PROD,     SAPOPER,\n";//日期, 线号, 条码段, 产品代码, SAP工序
                    sql += "                               STATUS, EQUIPID,  TACKTIME, OPERATOR, RESULT, \n";//状态, 设备编号, 1,用户ID ,结果
                    sql += "                          WORKSHIFTCD, DEFECTGB, DEFECTCD) \n";                  //白夜班标志, 03,01
                    sql += " VALUES ('" + FACILITY + "', '" + ACESN + "', '" + SEQNUM.ToString() + "', '" + CUSTSN + "', '" + REC_inf.OPER + "',\n";
                    sql += "         '" + Date + "', '" + REC_inf.Line_Num + "', '" + ORDERNO + "', '" + PROD + "', '" + SAPOPER + "',\n";
                    sql += "         '" + STATUS + "', '" + MC_inf.MC_Num + "', '" + "1" + "', '" + REC_inf.User_ID + "', '" + Result + "',\n";
                    sql += "         '" + WORKSHIFTCD + "', '03', '01')\n";
                }

                ExecSql(sql, factory, false);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        /// <summary>执行SQL, Oracle=[true=Oracle,false=Access], factory=["DACE" OR "VACE"]</summary>
        /// <param name="SQL">执行的SQL</param>
        /// <param name="factory">"DACE","VACE"</param>
        /// <param name="Local">true=Oracle, false=本机Access</param>
        ///  /// <returns>成功返回:true, 失败返回:false</returns>
        public static bool ExecSql(string SQL, string factory, bool Local)
        {
            try
            {
                if (Local)//本机
                {
                    OleDbConnection Conn = new OleDbConnection(Access_Conn);
                    Conn.Open();//打开数据库连接
                    OleDbCommand cmd = new OleDbCommand(SQL, Conn);

                    cmd.ExecuteNonQuery();
                    Conn.Close();
                    cmd.Dispose();
                    return true;

                }
                else  //服务器
                {
                    OracleConnection Conn = new OracleConnection(ACE_Conn);
                    Conn.Open();

                    string sql = "alter session set nls_language='AMERICAN'"; //可以更改会话字符集环境              
                    var Cmd = new OracleCommand(sql, Conn);
                    Cmd.ExecuteNonQuery();

                    var cmd = new OracleCommand(SQL, Conn);
                    cmd.ExecuteNonQuery();//ExecuteNonQuery()方法主要用户更新数据，通常它使用Update,Insert,Delete语句来操作数据库

                    Conn.Close();
                    cmd.Dispose();
                    return true;
                }
            }
            catch (Exception ex) { throw ex; }
        }
        #endregion NED

        //%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK  
        #region EXCEL 到 DataTable
        /// <summary>Excel某sheet中内容导入到DataTable中,区分xsl和xslx分别处理</summary>
        /// <param name="filePath">Excel文件路径,含文件全名</param>
        /// <param name="sheetName">此Excel中sheet名</param>
        /// <returns>DataTable</returns>
        public static DataTable EXCEL_DT(string filePath, string sheetName)
        {
            DataTable dt = new DataTable();
            try
            {
                IWorkbook hssfworkbook = null;
                ISheet sheet;

                if (Path.GetExtension(filePath).ToLower() == ".xls".ToLower())//.xls
                {
                    Path.GetExtension(filePath).EndsWith(".xls");

                    #region .xls文件处理:HSSFWorkbook
                    using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        hssfworkbook = new HSSFWorkbook(file);

                    if (sheetName == null | sheetName == "")//sheet名为空
                        sheet = hssfworkbook.GetSheetAt(0);//取第1张表
                    else
                        sheet = hssfworkbook.GetSheet(sheetName);//取表名的张表

                    System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
                    HSSFRow headerRow = (HSSFRow)sheet.GetRow(0);

                    //一行最后一个方格的编号 即总的列数 
                    for (int j = 0; j < (sheet.GetRow(0).LastCellNum); j++)
                    {   //设置每个列名
                        HSSFCell cell = (HSSFCell)headerRow.GetCell(j);
                        dt.Columns.Add(cell.ToString());
                    }

                    while (rows.MoveNext())
                    {
                        IRow row = (HSSFRow)rows.Current;
                        DataRow dr = dt.NewRow();

                        if (row.RowNum == 0) continue;//第一行是标题，不需要导入

                        for (int i = 0; i < row.LastCellNum; i++)
                        {
                            if (i >= dt.Columns.Count)//cell count>column count,then break //每条记录的单元格数量不能大于表格栏位数量 20140213
                                break;
                            ICell cell = row.GetCell(i);
                            if ((i == 0) && (string.IsNullOrEmpty(cell.ToString()) == true))//每行第一个cell为空,break
                                break;
                            if (cell == null)
                                dr[i] = null;
                            else
                                dr[i] = cell.ToString();
                        }
                        dt.Rows.Add(dr);
                    }
                    #endregion
                }
                else //.xlsx
                {
                    #region .xlsx文件处理:XSSFWorkbook
                    using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        hssfworkbook = new XSSFWorkbook(file);

                    if (sheetName == null | sheetName == "")//sheet名为空
                        sheet = hssfworkbook.GetSheetAt(0);//取第1张表
                    else
                        sheet = hssfworkbook.GetSheet(sheetName);//取表名的张表

                    System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
                    XSSFRow headerRow = (XSSFRow)sheet.GetRow(0);

                    //一行最后一个方格的编号 即总的列数 
                    for (int j = 0; j < (sheet.GetRow(0).LastCellNum); j++)
                    {//设置每个列名
                        XSSFCell cell = (XSSFCell)headerRow.GetCell(j);
                        dt.Columns.Add(cell.ToString());
                    }

                    while (rows.MoveNext())
                    {
                        IRow row = (XSSFRow)rows.Current;
                        DataRow dr = dt.NewRow();

                        if (row.RowNum == 0) continue;//第一行是标题，不需要导入

                        for (int i = 0; i < row.LastCellNum; i++)
                        {
                            if (i >= dt.Columns.Count)//cell count>column count,then break //每条记录的单元格数量不能大于表格栏位数量 20140213
                                break;

                            ICell cell = row.GetCell(i);

                            if ((i == 0) && (string.IsNullOrEmpty(cell.ToString()) == true))//每行第一个cell为空,break
                                break;

                            if (cell == null)
                                dr[i] = null;
                            else
                                dr[i] = cell.ToString();
                        }
                        dt.Rows.Add(dr);
                    }
                    #endregion
                }
                hssfworkbook.Close();
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
            return dt;
        }
        #endregion END

        //%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK
        #region 导出到EXCEL 
        /// <summary>DataGrid 导出到Excel NPOI导出Excel，不依赖本地是否装有Excel，导出速度快</summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="sheetName">sheetName</param>
        /// <returns> 成功=true, 失败=false </returns>
        public static bool TO_EXCEL(DataGrid dg, string sheetName)
        {
            DataTable dt = DG_DT(dg);//DataGrid转换DataTable
            return DT_EXCEL(dt, sheetName);
        }
        /// <summary>DataTable 导出到Excel NPOI导出Excel，不依赖本地是否装有Excel，导出速度快</summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="sheetName">sheetName</param>
        /// <returns> 成功=true, 失败=false </returns>
        public static bool TO_EXCEL(DataTable dt, string sheetName)
        {
            return DT_EXCEL(dt, sheetName);
        }
        /// <summary>DataTable 导出到Excel NPOI导出Excel，不依赖本地是否装有Excel，导出速度快</summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="sheetName">sheetName</param>
        /// <returns> 成功=true, 失败=false </returns>
        private static bool DT_EXCEL(DataTable DT, string sheetName)
        {
            try
            {
                if (sheetName == "")
                    sheetName = "Sheet";
                if (DT == null || DT.Rows.Count == 0)
                    throw new Exception("表内没有发现数据");
                if (DT.Rows.Count * DT.Columns.Count > 64000)
                    throw new Exception("单元格超出64000的限制!");

                var workbook = new XSSFWorkbook();//*.xlsx
                if (sheetName == null)
                    sheetName = "Sheet";

                var sheet = workbook.CreateSheet(sheetName);//创建一个表 Sheet  table

                #region 设置文件属性
                NPOI.POIXMLProperties 属性 = workbook.GetProperties();//获取属性
                属性.CoreProperties.Title = "标题1";
                属性.CoreProperties.Subject = "主题2";
                属性.CoreProperties.Keywords = "标记3";
                属性.CoreProperties.Category = "类别4";
                属性.CoreProperties.Description = "备注5";
                //来源
                属性.CoreProperties.Creator = "LeakMC";//作者
                属性.CoreProperties.LastModifiedByUser = "LeakMC";//最后1次保存者2
                属性.CoreProperties.ContentStatus = "LeakMC 生成";//内容状态   new DateTime(2004, 5, 6)
                #endregion 设置文件属性 END

                #region 设置样式
                ICellStyle cellStyle = workbook.CreateCellStyle();//样式变量

                cellStyle.BorderTop = BorderStyle.Thin;//上边框线 
                cellStyle.BorderBottom = BorderStyle.Thin;//下边框线 
                cellStyle.BorderLeft = BorderStyle.Thin;//左边框线 
                cellStyle.BorderRight = BorderStyle.Thin;//右边框线 
                cellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;   //文字垂直对齐
                cellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;//文字水平对齐
                                                                                         //cellStyle.WrapText = true;  //是否换行 
                cellStyle.ShrinkToFit = true;//缩小字体填充


                //字体样式1：下划线，斜体，红色，fontsize=20
                IFont font = workbook.CreateFont();
                //font.FontName = "宋体";//字体
                //font.IsBold = true;  //粗体
                //font.IsItalic = true;  //斜体
                //font.IsStrikeout = true;//删除线
                //font.Underline = FontUnderlineType.Double;//下划线
                font.FontHeightInPoints = 12;//字号
                //font.Color = NPOI.HSSF.Util.HSSFColor.Red.Index;//字体颜色
                cellStyle.SetFont(font);
                #endregion 设置样式 END

                #region DataGrid To EXCEL
                //if (!DT.Columns.Contains("NO"))
                //    DT.Columns.Add("NO", typeof(Decimal), "000").SetOrdinal(0);//DT.Columns.Add("NO", typeof(Decimal), "000").SetOrdinal(0);

                var row = sheet.CreateRow(0);//新建标题
                for (var j = 0; j < DT.Columns.Count; j++)
                {
                    //0行=标题，黑底白字，字体加粗
                    var 标题样式 = workbook.CreateCellStyle();//新样式
                    var 标题font = workbook.CreateFont();
                    标题样式.CloneStyleFrom(cellStyle);//克隆前一个样式
                    标题font.IsBold = true;  //粗体
                    标题font.FontHeightInPoints = 12;//字号
                    标题font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;//字体颜色
                    标题样式.SetFont(标题font);

                    标题样式.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Black.Index;//单元格填充颜色
                    标题样式.FillPattern = FillPattern.SolidForeground;//填充样式

                    //sheet.GetRow(0).Height = 400;//行高
                    var Cell = row.CreateCell(j);// 新建1行.新建1列
                    Cell.SetCellValue(DT.Columns[j].ColumnName);//Excel单元格赋值
                    Cell.CellStyle = 标题样式;//设置样式
                    //Console.WriteLine(DT.Columns[j].ColumnName);
                }

                for (var i = 0; i < DT.Rows.Count; i++)
                {
                    row = sheet.CreateRow(i + 1);//新建1行

                    for (var j = 0; j < DT.Columns.Count; j++)
                    {
                        var Cell = row.CreateCell(j);// 新建1列
                        string text = DT.Rows[i][j].ToString();

                        if (IsNum(text, out double num))
                            Cell.SetCellValue(num); //Excel单元格赋值
                        else
                            Cell.SetCellValue(text); //Excel单元格赋值
                        Cell.CellStyle = cellStyle;//设置样式

                        if (sheet.GetRow(0).GetCell(j).StringCellValue.ToUpper() == "RESULT" || sheet.GetRow(0).GetCell(j).StringCellValue.ToUpper() == "结果")//气密数据设置结果单元格填充颜色
                        {
                            var newCellStyle = workbook.CreateCellStyle();//新样式
                            newCellStyle.CloneStyleFrom(cellStyle);//克隆前一个样式
                            if (text.ToUpper() == "PASS")
                                newCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Green.Index;//单元格填充绿色
                            else
                                newCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Red.Index;  //单元格填充红色

                            newCellStyle.FillPattern = FillPattern.SolidForeground;//填充样式                                                                  
                            Cell.CellStyle = newCellStyle;//设置样式
                        }
                        else
                            Cell.CellStyle = cellStyle;//设置样式
                        if (i == DT.Rows.Count - 1)//最后1行
                            sheet.AutoSizeColumn(j);//全部自动列宽
                    }
                }
                #endregion DataGrid To EXCEL END

                #region  弹出保存对话框,并保存文件
                SaveFileDialog sfd = new SaveFileDialog();
                //sfd.Filter = "Office 2007 File|*.xlsx|Office 2000-2003 File|*.xls|全部文件|*.*";
                sfd.Filter = "Office 2000-2003 File|*.xls|全部文件|*.*";

                if (workbook.ToString() == "NPOI.HSSF.UserModel.HSSFWorkbook")
                {
                    sfd.DefaultExt = ".xls";
                    sfd.Filter = "Office 2000-2003 File|*.xls|全部文件|*.*";
                }
                else
                {
                    sfd.DefaultExt = ".xlsx";
                    sfd.Filter = "Office 2007 File|*.xlsx|全部文件|*.*";
                }

                if ((bool)sfd.ShowDialog())
                {
                    if (sfd.FileName != "")
                    {
                        using (var fs = File.OpenWrite(sfd.FileName))
                        {
                            workbook.Write(fs);   //向打开的这个xls文件中写入mySheet表并保存
                                                  //MessageBox.Show("保存成功!");
                            return true;
                        }
                    }
                }
                #endregion
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
            return false;
        }
        /// <summary>
        /// DataGrid转换DataTable
        /// </summary>
        /// <param name="dg">dataGrid</param>
        /// <returns>返回dataTable</returns>
        public static DataTable DG_DT(DataGrid dg)
        {
            try
            {
                DataTable dt = null;

                if (dg.ItemsSource is DataView)
                {
                    dt = (dg.ItemsSource as DataView).Table;
                }
                else if (dg.ItemsSource is DataTable)
                {
                    dt = dg.ItemsSource as DataTable;
                }
                else if (dg.ItemsSource is DataSet)
                {
                    dt = (dg.ItemsSource as DataSet).Tables[0];
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion END

        //%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK  


        /// <summary>判断IP格式是否正确</summary>
        /// <param name="IP">IP</param>
        /// <returns>true, false</returns>
        public static bool Format_IP(string IP)
        {
            if (Regex.IsMatch(IP, @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])(\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])){3}$"))
                return true;
            return false;
        }


        /// <summary>判断字符串是否是数字</summary>
        /// <param name="str">字符串</param>
        /// <param name="result">返回转换的数字, 失败为-1</param>
        /// <returns>true, false</returns>
        public static bool IsNum(string str, out double result)
        {
            result = -1;
            try
            {
                System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@"^(-|\+)?\d+(\.\d+)?$");// ^(-|\+)?\d+(\.\d+)?$   ^\d+$             

                if (rex.IsMatch(str) && str.Length < 12)
                {
                    result = double.Parse(str);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary>判断字符串是否是数字</summary>
        /// <param name="str">字符串</param>
        /// <param name="result">返回转换的数字, 失败为-1</param>
        /// <returns>true, false</returns>
        public static bool IsNum(string str, out int result)
        {
            result = -1;
            try
            {
                System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@"^(-|\+)?\d+(\.\d+)?$");// ^(-|\+)?\d+(\.\d+)?$   ^\d+$             

                if (rex.IsMatch(str) && str.Length < 12)
                {
                    result = int.Parse(str);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

        }
        //%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK--%%--OK  

        /// <summary>字体集转换  ks_c_5601-1987->GB18030</summary>
        /// <param name="text">转换的文本</param>
        public static string Ksc_GB18030(String text)
        {
            Encoding KSC = Encoding.GetEncoding("EUC-KR");// 25"ks_c_5601-1987"  ko_KR.EUC-KR   EUC-KR
            Encoding GBK = Encoding.GetEncoding("GB18030");//GB18030  gb2312
            Byte[] gb = GBK.GetBytes(text);
            gb = Encoding.Convert(GBK, KSC, gb);
            return GBK.GetString(gb);
            //KSC.GetChars(gb);
        }



        /// <summary>读注册表</summary>
        /// <param name="name">注册表-名称</param>
        /// <param name="Default">注册表为空时,(默认)数据</param>
        /// <returns>object 注册表-数据</returns>
        public static object Get_Reg(string name, object Default)
        {
            try
            {
                var Reg = Registry.CurrentUser.CreateSubKey("software\\Leakage", true);
                object obj = Reg.GetValue(name);
                if (obj == null)
                {
                    Reg.SetValue(name, Default);
                    return Default;
                }
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>写注册表</summary>
        /// <param name="name">注册表-名称</param>
        /// <param name="Default">数据</param>
        /// <returns></returns>
        public static void Set_Reg(string name, object value)
        {
            try
            {
                var Reg = Registry.CurrentUser.CreateSubKey("software\\Leakage", true);
                Reg.SetValue(name, value);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static bool AddDataTableToDB(DataTable source, string tableName)
        {
            SqlTransaction tran = null;//声明一个事务对象
            SqlConnection Conn = null;

            try
            {
                using (Conn = new SqlConnection(@"server=.;uid=ASIS;pwd=ASIS;database=UH1;"))
                {
                    Conn.Open();//打开链接  
                    using (tran = Conn.BeginTransaction())
                    {
                        using (SqlBulkCopy copy = new SqlBulkCopy(Conn, SqlBulkCopyOptions.Default, tran))
                        {
                            copy.DestinationTableName = tableName; //指定服务器上目标表的名称                                                                
                            copy.BatchSize = 100000;//每10W条数据一个事物 
                            copy.BulkCopyTimeout = 60; //超时时间

                            #region 进行字段映射
                            if (tableName == "USERINFO")
                            {
                                //COMPANYCD, USERGB, USERID, USERNM, PASSWD, DEPTCD, REGDT, USEYN
                                //BIGO, UPDATEDT, UPDATEUSERID, UPDATEIP,GRADE, MANAGERID
                                //FROM USERINFO

                                //sql = "SELECT COMPANYCD,USERID,PASSWD,USERNM,DEPTCD,USEYN  \n"
                                //sql += "               *  FROM USERINFO \n";


                                copy.ColumnMappings.Add("COMPANYCD", "COMPANYCD");
                                copy.ColumnMappings.Add("USERGB", "USERGB");
                                copy.ColumnMappings.Add("USERID", "USERID");
                                copy.ColumnMappings.Add("USERNM", "USERNM");
                                copy.ColumnMappings.Add("PASSWD", "PASSWD");
                                copy.ColumnMappings.Add("DEPTCD", "DEPTCD");
                                copy.ColumnMappings.Add("REGDT", "REGDT");
                                copy.ColumnMappings.Add("USEYN", "USEYN");
                                copy.ColumnMappings.Add("BIGO", "BIGO");
                                copy.ColumnMappings.Add("UPDATEDT", "UPDATEDT");
                                copy.ColumnMappings.Add("UPDATEUSERID", "UPDATEUSERID");
                                copy.ColumnMappings.Add("UPDATEIP", "UPDATEIP");
                                copy.ColumnMappings.Add("GRADE", "GRADE");
                                copy.ColumnMappings.Add("MANAGERID", "MANAGERID");
                            }
                            else if (tableName == "")
                            {

                            }
                            #endregion


                            copy.WriteToServer(source); //执行把DataTable中的数据写入DB  
                            tran.Commit(); //提交事务  
                            return true; //返回True 执行成功！  
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                if (null != tran)
                    tran.Rollback();
                return false;//返回False 执行失败！  
            }
            finally
            {
                //if (Conn.State == ConnectionState.Open)
                //    Conn.Close();
            } //finally 块始终都会运行
        }
        public void SqlBulkCopyByDataTable(string 链接字符串, string 数据库中对应表名, DataTable 要写入数据库的DataTable, int batchSize = 100000)
        {
            using (SqlConnection connection = new SqlConnection(链接字符串))
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(链接字符串,
                SqlBulkCopyOptions.UseInternalTransaction))
                {
                    try
                    {
                        sqlBulkCopy.DestinationTableName = 数据库中对应表名;
                        sqlBulkCopy.BatchSize = batchSize;
                        for (int i = 0; i < 要写入数据库的DataTable.Columns.Count; i++)
                        {
                            sqlBulkCopy.ColumnMappings.Add(要写入数据库的DataTable.Columns[i].ColumnName, 要写入数据库的DataTable.Columns[i].ColumnName);
                        }
                        sqlBulkCopy.WriteToServer(要写入数据库的DataTable);
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                    finally { } //finally 块始终都会运行
                }
            }
        }

        //******************************************************************


        /// <summary> 执行存储过程</summary>
        /// <param name="S_StoredName">过程名</param>
        /// <param name="parm">参数</param>
        /// <returns>判断过程是否执行成功 (-1为成功) </returns>
        public string ExecP(string S_StoredName, OracleParameter[] parm)
        {
            try
            {
                OracleCommand cmd = new OracleCommand();
                OracleConnection conn = new OracleConnection(ACE_Conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = S_StoredName;

                //指明参数是输入还是输出型
                for (int i = 0; i < parm.Length; i++)
                {
                    parm[i].Direction = ParameterDirection.Input;
                }
                //传递参数给Oracle命令
                for (int i = 0; i < parm.Length; i++)
                {
                    cmd.Parameters.Add(parm[i]);
                }

                //打开连接
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                cmd.Connection = conn;
                //执行过程
                int I_Result = 0;
                I_Result = cmd.ExecuteNonQuery();
                return I_Result.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>执行存储过程  结果返回到DataTable</summary>
        /// <param name="S_StoredName">过程名</param>
        /// <param name="parm">参数</param>
        /// <param name="I_OutPut">输出参数是第几个</param>
        /// <param name="IsClob">是否输出类别是  Clob</param>
        /// <returns></returns>
        public DataTable ExecP(string S_StoredName, OracleParameter[] parm, int I_OutPut, Boolean IsClob)
        {
            DataTable DT = new DataTable();
            try
            {
                OracleCommand cmd = new OracleCommand();
                OracleConnection conn = new OracleConnection(ACE_Conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = S_StoredName;

                //指明参数是输入还是输出型
                for (int i = 0; i < parm.Length; i++)
                {
                    if (I_OutPut == i)
                    {
                        parm[i].Direction = ParameterDirection.Output;
                    }
                    else
                    {
                        parm[i].Direction = ParameterDirection.Input;
                    }
                }
                //传递参数给Oracle命令
                for (int i = 0; i < parm.Length; i++)
                {
                    cmd.Parameters.Add(parm[i]);
                }

                //打开连接
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                cmd.Connection = conn;

                //执行过程
                int I_Result = 0;
                I_Result = cmd.ExecuteNonQuery();
                //把结果装换成DataTable
                if (I_Result == -1)
                {
                    string S_Result = "";
                    if (IsClob == true)
                    {
                        S_Result = GetOracleClob(parm[I_OutPut].Value as Stream);
                    }
                    else
                    {
                        S_Result = parm[I_OutPut].Value.ToString();
                    }

                    string[] List_Row = S_Result.Split(';');
                    string[] List_Column = List_Row[0].Split(',');

                    for (int i = 0; i < List_Column.Length; i++)
                    {
                        DT.Columns.Add(List_Column[i]);
                    }

                    for (int i = 1; i < List_Row.Length; i++)
                    {
                        DataRow DR = DT.NewRow();
                        string[] List_Value = List_Row[i].Split(',');
                        for (int k = 0; k < List_Value.Length; k++)
                        {
                            DR[k] = List_Value[k].Trim();
                        }
                        DT.Rows.Add(DR);
                    }
                }
                return DT;
            }
            catch (Exception ex)
            {
                DT.Columns.Add("Error");
                DataRow DR = DT.NewRow();
                DR[0] = "Error" + ex.ToString();
                DT.Rows.Add(DR);
                return DT;
            }
        }

        /// <summary>读取 Oracle Clob 数据</summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        public string GetOracleClob(Stream Str)
        {
            int actual = 0;
            string S_Result = "";
            try
            {
                StreamReader streamreader = new StreamReader(Str, Encoding.Unicode);
                char[] cbuffer = new char[100];
                while ((actual = streamreader.Read(cbuffer, 0, cbuffer.Length)) > 0)
                {
                    S_Result += new string(cbuffer, 0, actual);
                }
            }
            catch (Exception ex)
            {
                S_Result = ex.ToString();
            }
            return S_Result;
        }

        /// <summary> 获取某字段值</summary>
        /// <param name="S_Sql"></param>
        /// <param name="S_Field"></param>
        /// <param name="factory">"DACE","VACE"</param>
        /// <returns></returns>
        public string ReadFieldStr(string S_Sql, string S_Field, string factory)
        {
            DataTable DT = Oracle_Data_Table(S_Sql);
            if (DT.Rows.Count > 0)
                return DT.Rows[0][S_Field].ToString();
            else
                return "";
        }

        /// <summary>判断是否是时间</summary>
        /// <param name="S_Str"></param>
        /// <returns></returns>
        public Boolean IsDateTime(string S_Str)
        {
            DateTime DT_Str;
            try
            {
                DT_Str = Convert.ToDateTime(S_Str);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //************************************字符串加密解密 S******************************************
        #region 字符串加密解密 S    
        /// <summary>作用：将字符串内容转化为16进制数据编码 </summary>
        /// <param name="strEncode"></param>
        /// <returns></returns>
        public string Encode(string strEncode)
        {
            string strReturn = "";//  存储转换后的编码
            foreach (short shortx in strEncode.ToCharArray())
            {
                strReturn += shortx.ToString("X2");
            }
            return strReturn;
        }
        /// <summary>作用：将16进制数据编码转化为字符串 </summary>
        /// <param name="strDecode"></param>
        /// <returns></returns>
        public string Decode(string strDecode)
        {
            string sResult = "";
            for (int i = 0; i < strDecode.Length / 2; i++)
            {
                sResult += (char)short.Parse(strDecode.Substring(i * 2, 2), global::System.Globalization.NumberStyles.HexNumber);
            }
            return sResult;
        }
        /// <summary>加密字符串 </summary>
        /// <param name="v_Password"></param>
        /// <param name="v_key"></param>
        /// <returns></returns>
        public string EncryptPassword(string v_Password, string v_key)
        {
            int i, j;
            int a = 0, b = 0, c = 0;
            string hexS = "", hexskey = "", midS = "", tmpstr = "";

            hexS = Encode(v_Password);
            hexskey = Encode(v_key);
            midS = hexS;

            for (i = 1; i <= hexskey.Length / 2; i++)
            {
                if (i != 1)
                {
                    midS = tmpstr;
                }
                tmpstr = "";
                for (j = 1; j <= midS.Length / 2; j++)
                {
                    a = (char)short.Parse(midS.Substring((j - 1) * 2, 2), global::System.Globalization.NumberStyles.HexNumber);
                    b = (char)short.Parse(hexskey.Substring((i - 1) * 2, 2), global::System.Globalization.NumberStyles.HexNumber);

                    //a = (char)short.Parse(Convert.ToString(midS[2 * j - 2]) + Convert.ToString(midS[2 * j-1]), global::System.Globalization.NumberStyles.HexNumber);
                    //b = (char)short.Parse(Convert.ToString(hexskey[2 * i - 2]) + Convert.ToString(hexskey[2 * i-1]), global::System.Globalization.NumberStyles.HexNumber);

                    c = a ^ b;
                    tmpstr += Encode(Convert.ToString((Convert.ToChar(c))));
                }
            }
            return tmpstr;
        }

        /// <summary> 解密字符串</summary>
        /// <param name="v_Password"></param>
        /// <param name="v_key"></param>
        /// <returns></returns>
        public string DecryptPassword(string v_Password, string v_key)
        {
            int i, j;
            int a = 0, b = 0, c = 0;
            string hexS = "", hexskey = "", midS = "", tmpstr = "";

            hexS = v_Password;
            if (hexS.Length % 2 == 1)
            {
                //Response.Write("<script>alert(\"密文错误，无法解密字符串\");</script>");
            }
            hexskey = Encode(v_key);
            tmpstr = hexS;
            midS = hexS;
            for (i = hexskey.Length / 2; i >= 1; i--)
            {
                if (i != hexskey.Length / 2)
                {
                    midS = tmpstr;
                }
                tmpstr = "";
                for (j = 1; j <= midS.Length / 2; j++)
                {
                    a = (char)short.Parse(midS.Substring((j - 1) * 2, 2), global::System.Globalization.NumberStyles.HexNumber);
                    b = (char)short.Parse(hexskey.Substring((i - 1) * 2, 2), global::System.Globalization.NumberStyles.HexNumber);
                    c = a ^ b;
                    tmpstr += Encode(Convert.ToString((Convert.ToChar(c))));
                }
            }
            return Decode(tmpstr);
        }
        #endregion 字符串加密解密 E
        //************************************字符串加密解密 E******************************************




        private void DataGrid_Init()//初始化数据表格
        {
            DataGrid dataGrid1 = new DataGrid();



            #region 居中显示设置
            Style StyleCenter = new Style(typeof(TextBlock));
            StyleCenter.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center));
            #endregion
            double fontSize = 16; //字号大小
            dataGrid1.Columns.Clear();//清除

            #region 生成列->安装调频螺栓
            // 生成第1列
            CheckBox checkBox = new CheckBox();
            //checkBox.Click += Select_All_Events;//全选事件
            dataGrid1.Columns.Add(new DataGridCheckBoxColumn() //添加第1列
            {
                Header = checkBox,//选择框
                Width = 50.0,     //列宽
                //IsReadOnly = false //是否只读
            });

            // 生成第2列
            DataGridTextColumn column = new DataGridTextColumn()//添加第2列
            {
                Header = "点 位",    //列名 
                FontSize = fontSize, //字号
                FontWeight = FontWeights.Bold,//粗体
                                              //FontStyle = FontStyles.Italic ,//斜体

                Width = 100.0,      //列宽
                Binding = new Binding("NO"),//绑定数据
                ElementStyle = StyleCenter  //居中显示
                //IsReadOnly = false //是否只读
            };
            column.Binding.StringFormat = "0000";//显示格式 
            dataGrid1.Columns.Add(column);

            // 生成第3列
            column = new DataGridTextColumn()//添加第3列
            {
                Header = "X轴坐标",  //列名
                FontSize = fontSize, //字号
                Width = 100.0,      //列宽
                Binding = new Binding("X_axis"),//绑定数据
                ElementStyle = StyleCenter      //居中显示
                //IsReadOnly = false //是否只读
            };
            column.Binding.StringFormat = "0.00";//显示格式          
            dataGrid1.Columns.Add(column);

            // 生成第4列
            column = new DataGridTextColumn()//添加第4列
            {
                Header = "Y轴坐标",  //列名 
                FontSize = fontSize, //字号
                Width = 100.0,      //列宽
                Binding = new Binding("Y_axis"),//绑定数据
                ElementStyle = StyleCenter      //居中显示
                //IsReadOnly = false //是否只读
            };
            column.Binding.StringFormat = "0.00";//显示格式
            dataGrid1.Columns.Add(column);


            if (false)
            {
                // 生成第5列
#pragma warning disable CS0162 // 检测到无法访问的代码
                column = new DataGridTextColumn()//添加第4列
#pragma warning restore CS0162 // 检测到无法访问的代码
                {
                    Header = "小圆半径", //列名 
                    FontSize = fontSize, //字号
                    Width = 100.0,       //列宽
                    Binding = new Binding("R_Min"),//绑定数据
                    ElementStyle = StyleCenter      //居中显示
                                                    //IsReadOnly = false //是否只读
                };
                column.Binding.StringFormat = "0.00";//显示格式
                dataGrid1.Columns.Add(column);

                // 生成第5列
                column = new DataGridTextColumn()//添加第4列
                {
                    Header = "大圆半径",    //列名 
                    FontSize = fontSize, //字号
                    Width = 100.0,       //列宽
                    Binding = new Binding("R_Max"),//绑定数据
                    ElementStyle = StyleCenter      //居中显示
                                                    //IsReadOnly = false //是否只读
                };
                column.Binding.StringFormat = "00";//显示格式
                dataGrid1.Columns.Add(column);
            }
            if (true)
            {
                // 生成第5列
                column = new DataGridTextColumn()//添加第4列
                {
                    Header = "锁附深度", //列名 
                    FontSize = fontSize, //字号
                    Width = 100.0,       //列宽
                    Binding = new Binding("Detection_height"),//绑定数据
                    ElementStyle = StyleCenter      //居中显示
                                                    //IsReadOnly = false //是否只读
                };
                column.Binding.StringFormat = "0.00";//显示格式
                dataGrid1.Columns.Add(column);

                // 生成第5列
                column = new DataGridTextColumn()//添加第4列
                {
                    Header = "料 号",    //列名 
                    FontSize = fontSize, //字号
                    Width = 100.0,       //列宽
                    Binding = new Binding("Item_No"),//绑定数据
                    ElementStyle = StyleCenter      //居中显示
                                                    //IsReadOnly = false //是否只读
                };
                column.Binding.StringFormat = "00";//显示格式
                dataGrid1.Columns.Add(column);
            }
            #endregion


        }



    }



    /*

    //需要添加引用：
    //类型库：
    //Microsoft Excel 15.0 Object Library
    //使用命名空间：
    using System.Reflection;
    using Excel = Microsoft.Office.Interop.Excel;

    #region ExportToExcel 导出为Excel
    /// <summary>
    /// 导出Excel类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class ExportToExcel<T, U>
        where T : class
        where U : List<T>
    {
        public List<T> DataToPrint;


        // Excel 对象实例.
        private Excel.Application _excelApp = null;
        private Excel.Workbooks _books = null;
        private Excel._Workbook _book = null;
        private Excel.Sheets _sheets = null;
        private Excel._Worksheet _sheet = null;
        private Excel.Range _range = null;
        private Excel.Font _font = null;

        // 可选 参数
        private object _optionalValue = System.Reflection.Missing.Value;
        /// <summary>
        /// 生成报表，和其他功能
        /// </summary>
        /// <returns></returns>
        public int GenerateReport()
        {
            int result = 1;
            try
            {
                if (DataToPrint != null)
                {
                    if (DataToPrint.Count != 0)
                    {
                        CreateExcelRef();
                        FillSheet();
                        OpenReport();
                    }
                }
            }
            catch (Exception e)
            {
                result = 0;
                //("Excel导出失敗！\n", e.Message);
            }
            finally
            {
                ReleaseObject(_sheet);
                ReleaseObject(_sheets);
                ReleaseObject(_book);
                ReleaseObject(_books);
                ReleaseObject(_excelApp);
            }
            return result;
        }
        /// <summary>
        /// 展示 Excel 程序
        /// </summary>
        private void OpenReport()
        {
            _excelApp.Visible = true;
        }
        /// <summary>
        /// 填充 Excel sheet
        /// </summary>
        private void FillSheet()
        {
            object[] header = CreateHeader();
            WriteData(header);
        }
        /// <summary>
        /// 将数据写入 Excel sheet
        /// </summary>
        /// <param name="header"></param>
        private void WriteData(object[] header)
        {
            object[,] objData = new object[DataToPrint.Count, header.Length];

            for (int j = 0; j < DataToPrint.Count; j++)
            {
                var item = DataToPrint[j];
                for (int i = 0; i < header.Length; i++)
                {
                    var y = typeof(T).InvokeMember(header[i].ToString(),
                    BindingFlags.GetProperty, null, item, null);
                    objData[j, i] = (y == null) ? "" : y.ToString();
                }
            }
            AddExcelRows("A2", DataToPrint.Count, header.Length, objData);
            AutoFitColumns("A1", DataToPrint.Count + 1, header.Length);
        }
        /// <summary>
        /// 根据数据拟合 列
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        private void AutoFitColumns(string startRange, int rowCount, int colCount)
        {
            _range = _sheet.get_Range(startRange, _optionalValue);
            _range = _range.get_Resize(rowCount, colCount);
            _range.Columns.AutoFit();
        }
        /// <summary>
        /// 根据属性名创建列标题
        /// </summary>
        /// <returns></returns>
        private object[] CreateHeader()
        {
            PropertyInfo[] headerInfo = typeof(T).GetProperties();

            // 为 标头 创建 Array
            // 开始从 A1 处添加
            List<object> objHeaders = new List<object>();
            for (int n = 0; n < headerInfo.Length; n++)
            {
                objHeaders.Add(headerInfo[n].Name);
            }

            var headerToAdd = objHeaders.ToArray();
            AddExcelRows("A1", 1, headerToAdd.Length, headerToAdd);
            SetHeaderStyle();

            return headerToAdd;
        }
        /// <summary>
        /// 列标题设置为加粗字体
        /// </summary>
        private void SetHeaderStyle()
        {
            _font = _range.Font;
            _font.Bold = true;
        }
        /// <summary>
        /// 添加行
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        /// <param name="values"></param>
        private void AddExcelRows(string startRange, int rowCount,
        int colCount, object values)
        {
            _range = _sheet.get_Range(startRange, _optionalValue);
            _range = _range.get_Resize(rowCount, colCount);
            _range.set_Value(_optionalValue, values);
        }
        /// <summary>
        /// 创建 Excel 传递的参数实例
        /// </summary>
        private void CreateExcelRef()
        {
            _excelApp = new Excel.Application();
            _books = (Excel.Workbooks)_excelApp.Workbooks;
            _book = (Excel._Workbook)(_books.Add(_optionalValue));
            _sheets = (Excel.Sheets)_book.Worksheets;
            _sheet = (Excel._Worksheet)(_sheets.get_Item(1));
        }
        /// <summary>
        /// 释放未使用的对象
        /// </summary>
        /// <param name="obj"></param>
        private void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                //MessageBox.Show(ex.Message.ToString());

            }
            finally
            {
                GC.Collect();
            }
        }
    }
    #endregion 
    
     /// <summary>
        /// 导出为Excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            Common.ExportToExcel<JFEntity.LineInfo, List<JFEntity.LineInfo>> exporttoexcel = 
                new Common.ExportToExcel<JFEntity.LineInfo, List<JFEntity.LineInfo>>();
            //实例化exporttoexcel对象
            exporttoexcel.DataToPrint = (List<JFEntity.LineInfo>)dgtOnlineRecord.ItemsSource;
            exporttoexcel.GenerateReport();

        }
 
     */







}
