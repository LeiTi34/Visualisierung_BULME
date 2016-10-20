using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace vis1
{
    public partial class SingleShotSetVals : Form
    {
        public float SingleShotTrigger;
        public int SingleShotChannel;

        public SingleShotSetVals(float SSTin, int SSCin)
        {
            InitializeComponent();
            textBox1.Text = $@"{SSTin}";
            numericUpDown1.Text = $@"{SSCin + 1}";
            SingleShotTrigger = SSTin;
            SingleShotChannel = SSCin;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            SingleShotChannel = numericUpDown1.DecimalPlaces - 1;
            SingleShotTrigger = float.Parse(textBox1.Text);
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
