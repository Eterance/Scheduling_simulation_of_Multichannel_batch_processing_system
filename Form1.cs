using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

/// <summary>
/// 不建议使用 datatable 的下标索引获取和设置值。
/// C#提供了 DataRowExtensions 类的 datarow.setField<T>() 和 dataview.Field<T>() 泛型方法，
/// 可以强类型地获取和设置指定列的值。
/// </summary>
namespace 多道批处理系统的两级调度_1
{
    public partial class Form1 : Form
    {
        public static Form1 formMain;
        /// <summary>
        /// 已完成作业队列
        /// </summary>
        DataTable TableResult;
        /// <summary>
        /// 未提交作业队列
        /// </summary>
        DataTable TableWork;
        /// <summary>
        /// 已提交未就绪作业队列
        /// </summary>
        DataTable TableReady;
        /// <summary>
        /// 未提交作业队列表，便于复制
        /// </summary>
        DataTable TableWorkBackUp;
        /// <summary>
        /// 内存中就绪作业队列
        /// </summary>
        DataTable TableRAM;
        /// <summary>
        /// 正在执行作业
        /// </summary>
        DataTable TableCPU;
        /// <summary>
        /// 空闲内存表
        /// </summary>
        DataTable TableExtraRAMAddress;
        /// <summary>
        /// 磁带机控件数组。
        /// </summary>
        Label[] labelTapes;
        /// <summary>
        /// 磁带机剩余
        /// </summary>
        int TapeRemain = 4;
        /// <summary>
        /// 判断是不是第一次循环。
        /// </summary>
        bool isFirst = true;
        /// <summary>
        /// 判断是否暂停。
        /// </summary>
        bool isPause = false;
        /// <summary>
        /// 判断是否单步执行。
        /// </summary>
        bool isSingleStep = false;
        /// <summary>
        /// 已使用的内存, 单位为K。
        /// </summary>
        int RAMUtility = 0;
        /// <summary>
        /// 颜色类。
        /// </summary>
        public class ImColors
        {
            public static Color EmptyBlack = Color.FromArgb(30, 31, 32);
            public static Color Job1Color = Color.FromArgb(35, 82, 48);
            public static Color Job2Color = Color.FromArgb(17, 61, 111);
            public static Color Job3Color = Color.FromArgb(67, 43, 55);
            public static Color Job4Color = Color.FromArgb(98, 73, 45);
            public static Color Job5Color = Color.FromArgb(64, 34, 14);
        }

        public Form1()
        {
            InitializeComponent();
            formMain = this;//把这两个东西联系起来。
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitDataGridView();
            InitTableResult();
            InitTableReady();
            InitTableWork();
            InitTableCPU();
            InitTableRAM();
            InitTableRAMAddress();

            dataGridViewWork.DataSource = TableWork;
            dataGridViewReady.DataSource = TableReady;
            dataGridViewRAM.DataSource = TableRAM;
            dataGridViewCPU.DataSource = TableCPU;
            dataGridViewResult.DataSource = TableResult;
            dataGridViewRAMExtra.DataSource = TableExtraRAMAddress;
            TableWork.DefaultView.Sort = "到达时间 ASC";
            TableReady.DefaultView.Sort = "估计运行时间 ASC";
            TableRAM.DefaultView.Sort = "剩余时间 ASC";
            AddWork();
            InitTapeView();
        }


        private void InitTapeView()
        {
            labelTapes = new Label[4];
            labelTapes[0] = labelTape0;
            labelTapes[1] = labelTape1;
            labelTapes[2] = labelTape2;
            labelTapes[3] = labelTape3;
        }

        private void InitTableWork()
        {
            TableWork = new DataTable();
            TableWork.Columns.Add("作业名称", typeof(string));//1
            TableWork.Columns.Add("到达时间", typeof(int));//0
            TableWork.Columns.Add("估计运行时间", typeof(int));//2
            TableWork.Columns.Add("内存需要", typeof(int));//3
            TableWork.Columns.Add("磁带机需要", typeof(int));//3
        }

        private void InitTableReady()
        {
            TableReady = new DataTable();
            TableReady.Columns.Add("作业名称", typeof(string));//1
            TableReady.Columns.Add("到达时间", typeof(int));//0
            TableReady.Columns.Add("估计运行时间", typeof(int));//2
            TableReady.Columns.Add("内存需要", typeof(int));//3
            TableReady.Columns.Add("磁带机需要", typeof(int));//3
        }
        private void InitTableRAM()
        {
            TableRAM = new DataTable();
            TableRAM.Columns.Add("作业名称", typeof(string));//1
            TableRAM.Columns.Add("到达时间", typeof(int));//0
            TableRAM.Columns.Add("估计运行时间", typeof(int));//2
            TableRAM.Columns.Add("剩余时间", typeof(int));//4
            TableRAM.Columns.Add("首地址", typeof(int));//3
            TableRAM.Columns.Add("内存需要", typeof(int));//3
            TableRAM.Columns.Add("磁带机需要", typeof(int));//3
        }
        private void InitTableCPU()
        {
            TableCPU = new DataTable();
            TableCPU.Columns.Add("作业名称", typeof(string));//0
            TableCPU.Columns.Add("到达时间", typeof(int));//8
            TableCPU.Columns.Add("估计运行时间", typeof(int));//2
            TableCPU.Columns.Add("已用时间", typeof(int));//3
            TableCPU.Columns.Add("剩余时间", typeof(int));//4
            TableCPU.Columns.Add("首地址", typeof(int));//3
            TableCPU.Columns.Add("内存需要", typeof(int));//3
            TableCPU.Columns.Add("磁带机需要", typeof(int));//3
            TableCPU.Columns.Add("当前时间", typeof(int));//7
            TableCPU.Columns.Add("累计运行时间", typeof(int));//7
            TableCPUAddNewRow();
        }

