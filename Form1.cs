using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

/// <summary>
/// 不建议使用 datatable 的下标索引获取和设置值。
/// C#提供了 DataRowExtensions 类的 datarow.setField<T>() 和 dataview.Field<T>() 泛型方法，
/// 可以强类型地获取和设置指定列的值。
/// </summary>
namespace 存储管理
{
    public partial class Form1 : Form
    {
        DataTable TableResult;
        DataTable TableWork;
        DataTable TableWorkBackUp;
        DataTable TableReady;
        DataTable TableRAM;
        Queue<DataRow> QueueReady;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitDataGridView();
            InitTableResult();
            InitTableWork();
            InitTableRAM();
            InitTableReady();
            QueueReady = new Queue<DataRow>();
            dataGridViewResult.DataSource = TableResult;
            dataGridViewWork.DataSource = TableWork;
            dataGridViewRAM.DataSource = TableRAM;
            dataGridViewReady.DataSource = TableReady;
            TableWork.DefaultView.Sort = "提交时间 ASC";
            TableReady.DefaultView.Sort = "提交时间 ASC";
        }


        private void InitTableResult()
        {
            TableResult = new DataTable();
            TableResult.Columns.Add("提交时间", typeof(int));//5
            TableResult.Columns.Add("作业名称", typeof(string));//0
            TableResult.Columns.Add("服务时间", typeof(int));//2
            TableResult.Columns.Add("开始时间", typeof(int));//1
            TableResult.Columns.Add("完成时间", typeof(int));//3
            TableResult.Columns.Add("静态优先级", typeof(int));//4
            TableResult.Columns.Add("周转时间", typeof(float));//6
            TableResult.Columns.Add("带权周转时间", typeof(float));//7
        }
        private void InitTableWork()
        {
            TableWork = new DataTable();
            TableWork.Columns.Add("提交时间", typeof(int));//0
            TableWork.Columns.Add("作业名称", typeof(string));//1
            TableWork.Columns.Add("服务时间", typeof(int));//2
            TableWork.Columns.Add("静态优先级", typeof(int));//3
        }
        private void InitTableReady()
        {
            TableReady = new DataTable();
            TableReady.Columns.Add("提交时间", typeof(int));//0
            TableReady.Columns.Add("作业名称", typeof(string));//1
            TableReady.Columns.Add("服务时间", typeof(int));//2
            TableReady.Columns.Add("静态优先级", typeof(int));//3
        }
        private void InitTableRAM()
        {
            TableRAM = new DataTable();
            TableRAM.Columns.Add("提交时间", typeof(int));//8
            TableRAM.Columns.Add("作业名称", typeof(string));//0
            TableRAM.Columns.Add("服务时间", typeof(int));//2
            TableRAM.Columns.Add("开始时间", typeof(int));//1
            TableRAM.Columns.Add("已用时间", typeof(int));//3
            TableRAM.Columns.Add("剩余时间", typeof(int));//4
            TableRAM.Columns.Add("时间片剩余", typeof(int));//6
            TableRAM.Columns.Add("静态优先级", typeof(int));//5
            TableRAM.Columns.Add("累计运行时间", typeof(int));//7
        }


