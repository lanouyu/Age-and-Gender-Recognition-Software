using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CheckAgeAndGender
{
    public partial class Start : Form
    {
        public Start()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UploadMode ftm1 = new UploadMode();
            //ftm1.sta
            ftm1.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Video ftm2 = new Video();
            //ftm1.sta
            ftm2.Show();
            this.Hide();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void Start_Load(object sender, EventArgs e)
        {

        }
    }
}
