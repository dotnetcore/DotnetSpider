namespace Java2Dotnet.Spider.Monitor
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.刷新ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.删除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tbRunningProcessCount = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.tbPagePerSecond = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tbErrorPageCount = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tbTotalRequestCount = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tbLeftRequestCount = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tbProcessCount = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.tbStartTime = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.tbEndTime = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.tbTaskStatus = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.btnClearDb = new System.Windows.Forms.Button();
			this.contextMenuStrip1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// listBox1
			// 
			this.listBox1.ContextMenuStrip = this.contextMenuStrip1;
			this.listBox1.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 20;
			this.listBox1.Location = new System.Drawing.Point(4, 5);
			this.listBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(296, 664);
			this.listBox1.TabIndex = 0;
			this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.刷新ToolStripMenuItem,
            this.删除ToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(101, 48);
			// 
			// 刷新ToolStripMenuItem
			// 
			this.刷新ToolStripMenuItem.Name = "刷新ToolStripMenuItem";
			this.刷新ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
			this.刷新ToolStripMenuItem.Text = "刷新";
			this.刷新ToolStripMenuItem.Click += new System.EventHandler(this.刷新ToolStripMenuItem_Click);
			// 
			// 删除ToolStripMenuItem
			// 
			this.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
			this.删除ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
			this.删除ToolStripMenuItem.Text = "删除";
			this.删除ToolStripMenuItem.Click += new System.EventHandler(this.删除ToolStripMenuItem_Click);
			// 
			// tbRunningProcessCount
			// 
			this.tbRunningProcessCount.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.tbRunningProcessCount.Location = new System.Drawing.Point(91, 77);
			this.tbRunningProcessCount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbRunningProcessCount.Name = "tbRunningProcessCount";
			this.tbRunningProcessCount.ReadOnly = true;
			this.tbRunningProcessCount.Size = new System.Drawing.Size(279, 26);
			this.tbRunningProcessCount.TabIndex = 3;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.label5.Location = new System.Drawing.Point(4, 72);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(79, 20);
			this.label5.TabIndex = 21;
			this.label5.Text = "线程运行数";
			// 
			// tbPagePerSecond
			// 
			this.tbPagePerSecond.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.tbPagePerSecond.Location = new System.Drawing.Point(91, 185);
			this.tbPagePerSecond.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbPagePerSecond.Name = "tbPagePerSecond";
			this.tbPagePerSecond.ReadOnly = true;
			this.tbPagePerSecond.Size = new System.Drawing.Size(279, 26);
			this.tbPagePerSecond.TabIndex = 6;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.label4.Location = new System.Drawing.Point(4, 180);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(79, 20);
			this.label4.TabIndex = 19;
			this.label4.Text = "每秒页面数";
			// 
			// tbErrorPageCount
			// 
			this.tbErrorPageCount.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.tbErrorPageCount.ForeColor = System.Drawing.Color.Red;
			this.tbErrorPageCount.Location = new System.Drawing.Point(91, 149);
			this.tbErrorPageCount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbErrorPageCount.Name = "tbErrorPageCount";
			this.tbErrorPageCount.ReadOnly = true;
			this.tbErrorPageCount.Size = new System.Drawing.Size(279, 26);
			this.tbErrorPageCount.TabIndex = 5;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.label3.Location = new System.Drawing.Point(4, 144);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(79, 20);
			this.label3.TabIndex = 17;
			this.label3.Text = "错误页面数";
			// 
			// tbTotalRequestCount
			// 
			this.tbTotalRequestCount.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.tbTotalRequestCount.Location = new System.Drawing.Point(91, 5);
			this.tbTotalRequestCount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbTotalRequestCount.Name = "tbTotalRequestCount";
			this.tbTotalRequestCount.ReadOnly = true;
			this.tbTotalRequestCount.Size = new System.Drawing.Size(279, 26);
			this.tbTotalRequestCount.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Top;
			this.label2.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.label2.Location = new System.Drawing.Point(4, 0);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(79, 20);
			this.label2.TabIndex = 15;
			this.label2.Text = "请求总数";
			// 
			// tbLeftRequestCount
			// 
			this.tbLeftRequestCount.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.tbLeftRequestCount.Location = new System.Drawing.Point(91, 41);
			this.tbLeftRequestCount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbLeftRequestCount.Name = "tbLeftRequestCount";
			this.tbLeftRequestCount.ReadOnly = true;
			this.tbLeftRequestCount.Size = new System.Drawing.Size(279, 26);
			this.tbLeftRequestCount.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.label1.Location = new System.Drawing.Point(4, 36);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(65, 20);
			this.label1.TabIndex = 13;
			this.label1.Text = "剩余请求";
			// 
			// tbProcessCount
			// 
			this.tbProcessCount.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.tbProcessCount.Location = new System.Drawing.Point(91, 113);
			this.tbProcessCount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbProcessCount.Name = "tbProcessCount";
			this.tbProcessCount.ReadOnly = true;
			this.tbProcessCount.Size = new System.Drawing.Size(279, 26);
			this.tbProcessCount.TabIndex = 4;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.label6.Location = new System.Drawing.Point(4, 108);
			this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(51, 20);
			this.label6.TabIndex = 23;
			this.label6.Text = "线程数";
			// 
			// tbStartTime
			// 
			this.tbStartTime.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.tbStartTime.Location = new System.Drawing.Point(91, 257);
			this.tbStartTime.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbStartTime.Name = "tbStartTime";
			this.tbStartTime.ReadOnly = true;
			this.tbStartTime.Size = new System.Drawing.Size(279, 26);
			this.tbStartTime.TabIndex = 8;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.label7.Location = new System.Drawing.Point(4, 252);
			this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(65, 20);
			this.label7.TabIndex = 25;
			this.label7.Text = "开始时间";
			// 
			// tbEndTime
			// 
			this.tbEndTime.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.tbEndTime.Location = new System.Drawing.Point(91, 293);
			this.tbEndTime.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbEndTime.Name = "tbEndTime";
			this.tbEndTime.ReadOnly = true;
			this.tbEndTime.Size = new System.Drawing.Size(279, 26);
			this.tbEndTime.TabIndex = 9;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.label8.Location = new System.Drawing.Point(4, 288);
			this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(65, 20);
			this.label8.TabIndex = 27;
			this.label8.Text = "结束时间";
			// 
			// tbTaskStatus
			// 
			this.tbTaskStatus.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.tbTaskStatus.Location = new System.Drawing.Point(91, 221);
			this.tbTaskStatus.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbTaskStatus.Name = "tbTaskStatus";
			this.tbTaskStatus.ReadOnly = true;
			this.tbTaskStatus.Size = new System.Drawing.Size(279, 26);
			this.tbTaskStatus.TabIndex = 7;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.label9.Location = new System.Drawing.Point(4, 216);
			this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(65, 20);
			this.label9.TabIndex = 30;
			this.label9.Text = "任务状态";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.listBox1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(737, 674);
			this.tableLayoutPanel1.TabIndex = 32;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.tbLeftRequestCount, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.label8, 0, 8);
			this.tableLayoutPanel2.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.tbStartTime, 1, 7);
			this.tableLayoutPanel2.Controls.Add(this.label9, 0, 6);
			this.tableLayoutPanel2.Controls.Add(this.label7, 0, 7);
			this.tableLayoutPanel2.Controls.Add(this.tbTotalRequestCount, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.label5, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.tbRunningProcessCount, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.label6, 0, 3);
			this.tableLayoutPanel2.Controls.Add(this.tbProcessCount, 1, 3);
			this.tableLayoutPanel2.Controls.Add(this.label3, 0, 4);
			this.tableLayoutPanel2.Controls.Add(this.label4, 0, 5);
			this.tableLayoutPanel2.Controls.Add(this.tbErrorPageCount, 1, 4);
			this.tableLayoutPanel2.Controls.Add(this.tbPagePerSecond, 1, 5);
			this.tableLayoutPanel2.Controls.Add(this.tbTaskStatus, 1, 6);
			this.tableLayoutPanel2.Controls.Add(this.tbEndTime, 1, 8);
			this.tableLayoutPanel2.Controls.Add(this.btnClearDb, 1, 9);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(308, 5);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 10;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(425, 664);
			this.tableLayoutPanel2.TabIndex = 1;
			// 
			// btnClearDb
			// 
			this.btnClearDb.Location = new System.Drawing.Point(91, 329);
			this.btnClearDb.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnClearDb.Name = "btnClearDb";
			this.btnClearDb.Size = new System.Drawing.Size(108, 35);
			this.btnClearDb.TabIndex = 10;
			this.btnClearDb.Text = "清空数据库";
			this.btnClearDb.UseVisualStyleBackColor = true;
			this.btnClearDb.Click += new System.EventHandler(this.btnClearDb_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(737, 674);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "Form1";
			this.Text = "Monitor";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.TextBox tbRunningProcessCount;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox tbPagePerSecond;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbErrorPageCount;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbTotalRequestCount;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbLeftRequestCount;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbProcessCount;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox tbStartTime;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox tbEndTime;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem 刷新ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 删除ToolStripMenuItem;
		private System.Windows.Forms.TextBox tbTaskStatus;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Button btnClearDb;
	}
}