        private void InitDataGridView()
        {
            dataGridViewResult.AutoGenerateColumns = true;
            dataGridViewWork.AutoGenerateColumns = true;
            dataGridViewRAM.AutoGenerateColumns = true;
            dataGridViewReady.AutoGenerateColumns = true;
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
        /// 将队列任务加进内存。
        /// </summary>
        private void LoadIntoRAM()
        {
            if (TableWork.Rows.Count > 0)//队列不为空
            {

            }
            else// 队列为空
            {

            }
        }

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

        private void buttonRandomWork_Click(object sender, EventArgs e)
        {
            int Intime = RandomNumberGenerator(0, 99);
            int time = RandomNumberGenerator(1, 20);
            DataRow dr = TableWork.NewRow();
            dr.SetField("提交时间", Intime);
            dr.SetField("作业名称", Guid.NewGuid().ToString().Substring(0, 4));
            dr.SetField("服务时间", time);
            dr.SetField("静态优先级", RandomNumberGenerator(0, 20));
            TableWork.Rows.Add(dr);
            buttonStart.Enabled = true;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            timer1.Enabled = false;
            ClearTable(ref TableResult);
            ClearTable(ref TableWork);
            ClearTable(ref TableRAM);
            ClearTable(ref TableReady);
            buttonStart.Enabled = false;
            panel1.Enabled = true;
            groupBox1.Enabled = true;
            buttonReset.Enabled = false;
            //radioButtonFCFS.Checked = true;
        }

        private void radioButtonSJF_Click(object sender, EventArgs e)
        {
            TableReady.DefaultView.Sort = "服务时间 ASC";
        }

        private void radioButtonPSA_Click(object sender, EventArgs e)
        {
            TableReady.DefaultView.Sort = "静态优先级 DESC";
        }

        private void radioButtonHRRM_Click(object sender, EventArgs e)
        {
            TableReady.DefaultView.Sort = "动态优先级 DESC";
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            timer1.Interval = int.Parse(textBox1.Text);
            TableWorkBackUp = TableWork.Copy();
            groupBox1.Enabled = false;
            panel1.Enabled = false;
            buttonStart.Enabled = false;

            DataRow ramR = TableRAM.NewRow();
            ramR.SetField("作业名称", DBNull.Value);
            ramR.SetField("开始时间", DBNull.Value);
            ramR.SetField("服务时间", DBNull.Value);
            ramR.SetField("已用时间", DBNull.Value);
            ramR.SetField("剩余时间", DBNull.Value);
            ramR.SetField("静态优先级", DBNull.Value);
            ramR.SetField("时间片剩余", DBNull.Value);
            ramR.SetField("提交时间", DBNull.Value);
            ramR.SetField("累计运行时间", 0);
            TableRAM.Rows.Add(ramR);

            if (radioButtonRR.Checked == true)
            {
                dataGridViewReady.DataSource = QueueReady;
            }
            timer1.Enabled = true;
            timer1.Start();
        }

        /// <summary>
        /// 时钟周期
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (radioButtonRR.Checked == true)
            {

            }
            else
            {
                DataRow ramR = TableRAM.Rows[0];
                LoadReady(ramR.Field<int>("累计运行时间"));
                //有任务剩余时间不等于0，继续运行任务
                if (ramR.IsNull("作业名称") == false) 
                {
                    if (ramR.Field<int>("剩余时间") > 0)
                    {
                        ramR.SetField("已用时间", ramR.Field<int>("已用时间") + 1);//已用时间+1
                        ramR.SetField("剩余时间", ramR.Field<int>("剩余时间") - 1);//剩余时间-1
                        ramR.SetField("累计运行时间", ramR.Field<int>("累计运行时间") + 1);//累计时间+1
                    }
                    else//时间为0
                    {
                        //把运行完的作业加入结果表
                        DataRow dr = TableResult.NewRow();
                        dr.SetField("作业名称", ramR.Field<string>("作业名称"));
                        dr.SetField("开始时间", ramR.Field<int>("开始时间"));
                        dr.SetField("服务时间", ramR.Field<int>("服务时间"));
                        dr.SetField("静态优先级", ramR.Field<int>("静态优先级"));
                        dr.SetField("提交时间", ramR.Field<int>("提交时间"));
                        dr.SetField("完成时间", ramR.Field<int>("累计运行时间"));
                        dr.SetField("周转时间", (float)(dr.Field<int>("完成时间")) - (float)(dr.Field<int>("提交时间")));
                        dr.SetField("带权周转时间", ((float)(dr.Field<int>("完成时间")) - (float)(dr.Field<int>("提交时间"))) / (float)(ramR.Field<int>("服务时间")));
                        TableResult.Rows.Add(dr);

                        CleanRAM();
                        ramR.SetField("累计运行时间", ramR.Field<int>("累计运行时间") + 1);//累计时间+1
                        CalcAVG();
                        LoadNext();//加载下一个任务或结束
                    }
                }
                else
                {
                    ramR.SetField("累计运行时间", ramR.Field<int>("累计运行时间") + 1);//累计时间+1
                    LoadNext();//加载下一个任务或结束
                }
            }
        }

        private void RR()
        {
            
        }

        /// <summary>
        /// 检查并装入到达提交时间的队列。
        /// </summary>
        /// <param name="totalTime">累计时间</param>
        private void LoadReady(int totalTime)
        {
            DataRow[] foundRow;
            foundRow = TableWork.Select("提交时间 = " + totalTime.ToString(), "服务时间 ASC");//寻找所有符合提交时间的行，服务时间升序排列
            foreach (DataRow row in foundRow)//加入就绪队列
            {
                TableReady.ImportRow(row);
            }
            foreach (DataRow row in foundRow)//从原队列删除
            {
                TableWork.Rows.Remove(row);
            }
        }


        private void LoadReady_Queue(int totalTime)
        {
            DataRow[] foundRow;
            foundRow = TableWork.Select("提交时间 = " + totalTime.ToString(), "服务时间 ASC");//寻找所有符合提交时间的行，服务时间升序排列
            foreach (DataRow row in foundRow)//加入就绪队列
            {
                QueueReady.Enqueue(row);
            }
            foreach (DataRow row in foundRow)//从原队列删除
            {
                TableWork.Rows.Remove(row);
            }
        }

