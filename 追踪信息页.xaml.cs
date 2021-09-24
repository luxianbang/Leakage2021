using Leakage_Lib;
using Microsoft.Win32;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Leakage2021
{
    /// <summary>
    /// Data_Win.xaml 的交互逻辑
    /// </summary>
    public partial class 追踪信息页 : Page
    {

        public 追踪信息页()
        {
            InitializeComponent();
            Language_int(SYS_Set.LANG);//语言初始化

            // 隔行背景变色
            inf_DG.AlternationCount = 2;

            工厂代码_TextBoxn.Text = SYS_Set.Factory_Code;
            工厂_TextBoxn.Text = DB.Factory_Name;
        }


        //显示测试结果数据
        public void Display_data(DataTable DT)
        {
            try
            {
                inf_DG.ItemsSource = null;
                if (DT == null)
                    return;

                // DT = DB.更改列名加序号(DT);//变更列名

                inf_DG.IsReadOnly = true;
                inf_DG.AutoGenerateColumns = true;//这个要开启              
                inf_DG.ItemsSource = DT.DefaultView;//绑定
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
            bool state = DB.TO_EXCEL(inf_DG, REC_inf.Model);
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

            inf_DG.IsReadOnly = true;//只读
            inf_DG.AutoGenerateColumns = true;//这个要开启
            inf_DG.ItemsSource = table.DefaultView;//绑定
            MessageBox.Show("共导入:" + table.Rows.Count + "条记录");
        }

        //要实现绑定到变量，必须实现INotifyPropertyChanged  
        public class WinData : System.ComponentModel.INotifyPropertyChanged //属性更改接口
        {
            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;//属性更改事件

            private Brush back_Colour = Brushes.Red;
            public Brush Back_Colour
            {
                //获取值时将私有字段传出
                get { return back_Colour; }
                set
                {
                    back_Colour = value;//赋值时将值传给私有字段 
                    OnPropertyChanged("Back_Colour"); //一旦执行了赋值操作说明其值被修改了，则立马通过INotifyPropertyChanged接口告诉UI(IntValue)被修改了
                }
            }
            private void OnPropertyChanged(string propertyName)//通知Binding 属性的值改变了
            {
                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }


        private void DACE_追踪产品信息()
        {
            DataTable DT, Newdt;
            string sql, FACY = 工厂代码_TextBoxn.Text, SN = SN_TextBoxn.Text;

            try
            {
                if (SN_TextBoxn.Text.Length < 3)
                    throw new Exception("请正确输入条码!");
                #region SQL
                sql = "SELECT \n"; /*+FIRST_ROWS*/
                sql += "     A.LOT_NUMBER ACESN, A.ALIAS_LOT_NUMBER CUSTSN, A.PROD, \n"; //--ACE条码, 客户码, 产品名
                sql += "     DECODE (RESULT1, 'PARTS', (SELECT MAT_DESC FROM BOMMAT WHERE FACILITY = '" + FACY + "' AND A.PROD = MAT_ID),(SELECT PRD_DESC FROM WIPPRD WHERE FACILITY = '" + FACY + "' AND A.PROD = PROD)) MODEL, \n"; //--机种型号  decode(条件, 值1, 返回值1, 值2, 返回值2,…值n, 返回值n, 缺省值)
                sql += "     A.OPER, \n";//--工序号
                sql += "    (SELECT OPR_DESC FROM WIPOPR WHERE FACILITY = '" + FACY + "' AND OPER = A.OPER) OPER_Name, \n"; //--工序名
                sql += "     A.ENTITY LINE, A.TRAN_TIME DATE_TIME, A.OPERATOR USERID, \n";//----拉线号, 登陆时间, 操作者ID
                sql += "    (SELECT DISTINCT USERNM FROM USERINFO WHERE COMPANYCD = '" + FACY + "' AND USERGB = '2' AND USERID=A.OPERATOR) USER_NAME, \n";//--操作者名字
                sql += "     A.RESULT1, A.RESULT2,A.EQUIP_ID MC_NUM, \n";//--结果1, 结果2, 设备编号
                sql += "     (SELECT FACNAME FROM AMSEQS WHERE FACNO=A.EQUIP_ID AND OPER=A.OPER) EQU_DESC, \n";//--设备型号
                sql += "     NUMVAL3 Kpa, NUMVAL4 Pa, NUMVAL5 CCM, TXTVAL6 RESULT3 \n";//--CCM,气密结果,Kpa,Pa
                sql += "  FROM ( \n";
                sql += "           SELECT \n";
                sql += "                    A.LOT_NUMBER, HIST_SEQ, A.ALIAS_LOT_NUMBER, PROD, A.OPER, \n";
                sql += "                    TRAN_TIME, OPERATOR, ENTITY, \n";
                sql += "                    DECODE (RESULT_FG, 'P', 'Pass', 'S', 'Fail') RESULT1, \n";
                sql += "                    DECODE (RWRK_FG, 'Y', 'REWORK') RESULT2, \n";
                sql += "                    FACNO EQUIP_ID, \n";
                sql += "                    C.NUMVAL5, C.TXTVAL6, C.NUMVAL3, C.NUMVAL4 \n";
                sql += "                FROM WIPLTH A,AMSEMR B,FACRST C \n";
                sql += "                    WHERE A.FACILITY = '" + FACY + "' AND A.OPER <> '8100' AND A.LOT_NUMBER = '" + SN + "' AND A.OPER=B.OPER(+) AND  A.LOT_NUMBER=B.LOT_NUMBER(+) AND A.LOT_NUMBER=C.LOT_NUMBER(+) AND A.OPER=C.OPER(+) AND A.WTMSEQ=C.SEQ_NUM(+) \n";

                sql += "           UNION \n";//--合并结果集1
                sql += "           SELECT  LOT_NUMBER, HIST_SEQ, ALIAS_LOT_NUMBER, PROD, OPER,TRAN_TIME, OPERATOR, \n";
                sql += "                   ENTITY, SEND_OPR RESULT1,'' RESULT2,'',NULL,'',NULL,NULL\n";
                sql += "                FROM WIPLBR \n";
                sql += "                    WHERE FACILITY = '" + FACY + "' AND LOT_NUMBER = '" + SN + "' \n";

                sql += "           UNION \n";//--合并结果集2
                sql += "           SELECT   LOT_NUMBER, HIST_SEQ, SERIAL_ID, MAT_ID, OPER, TRAN_TIME, \n";
                sql += "                    OPERATOR, '' ENTITY, 'PARTS' RESULT1, '' RESULT2,'',NULL,'',NULL,NULL \n";
                sql += "                FROM WIPLTB \n";
                sql += "                    WHERE FACILITY = '" + FACY + "' AND LOT_NUMBER = '" + SN + "' \n";

                sql += "           UNION \n";//--合并结果集3
                sql += "           SELECT  A.LOT_NUMBER, 20 HIST_SEQ, B.ALIAS_LOT_NUMBER, A.PROD,'8100' OPER, A.TRAN_TIME, \n";
                sql += "                   A.OPERATOR, A.RTC_ENTITY ENTITY,'','','',NULL,'',NULL,NULL \n";
                sql += "                FROM WIPBLT A, WIPLOT B \n";
                sql += "                   WHERE A.FACILITY = '" + FACY + "' AND A.LOT_NUMBER = '" + SN + "' AND A.LOT_NUMBER = B.LOT_NUMBER(+) \n";

                sql += "          ) A";
                #endregion

                inf_DG.ItemsSource = null;
                this.Cursor = Cursors.Wait;//等待鼠标
                DT = DB.Oracle_Data_Table(sql);
                this.Cursor = Cursors.Arrow;//头鼠标

                //新表列名
                Newdt = new DataTable();
                Newdt.Columns.Add("ACE生产编号", typeof(string));
                Newdt.Columns.Add("客户生产编号", typeof(string));
                Newdt.Columns.Add("产品", typeof(string));
                Newdt.Columns.Add("产品名", typeof(string));
                Newdt.Columns.Add("工序", typeof(string));
                Newdt.Columns.Add("工序名", typeof(string));
                Newdt.Columns.Add("拉线", typeof(string));
                Newdt.Columns.Add("登陆时间", typeof(string));
                Newdt.Columns.Add("操作者ID", typeof(string));
                Newdt.Columns.Add("操作者名字", typeof(string));
                Newdt.Columns.Add("结果1", typeof(string));
                Newdt.Columns.Add("结果2", typeof(string));
                Newdt.Columns.Add("设备编号", typeof(string));

                //复制到新表
                for (var i = 0; i < DT.Rows.Count; i++)
                {
                    DataRow row = DT.Rows[i];//
                    DataRow rowNew = Newdt.NewRow();

                    rowNew["ACE生产编号"] = row["ACESN"];
                    rowNew["客户生产编号"] = row["CUSTSN"];
                    rowNew["产品"] = row["PROD"];
                    rowNew["产品名"] = row["MODEL"];
                    rowNew["工序"] = row["OPER"];
                    rowNew["工序名"] = row["OPER_Name"];
                    rowNew["拉线"] = row["LINE"];
                    rowNew["登陆时间"] = Convert.ToDateTime(row["DATE_TIME"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                    rowNew["操作者ID"] = row["USERID"];
                    rowNew["操作者名字"] = DB.Ksc_GB18030(row["USER_NAME"].ToString());
                    rowNew["结果1"] = row["RESULT1"];
                    rowNew["结果2"] = row["RESULT2"];
                    rowNew["设备编号"] = row["MC_NUM"];
                    Newdt.Rows.Add(rowNew);
                }
                DT.Dispose();

                if (DT.Rows.Count > 0)
                {
                    inf_DG.IsReadOnly = true;
                    inf_DG.AutoGenerateColumns = true;//这个要开启              
                    inf_DG.ItemsSource = Newdt.DefaultView;//绑定
                }
                else
                {
                    MessageBox.Show("没找到数据!");
                    SN_TextBoxn.Focus();
                    SN_TextBoxn.SelectAll();
                }
                Newdt.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { this.Cursor = Cursors.Arrow;/*头鼠标*/ } //finally 块始终都会运行
        }

        private void VACE_追踪产品信息()
        {
            DataTable DT, Newdt;
            string sql, FACY = 工厂代码_TextBoxn.Text, SN = SN_TextBoxn.Text;

            try
            {
                if (SN_TextBoxn.Text.Length < 3)
                    throw new Exception("Please input bar code correctly!");
                #region SQL
                sql = "SELECT \n";
                sql += "     A.ACESN, A.CUSTSN, A.PROD, \n"; //--ACE条码, 客户码, 产品名
                sql += "     DECODE (RESULT1, 'PARTS', (SELECT MAT_DESC FROM BOMMAT WHERE FACILITY = '6000' AND A.PROD = MAT_ID),(SELECT PRD_DESC FROM WIPPRD WHERE FACILITY = 'V100' AND A.PROD = PROD)) MODEL,\n"; //--机种型号  decode(条件, 值1, 返回值1, 值2, 返回值2,…值n, 返回值n, 缺省值)
                sql += "     A.OPER, \n";//--工序号
                sql += "    (SELECT OPERNM  FROM STDOPR WHERE FACILITY = '" + FACY + "' AND A.OPER = OPER) OPER_Name, \n"; //--工序名
                //sql += "    (SELECT OPERGRP FROM STDOPR WHERE FACILITY = '6000' AND A.OPER = OPER) OPERGRP, \n"; //--工序名
                sql += "     A.ENTITY LINE, A.TRANTIME DATE_TIME, A.OPERATOR USERID, \n";//----拉线号, 登陆时间, 操作者ID
                sql += "    (SELECT DISTINCT USERNM FROM SYSUSR WHERE FACILITY = '" + FACY + "' AND USERGB = '2' AND USERID=A.OPERATOR ) USER_NAME, \n";//--操作者名字
                sql += "     A.RESULT1, A.RESULT2,A.EQUIPID MC_NUM, \n";//--结果1, 结果2, 设备编号
                //        --(SELECT FACNAME FROM AMSEQS WHERE FACNO = A.EQUIPID AND OPER = A.OPER) EQU_DESC, --设备型号 没找到表名
                sql += "     NUMVAL3 Kpa, NUMVAL4 Pa, NUMVAL5 CCM, TXTVAL6 RESULT3 \n";//--CCM,气密结果,Kpa,Pa
                sql += "     FROM ( \n";
                sql += "           SELECT \n";
                sql += "                    A.ACESN, HISTSEQ, A.CUSTSN, PROD, A.OPER,\n";
                sql += "                    TRANTIME, OPERATOR, A.ENTITY,\n";
                sql += "                    DECODE (RESULTFG, 'P', 'Pass', 'S', 'Fail') RESULT1, \n";
                sql += "                    DECODE (RWRKFG, 'Y', 'REWORK') RESULT2, \n";
                sql += "                    EQUIPID,\n";
                sql += "                    C.NUMVAL5, C.TXTVAL6, C.NUMVAL3, C.NUMVAL4 \n";
                sql += "                FROM WIPLTH A,FACRST B,FACRST C \n";
                sql += "                    WHERE A.FACILITY = '" + FACY + "' AND A.OPER <> '8100' AND A.ACESN = '" + SN + "' AND A.OPER=B.OPER(+) AND  A.ACESN=B.ACESN(+) AND A.ACESN=C.ACESN(+) AND A.OPER=C.OPER(+) AND A.WTMSEQ=C.SEQNUM(+) \n";
                
                sql += "           UNION \n";//--合并结果集
                sql += "           SELECT   ACESN, HISTSEQ, CUSTSN, PROD, OPER, TRANTIME, OPERATOR, ENTITY, \n";
                sql += "                    SENDOPR RESULT1,'' RESULT2,'',NULL,'',NULL,NULL \n";
                sql += "                FROM WIPLBR \n";
                sql += "                    WHERE FACILITY = '" + FACY + "' AND ACESN = '" + SN + "' \n";


                sql += "           UNION \n";//--合并结果集
                sql += "           SELECT   ACESN, HISTSEQ, SERIALID, MATID, OPER, TRANTIME, \n";
                sql += "                    OPERATOR, '' ENTITY, 'PARTS' RESULT1, '' RESULT2,'',NULL,'',NULL,NULL \n";
                sql += "                FROM WIPLTB \n";
                sql += "                    WHERE FACILITY = '" + FACY + "' AND ACESN = '" + SN + "' \n";

                sql += "           UNION \n";//--合并结果集
                sql += "           SELECT   A.ACESN, 20 HISTSEQ, B.CUSTSN, A.PROD,'8100' OPER, A.TRANTIME, \n";
                sql += "                    A.OPERATOR, A.ENTITY ENTITY,'','','',NULL,'',NULL,NULL \n";
                sql += "                FROM WIPBLT A, WIPLOT B \n";
                sql += "                    WHERE A.FACILITY = '" + FACY + "' AND A.ACESN = '" + SN + "' AND A.ACESN = B.ACESN(+) \n";

                sql += "          ) A";


                #endregion

                inf_DG.ItemsSource = null;
                this.Cursor = Cursors.Wait;//等待鼠标
                DT = DB.Oracle_Data_Table(sql);
                this.Cursor = Cursors.Arrow;//头鼠标

                //新表列名
                Newdt = new DataTable();
                Newdt.Columns.Add("ACESN", typeof(string));
                Newdt.Columns.Add("CUSTSN", typeof(string));
                Newdt.Columns.Add("ProdName", typeof(string));
                Newdt.Columns.Add("MODEL", typeof(string));
                Newdt.Columns.Add("OPER", typeof(string));
                Newdt.Columns.Add("OperName", typeof(string));                               
                Newdt.Columns.Add("LINE", typeof(string));
                Newdt.Columns.Add("Land_Time", typeof(string));
                Newdt.Columns.Add("USER_ID", typeof(string));
                Newdt.Columns.Add("USER_Name", typeof(string));
                Newdt.Columns.Add("Result1", typeof(string));
                Newdt.Columns.Add("Result2", typeof(string));
                Newdt.Columns.Add("MC_NUM", typeof(string));

                //复制到新表
                for (var i = 0; i < DT.Rows.Count; i++)
                {
                    DataRow row = DT.Rows[i];//
                    DataRow rowNew = Newdt.NewRow();

                    rowNew["ACESN"] = row["ACESN"];//ACE条码
                    rowNew["CUSTSN"] = row["CUSTSN"];//客户码
                    rowNew["ProdName"] = row["PROD"];//产品名
                    rowNew["MODEL"] = row["MODEL"];//型号
                    rowNew["OPER"] = row["OPER"];//工序号
                    rowNew["OperName"] = row["OPER_Name"];//工序名
                    rowNew["LINE"] = row["LINE"];//拉线号
                    rowNew["Land_Time"] = Convert.ToDateTime(row["DATE_TIME"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");//登陆时间
                    rowNew["USER_ID"] = row["USERID"];//操作者ID
                    rowNew["USER_Name"] = row["USER_NAME"];//操作者名字
                    rowNew["Result1"] = row["RESULT1"];//结果1
                    rowNew["Result2"] = row["RESULT2"];//结果2
                    rowNew["MC_NUM"] = row["MC_NUM"];//
                    Newdt.Rows.Add(rowNew);
                }
                DT.Dispose();

                if (DT.Rows.Count > 0)
                {
                    inf_DG.IsReadOnly = false;
                    inf_DG.AutoGenerateColumns = true;//这个要开启              
                    inf_DG.ItemsSource = Newdt.DefaultView;//绑定
                }
                else
                {
                    MessageBox.Show("No Data Found!");
                    SN_TextBoxn.Focus();
                    SN_TextBoxn.SelectAll();
                }
                Newdt.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { this.Cursor = Cursors.Arrow;/*头鼠标*/ } //finally 块始终都会运行
        }

        private void SN_TextBoxn_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DB.Factory_Name == "DACE")//
                {
                    DACE_追踪产品信息();
                }
                else
                {
                    VACE_追踪产品信息();
                }
            }
        }

        private void 追踪信息_BT_Click(object sender, RoutedEventArgs e)
        {
            //if (DB.Local_login)
            //    return;
            if (DB.Factory_Name == "DACE")//2021000204656
            {
                DACE_追踪产品信息();
            }
            else
            {
                VACE_追踪产品信息();
            }
        }

        //语言初始化
        private void Language_int(string language)
        {
            if (language == "CN")
            {
                工厂编号_Label.Content = "工厂编号:";
                工厂名_Label.Content = "工厂名称:";
                ACE_SN_Label.Content = "ACE生产编号:";
                追踪信息_BT.Content = "查 询";
                //导出_EXCEL.Content = "导出EXCEL";
            }
            else
            {
                工厂编号_Label.Content = "Factory NUM:";
                工厂名_Label.Content = "Factory Name:";
                ACE_SN_Label.Content = "ACE_SN:";
                追踪信息_BT.Content = "QUERY";
                //导出_EXCEL.Content = "TO_EXCEL";
            }
        }

    }



}
