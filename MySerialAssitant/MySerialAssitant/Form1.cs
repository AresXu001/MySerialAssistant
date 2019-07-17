using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO.Ports;          //系统IO接口命名空间，引入之后才能添加串口接收函数
using System.Drawing.Drawing2D;

namespace MySerialAssitant
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataRecv); //添加串口接收函数
            serialPort1.Encoding = Encoding.GetEncoding("GB2312");   //串口编码引入GB2312编码(汉字编码)
        }

        //自动扫描可用串口并添加到串口号列表上
        private void SearchAndAddSerialToComboBox(SerialPort MyPort, ComboBox MyBox)
        {                                                               //将可用端口号添加到ComboBox
            //string[] MyString = new string[20];                         //最多容纳20个，太多会影响调试效率
            string Buffer;                                              //缓存
            MyBox.Items.Clear();                                        //清空ComboBox内容
            //int count = 0;
            for (int i = 1; i < 26; i++)                                //循环
            {
                try                                                     //核心原理是依靠try和catch完成遍历
                {
                    Buffer = "COM" + i.ToString();
                    MyPort.PortName = Buffer;
                    MyPort.Open();                                      //如果失败，后面的代码不会执行
                                                                        // MyString[count] = Buffer;
                    MyBox.Items.Add(Buffer);                            //打开成功，添加至下俩列表
                    MyPort.Close();                                     //关闭
                }
                catch
                {

                }
            }
            //MyBox.Text = MyString[0];                                   //初始化
        }

        private void port_DataRecv(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (radioButton1.Checked)  //如果接收是字符模式
                {
                    textBox1.AppendText(serialPort1.ReadExisting());  //接收到汉字，串口类自动处理，不需要特殊处理
                }
                else
                {
                    byte data;
                    data = (byte)serialPort1.ReadByte();
                    string str = Convert.ToString(data, 16).ToUpper();//转换为大写十六进制字符串
                    textBox1.AppendText("0x" + (str.Length == 1 ? "0" + str : str) + " ");//空位补“0”
                }
            }
            catch
            {
//                MessageBox.Show("接收数据错误", "提示");
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.IO.File.WriteAllText(@".\串口接收数据.txt", textBox1.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton3.Checked = true;

            pictureBox1.BackColor = Color.Gray;

            //下列代码将picturebox1画成圆形
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(pictureBox1.ClientRectangle);
            Region region = new Region(gp);
            pictureBox1.Region = region;
            gp.Dispose();
            region.Dispose();

            //            button6.BackgroundImage = Properties.Resources.OFF33x33;

            comboBox1.Text = "115200";                            //默认波特率为115200
            SearchAndAddSerialToComboBox(serialPort1,comboBox2);  //自动扫描可用串口并添加到串口号列表上
           
            Control.CheckForIllegalCrossThreadCalls = false;   //加载时 取消跨线程检查(接收数据会跨线程会产生错误)
        }

        //清除接收窗口按钮
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Clear();         //清除接收窗口
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)   //如果串口是打开的
            {
                try
                {
                    serialPort1.Close();
                    button1.Text = "打开串口";
                    //                    button6.BackgroundImage = Properties.Resources.OFF33x33;
                    pictureBox1.Image = Properties.Resources.OFF33x33;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                }
                catch
                {

                }

            }
            else
            {
                try
                {
                    serialPort1.PortName = comboBox2.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox1.Text, 10);
                    serialPort1.Open();
                    button1.Text = "关闭串口";
                    //                   button6.BackgroundImage = Properties.Resources.ON33x33;
                    pictureBox1.Image = Properties.Resources.ON33x33;
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                }
                catch
                {
                    MessageBox.Show("打开串口失败！", "提示");
                    System.Media.SystemSounds.Asterisk.Play();
                }
            }
        }
          
        //扫描串口按钮
        private void button5_Click(object sender, EventArgs e)
        {
            SearchAndAddSerialToComboBox(serialPort1, comboBox2);  //自动扫描可用串口并添加到串口号列表上
        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[1];

            if (serialPort1.IsOpen && comboBox2.Text != "")  //如果串口是打开的切发送窗口有数据
            {
                if (radioButton3.Checked)     //如果是发送是字符模式
                {
                    try
                    {
                        serialPort1.Write(textBox2.Text);
                    }
                    catch
                    {
                        serialPort1.Close();
                        button1.Text = "打开串口";
                        MessageBox.Show("写入数据错误", "提示");
                    }
                }
                else
                {
                    try
                    {
                        for (int i = 0; i < (textBox2.Text.Length - textBox2.Text.Length % 2) / 2; i++)//取余3运算作用是防止用户输入的字符为奇数个
                        {
                            data[0] = Convert.ToByte(textBox2.Text.Substring(i * 2, 2), 16);
                            serialPort1.Write(data, 0, 1);//循环发送（如果输入字符为0A0BB,则只发送0A,0B）
                        }
                        if (textBox2.Text.Length % 2 != 0)//剩下一位单独处理
                        {
                            data[0] = Convert.ToByte(textBox2.Text.Substring(textBox2.Text.Length - 1, 1), 16);//单独发送B（0B）
                            serialPort1.Write(data, 0, 1);//发送
                        }
                    }
                    catch
                    {
                        serialPort1.Close();
                        button1.Text = "打开串口";
                        MessageBox.Show("写入数据错误", "提示");
                    }
                }

            }
        }
    }
}
