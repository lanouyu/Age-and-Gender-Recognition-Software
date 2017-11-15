using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AgeGenderSDK;
using System.Drawing.Imaging;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.IO;
using System.Diagnostics;
namespace CheckAgeAndGender
{
    public partial class Video : Form
    {
        LibCaffeWrapper Gender;
        LibCaffeWrapper Age;
        //LibCaffeWrapper ANG;
        CameraWrapper Camera = null;
        private Bitmap m_camFrame = null;
        private Object thisLock = new Object();  
        //每帧摄像头数据和锁x
        private static readonly object m_ImageLocker = new object();
        //分辨率相关x
        private int m_nWidth = 640;
        private int m_nHeight = 480;
        private bool m_bNeedSubImage = false;
        private float m_fRadio = 0.5f;
        private int m_nFPS = 30;
        public delegate void CameraError();
        private int m_nFrameCount = 0;
        //多少帧后强制释放内存x
        private int m_nFramesForGCCollection = 100;
        private CameraError m_cbCameraError = null;
        //界面刷新回调函数x
        private UpdateCameraFrame m_cbUpdateCam = null;
        public delegate void UpdateCameraFrame(Bitmap bmp);
        System.IO.FileInfo file;
        //摄像头图像翻转x
        public static bool m_bFlipCamera = true;
        Image midImage;
        Stopwatch watch = new Stopwatch();
        string AgeString = "";
        string GenderString = "";
        int CheckNum = 0;
        int ManNum = 0;
        int BabyNum = 0;
        int ChildNum = 0;
        int TeenNum = 0;
        int ZhongnianNum = 0;
        int TherNum = 0;
        static string StartLoc = Application.StartupPath;
        int flagLock = 0;

        int AgeClass = 0;
        float AgeConfidence = 0;
        int GenderClass = 0;
        float GenderConfidence = 0;
        Bitmap camFrame;
        Byte[] tmpFile;
        Thread OpenCameraTh;
        Thread AgeThread;
        Thread GenderThread;
        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
        public Video()
        {

            Camera = new CameraWrapper(0, null);
            Gender = new LibCaffeWrapper(0);
            Age = new LibCaffeWrapper(1);
            //ANG = new LibCaffeWrapper(0);
            Gender.Predict(StartLoc + "\\m\\deploy_gender.prototxt",
                    StartLoc + "\\m\\gender_500001229.caffemodel",
                    StartLoc + "\\m\\g_mean1229.binaryproto",
                    StartLoc + "\\example_image.jpg");
            Age.Predict(StartLoc + "\\m\\deploy_age.prototxt",
                   StartLoc + "\\m\\age_net.caffemodel",
                   StartLoc + "\\m\\g_mean1229.binaryproto",
                   StartLoc + "\\example_image.jpg");

            //ANG.ANGPredict(Application.StartupPath + "\\m\\deploy_age.prototxt",
            //       Application.StartupPath + "\\m\\age_net.caffemodel",
            //       Application.StartupPath + "\\m\\g_mean1229.binaryproto",
            //       Application.StartupPath + "\\m\\deploy_gender.prototxt",
            //        Application.StartupPath + "\\m\\gender_500001229.caffemodel",
            //        Application.StartupPath + "\\m\\g_mean1229.binaryproto",
            //       Application.StartupPath + "\\example_image.jpg");
            //MessageBox.Show(Application.StartupPath);
            InitializeComponent();
            axWindowsMediaPlayer2.windowlessVideo = false;
            axWindowsMediaPlayer2.uiMode = "none";
        }

