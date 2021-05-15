using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace clsScheduling
{
    class clsBenchmark
    {
        /// <summary>
        /// A partir de un fichero times y un fichero machines
        /// genera la estructura de datos del problema JobShop
        /// El fichero.times tiene los tiempos de las operaciones
        /// El fichero.machines tiene las maquinas de las operaciones
        /// El resultado es una clase clsDatosJobShop
        /// </summary>
        /// <param name="strDirPath"></param>
        /// <param name="strFileRaizNombre"></param>
        /// <param name="cSchedule"></param>
        /// <returns></returns>
        public clsDatosJobShop CargarProblema(string strDirPath, string strFileRaizNombre)
        {
            clsDatosJobShop cData = new clsDatosJobShop();

            // Chequea si el fichero machine se encuentra en el path y carga los datos
            CargarFicheroMachines(strDirPath + @"\" + strFileRaizNombre + ".machines", ref cData.dicIdOperationIdMachine);
            // Chequea si el fichero machine se encuentra en el path
            CargarFicheroJobsAndTimes(strDirPath + @"\" + strFileRaizNombre + ".times", ref cData.dicIdOperationTime, ref cData.dicIdOperationIdJob);
            // Comprueba que todos los diccionarios tengan el mismo numero de valores
            if (cData.dicIdOperationTime.Count != cData.dicIdOperationIdMachine.Count)
                new Exception("Numero de datos no consistente");
            // Carga el inicio y fin de cada trabajo y genera un primera solucion de ordenacion en maquina (scheduling)
            Int32 intNumOperaciones = cData.dicIdOperationIdJob.Count;
            Int32 intIdJobLast = -1;
            Int32 intIdMachineLast = -1;
            for (Int32 intI = 0; intI < intNumOperaciones; intI++)
            {
                Int32 intIdOperacion = intI + 1;
                Int32 intIdJob = cData.dicIdOperationIdJob[intIdOperacion];
                Int32 intIdMachine = cData.dicIdOperationIdMachine[intIdOperacion];
                // Cambia el trabajo
                if (intIdJob != intIdJobLast)
                {
                    cData.dicIdJobIdOperationFirst.Add(intIdJob, intIdOperacion);
                    // SI el trabajo antiguo es el -1 entonces es la primera vez 
                    if (intIdJobLast != -1)
                    {
                        cData.dicIdJobIdOperationLast.Add(intIdJobLast, intIdOperacion - 1);
                        cData.dicIdOperationIdNextInJob.Add(intIdOperacion - 1, -1);
                    }
                    cData.dicIdOperationIdPreviousInJob.Add(intIdOperacion, -1);
                    intIdJobLast = intIdJob;
                }
                else
                {
                    cData.dicIdOperationIdNextInJob.Add(intIdOperacion - 1, intIdOperacion);
                    cData.dicIdOperationIdPreviousInJob.Add(intIdOperacion, intIdOperacion - 1);
                }

            }
            cData.dicIdJobIdOperationLast.Add(intIdJobLast, intNumOperaciones);
            cData.dicIdOperationIdNextInJob.Add(intNumOperaciones, -1);

            return cData;
        }

        private void CargarFicheroMachines(string strPathFile, ref Dictionary<Int32, Int32> dicMachines)
        {
            dicMachines = new Dictionary<int, int>();
            if (!File.Exists(strPathFile))
                new Exception("Fichero " + strPathFile + " no encontrado");
            string[] strLines = File.ReadAllLines(strPathFile);
            Int32 intOperation = 1;
            Int32 intJob = 1;
            foreach (string strLine in strLines)
            {
                string[] strSplit = Regex.Split(strLine.Trim(), @" +");
                //if (strSplit.Length != 15)
                //    new Exception("Error en lectura linea fichero " + strPathFile);
                foreach (string strValue in strSplit)
                {
                    dicMachines.Add(intOperation, Convert.ToInt32(strValue));
                    intOperation++;
                }
                intJob++;
            }
        }

        private void CargarFicheroJobsAndTimes(string strPathFile, ref Dictionary<Int32, double> dicTimes, ref Dictionary<Int32, Int32> dicJobs)
        {
            dicTimes = new Dictionary<int, double>();
            dicJobs = new Dictionary<int, int>();
            if (!File.Exists(strPathFile))
                new Exception("Fichero " + strPathFile + " no encontrado");
            string[] strLines = File.ReadAllLines(strPathFile);
            Int32 intOperation = 1;
            Int32 intJob = 1;
            foreach (string strLine in strLines)
            {
                string[] strSplit = Regex.Split(strLine.Trim(), @" +");
                //if (strSplit.Length != 15)
                //    new Exception("Error en lectura linea fichero " + strPathFile);
                foreach (string strValue in strSplit)
                {
                    dicTimes.Add(intOperation, Convert.ToDouble(strValue));
                    dicJobs.Add(intOperation, intJob);
                    intOperation++;
                }
                intJob++;
            }
        }

    }

}
