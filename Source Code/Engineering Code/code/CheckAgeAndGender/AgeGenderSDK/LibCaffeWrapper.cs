using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;


namespace AgeGenderSDK
{
   
    public class LibCaffeWrapper
    {

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PredictionInfo
        {
            public int classInfo;
            public float confidence;  
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct AgeGenderInfo
        {
            public int GenderInfo;
            public float GenderConfidence;
            public int AgeInfo;
            public float AgeConfidence;
        }

        [DllImport("libcaffe\\libcaffe.dll", EntryPoint = "AgeLoad", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int AgeLoad(string amodel_file, string atrained_file, string amean_file);

        [DllImport("libcaffe\\libcaffe.dll", EntryPoint = "GenderLoad", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int GenderLoad(string gmodel_file, string gtrained_file, string gmean_file);

        [DllImport("libcaffe\\libcaffe.dll", EntryPoint = "imageLoad", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int imageLoad(string image_file);

        [DllImport("libcaffe\\libcaffe.dll", EntryPoint = "AgePre", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int AgePre(string image_file,ref PredictionInfo p);

        [DllImport("libcaffe\\libcaffe.dll", EntryPoint = "GenderPre", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int GenderPre(string image_file,ref PredictionInfo p);









        [DllImport("libcaffe\\libcaffe.dll", EntryPoint = "GenderPrediction", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int GenderPrediction(string model_file, string trained_file, string mean_file, string image_file,ref PredictionInfo p);

        [DllImport("libcaffe\\libcaffe.dll", EntryPoint = "AgeGenderPrediction", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int AgeGenderPrediction(string amodel_file, string atrained_file, string amean_file, string gmodel_file, string gtrained_file, string gmean_file, string image_file, ref AgeGenderInfo p);
        
        [DllImport("libcaffe\\libcaffe.dll", EntryPoint = "add", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int add(int a ,int b);
        [DllImport("libcaffe\\libcaffe.dll", EntryPoint = "AgePrediction", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int AgePrediction(string model_file, string trained_file, string mean_file, string image_file,  PredictionInfo p);

   		public int GoA;   //0: gender   1:age
   		public PredictionInfo Prediction;
        public AgeGenderInfo ANGPrediction;

        public PredictionInfo AgePreInfo;
        public PredictionInfo GenderPreInfo;

   		public LibCaffeWrapper(int classInfo)
   		{
   			GoA = classInfo;
            Prediction = new PredictionInfo();
            Prediction.classInfo = -1;
            Prediction.confidence = -1;

            AgePreInfo = new PredictionInfo();
            AgePreInfo.classInfo = -1;
            AgePreInfo.confidence = -1;

            GenderPreInfo = new PredictionInfo();
            GenderPreInfo.classInfo = -1;
            GenderPreInfo.confidence = -1;

            GoA = classInfo;
   		}

        public void AgeModelLoad(string amodel_file, string atrained_file, string amean_file)
        {
            AgeLoad(amodel_file, atrained_file, amean_file);
        }

        public void GenderModelLoad( string gmodel_file, string gtrained_file, string gmean_file)
        {
            GenderModelLoad(gmodel_file, gtrained_file, gmean_file);
        }

        public void Imgload(string image_file)
        {
            imageLoad(image_file);
        }

        public void AP(string image_file)
        {
            AgePre(image_file,ref AgePreInfo);
        }

        public void GP(string image_file)
        {
            GenderPre(image_file,ref GenderPreInfo);
        }

        public int GetClass()
        {
            try
            {
                if(GoA == 0)
                    return GenderPreInfo.classInfo;
                else
                    return AgePreInfo.classInfo;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public float GetCon()
        {

            try
            {
                if (GoA == 0)
                    return GenderPreInfo.confidence;
                else
                    return AgePreInfo.confidence;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public void Predict(string model_file, string trained_file, string mean_file, string image_file)
   		{
            int x = add(1, 2);


            x = GenderPrediction(model_file, trained_file, mean_file, image_file, ref Prediction);
            //System.Console.WriteLine(Prediction.classInfo.ToString());
            System.Console.WriteLine(x.ToString());
   		}

        public void ANGPredict(string amodel_file, string atrained_file, string amean_file, string gmodel_file, string gtrained_file, string gmean_file, string image_file)
        {
            int x = add(1, 2);


            x = AgeGenderPrediction(amodel_file, atrained_file, amean_file, gmodel_file, gtrained_file, gmean_file, image_file, ref ANGPrediction);
            //System.Console.WriteLine(Prediction.classInfo.ToString());
            System.Console.WriteLine(x.ToString());
        }

        public int GetAgeClassInfo()
        {
            try
            {
                return ANGPrediction.AgeInfo;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public float GetAgeConfidence()
        {
            try
            {
                return ANGPrediction.AgeConfidence;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public int GetGenderClassInfo()
        {
            try
            {
                return ANGPrediction.GenderInfo;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public float GetGenderConfidence()
        {
            try
            {
                return ANGPrediction.GenderConfidence;
            }
            catch (Exception e)
            {
                return -1;
            }
        }


   		public int GetClassInfo()
   		{
   			try
   			{
   				return Prediction.classInfo;
   			}
            catch(Exception e)
            {
                return -1;   
            }
   		}

         public float GetConfidence()
   		{

   			try
   			{
   				return Prediction.confidence;
   			}
            catch(Exception e)
            {
                return -1;   
            }
   		}
    }

}