        private void TableCPUAddNewRow()
        {
            DataRow ramR = TableCPU.NewRow();
            ramR.SetField("到达时间", 0);
            ramR.SetField("作业名称", DBNull.Value);
            ramR.SetField("估计运行时间", 0);
            ramR.SetField("已用时间", 0);
            ramR.SetField("剩余时间", 0);
            ramR.SetField("首地址", 0);
            ramR.SetField("内存需要", 0);
            ramR.SetField("磁带机需要", 0);
            ramR.SetField("当前时间", 1000);
            ramR.SetField("累计运行时间", 0);
            TableCPU.Rows.Add(ramR);
        }

        private void InitTableResult()
        {
            TableResult = new DataTable();
            TableResult.Columns.Add("作业名称", typeof(string));//0
            TableResult.Columns.Add("到达时间", typeof(int));//5
            TableResult.Columns.Add("估计运行时间", typeof(int));//2
            TableResult.Columns.Add("完成时间", typeof(int));//3
            TableResult.Columns.Add("周转时间", typeof(float));//6
            TableResult.Columns.Add("带权周转时间", typeof(float));//7
            TableResult.DefaultView.Sort = "完成时间 ASC";
        }

        /// <summary>
        /// 空闲的内存碎片位置。
        /// </summary>
        private void InitTableRAMAddress()
        {
            TableExtraRAMAddress = new DataTable();
            TableExtraRAMAddress.Columns.Add("首地址", typeof(int));//8
            TableExtraRAMAddress.Columns.Add("长度", typeof(int));//2
            DataRow dr = TableExtraRAMAddress.NewRow();
            dr.SetField("首地址", 0);
            dr.SetField("长度", 100);
            TableExtraRAMAddress.Rows.Add(dr);
        }

        private void InitDataGridView()
        {
            dataGridViewResult.AutoGenerateColumns = true;
            dataGridViewWork.AutoGenerateColumns = true;
            dataGridViewCPU.AutoGenerateColumns = true;
            dataGridViewRAM.AutoGenerateColumns = true;
        }

        /// <summary>
        /// 按照六十进制增加时间。
        /// </summary>
        /// <param name="time"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        private int TimeAdd(int time, int delta)
        {
            time = time + delta;
            int min = Minute(time);
            if (min >= 60)
            {
                time = time - 60 + 100;
            }
            return time;
        }

        /// <summary>
        /// 取得分钟位。
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int Minute(int time)
        {
            int min = time % 100;//取后两位
            return min;
        }

        /// <summary>
        /// 取得小时位。
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int Hour(int time)
        {
            int min = Minute(time);
            int hrs = (time - min) / 100;
            return hrs;
        }

        /// <summary>
        /// 按照六十进制计算时间差。
        /// </summary>
        /// <param name="time1"></param>
        /// <param name="time2"></param>
        /// <returns></returns>
        private int TimeMinus(int time1, int time2)
        {
            int big = 0, small = 0, res = 0;
            if (time1 == time2)
            {
                return 0;
            }
            if (time1 >= time2)
            {
                big = time1;
                small = time2;
            }
            else
            {
                big = time2;
                small = time1;
            }
            if (Hour(big) == Hour(small))
            {
                res = big - small;
            }
            else
            {
                res = (Hour(big) - Hour(small) - 1) * 60 + Minute(big) + (60 - Minute(small));
            }
            return res;
        }

        /// <summary>
        /// 输入资源需要，返回分配的首地址。如果返回 99999 则表示分配失败。
        /// </summary>
        /// <param name="ramNeed">内存需要</param>
        /// <param name="typeNeed">磁带机需要</param>
        /// <returns></returns>
        private int ResourceAllocation(int ramNeed, int typeNeed)
        {
            bool isOK = false;
            int add = 0;
            int room = 0;
            if (typeNeed >TapeRemain)//剩余磁带机数不足
            {
                return 99999;
            }
            foreach (DataRow dr in TableExtraRAMAddress.Rows)//寻找可以容纳的空闲内存
            {
                if (dr.Field<int>("长度") >= ramNeed)
                {
                    add = dr.Field<int>("首地址");
                    room = dr.Field<int>("长度");
                    TableExtraRAMAddress.Rows.Remove(dr);
                    isOK = true;
                    break;
                }
            }
            if (room > ramNeed)//大于所需空间，划分碎片
            {
                int nextAdd = add + ramNeed;//碎片首地址
                int nextRoom = room - ramNeed;//碎片长度
                DataRow dr = TableExtraRAMAddress.NewRow();
                dr.SetField("首地址", nextAdd);
                dr.SetField("长度", nextRoom);
                TableExtraRAMAddress.Rows.Add(dr);
            }
            if (isOK == false)
            {
                return 99999;
            }
            else
            {
                ResortTableRAMAddress();
                TapeRemain = TapeRemain - typeNeed;
                return add;
            }
        }

        /// <summary>
        /// 回收内存并合并内存碎片。
        /// </summary>
        /// <param name="add"></param>
        private void RAMCollect(int add, int room)
        {
            DataRow dr = TableExtraRAMAddress.NewRow();
            dr.SetField("首地址", add);
            dr.SetField("长度", room);
            TableExtraRAMAddress.Rows.Add(dr);

            //倒序合并每一个碎片 A 。
            //如果检测到有碎片 B 屁股与 A 头相连
            //扩展 B 的长度并删除 A ， 然后重新倒序合并。
            for (int i = TableExtraRAMAddress.Rows.Count -1; i >= 0; i--)
            {
                DataRow drm = TableExtraRAMAddress.Rows[i];
                if (TableExtraRAMAddress.Rows.Count == 0)
                {
                    //只有一行，不必合并
                    break;
                }
                foreach (DataRow dr2 in TableExtraRAMAddress.Rows)
                {
                    if (dr2.Field<int>("长度") + dr2.Field<int>("首地址") == drm.Field<int>("首地址"))
                    {
                        dr2.SetField("长度", dr2.Field<int>("长度") + drm.Field<int>("长度"));
                        TableExtraRAMAddress.Rows.Remove(drm);
                        i = TableExtraRAMAddress.Rows.Count;//重设计数
                        break;
                    }
                }
            }
            ResortTableRAMAddress();
        }

