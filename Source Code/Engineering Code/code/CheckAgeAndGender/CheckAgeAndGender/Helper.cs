using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace CheckAgeAndGender
{
    [DataContract]
    public  class face_rectangle
    {
        [DataMember]
        public int top;
        [DataMember]
        public
        int left;
        [DataMember]
        public
        int width;
        [DataMember]
        public
        int height;
    }
    
    #region JSON helper
    public static class JSON
    {

        public static T parse<T>(string jsonString)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(ms);
            }
        }

        public static string JsonToString(object jsonObject)
        {
            using (var ms = new MemoryStream())
            {
                new DataContractJsonSerializer(jsonObject.GetType()).WriteObject(ms, jsonObject);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
    #endregion
        [DataContract]
        public class QueryFacePair
        {
            [DataMember]
            public string api_key = "HJd7Qwyn62FdNbn3HwjPJnc9MU9Yv8rH";
            [DataMember]
            public string api_secret = "-ipGLmfGDboKFkx8s_xEXu4AslKcXEJY";
            [DataMember]
            public Byte[] image_file = null;
            [DataMember]
            public int return_landmark = 0;
            [DataMember]
            public string return_attributes;
            //[DataMember]
            //public bool auto_flip = true;
        }
        


        #region http post
        public class Helper
        {
            public static string HttpUploadFile(string url, string path)
            {
                // 设置参数
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线
                request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
                byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
                byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                int pos = path.LastIndexOf("\\");
                string fileName = path.Substring(pos + 1);
                //请求头部信息 
                StringBuilder sbHeader = new StringBuilder(string.Format("Content-Disposition:form-data;name=\"file\";filename=\"{0}\"\r\nContent-Type:application/octet-stream\r\n\r\n", fileName));
                byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] bArr = new byte[fs.Length];
                fs.Read(bArr, 0, bArr.Length);
                fs.Close();
                Stream postStream = request.GetRequestStream();
                postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                postStream.Write(bArr, 0, bArr.Length);
                postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                postStream.Close();
                //发送请求并获取相应回应数据
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream instream = response.GetResponseStream();
                StreamReader sr = new StreamReader(instream, Encoding.UTF8);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                return content;
            }
            
            
            public static string PostToInterface(string urlStr, string parms)
            {
                try
                {
                    string webpageContent = string.Empty;
                    byte[] byteArray = Encoding.UTF8.GetBytes(parms);

                    var url = WebRequest.Create(urlStr);

                    HttpWebRequest webRequest = (HttpWebRequest)(url);
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/json";
                    webRequest.ContentLength = byteArray.Length;

                    using (Stream webpageStream = webRequest.GetRequestStream())
                    {
                        webpageStream.Write(byteArray, 0, byteArray.Length);
                    }

                    using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                        {
                            webpageContent = reader.ReadToEnd();
                        }
                    }
                    return webpageContent;

                }
                catch (Exception)
                {
                    //System.Windows.Forms.MessageBox.Show("服务器返回错误!");
                }

                return null;
            }

        #endregion

            public bool Obtain(Byte[] Image_file, ref face_rectangle cr)
            {
                QueryFacePair data = new QueryFacePair();
                data.image_file = Image_file;
                string output = JSON.JsonToString(data);
                string ret;
                string result = PostToInterface("https://api-cn.faceplusplus.com/facepp/v3/detect", output);
                if (result != null)
                {
                    try
                    {
                        cr = JSON.parse<face_rectangle>(result);

                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                        return false;
                    }
                }

                return true;
            }
        };

}
