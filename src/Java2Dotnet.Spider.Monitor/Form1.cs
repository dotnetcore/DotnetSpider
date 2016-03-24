using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Java2Dotnet.Spider.Extension.Scheduler;

namespace Java2Dotnet.Spider.Monitor
{
	public partial class Form1 : Form
	{
		private RedisSchedulerManager _manager;
		private string _selectedIdentify = string.Empty;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			string host = ConfigurationManager.AppSettings["redishost"];
			string pass = ConfigurationManager.AppSettings["redishostpass"];

			if (!string.IsNullOrEmpty(host))
			{
				_manager = new RedisSchedulerManager(host, pass);
			}

			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					if ((!IsDisposed || IsHandleCreated || components != null))
					{
						Invoke(new Action(RefreshTask));
					}
					Thread.Sleep(60000);
				}
			});

			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					if ((!IsDisposed || IsHandleCreated || components != null))
					{
						Invoke(new Action(() =>
						{
							if (!string.IsNullOrEmpty(_selectedIdentify))
							{
								SpiderStatus spiderStatus = _manager.GetTaskStatus(_selectedIdentify);

								tbErrorPageCount.Text = spiderStatus.ErrorPageCount.ToString();
								tbLeftRequestCount.Text = spiderStatus.LeftPageCount.ToString();
								tbTotalRequestCount.Text = spiderStatus.TotalPageCount.ToString();
								tbPagePerSecond.Text = spiderStatus.PagePerSecond.ToString(CultureInfo.InvariantCulture);
								tbRunningProcessCount.Text = spiderStatus.AliveThreadCount.ToString();
								tbProcessCount.Text = spiderStatus.ThreadCount.ToString();
								tbStartTime.Text = spiderStatus.StartTime.ToString(CultureInfo.InvariantCulture);
								tbEndTime.Text = spiderStatus.EndTime.ToString(CultureInfo.InvariantCulture);
								tbTaskStatus.Text = spiderStatus.Status;
							}
							else
							{
								SentEmptyInfo();
							}
						}));
					}
					Thread.Sleep(1000);
				}
			});
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void RefreshTask()
		{
			listBox1.Items.Clear();
			IDictionary<string, double> taskList = _manager.GetTaskList(0, 1000);
			foreach (var task in taskList)
			{
				listBox1.Items.Add(task.Key);
			}

			if (taskList.Keys.Contains(_selectedIdentify))
			{
				listBox1.SelectedItem = _selectedIdentify;
			}
			else
			{
				_selectedIdentify = string.Empty;
			}
		}

		private void SentEmptyInfo()
		{
			tbErrorPageCount.Text = "";
			tbLeftRequestCount.Text = "";
			tbTotalRequestCount.Text = "";
			tbPagePerSecond.Text = "";
			tbRunningProcessCount.Text = "";
			tbProcessCount.Text = "";
			tbStartTime.Text = "";
			tbEndTime.Text = "";
			tbTaskStatus.Text = "";
		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listBox1.SelectedItem != null)
			{
				_selectedIdentify = listBox1.SelectedItem.ToString();
			}
		}


		private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RefreshTask();
		}

		private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show(this, @"您确定要删除此任务的记录吗?", @"警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) ==
				DialogResult.Yes)
			{
				//SpiderStatus spiderStatus = manager.GetTaskStatus(_selectedIdentify);
				//if (spiderStatus.Status != "Running" && spiderStatus.Status != "Init")
				//{
				_manager.RemoveTask(_selectedIdentify);
				RefreshTask();
				//}
				//else
				//{
				//	MessageBox.Show(this, @"只能删除已完成或停止的任务", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
				//}
			}
		}

		private void btnClearDb_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show(this, @"确定要清空数据库吗?", @"警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				_manager.ClearDb();
				RefreshTask();
			}
		}
	}
}
