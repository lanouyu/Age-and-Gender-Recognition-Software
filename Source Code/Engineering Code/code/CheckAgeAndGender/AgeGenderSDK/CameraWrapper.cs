using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AgeGenderSDK
{
    
    public class CameraWrapper
    {
        public const int FUNC_START = 0x00000001;
        public const int FUNC_CLOSE = 0x00000003;
        public const int FUNC_THREAD_CAMERA = 0x00000003;

        [DllImport("Camera.dll", EntryPoint = "GetCameraCount", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCameraCount(ref int nCameraCount);
        [DllImport("Camera.dll", EntryPoint = "GetCameraId", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCameraId(byte[] targetDevicePath, ref int deviceId);
        [DllImport("Camera.dll", EntryPoint = "OpenCamera", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int OpenCamera(int nID, int nWidth, int nHeight,int nFPS);
        [DllImport("Camera.dll", EntryPoint = "GetCameraInfo", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCameraInfo(int nID, ref int nWidth, ref int nHeight, ref int nSize, ref int nFourcc);
        [DllImport("Camera.dll", EntryPoint = "GetImage", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetImage(int nID, bool bFlipRedBlue, bool bFlipImage);
        [DllImport("Camera.dll", EntryPoint = "CloseCamera", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CloseCamera(int nID);
        [DllImport("Camera.dll", EntryPoint = "GetCamInputFilterInfo", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCamInputFilterInfo(int nID, ref int nWidth, ref int nHeight, ref int nFourcc);

        public delegate void FrameCallback(int nID, IntPtr frameData);
        public delegate void ErrorCallback(int nID, string strError);

        Thread m_ThreadCamera = null;
        int m_nID = -1;
        bool m_bRunning = false;
        int m_nOpenCount = 0;
        bool m_bException = false;
        FrameCallback m_cbFrame = null;
        ErrorCallback m_cbError = null;        
                        
        public CameraWrapper(int nID,ErrorCallback func=null)
        {
            m_nID = nID;
            m_cbError = func;
            m_bRunning = false;
            m_ThreadCamera = null;
            m_cbFrame = null;
            m_nOpenCount = 0;
        }

        public int GetCameraID()
        {
            return m_nID;
        }

        //[HandleProcessCorruptedStateExceptions]
        public bool Start(FrameCallback func,int nWidht=640,int nHeight=480,int nFPS=30)
        {
            try
            {
                if (m_ThreadCamera!=null)
                {
                    Close();
                }

                m_bException = false;

                if(0!=OpenCamera(m_nID, nWidht, nHeight,nFPS))
                    return false;
                m_nOpenCount++;

                m_cbFrame = func;
                ParameterizedThreadStart ParStart = new ParameterizedThreadStart(ThreadCamera);
                m_ThreadCamera = new Thread(ParStart);
                m_ThreadCamera.Start(m_nID);

                return true;
            }            
            catch (System.Exception ex)
            {
                m_bException = true;
                ProcessException(m_nID,FUNC_START,ex.ToString());            	
            }

            return false;
        }

        //[HandleProcessCorruptedStateExceptions]
        public void Close()
        {
            try
            {                
                m_bRunning = false;
                int nTry = 200;
                while (true && m_ThreadCamera != null && m_bException==false)
                {
                    nTry--;
                    System.Windows.Forms.Application.DoEvents();
                    bool bret = m_ThreadCamera==null||m_ThreadCamera.Join(10);
                    if (bret || nTry<=0)
                    {                        
                        break;
                    }
                }
                m_cbFrame = null;
                m_ThreadCamera = null;
                
                //make sure the camera is closed
                if (m_nOpenCount>0)
                {
                    m_nOpenCount = 0;
                    CloseCamera(m_nID);                    
                }
            }            
            catch (System.Exception ex)
            {
                m_bException = true;                
                ProcessException(m_nID,FUNC_CLOSE, ex.ToString());
            }
            m_cbError = null;
        }

        public static void GetInfo(int nID,ref int nWidth, ref int nHeight, ref int nSize, ref int nFourcc)
        {
            GetCameraInfo(nID,ref nWidth,ref nHeight,ref nSize,ref nFourcc);
        }

        public static void GetCamInfo(int nID, ref int nWidth, ref int nHeight, ref int nFourcc)
        {
            GetCamInputFilterInfo(nID, ref nWidth, ref nHeight, ref nFourcc);
        }


        //[HandleProcessCorruptedStateExceptions]
        public void ThreadCamera(object ParObject)
        {
            try
            {
                int nFreezeCount = 0;
                int nID = 0;
                int.TryParse(ParObject.ToString(), out nID);

                m_bRunning = true;
                bool bStarted = false;
                while (m_bRunning)
                {
                    IntPtr data = GetImage(nID,false,true);
                    if (data != (IntPtr)0)
                    {
                        if (m_bRunning)
                        {
                            ProcessFrame(nID, data);
                        }
                        bStarted=true;
                        nFreezeCount=0;                      
                    }
                    else
                    {
                        nFreezeCount++;
                    }

                    if (bStarted==false)
                    {//need to wait the camera to start,so we will wait 500 empty frames
                        if (nFreezeCount > 500)
                        {
                            ProcessFrame(nID, data);
                            break;
                        }
                    }
                    else
                    {//after the camera started, we will only wait 200 empty frames
                        if (nFreezeCount > 200)
                        {
                            ProcessFrame(nID, data);
                            break;
                        }
                    }
                    

                    System.Threading.Thread.Sleep(10);
                }

                if (m_nOpenCount>0)
                {
                    m_nOpenCount = 0;
                    CloseCamera(m_nID);
                    
                }
            }            
            catch (System.Exception ex)
            {
                m_bException = true;
                ProcessException(m_nID, FUNC_THREAD_CAMERA, ex.ToString());
            }            
        }

        public void SwitchCamera()
        {
            int nCount = 0;
            int nRet = GetCameraCount(ref nCount);
            {
                if (nCount<=1)
                {
                    return;
                }

                m_nID = (m_nID + 1) % nCount;
            }
        }
        
        private void ProcessFrame(int nID, IntPtr frameData)
        {
            if (m_cbFrame!=null)
            {
                m_cbFrame(nID, frameData);
            }
        }

        private void ProcessException(int nID, int nFunctionID,string strMsg)
        {
            if (m_cbError != null)
            {
                m_cbError(nID, strMsg);
            }
        }
    }
}