        /// <summary>
        /// 清除内存。
        /// </summary>
        private void CleanRAM()
        {
            DataRow ramR = TableRAM.Rows[0];
            ramR.SetField("作业名称", DBNull.Value);
            ramR.SetField("开始时间", DBNull.Value);
            ramR.SetField("服务时间", DBNull.Value);
            ramR.SetField("已用时间", DBNull.Value);
            ramR.SetField("剩余时间", DBNull.Value);
            ramR.SetField("静态优先级", DBNull.Value);
            ramR.SetField("时间片剩余", DBNull.Value);
            ramR.SetField("提交时间", DBNull.Value);
        }
        /// <summary>
        /// 加载下一个就绪任务以及控制处理结束。
        /// </summary>
        private void LoadNext()
        {
            if (TableReady.DefaultView.Count >0)//还有就绪任务
            {
                DataTable dt = new DataTable();
                dt = TableReady.DefaultView.ToTable();
                DataRow ramR = TableRAM.Rows[0];
                DataRow readyR = dt.Rows[0];
                ramR.SetField("作业名称", readyR.Field<string>("作业名称"));
                ramR.SetField("开始时间", ramR.Field<int>("累计运行时间"));
                ramR.SetField("服务时间", readyR.Field<int>("服务时间"));
                ramR.SetField("静态优先级", readyR.Field<int>("静态优先级"));
                ramR.SetField("已用时间",0);
                ramR.SetField("剩余时间", readyR.Field<int>("服务时间"));
                ramR.SetField("提交时间", readyR.Field<int>("提交时间"));
                TableReady.DefaultView.Delete(0);//删除第一行，也就是进内存的行
            }
            else//无就绪任务
            {
                if (TableWork.DefaultView.Count > 0)//提交队列还有任务
                {
                    return;//跳过该时钟周期
                }
                else//提交队列无任务，处理结束
                {
                    timer1.Stop();
                    timer1.Enabled = false;
                    ClearTable(ref TableRAM);
                    MessageBox.Show("执行完成。");
                    buttonReset.Enabled = true;
                }
            }
        }
        private void LoadNext_Queue()
        {
            if (QueueReady.Count > 0)//还有就绪任务
            {
                //DataTable dt = new DataTable();
                //dt = TableReady.DefaultView.ToTable();
                DataRow ramR = TableRAM.Rows[0];
                DataRow readyR = QueueReady.Dequeue();
                ramR.SetField("作业名称", readyR.Field<string>("作业名称"));
                ramR.SetField("开始时间", ramR.Field<int>("累计运行时间"));
                ramR.SetField("服务时间", readyR.Field<int>("服务时间"));
                ramR.SetField("静态优先级", readyR.Field<int>("静态优先级"));
                ramR.SetField("已用时间", 0);
                ramR.SetField("剩余时间", readyR.Field<int>("服务时间"));
                ramR.SetField("提交时间", readyR.Field<int>("提交时间"));
                //TableReady.DefaultView.Delete(0);//删除第一行，也就是进内存的行
            }
            else//无就绪任务
            {
                if (TableWork.DefaultView.Count > 0)//提交队列还有任务
                {
                    return;//跳过该时钟周期
                }
                else//提交队列无任务，处理结束
                {
                    timer1.Stop();
                    timer1.Enabled = false;
                    ClearTable(ref TableRAM);
                    MessageBox.Show("执行完成。");
                    buttonReset.Enabled = true;
                    dataGridViewReady.DataSource = TableReady;
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
            timer1.Stop();
            timer1.Enabled = false;
            ClearTable(ref TableResult);
            ClearTable(ref TableWork);
            ClearTable(ref TableRAM);
            ClearTable(ref TableReady);
            buttonStart.Enabled = true;
            panel1.Enabled = true;
            groupBox1.Enabled = true;
            for (int i = 0; i < TableWorkBackUp.Rows.Count; i++)
            {
                TableWork.ImportRow(TableWorkBackUp.Rows[i]);
            }
            buttonReset.Enabled = false;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 49 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void radioButtonRR_Click(object sender, EventArgs e)
        {
            TableReady.DefaultView.Sort = "提交时间 ASC";
        }

        private void buttonAddWork_Click(object sender, EventArgs e)
        {
            if (textBoxInTime.Text != "" && textBoxName.Text != "" && textBoxTime.Text != "" && textBoxPriority.Text != "")
            {
                if (int.Parse(textBoxInTime.Text) > 20)
                {
                    textBoxInTime.Text = "20";
                }
                if (int.Parse(textBoxPriority.Text) > 20)
                {
                    textBoxPriority.Text = "20";
                }
                if (int.Parse(textBoxTime.Text) > 20)
                {
                    textBoxTime.Text = "20";
                }
                if (int.Parse(textBoxTime.Text) == 0)
                {
                    textBoxTime.Text = "0";
                }
                DataRow dr = TableWork.NewRow();
                dr.SetField("提交时间", int.Parse(textBoxInTime.Text));
                dr.SetField("作业名称", textBoxName.Text);
                dr.SetField("服务时间", int.Parse(textBoxTime.Text));
                dr.SetField("静态优先级", int.Parse(textBoxPriority.Text));
                TableWork.Rows.Add(dr);
                buttonStart.Enabled = true;
            }
        }
    }
}