        /// <summary>
        /// 对空闲内存表进行重排序。
        /// </summary>
        private void ResortTableRAMAddress()
        {
            DataView dataView = TableExtraRAMAddress.DefaultView;
            dataView.Sort = "首地址 ASC";
            TableExtraRAMAddress = dataView.ToTable().Copy();
            dataGridViewRAMExtra.DataSource = TableExtraRAMAddress;//重新绑定，不然无法显示更改
        }

        /// <summary>
        /// 更改用户界面的可视化图标和统计数据。
        /// </summary>
        private void RefreshInterface(DataRow ramR)
        {
            RefreshRAMView();
            RefreshTapeView();
            labelT.Text = ramR.Field<int>("累计运行时间").ToString();
            labelCT.Text = ramR.Field<int>("当前时间").ToString();
            labelTapeRemain.Text = (4 - TapeRemain).ToString() + " / 4";
        }
        /// <summary>
        /// 刷新磁带机可视化视图。
        /// </summary>
        private void RefreshTapeView()
        {
            int index = 0;
            int ramC = TableRAM.Rows.Count; 
            int cpuC = 0;
            if (TableCPU.Rows[0].IsNull("作业名称") == false)//必须有任务正在执行才算
            {
                cpuC = 1;
            }
            int empty = 4 - ramC - cpuC;
            foreach (DataRow dr in TableRAM.Rows)
            {
                for (int i = 0; i < dr.Field<int>("磁带机需要"); i++)
                {
                    labelTapes[index].Text = dr.Field<string>("作业名称") + "\r\n使用中";
                    index++;
                }
            }
            for (int j = 0; j < TableCPU.Rows[0].Field<int>("磁带机需要"); j++)
            {
                labelTapes[index].Text = TableCPU.Rows[0].Field<string>("作业名称") + "\r\n使用中";
                index++;
            }
            for (int j = index; j < 4; j++)
            {
                labelTapes[index].Text = "空闲";
                index++;
            }
            foreach (Label label in labelTapes)
            {
                Color color;
                switch (label.Text)
                {
                    case "JOB1\r\n使用中":
                        color = ImColors.Job1Color;
                        break;
                    case "JOB2\r\n使用中":
                        color = ImColors.Job2Color;
                        break;
                    case "JOB3\r\n使用中":
                        color = ImColors.Job3Color;
                        break;
                    case "JOB4\r\n使用中":
                        color = ImColors.Job4Color;
                        break;
                    case "JOB5\r\n使用中":
                        color = ImColors.Job5Color;
                        break;
                    default:
                        color = ImColors.EmptyBlack;
                        break;
                }
                label.BackColor = color;
            }
        }

        /// <summary>
        /// 重置磁带机可视化视图。
        /// </summary>
        private void ResetTapeView()
        {
            foreach (Label label in labelTapes)
            {
                label.BackColor = ImColors.EmptyBlack;
                label.Text = "空闲";
            }
        }
        /// <summary>
        /// 重置内存可视化视图。
        /// </summary>
        private void ResetRAMView()
        {
            panelRAMViewBG.Controls.Clear();
            RAMViewAdd(0, 100, "0-99, 100K", ImColors.EmptyBlack);
        }

        /// <summary>
        /// 刷新内存可视化视图。
        /// </summary>
        private void RefreshRAMView()
        {
            RAMUtility = 0;
            foreach (DataRow dr in TableExtraRAMAddress.Rows)//加空闲内存
            {
                int add = dr.Field<int>("首地址");
                int room = dr.Field<int>("长度");
                if (room == 100)
                {
                    panelRAMViewBG.Controls.Clear();
                }
                string text = add.ToString() + "-" + (room + add - 1).ToString() + ", " + room.ToString() + "K";
                RAMViewAdd(add, room, text, ImColors.EmptyBlack);
            }
            foreach (DataRow dr in TableRAM.Rows)//加已用内存
            {
                int add = dr.Field<int>("首地址");
                int room = dr.Field<int>("内存需要");
                RAMUtility = RAMUtility + room;
                string jobName = dr.Field<string>("作业名称");
                string text = jobName + ", " + add.ToString() + "-" + (room + add - 1).ToString() + ", " + room.ToString() + "K";
                Color color;
                switch (jobName)
                {
                    case "JOB1":
                        color = ImColors.Job1Color;
                        break;
                    case "JOB2":
                        color = ImColors.Job2Color;
                        break;
                    case "JOB3":
                        color = ImColors.Job3Color;
                        break;
                    case "JOB4":
                        color = ImColors.Job4Color;
                        break;
                    case "JOB5":
                        color = ImColors.Job5Color;
                        break;
                    default:
                        color = ImColors.EmptyBlack;
                        break;
                }
                RAMViewAdd(add, room, text, color);
            }
            foreach (DataRow dr in TableCPU.Rows)//加正在执行
            {
                int add = dr.Field<int>("首地址");
                int room = dr.Field<int>("内存需要");
                RAMUtility = RAMUtility + room;
                string jobName = dr.Field<string>("作业名称");
                string text = jobName + ", " + add.ToString() + "-" + (room + add - 1).ToString() + ", " + room.ToString() + "K";
                Color color;
                switch (jobName)
                {
                    case "JOB1":
                        color = ImColors.Job1Color;
                        break;
                    case "JOB2":
                        color = ImColors.Job2Color;
                        break;
                    case "JOB3":
                        color = ImColors.Job3Color;
                        break;
                    case "JOB4":
                        color = ImColors.Job4Color;
                        break;
                    case "JOB5":
                        color = ImColors.Job5Color;
                        break;
                    default:
                        color = ImColors.Job1Color;
                        break;
                }
                RAMViewAdd(add, room, text, color);
                labelRAMUtility.Text = RAMUtility.ToString() + "K  / 100K  ";
            }
        }

