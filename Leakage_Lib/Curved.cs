using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Leakage_Lib
{
    //try
    //{
    //     throw new Exception("表内没有发现数据");
    //}
    //catch (MyEx ex) { throw ex; }

    /// <summary>绘制曲线图表</summary>
    public class Curved
    {
        /// <summary>绘制充压曲线数据 </summary>
        public static PointCollection Kpa_Curve_Data = new PointCollection();//折线数据点   
        /// <summary>绘制泄漏曲线数据</summary>
        public static PointCollection Leak_Curve_Data = new PointCollection();

        /// <summary>充气曲线颜色</summary>
        public static Brush Fill_Curved_color = Brushes.Yellow;//new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));
        /// <summary>泄漏曲线颜色</summary>
        public static Brush Leak_Curved_color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF00FF")); //Brushes.Green;

     
        /// <summary>Y轴充气值比率</summary>
        public static double Y_Fill_Ratio = 0;
        /// <summary>Y轴泄漏值比率</summary>
        public static double Y_Leak_Ratio = 0;
        /// <summary>画板左右留边</summary>
        public static readonly double LR_Width = 60; //左右留边
        /// <summary>画板稳定位置</summary>
        public static double Settling_pos = 0; //稳定位置    
        /// <summary>画板Y轴零位</summary>
        public static double Zero_pos = 0; //零位
        /// <summary>测试过程的总时间</summary>
        public static double Total_Time = 0; //总时间


        /// <summary>X轴时间值比率</summary>
        private static double X_Ratio = 1;
        /// <summary>画板上留边</summary>
        private static readonly double TOP_H = 60;   //上留边
        /// <summary>画板下留边</summary>
        private static readonly double Bottom_H = 40;//下留边
        /// <summary>画板罐稳定位置</summary>
        private static double Tank_Settling_pos = 0; //罐稳定位置
        /// <summary>画板充气位置</summary>
        private static double Fill_pos = 0;          //充气位置


        /// <summary>添加曲线数据</summary>
        public static void Add_Curve_Data(double time)
        {
            double T = time * Curved.X_Ratio;

            double X = Curved.LR_Width + T;
            double Y = Curved.Zero_pos;

            Curved.Kpa_Curve_Data.Add(new Point(X, Y - TestData.Fill_Press * Curved.Y_Fill_Ratio));//充气曲线

            if (T >= Curved.Settling_pos - Curved.LR_Width)
            {

                if (SetData.Leak_Unit == "Pa")
                {
                    Curved.Leak_Curve_Data.Add(new Point(X, Y - TestData.Leak_Pa * Curved.Y_Leak_Ratio));
                }
                else if (SetData.Leak_Unit == "CCM")
                {
                    Curved.Leak_Curve_Data.Add(new Point(X, Y - TestData.Leak_CCM * Curved.Y_Leak_Ratio));
                }
            }
        }




        /// <summary>绘制绘制曲线与标签,Kpa_Curve_Data,Leak_Curve_Data 添加数据会自动绘制曲线</summary>
        public static void Draw_Line_Label(Canvas canvas)
        {
            try
            {
                if (SetData.Fill_Unit == "")
                {
                    SetData.Fill_Unit = "Kpa";
                }
                if (SetData.Leak_Unit == "")
                {
                    SetData.Leak_Unit = "Pa";
                }

                Zero_pos = ((canvas.ActualHeight - Bottom_H) * 0.7);

                TextBlock label; //标签变量
                Line      lines; //线变量
                Path      Arrow; //箭头变量

                Clear(canvas);// 清除画布  

                if (SetData.Comm_MODE != Comm_Mode.WaYeal_COM)//通信方式不等于 WaYeal_COM
                {
                    if (SetData.Work_MODE == Work_Mode.Fully_Sealed)//全封闭式
                    {
                        Y_Fill_Ratio = Zero_pos * 0.7 / ((SetData.Tank_Press_Hi==0) ? 1:SetData.Tank_Press_Hi); //Y轴充气比率
                    }
                    else
                    {
                        SetData.Tank_Settling_Time = 0;
                        Y_Fill_Ratio = Zero_pos * 0.7 / ((SetData.Fill_Press_Hi == 0) ? 1 : SetData.Fill_Press_Hi); //Y轴充气比率
                    }
                        
                    Total_Time = SetData.Tank_Settling_Time + SetData.Fill_Time + SetData.Settling_Time + SetData.Test_Time;//总时间
                    X_Ratio = (canvas.ActualWidth - LR_Width * 2) / Total_Time; //X轴宽度比率

                    Tank_Settling_pos = SetData.Tank_Settling_Time * X_Ratio + LR_Width;
                    Fill_pos = (SetData.Tank_Settling_Time + SetData.Fill_Time) * X_Ratio + LR_Width;
                    Settling_pos = (SetData.Tank_Settling_Time + SetData.Fill_Time + SetData.Settling_Time) * X_Ratio + LR_Width;

                    if (SetData.Comm_MODE != Comm_Mode.lnterTech_COM) //通信方式不等于 lnterTech_COM
                    { 
                        //Leak_Max = (LeakMC.SET_Data.Upper_Leak_Limit * 10);
                        Y_Leak_Ratio = Zero_pos * 0.2 / ((SetData.Upper_Leak_Limit == 0) ? 1 : SetData.Upper_Leak_Limit); //Y轴泄漏比率
                    }
                    else 
                    { 
                        //Leak_Max = (LeakMC.SET_Data.Upper_Leak_Limit * 5);
                        Y_Leak_Ratio = Zero_pos * 0.4 / ((SetData.Upper_Leak_Limit == 0) ? 1 : SetData.Upper_Leak_Limit); //Y轴泄漏比率
                    }

                    //****************** 画线和箭头 ******************
                    int Arrow_W = 8, Arrow_L = 10, Arrow_line_W = 2;//箭头宽度，箭头长度，箭头线宽
                    /*X轴底线 ---- 1  */
                    lines = new Line();//线
                    lines.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0395FB"));//线色
                    lines.StrokeThickness = Arrow_line_W;//线宽
                    //lines.StrokeStartLineCap = PenLineCap.Triangle;
                    lines.X1 = LR_Width - 10;
                    lines.Y1 = canvas.ActualHeight - Bottom_H;
                    lines.X2 = canvas.ActualWidth - LR_Width + 10;
                    lines.Y2 = lines.Y1;
                    canvas.Children.Add(lines);//底 -
                    /* X轴箭头 */
                    Arrow = new Path();
                    Arrow.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0395FB"));//填充色
                    PathFigure X_Figure = new PathFigure();
                    X_Figure.IsClosed = true;
                    X_Figure.StartPoint = new System.Windows.Point(lines.X2, lines.Y2 - Arrow_W / 2);                          //路径的起点
                    X_Figure.Segments.Add(new LineSegment(new Point(lines.X2, lines.Y2 + Arrow_W / 2), false)); //第2个点
                    X_Figure.Segments.Add(new LineSegment(new Point(lines.X2 + Arrow_L, lines.Y2), false)); //第3个点
                    PathGeometry X_Geometry = new PathGeometry();
                    X_Geometry.Figures.Add(X_Figure);
                    Arrow.Data = X_Geometry;
                    canvas.Children.Add(Arrow);
                    /* 0点横线虚线 ----  */
                    lines = new Line();//线
                    lines.Stroke = Brushes.White;//白色 new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF0F0F0"));//线色
                    lines.StrokeDashArray = new DoubleCollection() { 20, 10, 20 };//虚线
                    lines.StrokeThickness = 0.5;//线宽
                    lines.X1 = LR_Width - 10;
                    lines.Y1 = Zero_pos;
                    lines.X2 = canvas.ActualWidth - LR_Width + 10;
                    lines.Y2 = lines.Y1;
                    canvas.Children.Add(lines);//0点横线虚线 -
                    /* 左Y轴线 |  2  */
                    lines = new Line();
                    lines.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0395FB"));//天蓝色
                    lines.StrokeThickness = Arrow_line_W;//线宽
                    lines.X1 = LR_Width;
                    lines.Y1 = canvas.ActualHeight - Bottom_H + 10;
                    lines.X2 = lines.X1;
                    lines.Y2 = TOP_H;
                    canvas.Children.Add(lines);
                    /* 左Y轴 | 箭头  */
                    Arrow = new Path();
                    Arrow.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0395FB"));//天蓝色
                    PathFigure LY_Figure = new PathFigure();
                    LY_Figure.IsClosed = true;
                    LY_Figure.StartPoint = new Point(lines.X2 - Arrow_W / 2, lines.Y2);                          //路径的起点
                    LY_Figure.Segments.Add(new LineSegment(new Point(lines.X2 + Arrow_W / 2, lines.Y2), false)); //第2个点
                    LY_Figure.Segments.Add(new LineSegment(new Point(lines.X2, lines.Y2 - Arrow_L), false)); //第3个点               
                    PathGeometry LY_Geometry = new PathGeometry();
                    LY_Geometry.Figures.Add(LY_Figure);
                    Arrow.Data = LY_Geometry;
                    canvas.Children.Add(Arrow);
                    /* 右Y轴线 | 3  */
                    lines = new Line();
                    lines.Stroke = Brushes.Yellow;//黄色
                    lines.StrokeThickness = Arrow_line_W;//线宽
                    lines.X1 = canvas.ActualWidth - LR_Width;
                    lines.Y1 = canvas.ActualHeight - Bottom_H + 10;
                    lines.X2 = lines.X1;
                    lines.Y2 = TOP_H;
                    canvas.Children.Add(lines);
                    /* 右Y轴 | 箭头  */
                    Arrow = new Path();
                    Arrow.Fill = Brushes.Yellow;//黄色
                    PathFigure RY_Figure = new PathFigure();
                    RY_Figure.IsClosed = true;
                    RY_Figure.StartPoint = new Point(lines.X2 - Arrow_W / 2, lines.Y2);                          //路径的起点
                    RY_Figure.Segments.Add(new LineSegment(new Point(lines.X2 + Arrow_W / 2, lines.Y2), false)); //第2个点
                    RY_Figure.Segments.Add(new LineSegment(new Point(lines.X2, lines.Y2 - Arrow_L), false));     //第3个点              
                    PathGeometry RY_Geometry = new PathGeometry();
                    RY_Geometry.Figures.Add(RY_Figure);
                    Arrow.Data = RY_Geometry;
                    canvas.Children.Add(Arrow);

                    if (SetData.Work_MODE == Work_Mode.Fully_Sealed)//全封闭式
                    {
                        /* 罐稳定时间线 | 4  */
                        lines = new Line();
                        lines.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE606F1"));//线色
                        lines.StrokeThickness = 1;//线宽
                        lines.StrokeDashArray = new DoubleCollection() { 10, 5 };//虚线
                        lines.X1 = Tank_Settling_pos;
                        lines.Y1 = canvas.ActualHeight - Bottom_H + 10;
                        lines.X2 = lines.X1;
                        lines.Y2 = TOP_H;
                        canvas.Children.Add(lines);
                    }

                    /* 充气时间线 | 5  */
                    lines = new Line();
                    lines.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF16706"));//Brushes.White;//白色
                    lines.StrokeThickness = 1;//线宽
                    lines.StrokeDashArray = new DoubleCollection() { 10, 5 };//虚线
                    lines.X1 = Fill_pos;
                    lines.Y1 = canvas.ActualHeight - Bottom_H + 10;
                    lines.X2 = lines.X1;
                    lines.Y2 = TOP_H;
                    canvas.Children.Add(lines);
                    /* 稳定时间线 |  */
                    lines = new Line();
                    lines.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF06F1B1"));//Brushes.White;//白色
                    lines.StrokeThickness = 1;//线宽
                    lines.StrokeDashArray = new DoubleCollection() { 10, 5 };//虚线
                    lines.X1 = Settling_pos;
                    lines.Y1 = canvas.ActualHeight - Bottom_H + 10;
                    lines.X2 = lines.X1;
                    lines.Y2 = TOP_H;
                    canvas.Children.Add(lines);

                    if (SetData.Comm_MODE != Comm_Mode.WaYeal_COM)
                    {
                        //充气压力上限线 - - - -
                        lines = new Line();
                        lines.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC9F502"));//
                        lines.StrokeThickness = 0.5;//线宽
                        lines.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };//虚线
                                                                                     //lines.StrokeDashCap = PenLineCap.Triangle;
                                                                                     //lines.StrokeEndLineCap = PenLineCap.Square;
                                                                                     //lines.StrokeStartLineCap = PenLineCap.Round;
                        lines.X1 = LR_Width - 10;
                        lines.Y1 = Zero_pos - Y_Fill_Ratio * SetData.Fill_Press_Hi; 

                        lines.X2 = canvas.ActualWidth - LR_Width + 10;
                        lines.Y2 = lines.Y1;
                        canvas.Children.Add(lines);
                        //充气压力下限线 - - - -
                        lines = new Line();
                        lines.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC9F502"));//
                        lines.StrokeThickness = 0.5;//线宽
                        lines.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };//虚线
                                                                                     //lines.StrokeDashCap = PenLineCap.Triangle;
                                                                                     //lines.StrokeEndLineCap = PenLineCap.Square;
                                                                                     //lines.StrokeStartLineCap = PenLineCap.Round;
                        lines.X1 = LR_Width - 10;
                        lines.Y1 = Zero_pos - Y_Fill_Ratio * SetData.Fill_Press_Lo;
                        lines.X2 = canvas.ActualWidth - LR_Width + 10;
                        lines.Y2 = lines.Y1;
                        canvas.Children.Add(lines);
                    }
                    /* 泄漏值上限值上限线 - - - -  */
                    lines = new Line();
                    lines.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF0000"));//
                    lines.StrokeThickness = 0.5;//线宽
                    lines.StrokeDashArray = new DoubleCollection() { 10, 3 };//虚线
                    lines.StrokeDashCap = PenLineCap.Triangle;
                    lines.StrokeEndLineCap = PenLineCap.Square;
                    lines.StrokeStartLineCap = PenLineCap.Round;
                    lines.X1 = Settling_pos - 10;
                    //lines.Y1 = Zero_pos - (canvas.ActualHeight - TOP_H - Bottom_H) / Leak_Max * LeakMC.SET_Data.Upper_Leak_Limit;
                    lines.Y1 = Zero_pos - Y_Leak_Ratio * SetData.Upper_Leak_Limit;
                    
                    lines.X2 = canvas.ActualWidth - LR_Width + 10;
                    lines.Y2 = lines.Y1;
                    canvas.Children.Add(lines);
                    /* 泄漏值上限值下限线 - - - -  */
                    lines = new Line();
                    lines.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF0000"));//
                    lines.StrokeThickness = 0.5;//线宽
                    lines.StrokeDashArray = new DoubleCollection() { 10, 3 };//虚线
                    lines.StrokeDashCap = PenLineCap.Triangle;
                    lines.StrokeEndLineCap = PenLineCap.Square;
                    lines.StrokeStartLineCap = PenLineCap.Round;
                    lines.X1 = Settling_pos - 10;
                    //lines.Y1 = Zero_pos - (canvas.ActualHeight - TOP_H - Bottom_H) / Leak_Max * LeakMC.SET_Data.Lower_Leak_Limit;
                    lines.Y1 = Zero_pos - Y_Leak_Ratio * SetData.Lower_Leak_Limit;
                    lines.X2 = canvas.ActualWidth - LR_Width + 10;
                    lines.Y2 = lines.Y1;
                    canvas.Children.Add(lines);


                    //******************************** 画标签 ********************************
                    /* 单位秒标签 */
                    double 标签Top = canvas.ActualHeight - Bottom_H + 5;
                    double 标签Len = 0;
                    label = new TextBlock();
                    label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFB9603"));//字体 
                    label.FontSize = 14;
                    label.Text = "S";
                    标签Len = canvas.ActualWidth - LR_Width + 10;
                    Canvas.SetLeft(label, 标签Len);
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);
                    /* 时间0位标签 */
                    label = new TextBlock();
                    label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFB9603"));//字体
                    label.FontSize = 14;
                    label.Text = "0.0 S";
                    标签Len = LR_Width - 30;
                    Canvas.SetLeft(label, 标签Len);
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);
                    /* 罐稳定时间标签 */
                    if (SetData.Work_MODE == Work_Mode.Fully_Sealed)//全封闭式
                    {
                        label = new TextBlock();
                        label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFB9603"));//字体
                        label.FontSize = 14;
                        label.Text = "(" + SetData.Tank_Settling_Time.ToString("F1") + ") ";
                        label.Text += SetData.Tank_Settling_Time.ToString("F1");
                        标签Len = Tank_Settling_pos + 5;
                        Canvas.SetLeft(label, 标签Len);
                        Canvas.SetTop(label, 标签Top);
                        canvas.Children.Add(label);
                    }
                    /* 充气时间标签 */
                    label = new TextBlock();
                    label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFB9603"));//字体
                    label.FontSize = 14;
                    label.Text = "(" + SetData.Fill_Time.ToString("F1") + ") ";
                    label.Text += (SetData.Tank_Settling_Time + SetData.Fill_Time).ToString("F1");
                    标签Len = Fill_pos + 5;
                    Canvas.SetLeft(label, 标签Len);
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);
                    /* 稳定时间标签 */
                    label = new TextBlock();
                    label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFB9603"));//字体
                    label.FontSize = 14;
                    label.Text = "(" + SetData.Settling_Time.ToString("F1") + ") ";
                    label.Text += (SetData.Tank_Settling_Time + SetData.Fill_Time + SetData.Settling_Time).ToString("F1");
                    标签Len = Settling_pos + 5;
                    Canvas.SetLeft(label, 标签Len);
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);
                    /* 检测时间标签  */
                    label = new TextBlock();
                    label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFB9603"));//字体
                    label.FontSize = 14;
                    label.Text = "(" + SetData.Test_Time.ToString("F1") + ") ";
                    label.Text += Total_Time.ToString("F1");
                    标签Len = canvas.ActualWidth - LR_Width - (label.FontSize * label.Text.Length)*0.5;
                    Canvas.SetLeft(label, 标签Len);
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);

                    /* Kpa单位标签 */
                    标签Len = canvas.ActualWidth - LR_Width + 10;
                    label = new TextBlock();
                    label.Foreground = System.Windows.Media.Brushes.Yellow;//字体 黄色
                    label.FontSize = 14;
                    label.Text = SetData.Fill_Unit;
                    标签Top = TOP_H-15;
                    Canvas.SetRight(label, 标签Len);
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);

                    /* 0 Kpa位标签  */
                    label = new TextBlock();
                    label.Foreground = System.Windows.Media.Brushes.Yellow;//字体 黄色
                    label.FontSize = 14;
                    label.Text = "0.0";
                    标签Top = Zero_pos - label.FontSize / 2 - 2;
                    Canvas.SetRight(label, 标签Len);
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);
                    /* Kpa充气上限标签  */
                    label = new TextBlock();
                    label.Foreground = System.Windows.Media.Brushes.Yellow;//字体 黄色
                    label.FontSize = 14;
                    label.Text = SetData.Fill_Press_Hi.ToString("F1");
                    //Canvas.SetRight(label, 标签Len);
                    Canvas.SetLeft(label, LR_Width + 10);

                    标签Top = Zero_pos - Y_Fill_Ratio * SetData.Fill_Press_Hi - label.FontSize * 1.5;
                    //Canvas.SetBottom(label, 标签Top); //底部
                    Canvas.SetTop(label, 标签Top);//上部
                    canvas.Children.Add(label);
                    /* Kpa充气下限标签  */
                    label = new TextBlock();
                    label.Foreground = Brushes.Yellow;//字体 黄色
                    label.FontSize = 14;
                    label.Text = SetData.Fill_Press_Lo.ToString("F1");
                    //Canvas.SetRight(label, 标签Len);
                    Canvas.SetLeft(label, LR_Width + 10);
                    标签Top = Zero_pos - Y_Fill_Ratio * SetData.Fill_Press_Lo;
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);

                    /* 单位CCM标签  */
                    标签Len = canvas.ActualWidth - LR_Width + 10;
                    label = new TextBlock();
                    label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0FD0EE"));//字体 
                    label.FontSize = 14;
                    label.Text = " " + SetData.Leak_Unit;
                    标签Top = TOP_H;
                    Canvas.SetLeft(label, 标签Len);
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);
                    /* 0 CCM位标签  */
                    label = new TextBlock();
                    label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0FD0EE"));
                    label.FontSize = 14;
                    label.Text = " 0.00";
                    标签Top = Zero_pos - label.FontSize / 2 - 2;
                    Canvas.SetLeft(label, 标签Len);
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);
                    /* 泄漏上限标签  */
                    label = new TextBlock();
                    label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0FD0EE"));
                    label.FontSize = 14;
                    label.Text = SetData.Upper_Leak_Limit.ToString("F2");
                    if (SetData.Upper_Leak_Limit >= 0)
                    {
                        label.Text = " " + label.Text;
                    }
                    标签Top = Zero_pos - Y_Leak_Ratio * SetData.Upper_Leak_Limit - label.FontSize / 2 - 2;
                    Canvas.SetLeft(label, 标签Len);
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);
                    /* 泄漏下限标签  */
                    label = new TextBlock();
                    label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0FD0EE"));
                    label.FontSize = 14;
                    label.Text = SetData.Lower_Leak_Limit.ToString("F2");
                    标签Top = Zero_pos - Y_Leak_Ratio * SetData.Lower_Leak_Limit - label.FontSize / 2 - 2;
                    Canvas.SetLeft(label, 标签Len);
                    Canvas.SetTop(label, 标签Top);
                    canvas.Children.Add(label);

                    /* 绘制X轴刻度线  */
                    for (int i = 1; i < (canvas.ActualWidth - LR_Width * 2) / X_Ratio; i++)
                    {
                        Line 标尺线 = new Line(); //主x轴标尺
                        标尺线.StrokeEndLineCap = PenLineCap.Triangle;
                        标尺线.StrokeThickness = 1;
                        标尺线.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFB9603"));//FFFB9603

                        //原点 O =(左右留边,画布高度 - 下留边)                   
                        标尺线.Y1 = canvas.ActualHeight - Bottom_H - 1;//原点Y = 画布高度 - 下留边  (-1=减线宽)
                        if (i % 10 == 0)
                        {
                            标尺线.StrokeThickness = 5;//线宽 
                            标尺线.Y2 = 标尺线.Y1 - 10;//大刻度线
                        }
                        else if (i % 5 == 0)
                        {
                            标尺线.StrokeThickness = 4;//线宽 
                            标尺线.Y2 = 标尺线.Y1 - 6;//大刻度线
                        }
                        else
                        {
                            标尺线.StrokeThickness = 3;//线宽 
                            标尺线.Y2 = 标尺线.Y1 - 3;//小刻度线
                        }

                        标尺线.X1 = LR_Width + i * X_Ratio;
                        标尺线.X2 = 标尺线.X1;

                        canvas.Children.Add(标尺线);
                    }
                    /* 绘制Y轴刻度线 */
                    for (int i = 0; i < (Zero_pos - TOP_H) / Y_Fill_Ratio; i++)
                    {
                        Line 标尺线 = new Line(); //主Y轴标尺
                        标尺线.StrokeEndLineCap = PenLineCap.Triangle;
                        标尺线.StrokeThickness = 1;
                        标尺线.Stroke = Brushes.Yellow;// new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0395FB"));//天蓝色

                        标尺线.X1 = LR_Width + 1; //原点X=左右留边 (+1=加线宽)

                        if (i % 10 == 0)
                        {
                            标尺线.StrokeThickness = 4;//线宽 
                            标尺线.X2 = 标尺线.X1 + 8; //大刻度线
                            /* Kpa标签 */
                            label = new TextBlock();
                            label.Foreground = Brushes.Yellow;//字体 黄色
                            label.FontSize = 14;
                            label.Text = i.ToString("F0");
                            Canvas.SetRight(label, 标签Len);
                            标签Top = Zero_pos - Y_Fill_Ratio * i - label.FontSize * 0.6;
                            Canvas.SetTop(label, 标签Top);//上部
                            canvas.Children.Add(label);
                        }
                        else if (i % 5 == 0)
                        {
                            标尺线.StrokeThickness = 3;//线宽 
                            标尺线.X2 = 标尺线.X1 + 4;//中刻度线
                            if (Y_Fill_Ratio > 9)
                            {
                                /* Kpa标签 */
                                label = new TextBlock();
                                label.Foreground = System.Windows.Media.Brushes.Yellow;//字体 黄色
                                label.FontSize = 12;
                                label.Text = i.ToString("F0");
                                Canvas.SetRight(label, 标签Len);
                                标签Top = Zero_pos - Y_Fill_Ratio * i - label.FontSize * 0.6;
                                Canvas.SetTop(label, 标签Top);//上部
                                canvas.Children.Add(label);
                            }
                        }
                        else
                        {
                            标尺线.X2 = 标尺线.X1 + 2;//小刻度线
                        }
                        标尺线.Y1 = Zero_pos - i * Y_Fill_Ratio;
                        标尺线.Y2 = 标尺线.Y1;
                        canvas.Children.Add(标尺线);
                    }
                    Draw_Curve(canvas);//绘制曲线
                }
            }
            catch (System.Exception EX)
            {
                MessageBox.Show(EX.Message, "Message");
            }

        }
        /// <summary>绘制曲线</summary>
        private static void Draw_Curve(Canvas canvas)
        {
            //画输入压力
            Polyline Kpa_Curved = new Polyline(); //绘制一系列相互连接的直线
            Kpa_Curved.Stroke = Fill_Curved_color;
            Kpa_Curved.StrokeThickness = 2;   //线宽
            Kpa_Curved.Points = Kpa_Curve_Data;
            canvas.Children.Add(Kpa_Curved);

            //画差压值
            Polyline Leak_Curved = new Polyline();//绘制一系列相互连接的直线
            Leak_Curved.Stroke = Leak_Curved_color;
            Leak_Curved.StrokeThickness = 2;   //线宽
            Leak_Curved.Points = Leak_Curve_Data;
            canvas.Children.Add(Leak_Curved);
        }
        /// <summary>清空画布</summary>
        private static void Clear(Canvas canvas)
        {
            canvas.Children.Clear();// 清除画布  
            //canvas.Background = Brushes.Black;//画布背景黑色
            Kpa_Curve_Data.Clear();//清除折线数据
            Leak_Curve_Data.Clear();//清除折线数据
        }

    }
}
