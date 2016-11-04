namespace vis1
{
  partial class VisForm3
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.m_SendEd = new System.Windows.Forms.TextBox();
            this.m_Disp1 = new System.Windows.Forms.Label();
            this.m_Disp2 = new System.Windows.Forms.Label();
            this.m_Disp3 = new System.Windows.Forms.Label();
            this.m_DispTimer = new System.Windows.Forms.Timer(this.components);
            this.m_MsgLb = new System.Windows.Forms.ListBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.curveWinMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keyBoardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.barWinMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.osziToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setSingleShotValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.enableToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.controMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.acqOnOffMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emptyReceiceBufferMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearMessagesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.acqPointMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_Disp4 = new System.Windows.Forms.Label();
            this.m_Disp5 = new System.Windows.Forms.Label();
            this.m_Disp6 = new System.Windows.Forms.Label();
            this._sb = new ZedHL.SliderBank();
            this.m_Disp7 = new System.Windows.Forms.Label();
            this.m_Disp8 = new System.Windows.Forms.Label();
            this.m_Disp9 = new System.Windows.Forms.Label();
            this._decodeTimer = new System.Windows.Forms.Timer(this.components);
            this.Send = new System.Windows.Forms.Button();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // m_SendEd
            // 
            this.m_SendEd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_SendEd.Location = new System.Drawing.Point(12, 36);
            this.m_SendEd.Name = "m_SendEd";
            this.m_SendEd.Size = new System.Drawing.Size(148, 22);
            this.m_SendEd.TabIndex = 1;
            this.m_SendEd.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnSendEditKeyDown);
            // 
            // m_Disp1
            // 
            this.m_Disp1.AutoSize = true;
            this.m_Disp1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_Disp1.Location = new System.Drawing.Point(267, 45);
            this.m_Disp1.Name = "m_Disp1";
            this.m_Disp1.Size = new System.Drawing.Size(50, 16);
            this.m_Disp1.TabIndex = 3;
            this.m_Disp1.Text = "V1111";
            // 
            // m_Disp2
            // 
            this.m_Disp2.AutoSize = true;
            this.m_Disp2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_Disp2.Location = new System.Drawing.Point(267, 76);
            this.m_Disp2.Name = "m_Disp2";
            this.m_Disp2.Size = new System.Drawing.Size(50, 16);
            this.m_Disp2.TabIndex = 4;
            this.m_Disp2.Text = "V2222";
            // 
            // m_Disp3
            // 
            this.m_Disp3.AutoSize = true;
            this.m_Disp3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_Disp3.Location = new System.Drawing.Point(267, 107);
            this.m_Disp3.Name = "m_Disp3";
            this.m_Disp3.Size = new System.Drawing.Size(50, 16);
            this.m_Disp3.TabIndex = 5;
            this.m_Disp3.Text = "V3333";
            // 
            // m_MsgLb
            // 
            this.m_MsgLb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_MsgLb.FormattingEnabled = true;
            this.m_MsgLb.Location = new System.Drawing.Point(336, 36);
            this.m_MsgLb.Name = "m_MsgLb";
            this.m_MsgLb.Size = new System.Drawing.Size(165, 290);
            this.m_MsgLb.TabIndex = 11;
            this.m_MsgLb.SelectedIndexChanged += new System.EventHandler(this.m_MsgLb_SelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.osziToolStripMenuItem,
            this.controMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(848, 24);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToCSVToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveToCSVToolStripMenuItem
            // 
            this.saveToCSVToolStripMenuItem.Name = "saveToCSVToolStripMenuItem";
            this.saveToCSVToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.saveToCSVToolStripMenuItem.Text = "Save";
            this.saveToCSVToolStripMenuItem.Click += new System.EventHandler(this.saveToCSVToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.curveWinMenuItem,
            this.keyBoardMenuItem,
            this.barWinMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // curveWinMenuItem
            // 
            this.curveWinMenuItem.CheckOnClick = true;
            this.curveWinMenuItem.Name = "curveWinMenuItem";
            this.curveWinMenuItem.Size = new System.Drawing.Size(167, 22);
            this.curveWinMenuItem.Text = "CurveWin On/Off";
            this.curveWinMenuItem.Click += new System.EventHandler(this.OnCurveWinOnOffMenue);
            // 
            // keyBoardMenuItem
            // 
            this.keyBoardMenuItem.CheckOnClick = true;
            this.keyBoardMenuItem.Name = "keyBoardMenuItem";
            this.keyBoardMenuItem.Size = new System.Drawing.Size(167, 22);
            this.keyBoardMenuItem.Text = "KeyBoard On/Off";
            this.keyBoardMenuItem.Click += new System.EventHandler(this.OnKeyBoardMenue);
            // 
            // barWinMenuItem
            // 
            this.barWinMenuItem.CheckOnClick = true;
            this.barWinMenuItem.Name = "barWinMenuItem";
            this.barWinMenuItem.Size = new System.Drawing.Size(167, 22);
            this.barWinMenuItem.Text = "BarWin On/Off";
            this.barWinMenuItem.Click += new System.EventHandler(this.OnBarWinMenue);
            // 
            // osziToolStripMenuItem
            // 
            this.osziToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setSingleShotValueToolStripMenuItem});
            this.osziToolStripMenuItem.Name = "osziToolStripMenuItem";
            this.osziToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.osziToolStripMenuItem.Text = "Oszi";
            // 
            // setSingleShotValueToolStripMenuItem
            // 
            this.setSingleShotValueToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setValueToolStripMenuItem,
            this.resetToolStripMenuItem1,
            this.enableToolStripMenuItem1});
            this.setSingleShotValueToolStripMenuItem.Name = "setSingleShotValueToolStripMenuItem";
            this.setSingleShotValueToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.setSingleShotValueToolStripMenuItem.Text = "Single Shot";
            // 
            // setValueToolStripMenuItem
            // 
            this.setValueToolStripMenuItem.Name = "setValueToolStripMenuItem";
            this.setValueToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.setValueToolStripMenuItem.Text = "Set Value";
            this.setValueToolStripMenuItem.Click += new System.EventHandler(this.setValueToolStripMenuItem_Click);
            // 
            // resetToolStripMenuItem1
            // 
            this.resetToolStripMenuItem1.Name = "resetToolStripMenuItem1";
            this.resetToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.resetToolStripMenuItem1.Text = "Reset";
            this.resetToolStripMenuItem1.Click += new System.EventHandler(this.resetToolStripMenuItem1_Click);
            // 
            // enableToolStripMenuItem1
            // 
            this.enableToolStripMenuItem1.CheckOnClick = true;
            this.enableToolStripMenuItem1.Name = "enableToolStripMenuItem1";
            this.enableToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.enableToolStripMenuItem1.Text = "Enable";
            this.enableToolStripMenuItem1.Click += new System.EventHandler(this.enableToolStripMenuItem1_Click);
            // 
            // controMenuItem
            // 
            this.controMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.acqOnOffMenuItem,
            this.emptyReceiceBufferMenuItem,
            this.clearMessagesMenuItem,
            this.acqPointMenuItem});
            this.controMenuItem.Name = "controMenuItem";
            this.controMenuItem.Size = new System.Drawing.Size(59, 20);
            this.controMenuItem.Text = "Control";
            // 
            // acqOnOffMenuItem
            // 
            this.acqOnOffMenuItem.CheckOnClick = true;
            this.acqOnOffMenuItem.Name = "acqOnOffMenuItem";
            this.acqOnOffMenuItem.Size = new System.Drawing.Size(183, 22);
            this.acqOnOffMenuItem.Text = "Acq. On/Off";
            this.acqOnOffMenuItem.Click += new System.EventHandler(this.OnAcqOnOffMenue);
            // 
            // emptyReceiceBufferMenuItem
            // 
            this.emptyReceiceBufferMenuItem.Name = "emptyReceiceBufferMenuItem";
            this.emptyReceiceBufferMenuItem.Size = new System.Drawing.Size(183, 22);
            this.emptyReceiceBufferMenuItem.Text = "Empty ReceiceBuffer";
            this.emptyReceiceBufferMenuItem.Click += new System.EventHandler(this.OnEmptyReceiveBufferMenue);
            // 
            // clearMessagesMenuItem
            // 
            this.clearMessagesMenuItem.Name = "clearMessagesMenuItem";
            this.clearMessagesMenuItem.Size = new System.Drawing.Size(183, 22);
            this.clearMessagesMenuItem.Text = "Clear Messages";
            this.clearMessagesMenuItem.Click += new System.EventHandler(this.OnClearMessagesMenue);
            // 
            // acqPointMenuItem
            // 
            this.acqPointMenuItem.CheckOnClick = true;
            this.acqPointMenuItem.Name = "acqPointMenuItem";
            this.acqPointMenuItem.Size = new System.Drawing.Size(183, 22);
            this.acqPointMenuItem.Text = "AcqPoints On/Off";
            this.acqPointMenuItem.Click += new System.EventHandler(this.OnAcqPointsOnOffMenue);
            // 
            // m_Disp4
            // 
            this.m_Disp4.AutoSize = true;
            this.m_Disp4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_Disp4.Location = new System.Drawing.Point(267, 138);
            this.m_Disp4.Name = "m_Disp4";
            this.m_Disp4.Size = new System.Drawing.Size(50, 16);
            this.m_Disp4.TabIndex = 16;
            this.m_Disp4.Text = "V4444";
            // 
            // m_Disp5
            // 
            this.m_Disp5.AutoSize = true;
            this.m_Disp5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_Disp5.Location = new System.Drawing.Point(267, 169);
            this.m_Disp5.Name = "m_Disp5";
            this.m_Disp5.Size = new System.Drawing.Size(50, 16);
            this.m_Disp5.TabIndex = 17;
            this.m_Disp5.Text = "V5555";
            // 
            // m_Disp6
            // 
            this.m_Disp6.AutoSize = true;
            this.m_Disp6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_Disp6.Location = new System.Drawing.Point(267, 200);
            this.m_Disp6.Name = "m_Disp6";
            this.m_Disp6.Size = new System.Drawing.Size(50, 16);
            this.m_Disp6.TabIndex = 18;
            this.m_Disp6.Text = "V6666";
            // 
            // _sb
            // 
            this._sb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._sb.Location = new System.Drawing.Point(9, 64);
            this._sb.Name = "_sb";
            this._sb.Size = new System.Drawing.Size(243, 269);
            this._sb.TabIndex = 19;
            // 
            // m_Disp7
            // 
            this.m_Disp7.AutoSize = true;
            this.m_Disp7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_Disp7.Location = new System.Drawing.Point(267, 231);
            this.m_Disp7.Name = "m_Disp7";
            this.m_Disp7.Size = new System.Drawing.Size(50, 16);
            this.m_Disp7.TabIndex = 20;
            this.m_Disp7.Text = "V7777";
            // 
            // m_Disp8
            // 
            this.m_Disp8.AutoSize = true;
            this.m_Disp8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_Disp8.Location = new System.Drawing.Point(267, 262);
            this.m_Disp8.Name = "m_Disp8";
            this.m_Disp8.Size = new System.Drawing.Size(50, 16);
            this.m_Disp8.TabIndex = 21;
            this.m_Disp8.Text = "V8888";
            // 
            // m_Disp9
            // 
            this.m_Disp9.AutoSize = true;
            this.m_Disp9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_Disp9.Location = new System.Drawing.Point(268, 294);
            this.m_Disp9.Name = "m_Disp9";
            this.m_Disp9.Size = new System.Drawing.Size(50, 16);
            this.m_Disp9.TabIndex = 22;
            this.m_Disp9.Text = "V9999";
            // 
            // Send
            // 
            this.Send.Location = new System.Drawing.Point(166, 36);
            this.Send.Name = "Send";
            this.Send.Size = new System.Drawing.Size(75, 23);
            this.Send.TabIndex = 23;
            this.Send.Text = "Send";
            this.Send.UseVisualStyleBackColor = true;
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.ChartAreas[0].AxisX.Minimum = 0;
            this.chart1.ChartAreas[0].AxisX.Maximum = 20;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(522, 26);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(314, 300);
            this.chart1.TabIndex = 24;
            this.chart1.Text = "chart1";
            // 
            // VisForm3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 335);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.Send);
            this.Controls.Add(this.m_Disp9);
            this.Controls.Add(this.m_Disp8);
            this.Controls.Add(this.m_Disp7);
            this.Controls.Add(this._sb);
            this.Controls.Add(this.m_Disp6);
            this.Controls.Add(this.m_Disp5);
            this.Controls.Add(this.m_Disp4);
            this.Controls.Add(this.m_MsgLb);
            this.Controls.Add(this.m_SendEd);
            this.Controls.Add(this.m_Disp3);
            this.Controls.Add(this.m_Disp2);
            this.Controls.Add(this.m_Disp1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "VisForm3";
            this.Text = "SvVis3  V2.2";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.TextBox m_SendEd;
    private System.Windows.Forms.Label m_Disp1;
    private System.Windows.Forms.Label m_Disp2;
    private System.Windows.Forms.Label m_Disp3;
    private System.Windows.Forms.Timer m_DispTimer;
    private System.Windows.Forms.ListBox m_MsgLb;
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem controMenuItem;
    private System.Windows.Forms.ToolStripMenuItem acqOnOffMenuItem;
    private System.Windows.Forms.ToolStripMenuItem emptyReceiceBufferMenuItem;
    private System.Windows.Forms.ToolStripMenuItem clearMessagesMenuItem;
    private System.Windows.Forms.ToolStripMenuItem acqPointMenuItem;
    private System.Windows.Forms.Label m_Disp4;
    private System.Windows.Forms.Label m_Disp5;
    private System.Windows.Forms.Label m_Disp6;
    private ZedHL.SliderBank _sb;
    private System.Windows.Forms.Label m_Disp7;
    private System.Windows.Forms.Label m_Disp8;
    private System.Windows.Forms.Label m_Disp9;
    private System.Windows.Forms.Timer _decodeTimer;
    private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem curveWinMenuItem;
    private System.Windows.Forms.ToolStripMenuItem keyBoardMenuItem;
    private System.Windows.Forms.ToolStripMenuItem barWinMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToCSVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem osziToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setSingleShotValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem enableToolStripMenuItem1;
        private System.Windows.Forms.Button Send;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
    }
}