        /// <summary>
        /// 向内存可视化视图添加控件。
        /// </summary>
        /// <param name="ramAddress">首地址</param>
        /// <param name="ramRoom">长度</param>
        /// <param name="text">文本</param>
        /// <param name="backColor">背景颜色</param>
        private void RAMViewAdd(int ramAddress, int ramRoom, string text, Color backColor)
        {
            Label labelX = new Label();                        //聊天中指示条
            labelX.Name = "label" + Guid.NewGuid().ToString().Substring(0, 4);
            panelRAMViewBG.Controls.Add(labelX);
            labelX.AutoSize = false;
            //根据可视化视图的长度动态计算长度倍乘系数
            int con = panelRAMViewBG.Width / 100;
            labelX.Location = new Point(ramAddress * con, -1);
            labelX.Size = new Size(ramRoom * con, panelRAMViewBG.Height);
            labelX.BorderStyle = BorderStyle.FixedSingle;
            labelX.Font = new Font("微软雅黑", 9);
            labelX.BackColor = backColor;
            labelX.Text = text;
            labelX.TextAlign = ContentAlignment.MiddleCenter;
            labelX.ForeColor = Color.DarkGray;
            labelX.BringToFront();
        }

        /// <summary>
        /// 生成指定范围内的整数随机数。
        /// </summary>
        /// <param name="min">最小值，默认1</param>
        /// <param name="max">最大值，默认99</param>
        /// <returns></returns>
        private int RandomNumberGenerator(int min = 0, int max = 100)
        {
            int number = 0;
            while (true)
            {
                string str = Guid.NewGuid().ToString();
                string numStr = null;
                foreach (char item in str)
                {
                    if (item >= 48 && item <= 58)
                    {
                        numStr += item;
                    }
                }
                number = int.Parse(numStr.Substring(0, max.ToString().Length));
                if (number >= min && number <= max)
                {
                    break;
                }
            }
            return number;
        }

        /// <summary>
        /// 删除表中所有行。
        /// </summary>
        /// <param name="dt"></param>
        private void ClearTable(ref DataTable dt)
        {
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                dt.Rows.RemoveAt(i);
            }
        }