        public void SetCameraSize(int nWidth, int nHeight, int nFPS)
        {
            m_nWidth = nWidth;
            m_nHeight = nHeight;
            m_nFPS = nFPS;
        }
        //this is the callback for camera
        private void WebCamCallBack(int nID, IntPtr pData)
        {
            try
            {
                int nWidth = 0, nHeight = 0, nSize = 0, nFourCC = 0;
                CameraWrapper.GetInfo(nID, ref nWidth, ref nHeight, ref nSize, ref nFourCC);
                if ((uint)nFourCC != 0xe436eb7d)//MEDIASUBTYPE_RGB24
                    return;

                if (pData == (IntPtr)0)
                {
                    if (m_cbCameraError != null)
                    {
                        m_cbCameraError();
                    }
                    return;
                }

               
                
                
               while(flagLock != 0);
                //update UI
                flagLock = 1;
                camFrame = new Bitmap(nWidth, nHeight, 3 * nWidth, PixelFormat.Format24bppRgb, pData);
                if (m_bFlipCamera)
                {
                    camFrame.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }

                if (m_nFrameCount == 90)
                {

                    //midImage = camFrame.Clone();
                    bool bLock = Monitor.TryEnter(m_ImageLocker, 1000);
                    if (bLock)
                    {

                        try
                        {
                            if (m_camFrame != null)
                            {
                                m_camFrame.Dispose();
                            }
                            using (m_camFrame = new Bitmap(camFrame))
                            {

                                m_camFrame.Save(StartLoc + "\\1.png", System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("请用管理员权限运行程序");
                        }
                        //m_camFrame = new Bitmap(camFrame);
                        //m_camFrame.Save("D:\\1.png", System.Drawing.Imaging.ImageFormat.Png);
                        Monitor.Exit(m_ImageLocker);
                    }
                    if (CheckNum == 0)
                    {
                        ManNum = 0;
                        BabyNum = 0;
                        ChildNum = 0;
                        TeenNum = 0;
                        ZhongnianNum = 0;
                        TherNum = 0;
                    }
                    CheckNum++;
                    watch.Reset();
                    watch.Start();
                    GenderThread = new Thread(GenderPredict);
                    GenderThread.IsBackground = true;
                    GenderThread.Start();

                    AgeThread = new Thread(AgePredict);
                    AgeThread.Start();

                    if (CheckNum == 3)
                    {
                        VideoPlay();
                        //Play = new Thread(VideoPlay);
                        //Play.Start();
                        CheckNum = 0;
                    }


                    //label4.Text = GenderString;
                    //label1.Text = AgeString;

                }
                if (PicBoxVideo != null)
                {

                        PicBoxVideo.Image = camFrame;
                       
                            //System.Console.WriteLine(PicBoxVideo.ImageLocation);

                }

               
                            
                            //imagedata = imageToByteArray(PicBoxVideo.Image);
                           // midImage.Save("D:\\1.png");
                            

                            
                flagLock = 0;
                
                      
               
                m_nFrameCount++;
                if (m_nFrameCount % m_nFramesForGCCollection == 0)
                {
                    m_nFrameCount = 0;
                    GC.Collect();
                }
            }
            catch (System.InvalidOperationException)
            {
                //do nothing
                Camera.Close();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("In WebCamCallBack " + ex.ToString());
            }
        }

        private void BtnOpenCamera_Click(object sender, EventArgs e)
        {
            //if (OpenCameraTh != null)
            //{
            //    OpenCameraTh.Abort(); ;
            //    OpenCameraTh = null;
            //}
            //OpenCameraTh = new Thread(OpenCamera);
            //OpenCameraTh.Start();

            OpenCamera();
        }

        private void OpenCamera()
        {
            Camera.Start(WebCamCallBack);
        }

        private void BtnSwitchCamera_Click(object sender, EventArgs e)
        {
            if (Camera == null)
            {
                return;
            }


            Camera.Close();
            Camera.SwitchCamera();
            Camera.Start(WebCamCallBack);
        }

        private void BtnCloseCamera_Click(object sender, EventArgs e)
        {
            Camera.Close();
        } //camerawrapper   ProcessFrame

        public void UpdateFrame()
        {
            try
            {
                MethodInvoker methodInvokerDelegate = delegate()
                {
                    //if (CheckNum == 0)
                    //{
                    //     ManNum = 0;
                    //     BabyNum = 0;
                    //     ChildNum = 0;
                    //     TeenNum = 0;
                    //     ZhongnianNum = 0;
                    //     TherNum = 0;
                    //}
                    //CheckNum++;
                    //Gender.Predict(StartLoc + "\\m\\deploy_gender.prototxt",
                    //StartLoc + "\\m\\gender_500001229.caffemodel",
                    //StartLoc + "\\m\\g_mean1229.binaryproto",
                    //StartLoc + "\\1.png");

                    //Age.Predict(StartLoc + "\\m\\deploy_age.prototxt",
                    //    StartLoc + "\\m\\age_net.caffemodel",
                    //    StartLoc + "\\m\\g_mean1229.binaryproto",
                    //   StartLoc + "\\1.png");
                   // ANG.ANGPredict(Application.StartupPath + "\\m\\deploy_age.prototxt",
                   //Application.StartupPath + "\\m\\age_net.caffemodel",
                   //Application.StartupPath + "\\m\\g_mean1229.binaryproto",
                   //Application.StartupPath + "\\m\\deploy_gender.prototxt",
                   // Application.StartupPath + "\\m\\gender_500001229.caffemodel",
                   // Application.StartupPath + "\\m\\g_mean1229.binaryproto",
                   //Application.StartupPath + "\\1.png");

                    //GenderClass = ANG.GetGenderClassInfo();
                    //AgeClass = ANG.GetAgeClassInfo();
                    //GenderClass = Gender.GetClassInfo();
                    ////AgeClass = Age.GetClassInfo();
                    //if (GenderClass == 0)
                    //{
                    //    GenderString = "Male";
                    //    ManNum++;
                    //}
                    //else if (GenderClass == 1)
                    //    GenderString = "Female";
                    //else GenderString = "Unknow";
                    //if (AgeClass == 0)
                    //{
                    //    AgeString = "(0-2)";
                    //    BabyNum++;
                    //}
                    //else if (AgeClass == 1)
                    //{
                    //    AgeString = "(4-6)";
                    //    ChildNum++;
                    //}
                    //else if (AgeClass == 2)
                    //{
                    //    AgeString = "(8-13)";
                    //    ChildNum++;
                    //}
                    //else if (AgeClass == 3)
                    //{
                    //    AgeString = "(15-20)";
                    //    TeenNum++;
                    //}
                    //else if (AgeClass == 4)
                    //{
                    //    AgeString = "(25-32)";
                    //    TeenNum++;
                    //}
                    //else if (AgeClass == 5)
                    //{
                    //    AgeString = "(38-43)";
                    //    ZhongnianNum++;
                    //}
                    //else if (AgeClass == 6)
                    //{
                    //    AgeString = "(48-53)";
                    //    ZhongnianNum++;
                    //}
                    //else if (AgeClass == 7)
                    //{
                    //    AgeString = "(60- )";
                    //    TherNum++;
                    //}
                    //else AgeString = "识别错误";

                    //if (CheckNum == 3)
                    //{
                    //   VideoPlay();
                    //    //Play = new Thread(VideoPlay);
                    //    //Play.Start();
                    //    CheckNum = 0;
                    //}
                   

                    //label4.Text = GenderString;
                    //label1.Text = AgeString;
                    //label5.Text = Gender.GetConfidence().ToString();
                    //label6.Text = Age.GetConfidence().ToString();
                    //MessageBox.Show(Gender.GetClassInfo().ToString());
                    //MessageBox.Show(Age.GetClassInfo().ToString());
                };
                this.BeginInvoke(methodInvokerDelegate);
            

            }
            catch (System.Exception)
            {
            }
    
        
        }

        public void GenderPredict()
        {
            Gender.Predict(StartLoc + "\\m\\deploy_gender.prototxt",
                    StartLoc + "\\m\\gender_500001229.caffemodel",
                    StartLoc + "\\m\\g_mean1229.binaryproto",
                    StartLoc + "\\1.png");
            GenderClass = Gender.GetClassInfo();
            //AgeClass = Age.GetClassInfo();
            if (GenderClass == 0)
            {
                GenderString = "Male";
                ManNum++;
            }
            else if (GenderClass == 1)
                GenderString = "Female";
            else GenderString = "Unknow";
            try
            {
                MethodInvoker methodInvokerDelegate = delegate()
                {
                    label4.Text = GenderString;
                };
                this.BeginInvoke(methodInvokerDelegate);


            }
            catch (System.Exception)
            {
            }
        }

        public void AgePredict()
        {
            Age.Predict(StartLoc + "\\m\\deploy_age.prototxt",
                        StartLoc + "\\m\\age_net.caffemodel",
                        StartLoc + "\\m\\g_mean1229.binaryproto",
                       StartLoc + "\\1.png");
            AgeClass = Age.GetClassInfo();
            if (AgeClass == 0)
            {
                AgeString = "(0-2)";
                BabyNum++;
            }
            else if (AgeClass == 1)
            {
                AgeString = "(4-6)";
                ChildNum++;
            }
            else if (AgeClass == 2)
            {
                AgeString = "(8-13)";
                ChildNum++;
            }
            else if (AgeClass == 3)
            {
                AgeString = "(15-20)";
                TeenNum++;
            }
            else if (AgeClass == 4)
            {
                AgeString = "(25-32)";
                TeenNum++;
            }
            else if (AgeClass == 5)
            {
                AgeString = "(38-43)";
                ZhongnianNum++;
            }
            else if (AgeClass == 6)
            {
                AgeString = "(48-53)";
                ZhongnianNum++;
            }
            else if (AgeClass == 7)
            {
                AgeString = "(60- )";
                TherNum++;
            }
            else AgeString = "RECOGNIZATION FAILED!";
            try
            {
                MethodInvoker methodInvokerDelegate = delegate()
                {
                    label1.Text = AgeString;
                    watch.Stop();
                    label2.Text = (watch.ElapsedMilliseconds ).ToString();
                };
                this.BeginInvoke(methodInvokerDelegate);


            }
            catch (System.Exception)
            {
            }
        
        }
        private void Video_Load(object sender, EventArgs e)
        {

        }

        private void CloseVideo_Click(object sender, EventArgs e)
        {
            if (Camera != null)
                Camera.Close();
            this.Close();
        }

        public void VideoPlay()
        { 
           axWindowsMediaPlayer2.Ctlcontrols.stop();
            if (BabyNum > 1)
            {
                axWindowsMediaPlayer2.URL = StartLoc + "\\video\\Milk.mp4";
            }
            else if (ChildNum > 1 && ManNum > 1)
            {
                axWindowsMediaPlayer2.URL = StartLoc + "\\video\\legao.mp4";
            }
            else if (ChildNum > 1 && ManNum <= 1)
            {
                axWindowsMediaPlayer2.URL = StartLoc + "\\video\\Barbie.mp4";
            }
            else if (TeenNum > 1 && ManNum <= 1)
            {
                axWindowsMediaPlayer2.URL = StartLoc + "\\video\\Apple.mp4";
            }
            else if (TeenNum > 1 && ManNum > 1)
            {
                axWindowsMediaPlayer2.URL = StartLoc + "\\video\\jianpan.mp4";
            }
            else if (ZhongnianNum > 1 && ManNum <= 1)
            {
                axWindowsMediaPlayer2.URL = StartLoc + "\\video\\YSL.mp4";
            }
            else if (ZhongnianNum > 1 && ManNum > 1)
            {
                axWindowsMediaPlayer2.URL = StartLoc + "\\video\\watch.mp4";
            }
            else if (TherNum > 1)
            {
                axWindowsMediaPlayer2.URL = StartLoc + "\\video\\nao.mp4";
            }
            else 
            {
                if (AgeClass == 0)
                    axWindowsMediaPlayer2.URL = StartLoc + "\\video\\Milk.mp4";
                else if ((AgeClass == 1 || AgeClass == 2) && GenderClass == 0)
                    axWindowsMediaPlayer2.URL = StartLoc + "\\video\\legao.mp4";
                else if ((AgeClass == 1 || AgeClass == 2) && GenderClass == 1)
                    axWindowsMediaPlayer2.URL = StartLoc + "\\video\\Barbie.mp4";
                else if ((AgeClass == 3 || AgeClass == 4) && GenderClass == 0)
                    axWindowsMediaPlayer2.URL = StartLoc  + "\\video\\jianpan.mp4";
                else if ((AgeClass == 3 || AgeClass == 4) && GenderClass == 1)
                    axWindowsMediaPlayer2.URL = StartLoc + "\\video\\Apple.mp4";
                else if ((AgeClass == 5 || AgeClass == 6) && GenderClass == 0)
                    axWindowsMediaPlayer2.URL = StartLoc + "\\video\\watch.mp4";
                else if ((AgeClass == 5 || AgeClass == 6) && GenderClass == 1)
                    axWindowsMediaPlayer2.URL = StartLoc + "\\video\\YSL.mp4";
                else axWindowsMediaPlayer2.URL = StartLoc + "\\video\\nao.mp4";
            }
            axWindowsMediaPlayer2.Ctlcontrols.play();       
        }

        private void PicBoxVideo_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Camera.Close();
            Application.Exit();
            this.Close();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Camera.Close();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Start ftm1 = new Start();
            //ftm1.sta
            ftm1.Show();
            this.Hide();
        }
       
    }
    }
