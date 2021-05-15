using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;


namespace clsTsp
{
    class clsGenerarEjemplosParaCNN
    {
        public void GenerarEjemplos(string strPathProblema, string strPathFileIn, string strPathFileOut, string strPathFileImagenBaseOut)
        {
            Regex reg = new Regex(@"(\d+)_\d+");
            // Va leyendo el fichero por trozos y generando un nuevo fichero quitando repetidos
            Dictionary<string, Int32> dicAccionToCuenta = new Dictionary<string, int>();
            HashSet<string> hsProcesados = new HashSet<string>();
            StringBuilder sbFile = new StringBuilder();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(strPathFileIn))
            {
                string strLine;

                Int32 intCuenta = 0;
                while ((strLine = sr.ReadLine()) != null)
                {
                    string[] strSplit = strLine.Split(';');
                    if (strSplit.Length != 6)
                        new Exception("Longitud incorrecta");
                    // Si no es la primera linea de cabecera entra
                    if (intCuenta > 0)
                    {
                        string strIteracion = strSplit[0];
                        // Si la iteracion aun no ha sido procesada
                        if (!hsProcesados.Contains(strIteracion))
                        {

                            hsProcesados.Add(strIteracion);
                            // Obtiene la accion
                            string strAccion = strSplit[4];
                            strAccion = reg.Match(strAccion).Groups[1].ToString();
                            if (!dicAccionToCuenta.ContainsKey(strAccion))
                                dicAccionToCuenta.Add(strAccion, 0);
                            dicAccionToCuenta[strAccion]++;
                            sbFile.Append(strIteracion + ";" + strAccion + Environment.NewLine);
                        }
                    }
                    intCuenta++;
                }
            }
            // Escribe el fichero de salida sin repetidos
            File.WriteAllText(strPathFileOut, sbFile.ToString().Trim(), Encoding.GetEncoding("windows-1252"));
            sbFile = new StringBuilder();
            hsProcesados = null;

            // Genera la imagen basae
            GenerarImagenes(strPathProblema, strPathFileOut, strPathFileImagenBaseOut);

        }

        public void GenerarImagenes(string strPathProblema, string strPathFileIteraciones, string strPathDirImagenes)
        {
            // Genera la imagen base (la matriz)
            if (!File.Exists(strPathProblema))
                new Exception("Fichero no encontrado");
            List<clsPunto> lstPuntos = clsWriteObjectToFile.ReadFromBinaryFile<List<clsPunto>>(strPathProblema);
            Dictionary<string, double> dicParesPuntosToDistancia = new Dictionary<string, double>();
            for (Int32 intI = 0; intI < lstPuntos.Count; intI++)
            {
                for (Int32 intJ = 0; intJ < lstPuntos.Count; intJ++)
                {
                    double dblDistancia = CalcularDistancia(intI, intJ, lstPuntos);
                    dicParesPuntosToDistancia.Add(intI + "_" + intJ, dblDistancia);
                }
            }
            clsImagenes cImagenes = new clsImagenes();
            string strPathFileImagenBaseOut = strPathDirImagenes + "testc.jpg";
            cImagenes.PintarMatrizDistancia(lstPuntos, dicParesPuntosToDistancia, 299, 299, strPathFileImagenBaseOut);
            // Genera una imagen por cada iteracion y la guarda en su carpeta
            using (System.IO.StreamReader sr = new System.IO.StreamReader(strPathFileIteraciones))
            {
                string strLine;

                Int32 intCuenta = 1;
                while ((strLine = sr.ReadLine()) != null)
                {
                    string[] strSplit = strLine.Split(';');
                    if (strSplit.Length != 2)
                        new Exception("Longitud incorrecta");
                    string[] strSplitRecorrido = strSplit[0].Split('|');
                    List<Int32> lstRecorrido = new List<int>();
                    foreach (string strInt in strSplitRecorrido)
                        lstRecorrido.Add(Convert.ToInt32(strInt));
                    string strAccionDir = strPathDirImagenes + @"\" + strSplit[1] + @"\";
                    if (!Directory.Exists(strAccionDir))
                        Directory.CreateDirectory(strAccionDir);
                    string strPathFileImage = strAccionDir + intCuenta + ".jpg";
                    cImagenes.PintarRecorridoSobreMatrizDistancia(lstPuntos, lstRecorrido, strPathFileImagenBaseOut, 299, 299, strPathFileImage);
                    intCuenta++;
                }
            }
        }


        private double CalcularDistancia(Int32 intIndex1, Int32 intIndex2, List<clsPunto> lstPuntos)
        {
            double dblDistancia = Math.Sqrt(((lstPuntos[intIndex1].intX - lstPuntos[intIndex2].intX) * (lstPuntos[intIndex1].intX - lstPuntos[intIndex2].intX)) + ((lstPuntos[intIndex1].intY - lstPuntos[intIndex2].intY) * (lstPuntos[intIndex1].intY - lstPuntos[intIndex2].intY)));
            return dblDistancia;
        }

    }
}