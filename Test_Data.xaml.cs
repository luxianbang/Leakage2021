using Leakage_Lib;
using Microsoft.Win32;
using System;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Leakage2021
{
    /// <summary>
    /// Data_Win.xaml 的交互逻辑
    /// </summary>
    public partial class Test_Data : Page
    {

        //static string Model = RecordInfo.PROG_Model, ACE_SN = RecordInfo.SN, USERID = RecordInfo.User_ID, USERNM = RecordInfo.User_Name;
        //static string LINE = RecordInfo.Line_Num, OPER = RecordInfo.OREP, Date = RecordInfo.Test_Date, Time = RecordInfo.Test_HHMM;


        private void 写_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG

            //Debug.Assert(true);
            //Debug.Write("reerhehhrjhrjrjr");
            //Debug.WriteLine("352463735");

            //MessageBox.Show("DEBUG");

            try
            {

              
                Leak.ModbusTcp_Read_Set_Para();

                string sql = "SELECT * FROM AMES.W88IPWTM WHERE ROWNUM <= 10 \n";
                //DB.Oracle_Data_Table(sql);
                //DB.ExecSql(sql,"DACE",false);

                //alter session set nls_language = american

                //gg();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            //DataTable DT = DB.DG_DT(Leak_DG);
            //DT_Access();


#endif

        }


        private void gg()
        {
            try
            {
                MC_inf.MC_Num = "TEST";
                DB.Factory_Name = "DACE";
                SYS_Set.Factory_Code = "3000";
                REC_inf.SN = "5701481700181";//

                REC_inf.SN = "SX61AM06757";
                REC_inf.OPER = "3520";
                TestData.Result = 2;

                string 充气时间 = "30";
                string 检测时间 = "60";
                string 充气压力 = "48.7";
                string Leak_Pa = "32.1";
                string Leak_CCM = "1.22";
                string Result = "-NG";//“PASS”, “+NG”, “-NG”, “Big leak”, “Tank Air H”, “Tank Air L”, “Pressure H”, “Pressure L”, “Out of range”

                string[] LeackData = { 充气时间, 检测时间, 充气压力, Leak_Pa, Leak_CCM, Result };
                DB.Leak_Data_FACRST(LeackData, DB.Factory_Name, false);

                if (DB.Leak_Data_WIPWTM(DB.Factory_Name))
                {
                    MessageBox.Show("写入成功!");
                }
                return;

                //LeackData
                REC_inf.SN = "TEST";
                REC_inf.Line_Num = "TEST";
                REC_inf.OPER = "TEST";
                REC_inf.Model = "TEST";
                REC_inf.User_ID = "TEST";

               

                this.Cursor = Cursors.Wait;//等待鼠标

                DB.Leak_Data_WIPWTM(DB.Factory_Name);


                if (DB.Leak_Data_FACRST(LeackData, DB.Factory_Name, false))
                {
                    MessageBox.Show("写入成功!");
                }
                this.Cursor = Cursors.Arrow;//头鼠标
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { this.Cursor = Cursors.Arrow; /*头鼠标*/} //finally 块始终都会运行

        }


        private void TEST_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            try
            {
               
                string sql = "", SN;
                DataTable DT = null;


                // 5701141900008


                sql = "SELECT \n";
                sql += " FACILITY, LOT_NUMBER, SEQ_NUM, ALIAS_LOT_NUMBER, OPER, \n";
                sql += "                          PRODDT, ENTITY, ORDER_NO, PROD, E_OPER,\n";
                sql += "                          STATUS, FLAG, EQUIP_ID, DUE_TIME, USERID, \n";
                sql += "                          RESULT, WORKSHIFTCD, DEFECTGB, DEFECTCD \n";

                sql += " FROM AERP.WIPWTM \n";
                sql += "      WHERE OPER = '3520' \n";
                sql += "      AND ROWNUM <= 20  \n";


                DB.Factory_Name = "DACE";
                SYS_Set.Factory_Code = "3000";
                REC_inf.OPER = "3700";
                sql = "SELECT * FROM AMES.WIPWTM WHERE ROWNUM <= 10 \n";

                REC_inf.SN = "5701141900008";//
                REC_inf.SN = "2021000208900";
                //Console.WriteLine("");
                sql = "SELECT ACESN as ACESN, CUSTSN as CUSTSN, ORDERNO as ORDERNO,PROD as PROD, RWRKFG as RWRKFG \n";
                sql += "    FROM AMES.WIPLOT \n";
                sql += "        WHERE ACESN = '" + REC_inf.SN + "' \n";
                sql += "            OR CUSTSN = '" + REC_inf.SN + "' \n";

                sql = "SELECT * FROM AERP.WIPLOT WHERE OPER='3520' AND RWRK_FG='N' ORDER BY LAST_TRAN_TIME DESC ROWNUM <= 100\n";//
                sql = "SELECT * FROM (SELECT * FROM AMES.WIPLOT ORDER BY UPDATEDT DESC)  WHERE OPER='3700' AND ROWNUM <= 50\n";//最后10条记录并按降序排列 


                sql = "SELECT USERID, USERNM \n" //工厂号，用户ID，密码，名字，工序号，权限
                                + "        FROM AERP.USERINFO WHERE ROWNUM <= 30 \n";


                //sql = "alter system set nls_language='AMERICAN' scope=spfile nls_language";//可以更改数据库服务器字符集和客户端字符集环境
                //sql = "alter session set nls_language='SIMPLIFIED CHINESE'";//可以更改会话字符集环境
                //sql = "alter session set nls_language='AMERICAN'";//可以更改会话字符集环境
                //DB.ExecSql(sql, "DACE", false);

                //sql = "select * from nls_database_parameters";//数据库服务器字符集  KO16KSC5601   VACE AMERICAN
                //sql = "select * from nls_instance_parameters";//客户端字符集环境  AMERICAN             AMERICAN
                sql = "select* from nls_session_parameters";//会话字符集环境  SIMPLIFIED CHINESE       SIMPLIFIED CHINESE





                //sql = "SELECT SYSDATE FROM DUALW";
                DT = DB.Oracle_Data_Table(sql);

                //Console.WriteLine(DT.Columns[20].ColumnName);  

                //DataSet DS = new DataSet();
                //Leak_DG.ItemsSource = (DS.Tables[0] as DataTable).DefaultView;

                //DataTable Newdt = new DataTable();
                //Newdt.Columns.Add("USERID", typeof(string));
                //Newdt.Columns.Add("USERNM", typeof(string));

                //for (int i = 0; i < DT.Rows.Count; i++)
                //{
                //    DataRow row = DT.Rows[i];//
                //    DataRow rowNew = Newdt.NewRow();
                //    rowNew["USERID"] = row["USERID"];
                //    rowNew["USERNM"] = DB.Ksc_GB18030( row["USERNM"].ToString());
                //    Newdt.Rows.Add(rowNew);
                //}

                //DT = Newdt;



                Leak_DG.IsReadOnly = false;
                Leak_DG.AutoGenerateColumns = true;//这个要开启
                Leak_DG.ItemsSource = DT.DefaultView;//绑定

                return;



                DT.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Cursor = Cursors.Arrow;//头鼠标
#endif
        }

        public Test_Data()
        {
            InitializeComponent();
            Language_int(SYS_Set.LANG);//语言初始化

            查询_USER.Text = REC_inf.User_ID;

            // 隔行背景变色
            Leak_DG.AlternationCount = 2;

            //取现在日期
            Start_Date.SelectedDate = DateTime.Now;
            End_Date.SelectedDate = DateTime.Now;

            AERP.IsChecked = !DB.Local_login;
#if DEBUG
            //TEST_R.Visibility = Visibility.Visible;
            //TEST_W.Visibility = Visibility.Visible;

#endif
        }

        //条件查询数据
        private void 条件查询_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string FACILITY = SYS_Set.Factory_Code;
                string sql = "", StartDate = "", EndDate = "";
                DataTable DT = null;

                if (Start_Date.SelectedDate == null || End_Date.SelectedDate == null)
                {
                    Start_Date.SelectedDate = DateTime.Now;
                    End_Date.SelectedDate = DateTime.Now;
                }
                if (Start_Date.SelectedDate != null && End_Date.SelectedDate != null)
                {
                    if (Start_Date.SelectedDate > End_Date.SelectedDate)
                        End_Date.SelectedDate = Start_Date.SelectedDate;
                    DateTime StartDateTime = (DateTime)Start_Date.SelectedDate;
                    DateTime EndDateTime = (DateTime)End_Date.SelectedDate;
                    StartDate = StartDateTime.ToString("yyyyMMdd");// 20210512
                    EndDate = EndDateTime.ToString("yyyyMMdd");// 20210513
                }
                //Console.WriteLine("StartDate: " + StartDate);
                //Console.WriteLine("EndDate: "   + EndDate  );

                this.Cursor = Cursors.Wait;//等待鼠标
                if ((bool)AERP.IsChecked)
                {
                    if (DB.Factory_Name == "DACE")
                    {
                        //Oracle数据库
                        sql = "SELECT ROWNUM as ID,\n";
                        //sql = "SELECT ROW_NUMBER() OVER(ORDER BY TXTVAL4 DESC, TXTVAL5 DESC) as NO_,\n";// 排序  日期，时间降序 + 编号
                        sql += "TXTVAL1 as MODEL, LOT_NUMBER as SN, TXTVAL2 as USER_, TXTVAL3 as LINE, OPER, TXTVAL4 as DATE_, TXTVAL5 as TIME \n";
                        sql += "     , NUMVAL1 as InAirTime, NUMVAL2 as CheckTime, NUMVAL3 as InAir, NUMVAL4 as Leak, NUMVAL5 as CCM, TXTVAL6 as Result\n";
                        sql += "              FROM AERP.FACRST \n";
                        sql += "                    WHERE FACILITY = '" + FACILITY + "'\n";
                        if (Start_Date.SelectedDate != null && End_Date.SelectedDate != null)
                            sql += "                         AND TXTVAL4 >= '" + StartDate + "' AND TXTVAL4 <=  '" + EndDate + "' \n";
                        if (查询_MODEL.Text != "")
                            sql += "                         AND  TXTVAL1 LIKE '" + 查询_MODEL.Text.Trim().ToUpper() + "%'\n";
                        if (查询_USER.Text != "")
                            sql += "                         AND  TXTVAL2 = '" + 查询_USER.Text.Trim().ToUpper() + "'\n";
                        if ((bool)PASS.IsChecked && !(bool)FAIL.IsChecked)
                            sql += "                         AND  TXTVAL6 = 'PASS'\n";
                        if (!(bool)PASS.IsChecked && (bool)FAIL.IsChecked)
                            sql += "                         AND  TXTVAL6 <> 'PASS'\n";
                        if (!(bool)PASS.IsChecked && !(bool)FAIL.IsChecked)
                            sql += "                         AND  TXTVAL6 = 'ASIS'\n";//不返回数据
                        sql += "ORDER BY TXTVAL4 DESC, TXTVAL5 DESC \n";// 排序  日期，时间降序 + 编号
                    }
                    else
                    {
                        //Oracle数据库
                        sql = "SELECT ROWNUM as ID,\n";
                        sql += "TXTVAL1 as MODEL, ACESN as SN, TXTVAL2 as USER_, TXTVAL3 as LINE, OPER, TXTVAL4 as DATE_, TXTVAL5 as TIME \n";
                        sql += "     , NUMVAL1 as InAirTime, NUMVAL2 as CheckTime, NUMVAL3 as InAir, NUMVAL4 as Leak, NUMVAL5 as CCM, TXTVAL6 as Result\n";
                        sql += "              FROM AMES.FACRST \n";
                        sql += "                    WHERE FACILITY = '" + FACILITY + "'\n";
                        if (Start_Date.SelectedDate != null && End_Date.SelectedDate != null)
                            sql += "                         AND TXTVAL4 >= '" + StartDate + "' AND TXTVAL4 <=  '" + EndDate + "' \n";
                        if (查询_MODEL.Text != "")
                            sql += "                         AND  TXTVAL1 LIKE '" + 查询_MODEL.Text.Trim().ToUpper() + "%'\n";
                        if (查询_USER.Text != "")
                            sql += "                         AND  TXTVAL2 = '" + 查询_USER.Text.Trim().ToUpper() + "'\n";
                        if ((bool)PASS.IsChecked && !(bool)FAIL.IsChecked)
                            sql += "                         AND  TXTVAL6 = 'PASS'\n";
                        if (!(bool)PASS.IsChecked && (bool)FAIL.IsChecked)
                            sql += "                         AND  TXTVAL6 <> 'PASS'\n";
                        if (!(bool)PASS.IsChecked && !(bool)FAIL.IsChecked)
                            sql += "                         AND  TXTVAL6 = 'ASIS'\n";//不返回数据
                        sql += "ORDER BY TXTVAL4 DESC, TXTVAL5 DESC \n";// 排序  日期，时间降序 + 编号
                    }
                    DT = DB.Oracle_Data_Table(sql);
                }
                else
                {
                    //本机数据库 != 要用 <>
                    sql = "SELECT \n";
                    sql += " [ID], [MODEL], [SN], [USER] as USER_, [LINE], [OPER], [DATE] as DATE_, [TIME], [InAirTime], [CheckTime], [InAir], [Leak], [CCM], [Result] \n";
                    sql += "            FROM FACRST \n";
                    sql += "                    WHERE ID > 0 \n";
                    if (Start_Date.SelectedDate != null && End_Date.SelectedDate != null)
                        sql += "                         AND   DATE >= '" + StartDate + "' AND DATE <=  '" + EndDate + "' \n";
                    if (查询_MODEL.Text != "")
                        sql += "                         AND  MODEL LIKE '" + 查询_MODEL.Text.Trim().ToUpper() + "%'\n";
                    if (查询_USER.Text != "")
                        sql += "                         AND  USER = '" + 查询_USER.Text.Trim().ToUpper() + "'\n";
                    if ((bool)PASS.IsChecked && !(bool)FAIL.IsChecked)
                        sql += "                         AND  Result = 'PASS'\n";
                    if (!(bool)PASS.IsChecked && (bool)FAIL.IsChecked)
                        sql += "                         AND  Result <> 'PASS'\n";
                    if (!(bool)PASS.IsChecked && !(bool)FAIL.IsChecked)
                        sql += "                         AND Result = 'ASIS'\n";//不返回数据
                    sql += "        ORDER BY DATE DESC, TIME DESC \n";// 排序  日期，时间降序 + 编号

                    DT = DB.Access_Data_Table(sql);
                }

                this.Cursor = Cursors.Arrow;//头鼠标

                Display_data(DT);
                MessageBox.Show("查找到:" + DT.Rows.Count + "条记录");
                DT.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Cursor = Cursors.Arrow;//头鼠标
        }
        // 按条码查询
        private void SN_TextBoxn_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataTable DT;
            string FACILITY = SYS_Set.Factory_Code, sql;

            try
            {
                if (e != null && SN_TextBoxn.Text.Length < 3 && e.Key == Key.Enter)
                    throw new Exception("请正确输入条码!");

                else if (e == null || e.Key == Key.Enter)
                {
                    this.Cursor = Cursors.Wait;//等待鼠标
                    if ((bool)AERP.IsChecked)
                    {
                        if (DB.Factory_Name == "DACE")
                        {
                            //Oracle数据库 DACE AERP
                            sql = "SELECT ROWNUM as NO,\n";
                            //sql = "SELECT ROW_NUMBER() OVER(ORDER BY TXTVAL4 DESC, TXTVAL5 DESC) as NO_,\n";// 排序  日期，时间降序 + 编号
                            sql += "TXTVAL1 as MODEL, LOT_NUMBER as SN, TXTVAL2 as USER_, TXTVAL3 as LINE, OPER, TXTVAL4 as DATE_, TXTVAL5 as TIME \n";
                            sql += "     , NUMVAL1 as InAirTime, NUMVAL2 as CheckTime, NUMVAL3 as InAir, NUMVAL4 as Leak, NUMVAL5 as CCM, TXTVAL6 as Result\n";
                            sql += "              FROM AERP.FACRST\n";
                            sql += "                    WHERE FACILITY = '" + FACILITY + "'\n";
                            sql += "                         AND LOT_NUMBER LIKE '" + SN_TextBoxn.Text + "%'";
                            sql += "ORDER BY TXTVAL4 DESC, TXTVAL5 DESC \n";// 排序  日期，时间降序 + 编号

                        }
                        else // VACE
                        {
                            //Oracle数据库 VACE AMES
                            sql = "SELECT ROWNUM as NO,\n";
                            sql += "TXTVAL1 as MODEL, ACESN as SN, TXTVAL2 as USER_, TXTVAL3 as LINE, OPER, TXTVAL4 as DATE_, TXTVAL5 as TIME \n";
                            sql += "     , NUMVAL1 as InAirTime, NUMVAL2 as CheckTime, NUMVAL3 as InAir, NUMVAL4 as Leak, NUMVAL5 as CCM, TXTVAL6 as Result \n";
                            sql += "              FROM AMES.FACRST \n";
                            sql += "                    WHERE FACILITY = '" + FACILITY + "'\n";
                            sql += "                         AND ACESN LIKE '" + SN_TextBoxn.Text + "%'";
                            sql += "ORDER BY TXTVAL4 DESC, TXTVAL5 DESC \n";// 排序  日期，时间降序 + 编号
                        }

                        DT = DB.Oracle_Data_Table(sql);
                    }
                    else
                    { //本机数据库 != 要用 <>
                        sql = "SELECT \n";
                        sql += " [ID], [MODEL], [SN], [USER] as USER_, [LINE], [OPER], [DATE] as DATE_, [TIME], [InAirTime], [CheckTime], [InAir], [Leak], [CCM], [Result] \n";
                        sql += "            FROM FACRST \n";
                        sql += "                    WHERE SN LIKE '" + SN_TextBoxn.Text + "%'";
                        sql += "        ORDER BY DATE DESC, TIME DESC \n";// 排序  日期，时间降序 + 编号
                        DT = DB.Access_Data_Table(sql);
                    }
                    this.Cursor = Cursors.Arrow;//头鼠标

                    AERP.Focus();
                    Leak_DG.ItemsSource = null;
                    if (DT.Rows.Count > 0)
                        Display_data(DT);
                    else
                    {
                        if (e != null)
                            MessageBox.Show("没找到数据!");
                        SN_TextBoxn.Focus();
                        SN_TextBoxn.SelectAll();
                    }
                    DT.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { this.Cursor = Cursors.Arrow;/*头鼠标*/ } //finally 块始终都会运行
        }

        //显示测试结果数据
        public void Display_data(DataTable DT)
        {
            try
            {
                GC.Collect();
                if (DT == null)
                    return;
                //新表列名 CN
                DataTable Newdt = new DataTable();
                if (SYS_Set.LANG == "CN")//CN
                {
                    Newdt.Columns.Add("序号", typeof(string));
                    Newdt.Columns.Add("型号", typeof(string));
                    Newdt.Columns.Add("产品编号", typeof(string));
                    Newdt.Columns.Add("作业者", typeof(string));
                    Newdt.Columns.Add("拉线", typeof(string));
                    Newdt.Columns.Add("工序", typeof(string));
                    Newdt.Columns.Add("日期", typeof(string));
                    Newdt.Columns.Add("时间", typeof(string));
                    Newdt.Columns.Add("充气时间", typeof(string));
                    Newdt.Columns.Add("测试时间", typeof(string));
                    Newdt.Columns.Add("充气压力", typeof(string));
                    Newdt.Columns.Add("泄漏(Pa)", typeof(string));
                    Newdt.Columns.Add("泄漏(ccm)", typeof(string));
                    Newdt.Columns.Add("结果", typeof(string));

                    //复制到新表
                    int no = 0;
                    for (var i = 0; i < DT.Rows.Count; i++)
                    {
                        DataRow row = DT.Rows[i];//
                        DataRow rowNew = Newdt.NewRow();
                        no++;//序号
                        rowNew["序号"] = no.ToString();//
                        rowNew["型号"] = row["MODEL"];
                        rowNew["产品编号"] = row["SN"];
                        rowNew["作业者"] = row["USER_"];
                        rowNew["拉线"] = row["LINE"];
                        rowNew["工序"] = row["OPER"];
                        rowNew["日期"] = row["DATE_"];
                        rowNew["时间"] = row["TIME"];
                        rowNew["充气时间"] = row["InAirTime"];
                        rowNew["测试时间"] = row["CheckTime"];
                        rowNew["充气压力"] = row["InAir"];
                        rowNew["泄漏(Pa)"] = row["Leak"];
                        rowNew["泄漏(ccm)"] = row["CCM"];
                        rowNew["结果"] = row["Result"];
                        Newdt.Rows.Add(rowNew);
                    }
                }
                else //VACE EN
                {
                    Newdt.Columns.Add("NO", typeof(string));
                    Newdt.Columns.Add("MODEL", typeof(string));
                    Newdt.Columns.Add("SN", typeof(string));
                    Newdt.Columns.Add("USER", typeof(string));
                    Newdt.Columns.Add("LINE", typeof(string));
                    Newdt.Columns.Add("OPER", typeof(string));
                    Newdt.Columns.Add("DATE", typeof(string));
                    Newdt.Columns.Add("TIME", typeof(string));
                    Newdt.Columns.Add("InAirTime", typeof(string));
                    Newdt.Columns.Add("CheckTime", typeof(string));
                    Newdt.Columns.Add("InAir(Kpa)", typeof(string));
                    Newdt.Columns.Add("Leak(Pa)", typeof(string));
                    Newdt.Columns.Add("CCM", typeof(string));
                    Newdt.Columns.Add("Result", typeof(string));

                    //复制到新表
                    int no = 0;
                    for (var i = 0; i < DT.Rows.Count; i++)
                    {
                        DataRow row = DT.Rows[i];//
                        DataRow rowNew = Newdt.NewRow();
                        no++;//序号
                        rowNew["NO"] = no.ToString();//
                        rowNew["MODEL"] = row["MODEL"];
                        rowNew["SN"] = row["SN"];
                        rowNew["USER"] = row["USER_"];
                        rowNew["LINE"] = row["LINE"];
                        rowNew["OPER"] = row["OPER"];
                        rowNew["DATE"] = row["DATE_"];
                        rowNew["TIME"] = row["TIME"];
                        rowNew["InAirTime"] = row["InAirTime"];
                        rowNew["CheckTime"] = row["CheckTime"];
                        rowNew["InAir(Kpa)"] = row["InAir"];
                        rowNew["Leak(Pa)"] = row["Leak"];
                        rowNew["CCM"] = row["CCM"];
                        rowNew["Result"] = row["Result"];
                        Newdt.Rows.Add(rowNew);
                    }
                }
                DT.Dispose();

                Leak_DG.IsReadOnly = false; //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                Leak_DG.ItemsSource = null;
                Leak_DG.AutoGenerateColumns = true;//这个要开启              
                Leak_DG.ItemsSource = Newdt.DefaultView;//绑定
                Newdt.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Cursor = Cursors.Arrow;//头鼠标
        }

        //导出到EXCEL
        private void 导出_EXCEL_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;//等待鼠标
            bool state = DB.TO_EXCEL(Leak_DG, REC_inf.Model);
            this.Cursor = Cursors.Arrow;//头鼠标

            if (state)
                MessageBox.Show("导出数据成功!");
        }
        private void 导入_EXCEL_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog
            {
                //Title="打开Dxf或csv文件",//标题文本
                //AddExtension = true,//文件名添加扩展名
                FileName = string.Empty,
                FilterIndex = 1,
                Multiselect = false,
                RestoreDirectory = true,//还原目录
                DefaultExt = "dxf",//默认扩展名字符串
                                   //InitialDirectory = path,//初始目录
                Filter = "配方文件|*.xlsx;*.xls|所有文件|*.*",//
            };
            if (openFile.ShowDialog() == false)//打开对话框
                return;

            var table = DB.EXCEL_DT(openFile.FileName, null);

            Leak_DG.IsReadOnly = true;//只读
            Leak_DG.AutoGenerateColumns = true;//这个要开启
            Leak_DG.ItemsSource = table.DefaultView;//绑定
            MessageBox.Show("共导入:" + table.Rows.Count + "条记录");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SN_TextBoxn.Text = REC_inf.SN;

            if (SN_TextBoxn.Text.Length > 3)
            {
                Leak_DG.ItemsSource = null;
                SN_TextBoxn_PreviewKeyDown(null, null);
            }
        }


        //语言初始化
        private void Language_int(string language)
        {
            if (language == "CN")
            {
                查询_Label.Content = "查询\n条件";
                MODEL_Label.Content = "产品型号";
                USER_Label.Content = "操作者";
                Start_Date_Label.Content = "开始日期";
                End_Date_Label.Content = "结束日期";
                SN_Label.Content = "查询条码";

                条件查询_BT.Content = "查 询";
                //导出_EXCEL.Content = "导出EXCEL";
                导入_EXCEL.Content = "导入EXCEL";
            }
            else
            {
                查询_Label.Content = "Query\nCriteria";
                MODEL_Label.Content = "MODEL";
                USER_Label.Content = "USER";
                Start_Date_Label.Content = "Start_Date";
                End_Date_Label.Content = "End_Date";
                SN_Label.Content = "ACE_SN";

                条件查询_BT.Content = "QUERY";
                //导出_EXCEL.Content = "TO_EXCEL";
                导入_EXCEL.Content = "IN_EXCEL";
            }
        }



        //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //Leak_DG.ItemsSource = null;
        }

        private void 弹窗_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Window();
            //NewPage n = new NewPage();
            //w.Content = n;
            w.ShowDialog();
        }
    }



}
