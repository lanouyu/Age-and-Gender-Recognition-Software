using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AgeGenderSDK;
using System.IO;
using System.Drawing.Drawing2D;
namespace CheckAgeAndGender
{
    
    public partial class UploadMode : Form
    {
        //LibCaffeWrapper Gender;
        //LibCaffeWrapper Age;
        LibCaffeWrapper ANG;
        String PredictedImage;
        Image yasuo;
        int AgeClass = 0;
        float AgeConfidence = 0;
        int GenderClass = 0;
        float GenderConfidence = 0;
        face_rectangle rec = new face_rectangle();
        public UploadMode()
        {
            InitializeComponent();
            //Gender = new LibCaffeWrapper(0);
            //Age = new LibCaffeWrapper(1);
            ANG = new LibCaffeWrapper(0);
            axWindowsMediaPlayer2.windowlessVideo = false;
            axWindowsMediaPlayer2.uiMode = "none";
        }


        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            insert(CheckImage);
            
        }

        public void insert(PictureBox pic)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "请选择要插入的图片";
                ofd.Filter = "JPG图片|*.jpg|BMP图片|*.bmp|Gif图片|*.gif|png图片|*.PNG|JPEG图片|*.jpeg";
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pic.ImageLocation = ofd.FileName;
                    System.Console.WriteLine(pic.ImageLocation);
                }
                else
                {
                    MessageBox.Show("你没有选择图片", "信息提示");
                }
            }
        }

        private void StartCheckBtn_Click(object sender, EventArgs e)
        {
            if (CheckImage.Image != null)
            {

                PredictedImage = CheckImage.ImageLocation;

                //Gender.Predict(Application.StartupPath + "\\m\\deploy_gender.prototxt",
                //   Application.StartupPath + "\\m\\gender_500001229.caffemodel",
                //    Application.StartupPath + "\\m\\g_mean1229.binaryproto",
                //    PredictedImage);
                //Age.Predict(Application.StartupPath + "\\m\\deploy_age.prototxt",
                //    Application.StartupPath + "\\m\\age_net.caffemodel",
                //    Application.StartupPath + "\\m\\g_mean1229.binaryproto",
                //    PredictedImage);

                ANG.ANGPredict(Application.StartupPath + "\\m\\deploy_age.prototxt",
                   Application.StartupPath + "\\m\\age_net.caffemodel",
                   Application.StartupPath + "\\m\\g_mean1229.binaryproto",
                   Application.StartupPath + "\\m\\deploy_gender.prototxt",
                    Application.StartupPath + "\\m\\gender_500001229.caffemodel",
                    Application.StartupPath + "\\m\\g_mean1229.binaryproto",
                   PredictedImage);


                GenderClass = ANG.GetGenderClassInfo();
                AgeClass = ANG.GetAgeClassInfo();
                if (GenderClass == 0)
                    label1.Text = "Male";
                else if (GenderClass == 1)
                    label1.Text = "Female";
                else label1.Text = "RECOGNIZATION FAILED!";

                if (AgeClass == 0)
                    label4.Text = "(0-2)";
                else if (AgeClass == 1)
                    label4.Text = "(4-6)";
                else if (AgeClass == 2)
                    label4.Text = "(8-13)";
                else if (AgeClass == 3)
                    label4.Text = "(15-20)";
                else if (AgeClass == 4)
                    label4.Text = "(25-32)";
                else if (AgeClass == 5)
                    label4.Text = "(38-43)";
                else if (AgeClass == 6)
                    label4.Text = "(48-53)";
                else if (AgeClass == 7)
                    label4.Text = "(60- )";
                else label4.Text = "RECOGNIZATION FAILED";
                //LastAge = Age.GetClassInfo();
                //LastGender = Gender.GetClassInfo();
                //System.Console.WriteLine(Gender.GetClassInfo().ToString());
                //System.Console.WriteLine(Age.GetClassInfo().ToString());
                //if (LastAge == 0)
                //    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\Milk.mp4";
                //else if ((LastAge == 1 || LastAge == 2) && LastGender == 0)
                //    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\legao.mp4";
                //else if ((LastAge == 1 || LastAge == 2) && LastGender == 1)
                //    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\Barbie.mp4";
                //else if ((LastAge == 3 || LastAge == 4) && LastGender == 0)
                //    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\Apple.mp4";
                //else if ((LastAge == 3 || LastAge == 4) && LastGender == 1)
                //    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\Apple.mp4";
                //else if ((LastAge == 5 || LastAge == 6) && LastGender == 0)
                //    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\watch.mp4";
                //else if ((LastAge == 5 || LastAge == 6) && LastGender == 1)
                //    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\YSL.mp4";
                //else axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\nao.mp4";

                if (AgeClass == 0)
                    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\Milk.mp4";
                else if ((AgeClass == 1 || AgeClass == 2) && GenderClass == 0)
                    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\legao.mp4";
                else if ((AgeClass == 1 || AgeClass == 2) && GenderClass == 1)
                    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\Barbie.mp4";
                else if ((AgeClass == 3 || AgeClass == 4) && GenderClass == 0)
                    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\jianpan.mp4";
                else if ((AgeClass == 3 || AgeClass == 4) && GenderClass == 1)
                    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\Apple.mp4";
                else if ((AgeClass == 5 || AgeClass == 6) && GenderClass == 0)
                    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\watch.mp4";
                else if ((AgeClass == 5 || AgeClass == 6) && GenderClass == 1)
                    axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\YSL.mp4";
                else axWindowsMediaPlayer2.URL = Application.StartupPath + "\\video\\nao.mp4";

                axWindowsMediaPlayer2.Ctlcontrols.play();        //播放
            }
            else
                MessageBox.Show("Please upload a photo first!");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Start ftm1 = new Start();
            //ftm1.sta
            ftm1.Show();
            this.Hide();
        }

        public Image ZoomPicture(Image SourceImage, int TargetWidth, int TargetHeight)
          {
             int IntWidth; //新的图片宽
              int IntHeight; //新的图片高
              try
             {
                  System.Drawing.Imaging.ImageFormat format = SourceImage.RawFormat;
                  System.Drawing.Bitmap SaveImage = new System.Drawing.Bitmap(TargetWidth, TargetHeight);
                 Graphics g = Graphics.FromImage(SaveImage);
                 g.Clear(Color.White);  
                 if (SourceImage.Width > TargetWidth && SourceImage.Height <= TargetHeight)//宽度比目的图片宽度大，长度比目的图片长度小
                 {
                     IntWidth = TargetWidth;
                     IntHeight = (IntWidth * SourceImage.Height) / SourceImage.Width;
                 }
                 else if (SourceImage.Width <= TargetWidth && SourceImage.Height > TargetHeight)//宽度比目的图片宽度小，长度比目的图片长度大
                 {
                     IntHeight = TargetHeight;
                     IntWidth = (IntHeight * SourceImage.Width) / SourceImage.Height;
                 }
                 else if (SourceImage.Width <= TargetWidth && SourceImage.Height <= TargetHeight) //长宽比目的图片长宽都小
                 {
                    IntHeight = SourceImage.Width;
                     IntWidth = SourceImage.Height;
               }
                 else//长宽比目的图片的长宽都大
                 {
                     IntWidth = TargetWidth;
                     IntHeight = (IntWidth * SourceImage.Height) / SourceImage.Width;
                    if (IntHeight > TargetHeight)//重新计算
                     {
                         IntHeight = TargetHeight;
                         IntWidth = (IntHeight * SourceImage.Width) / SourceImage.Height;
                     }
                 }
 
                 g.DrawImage(SourceImage, (TargetWidth - IntWidth) / 2, (TargetHeight - IntHeight) / 2, IntWidth, IntHeight);
                 SourceImage.Dispose();
 
                 return SaveImage;
             }
             catch (Exception ex)
             {
               
             }
 
             return null;
         }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void CheckImage_Click(object sender, EventArgs e)
        {

            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(this.pictureBox1.ClientRectangle);
            Region reg = new Region(path);
            this.pictureBox1.Region = reg;

        }

        private void UploadMode_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click_1(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
