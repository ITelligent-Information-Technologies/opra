using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;



namespace clsTsp
{
    class Program
    {
        static void Main(string[] args)
        {
            TestGenerarProblemasEIteracionesTSP();
            return;

            clsRLConstructivo cRLCons = new clsRLConstructivo();
            cRLCons.RLTest();
            return;
            // Testear Reinforcement Learning
            clsRL cRL = new clsRL();
            cRL.RLTest();
            return;
            //clsSA cSa = new clsSA();
            ////for (Int32 intI = 41; intI < 51; intI++)
            //Int32 intI = 1;
            //cSa.Optimizar(intI);
            //return;

            string[] strFiles = Directory.GetFiles(@"C:\borrar\sa\");
            // string[] strFiles = Directory.GetFiles(@"C:\vbDll\Videos\Oficina\DosCamaras\CamaraIvan\out\ImagenesParaVideo\");
            List<Int32> lstIndex = new List<int>();
            foreach (string strFile in strFiles)
            {
                Int32 intIndex = Convert.ToInt32(Regex.Match(strFile, @"(\d+)\.jpg").Groups[1].ToString());
                lstIndex.Add(intIndex);
            }
            Int32[] intIndexArray = lstIndex.ToArray<Int32>();
            Array.Sort(intIndexArray, strFiles);

            CreateVideo(strFiles);

        }

        static void TestGenerarProblemasEIteracionesTSP()
        {
            // Paso-1: Generar un conjunto de problemas de ejemplo de TSP
            clsSA cSa = new clsSA();
            //for (Int32 intYear = 25; intYear < 201; intYear = intYear + 25)
            //{
            //    Int32 intNumeroCiudades = intYear;
            //    string strPathDirProblemas = @"C:\vbDll\TSP\Problemas\" + intNumeroCiudades + @"\";
            //    if (!Directory.Exists(strPathDirProblemas))
            //        Directory.CreateDirectory(strPathDirProblemas);
            //    cSa.GenerarProblemas(intNumeroCiudades, 100, strPathDirProblemas);
            //}

            // Paso-2: Dado un problema genera las iteraciones para resolverlas y las guarda
            //cSa.GenerarIteraciones(@"C:\vbDll\TSP\Problemas\100\100_1.ite");

            // Paso-3: Procesa las iteraciones del Paso-2 quitando las repetidas y guardando la mejor accion y genera un  nuevo fichero con estos resutlados
            clsGenerarEjemplosParaCNN cEjemplos = new clsGenerarEjemplosParaCNN();
            //cEjemplos.GenerarEjemplos(@"C:\vbDll\TSP\Iteraciones\100\100_1.csv", @"C:\vbDll\TSP\Iteraciones\100\100_1_sin_duplicados.csv");
            
            // Paso-4: Genera las imagenes a partir del fichero del paso-3
            cEjemplos .GenerarImagenes(@"C:\vbDll\TSP\Problemas\100\100_1.ite", @"C:\vbDll\TSP\Iteraciones\100\100_1_sin_duplicados.csv", @"C:\vbDll\TSP\imagenes\");

        }

        static void CreateVideo(string[] strPathFiles)
        {
            string fileName = @"C:\borrar\sa\test30.mp4";

            int fourcc = VideoWriter.Fourcc('H', '2', '6', '4');

            Backend[] backends = CvInvoke.WriterBackends;
            int backend_idx = 0; //any backend;
            foreach (Backend be in backends)
            {
                if (be.Name.Equals("MSMF"))
                {
                    backend_idx = be.ID;
                    break;
                }
            }

            double fps = 30;
            using (VideoWriter vw = new VideoWriter(fileName, backend_idx, fourcc, fps, new Size(1920, 1080), true))
            {
                foreach (var frame in strPathFiles)
                {
                    if (frame.Contains(".jpg"))
                    {
                        using (Bitmap bitmap = Image.FromFile(frame) as Bitmap)


                        //using (Bitmap resize = new Bitmap(bitmap, 800, 800))
                        using (Image<Bgr, Byte> imageCV = new Image<Bgr, byte>(frame))
                            vw.Write(imageCV.Mat);
                    }

                }

            }
        }

    }
}