        private void textBoxTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 )
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 往输入井添加课程设计规定的作业。
        /// </summary>
        private void AddWork()
        {
            DataRow dr = TableWork.NewRow();
            dr.SetField("到达时间", 1000);
            dr.SetField("作业名称", "JOB1");
            dr.SetField("估计运行时间", 25);
            dr.SetField("内存需要", 15);
            dr.SetField("磁带机需要",2);
            TableWork.Rows.Add(dr);
            DataRow dr2 = TableWork.NewRow();
            dr2.SetField("到达时间", 1020);
            dr2.SetField("作业名称", "JOB2");
            dr2.SetField("估计运行时间", 30);
            dr2.SetField("内存需要", 60);
            dr2.SetField("磁带机需要", 1);
            TableWork.Rows.Add(dr2);
            DataRow dr3 = TableWork.NewRow();
            dr3.SetField("到达时间", 1030);
            dr3.SetField("作业名称", "JOB3");
            dr3.SetField("估计运行时间", 10);
            dr3.SetField("内存需要", 50);
            dr3.SetField("磁带机需要", 3);
            TableWork.Rows.Add(dr3);
            DataRow dr4 = TableWork.NewRow();
            dr4.SetField("到达时间", 1035);
            dr4.SetField("作业名称", "JOB4");
            dr4.SetField("估计运行时间", 20);
            dr4.SetField("内存需要", 10);
            dr4.SetField("磁带机需要", 2);
            TableWork.Rows.Add(dr4);
            DataRow dr5 = TableWork.NewRow();
            dr5.SetField("到达时间", 1040);
            dr5.SetField("作业名称", "JOB5");
            dr5.SetField("估计运行时间", 15);
            dr5.SetField("内存需要", 30);
            dr5.SetField("磁带机需要", 2);
            TableWork.Rows.Add(dr5);
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (int.Parse(textBoxTime.Text) >= 2000)
            {
                NotificationSystem notificationSystem = new NotificationSystem();
                notificationSystem.PushNotification("注意", "超过2秒的时钟周期可能使调度执行时间变得很长。如果只是需要看清调度的每一步，请使用暂停和单步执行功能。", NotificationSystem.PresetColors.AttentionYellow);
            }
            /*if (int.Parse(textBoxTime.Text) <= 25)
            {
                NotificationSystem notificationSystem = new NotificationSystem();
                notificationSystem.PushNotification("注意", "过小的时钟周期可能使调度执行过程无法被看清。", NotificationSystem.PresetColors.AttentionYellow);
            }*/
            if (int.Parse(textBoxTime.Text) == 0)
            {
                NotificationSystem notificationSystem = new NotificationSystem();
                notificationSystem.PushNotification("错误", "时钟周期不允许为 0。", NotificationSystem.PresetColors.WarningRed);
                return;
            }
            timer1.Interval = int.Parse(textBoxTime.Text);
            TableWorkBackUp = TableWork.Copy();
            buttonStart.Enabled = false;
            textBoxTime.Enabled = false;
            panel1.Enabled = true;
            this.BackColor = Color.FromArgb(230, 110, 25);

            DataRow ramR = TableCPU.Rows[0];
            RefreshInterface(ramR);
            timer1.Enabled = true;
            timer1.Start();
        }

        /// <summary>
        /// 时钟周期循环。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            Clock();
        }

        /// <summary>
        /// 多道批处理系统的两级调度的核心方法。
        /// </summary>
        private void Clock()
        {
            DataRow ramR = TableCPU.Rows[0];
            LoadReady(ramR.Field<int>("当前时间"));//提交到达提交时间的进程
            LoadRAM();//为已提交进程分配资源
            if (isFirst == true)//第一次执行需要预先装入进程
            {
                LoadNextOrEnd();
                isFirst = false;
                RefreshInterface(ramR);
            }
            else
            {
                SeizeWorkDetect();//判断是否可以抢占并执行抢占
                if (ramR.IsNull("作业名称") == false) //CPU内有任务在执行
                {
                    if (ramR.Field<int>("剩余时间") > 0)//剩余时间不为0
                    {                        
                        ExecuteTask(ramR);//继续执行
                        RefreshInterface(ramR);
                    }
                    else//剩余时间为0
                    {
                        FinishTask(ramR);//结束任务
                    }
                }
                else//CPU没有任务在执行
                {
                    LoadNextOrEnd();//加载下一个任务或结束
                }
            }
        }

        /// <summary>
        /// 检查是否有符合抢占条件的进程。
        /// </summary>
        private void SeizeWorkDetect()
        {
            if (TableRAM.Rows.Count == 0)//内存就绪表没有任何行
            {
                return;
            }
            DataRow drcpu = TableCPU.Rows[0];
            DataRow drram = TableRAM.Rows[0];
            if (drcpu.Field<int>("剩余时间") > drram.Field<int>("剩余时间"))//符合抢占条件
            {
                //保存被抢占的进程
                DataRow dr = TableRAM.NewRow();
                dr.SetField("作业名称", drcpu.Field<string>("作业名称"));
                dr.SetField("到达时间", drcpu.Field<int>("到达时间"));
                dr.SetField("估计运行时间", drcpu.Field<int>("估计运行时间"));
                dr.SetField("剩余时间", drcpu.Field<int>("剩余时间"));
                dr.SetField("首地址", drcpu.Field<int>("首地址"));
                dr.SetField("内存需要", drcpu.Field<int>("内存需要"));
                dr.SetField("磁带机需要", drcpu.Field<int>("磁带机需要"));
                TableRAM.Rows.Add(dr);
                //输入抢占进程信息
                drcpu.SetField("作业名称", drram.Field<string>("作业名称"));
                drcpu.SetField("到达时间", drram.Field<int>("到达时间"));
                drcpu.SetField("估计运行时间", drram.Field<int>("估计运行时间"));
                drcpu.SetField("已用时间", 0);
                drcpu.SetField("剩余时间", drram.Field<int>("剩余时间"));
                drcpu.SetField("首地址", drram.Field<int>("首地址"));
                drcpu.SetField("内存需要", drram.Field<int>("内存需要"));
                drcpu.SetField("磁带机需要", drram.Field<int>("磁带机需要"));
                TableRAM.Rows.Remove(drram);
                //重排列内存表
                DataView dv3 = TableRAM.DefaultView;
                dv3.Sort = "剩余时间 ASC";//升序排列
                TableRAM = dv3.ToTable().Copy();
                dataGridViewRAM.DataSource = TableRAM;
            }
        }

        /// <summary>
        /// 继续执行任务。
        /// </summary>
        /// <param name="ramR"></param>
        private void ExecuteTask(DataRow ramR)
        {
            ramR.SetField("已用时间", ramR.Field<int>("已用时间") + 1);//已用时间+1
            ramR.SetField("剩余时间", ramR.Field<int>("剩余时间") - 1);//剩余时间-1
            ramR.SetField("累计运行时间", ramR.Field<int>("累计运行时间") + 1);//累计时间+1
            ramR.SetField("当前时间", TimeAdd(ramR.Field<int>("当前时间"), 1));//当前时间+1
        }

        /// <summary>
        /// 将剩余估计运行时间为 0 的作业送到结果表，并清空内存、重新计算各参数。
        /// </summary>
        /// <param name="ramR"></param>
        private void FinishTask(DataRow ramR)
        {
            //把运行完的作业加入结果表
            DataRow dr = TableResult.NewRow();
            dr.SetField("作业名称", ramR.Field<string>("作业名称"));
            dr.SetField("到达时间", ramR.Field<int>("到达时间"));
            dr.SetField("估计运行时间", ramR.Field<int>("估计运行时间"));
            dr.SetField("完成时间", ramR.Field<int>("当前时间"));
            dr.SetField("周转时间", TimeMinus(dr.Field<int>("完成时间"), dr.Field<int>("到达时间")));
            dr.SetField("带权周转时间", (float)(TimeMinus(dr.Field<int>("完成时间"), dr.Field<int>("到达时间")) / (float)(ramR.Field<int>("估计运行时间"))));
            TapeRemain = TapeRemain + ramR.Field<int>("磁带机需要");//还回磁带机
            TableResult.Rows.Add(dr);
            //回收内存
            RAMCollect(ramR.Field<int>("首地址"), ramR.Field<int>("内存需要"));
            CleanCPU();
            CalcAVG();//重新计算周转时间
            RefreshInterface(ramR);
            LoadNextOrEnd();//加载下一个任务或结束
        }

        /// <summary>
        /// 检查并装入到达到达时间的队列。
        /// </summary>
        /// <param name="currentTime">当前时间</param>
        private void LoadReady(int currentTime)
        {
            DataRow[] foundRow;
            foundRow = TableWork.Select("到达时间 = " + currentTime.ToString(), "估计运行时间 ASC");//寻找所有符合到达时间的行，估计运行时间升序排列
            foreach (DataRow row in foundRow)//加入就绪队列
            {
                TableReady.ImportRow(row);
            }
            foreach (DataRow row in foundRow)//从原队列删除
            {
                TableWork.Rows.Remove(row);
            }
        }

        /// <summary>
        /// 检查并装入符合资源请求的内存队列。
        /// </summary>
        private void LoadRAM()
        {
            DataView dv = TableReady.DefaultView;
            dv.Sort = "估计运行时间 DESC";//因为搜索是从下往上，所以倒序排列
            TableReady = dv.ToTable().Copy();
            for (int i = TableReady.Rows.Count -1; i >= 0; i--)
            {
                DataRow dr = TableReady.Rows[i];
                int add = ResourceAllocation(dr.Field<int>("内存需要"), dr.Field<int>("磁带机需要"));
                if (add == 99999)//不成功
                {
                }
                else
                {
                    DataRow dr2 = TableRAM.NewRow();
                    dr2.SetField("作业名称", dr.Field<string>("作业名称"));
                    dr2.SetField("到达时间", dr.Field<int>("到达时间"));
                    dr2.SetField("估计运行时间", dr.Field<int>("估计运行时间"));
                    dr2.SetField("剩余时间", dr.Field<int>("估计运行时间"));
                    dr2.SetField("首地址", add);
                    dr2.SetField("内存需要", dr.Field<int>("内存需要"));
                    dr2.SetField("磁带机需要", dr.Field<int>("磁带机需要"));
                    TableRAM.Rows.Add(dr2);
                    TableReady.Rows.Remove(dr);
                }
            }
            //重排列提交表
            DataView dv2 = TableReady.DefaultView;
            dv2.Sort = "估计运行时间 ASC";//升序排列
            TableReady = dv2.ToTable().Copy();
            dataGridViewReady.DataSource = TableReady;
            //重排列内存表
            DataView dv3 = TableRAM.DefaultView;
            dv3.Sort = "剩余时间 ASC";//升序排列
            TableRAM = dv3.ToTable().Copy();
            dataGridViewRAM.DataSource = TableRAM;
        }

        /// <summary>
        /// 清除内存里的无效的作业。
        /// </summary>
        private void CleanCPU()
        {
            DataRow ramR = TableCPU.Rows[0];
            ramR.SetField("作业名称", DBNull.Value);
            ramR.SetField("到达时间", 0);
            ramR.SetField("估计运行时间", 0);
            ramR.SetField("已用时间", 0);
            ramR.SetField("剩余时间", 0);
            ramR.SetField("首地址", 0);
            ramR.SetField("内存需要", 0);
            ramR.SetField("磁带机需要", 0);
        }

        /// <summary>
        /// 加载下一个就绪任务以及控制处理结束。
        /// </summary>
        private void LoadNextOrEnd()
        {
            if (TableRAM.DefaultView.Count >0)//还有就绪任务
            {
                DataTable dt = TableRAM.DefaultView.ToTable();
                DataRow ramR = TableCPU.Rows[0];
                DataRow readyR;
                readyR = dt.Rows[0];
                ramR.SetField("作业名称", readyR.Field<string>("作业名称"));
                ramR.SetField("到达时间", readyR.Field<int>("到达时间"));
                ramR.SetField("估计运行时间", readyR.Field<int>("估计运行时间"));
                ramR.SetField("已用时间",0);
                ramR.SetField("剩余时间", readyR.Field<int>("剩余时间"));
                ramR.SetField("首地址", readyR.Field<int>("首地址"));
                ramR.SetField("内存需要", readyR.Field<int>("内存需要"));
                ramR.SetField("磁带机需要", readyR.Field<int>("磁带机需要"));
                TableRAM.DefaultView.Delete(0);//删除第一行，也就是进内存的行
                RefreshInterface(ramR);
                RefreshRAMView();
            }
            else//无就绪任务
            {
                if (TableWork.DefaultView.Count > 0 || TableReady.DefaultView.Count > 0)//提交队列和就绪队列还有任务
                {
                    DataRow ramR = TableCPU.Rows[0];
                    RefreshInterface(ramR);
                    return;//跳过该时钟周期
                }
                else//提交队列无任务，处理结束
                {
                    RefreshRAMView();
                    timer1.Stop();
                    timer1.Enabled = false;
                    ClearTable(ref TableCPU);
                    this.BackColor = Color.FromArgb(0, 125, 236);
                    //MessageBox.Show("执行完成。");
                    NotificationSystem nf = new NotificationSystem();
                    nf.PushNotification("通知", "调度已执行完成。若要还原到初始状态，请点击“重置”按钮。", NotificationSystem.PresetColors.TipsBlue);
                    textBoxTime.Enabled = true;
                    buttonPause.Enabled = false;
                    buttonSingleStep.Enabled = false;
                    buttonSingleStep.Enabled = false;
                    buttonPause.Text = "暂停";
                    buttonPause.BackColor = Color.FromArgb(36, 37, 38);
                    isPause = false;
                }
            }
        }

        /// <summary>
        /// 计算平均周转时间和平均带权周转时间。
        /// </summary>
        private void CalcAVG()
        {
            float t = 0;
            float wt = 0;
            float count = TableResult.Rows.Count;
            foreach (DataRow dr in TableResult.Rows)
            {
                t = t + dr.Field<float>("周转时间");
                wt = wt + dr.Field<float>("带权周转时间");
            }
            labelAVGT.Text = (t / count).ToString();
            labelAVGWT.Text = (wt / count).ToString();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(0, 125, 236);
            timer1.Stop();
            timer1.Enabled = false;
            buttonSingleStep.Enabled = false;
            ClearTable(ref TableResult);
            ClearTable(ref TableWork);
            ClearTable(ref TableCPU);
            TableCPUAddNewRow();
            ClearTable(ref TableRAM);
            ClearTable(ref TableReady);
            ClearTable(ref TableExtraRAMAddress);
            DataRow dr = TableExtraRAMAddress.NewRow();
            dr.SetField("首地址", 0);
            dr.SetField("长度", 100);
            TableExtraRAMAddress.Rows.Add(dr);
            TapeRemain = 4;
            labelT.Text = "0";
            labelCT.Text = "1000";
            labelAVGT.Text = "0";
            labelAVGWT.Text = "0";
            RAMUtility = 0;
            labelRAMUtility.Text = "0K  / 100K";
            labelTapeRemain.Text = "0 / 4";
            buttonStart.Enabled = true;
            buttonPause.Enabled = true;
            buttonSingleStep.Enabled = true;
            panel1.Enabled = false;
            buttonPause.Text = "暂停";
            buttonPause.BackColor = Color.FromArgb(36, 37, 38);
            isPause = false;
            textBoxTime.Enabled = true;
            isFirst = true;
            ResetTapeView();
            ResetRAMView();
            for (int i = 0; i < TableWorkBackUp.Rows.Count; i++)
            {
                TableWork.ImportRow(TableWorkBackUp.Rows[i]);
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (isPause == true)//继续
            {
                this.BackColor = Color.FromArgb(255, 128, 0);
                buttonPause.Text = "暂停";
                buttonPause.BackColor = Color.FromArgb(36, 37, 38);
                timer1.Enabled = true;
                buttonSingleStep.Enabled = false;
                timer1.Start();
                isPause = false;
            }
            else//暂停
            {
                this.BackColor = Color.FromArgb(20, 152, 50);
                buttonPause.Text = "继续";
                buttonPause.BackColor = Color.FromArgb(17, 64, 98);
                timer1.Stop();
                timer1.Enabled = false;
                buttonSingleStep.Enabled = true;
                isPause = true;
            }
        }

        private void buttonSingleStep_Click(object sender, EventArgs e)
        {
            if (isPause == true)//继续单步
            {
                Clock();
            }
            else//运行状态下不允许单步
            {
            }
        }

        #region
        /// <summary>
        /// 点击任务栏实现窗口最小化与还原。
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;
                CreateParams cp = base.CreateParams;
                cp.Style = cp.Style | WS_MINIMIZEBOX;   // 允许最小化操作                  
                return cp;
            }
        }

        /// <summary>
        /// 拖动窗口的方法。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Controls_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        ///允许无边框窗口拖动————————————
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        ///————————End——————————
        #endregion

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        #region
        public class NotificationSystem
        {

            public NotificationSystem()
            {
            }
            /// <summary>
            /// 预设的背景颜色。
            /// </summary>
            public struct PresetColors
            {
                /// <summary>
                /// 警告，红色
                /// </summary>
                readonly public static Color WarningRed = Color.FromArgb(101, 27, 1);
                /// <summary>
                /// 注意，黄色
                /// </summary>
                readonly public static Color AttentionYellow = Color.FromArgb(102, 75, 0);
                /// <summary>
                /// 提示，蓝色
                /// </summary>
                readonly public static Color TipsBlue = Color.FromArgb(0, 43, 77);
                /// <summary>
                /// 标记，紫色
                /// </summary>
                readonly public static Color MarkPurple = Color.FromArgb(56, 34, 93);
                /// <summary>
                /// 通过，紫色
                /// </summary>
                readonly public static Color OKGreen = Color.FromArgb(8, 83, 8);
                /// <summary>
                /// 鼠标进入的颜色
                /// </summary>
                readonly internal static Color MouseOverBackColor = Color.FromArgb(100, 100, 100);
                /// <summary>
                /// 鼠标按下的颜色
                /// </summary>
                readonly internal static Color MouseDownBackColor = Color.FromArgb(90, 90, 90);
            }
            /// <summary>
            /// 文件的路径。
            /// </summary>
            string filePathMain = "";
            /// <summary>
            /// 通知背景坐标
            /// </summary>
            Point backPanelLocation = new Point(673, 545);
            /// <summary>
            /// 通知条被关闭的左坐标界限。
            /// </summary>
            int backPanelCloseLeftLimit = -100;
            /// <summary>
            /// 通知条被关闭的右坐标界限。
            /// </summary>
            int backPanelCloseRightLimit = 900;
            /// <summary>
            /// 通知大小
            /// </summary>
            Size backPanelSize = new Size(450, 100);
            /// <summary>
            /// 标题位置
            /// </summary>
            Point labelTopicLocation = new Point(10, 7);
            /// <summary>
            /// 标题大小
            /// </summary>
            Size labelTopicSize = new Size(156, 21);
            /// <summary>
            /// 信息位置
            /// </summary>
            Point labelMessageLocation = new Point(10, 42);
            /// <summary>
            /// 信息大小
            /// </summary>
            Size labelMessageSize = new Size(425, 45);
            /// <summary>
            /// 关闭按钮位置
            /// </summary>
            Point ButtonCloseLocation = new Point(409, 0);
            /// <summary>
            /// 关闭按钮大小
            /// </summary>
            Size ButtonCloseSize = new Size(41, 23);
            /// <summary>
            /// 文件夹按钮位置
            /// </summary>
            Point ButtonOpenLocation = new Point(240, 45);
            /// <summary>
            /// 文件夹按钮大小
            /// </summary>
            Size ButtonOpenSize = new Size(195, 40);
            /// <summary>
            /// 颜色偏移。
            /// </summary>
            int ColorOffset = 20;
            /// <summary>
            /// 鼠标是否拖动控件
            /// </summary>
            bool MoveFlag = false;
            /// <summary>
            /// x坐标
            /// </summary>
            int xPos = 0;
            /// <summary>
            /// y坐标
            /// </summary>
            int yPos = 0;

            readonly Font font = new Font("微软雅黑", 12);

            /// <summary>
            /// 鼠标按下时传递控件坐标。
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Control_MouseDown(object sender, MouseEventArgs e)
            {
                MoveFlag = true;//已经按下.
                xPos = e.X;//当前x坐标.
                yPos = e.Y;//当前y坐标.
            }
            /// <summary>
            /// 鼠标按下时传递控件坐标。
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Control_MouseDown2(object sender, MouseEventArgs e)
            {
                MoveFlag = true;//已经按下.
                xPos = e.X;//当前x坐标.
                yPos = e.Y;//当前y坐标.
                //MessageBox.Show(xPos.ToString() + "," + yPos.ToString());
            }
            /// <summary>
            /// 鼠标松开时停止。
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Control_MouseUp(object sender, MouseEventArgs e)
             {
                 MoveFlag = false;
                Control ss = (Control)sender;
                if (ss.Location != backPanelLocation)
                {
                    if (ss.Location.X >= backPanelCloseRightLimit || ss.Location.X <= backPanelCloseLeftLimit)
                    {
                        formMain.panelBG.Controls.Remove(ss);//移除该通知控件 
                        return;
                    }
                    ss.Location = backPanelLocation;
                }
            }
            /// <summary>
            /// 鼠标松开时停止父控件。
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Control_MouseUp2(object sender, MouseEventArgs e)
            {
                MoveFlag = false;
                Control ss = (Control)sender;
                Control sx = ss.Parent;
                if (sx.Location != backPanelLocation)
                {
                    if (sx.Location.X >= backPanelCloseRightLimit || sx.Location.X <= backPanelCloseLeftLimit)
                    {
                        formMain.panelBG.Controls.Remove(sx);//移除该通知控件 
                        return;
                    }
                    sx.Location = backPanelLocation;
                }
            }
            /// <summary>
            /// 与鼠标的移动同步移动控件。
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Control_MouseMove(object sender, MouseEventArgs e)
            {
                Control ss = (Control)sender;
                if (MoveFlag)
                {
                    ss.Left += Convert.ToInt16(e.X - xPos);//设置x坐标.
                    //ss.Top += Convert.ToInt16(e.Y - yPos);//设置y坐标.
                }
            }
            /// <summary>
            /// 与鼠标的移动同步移动父控件。
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Control_MouseMove2(object sender, MouseEventArgs e)
            {
                Control ss = (Control)sender;
                Control sx = ss.Parent;
                if (MoveFlag)
                {
                    sx.Left += Convert.ToInt16(e.X - xPos);//设置x坐标.
                    //sx.Top += Convert.ToInt16(e.Y - yPos);//设置y坐标.
                }
                sx.BackColor = Color.FromArgb(255, sx.BackColor.R, sx.BackColor.G, sx.BackColor.B);
                foreach (Control sd in sx.Controls)
                {
                    sd.BackColor = Color.FromArgb(255, sd.BackColor.R, sd.BackColor.G, sd.BackColor.B);
                }
            }

            /// <summary>
            /// 推送通知。
            /// </summary>
            /// <param name="message">通知内容</param>
            /// <param name="topic">通知标题 </param>
            /// <param name="backColor">通知的背景颜色。可以使用预设好的背景颜色类 NotificationSystem.PresetColors 里面的颜色。</param>
            /// <param name="filePath">可选参数。指示是否显示“打开文件夹”按钮，以及该文件夹的路径。若无此需求请不要提供该参数，保留默认。</param>
            public void PushNotification(string topic, string message, Color backColor, string filePath = "null")
            {
                Panel notiPanel = new Panel()
                {
                    Location = backPanelLocation,//标定坐标
                    Size = backPanelSize,//标定大小
                    BackColor = backColor,
                    BorderStyle = BorderStyle.FixedSingle,
                    Visible = true,
                };//新建panel
                SetDouble(notiPanel);
                notiPanel.MouseDown += new MouseEventHandler(Control_MouseDown);
                notiPanel.MouseUp += new MouseEventHandler(Control_MouseUp);
                notiPanel.MouseMove += new MouseEventHandler(Control_MouseMove);
                formMain.panelBG.Controls.Add(notiPanel);//往窗口添加控件
                CreateSubLabel(ref notiPanel, topic, backColor, labelTopicLocation, labelTopicSize);//标题
                CreateSubLabel(ref notiPanel, message, backColor, labelMessageLocation, labelMessageSize);//内容
                CreateSubButton(ref notiPanel, "", backColor, ButtonCloseLocation, ButtonCloseSize);//关闭按钮
                if (filePath != "null")
                {
                    filePathMain = filePath;
                    CreateSubButton(ref notiPanel, "打开文件夹", backColor, ButtonOpenLocation, ButtonOpenSize, 1);//打开文件夹
                }
                notiPanel.BringToFront();
            }

            /// <summary>
            /// 为控件提供双缓冲，防止画面撕裂和闪烁。
            /// </summary>
            /// <param name="cc"></param>
            public static void SetDouble(Control cc)
            {
                cc.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(cc, true, null);
            }

            private delegate void Dg(ref Panel panel, string content, Color backColor, Point location, Size size);
            /// <summary>
            /// 往通知 panel 添加 label。
            /// </summary>
            /// <param name="panel">通知 panel</param>
            /// <param name="content">label 内容</param>
            /// <param name="backColor">label 背景颜色</param>
            /// <param name="location">label 位置</param>
            /// <param name="size">label 大小</param>
            private void CreateSubLabel(ref Panel panel, string content, Color backColor, Point location, Size size)
            {
                Label label = new Label()//设置属性
                {
                    AutoSize = false,
                    BorderStyle = BorderStyle.None,
                    BackColor = backColor,
                    ForeColor = Color.White,
                    Font = font,
                    Text = content,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Location = location,
                    Size = size,
                    Visible = true
                };
                SetDouble(label);
                label.MouseDown += new MouseEventHandler(Control_MouseDown2);
                label.MouseUp += new MouseEventHandler(Control_MouseUp2);
                label.MouseMove += new MouseEventHandler(Control_MouseMove2);
                panel.Controls.Add(label);
                label.BringToFront();
            }

            /// <summary>
            /// 往通知 panel 添加 button。
            /// </summary>
            /// <param name="panel">通知 panel</param>
            /// <param name="content">label 内容</param>
            /// <param name="backColor">label 背景颜色</param>
            /// <param name="location">label 位置</param>
            /// <param name="size">label 大小</param>
            /// <param name="borderSize">边框大小，默认为 0</param>
            private void CreateSubButton(ref Panel panel, string content, Color backColor, Point location, Size size, int borderSize = 0)
            {
                Button button = new Button()
                {//属性设置
                    AutoSize = false,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = backColor,
                    ForeColor = Color.White,
                    Font = font,
                    Text = content,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = location,
                    Size = size,
                    Visible = true
                };
                FlatButtonAppearance flatButtonAppearance = button.FlatAppearance;
                flatButtonAppearance.BorderColor = Color.Gray;
                flatButtonAppearance.MouseDownBackColor = PresetColors.MouseDownBackColor;
                flatButtonAppearance.MouseOverBackColor = PresetColors.MouseOverBackColor;
                flatButtonAppearance.BorderSize = borderSize;
                if (borderSize == 0)//边框值为0，这是一个关闭按钮
                {
                    //button.BackgroundImage = Properties.Resources.guanbi3;
                    button.Text = "x";
                    button.Font = new Font("微软雅黑", 9);
                    button.BackgroundImageLayout = ImageLayout.Zoom;
                    button.Click += new EventHandler(CloseNoti_Click);//绑定点击事件
                }
                else//打开文件夹按钮
                {
                    button.BackColor = Color.Gray;
                    //button.Click += new EventHandler(Button_Click);//绑定点击事件
                }
                SetDouble(button);
                panel.Controls.Add(button);
                button.BringToFront();
            }

            /// <summary>
            /// 通知关闭按钮的行为。
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void CloseNoti_Click(object sender, EventArgs e)
            {
                Button button = (Button)(sender);//获取触发本消息处理的 Button 控件
                foreach (Panel panel1 in formMain.panelBG.Controls)
                {
                    if (panel1.Controls.Contains(button) == true)
                    {
                        formMain.panelBG.Controls.Remove(panel1);//移除该通知控件 
                        break;
                    }
                }
            }
        }
        #endregion

        private void buttonMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
