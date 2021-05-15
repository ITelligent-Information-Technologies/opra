using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsTsp
{
    class clsRLConstructivo
    {
        public void RLTest()
        {
            (clsRLData cData, List<Int32> lstRecorrido) = GenerarProblema(500, 500, 8);
            clsWriteObjectToFile.WriteToBinaryFile<clsRLData>(@"C:\vbDll\TSP\problema_8.ite", cData);
           // clsRLData cData= clsWriteObjectToFile.ReadFromBinaryFile<clsRLData>(@"C:\vbDll\TSP\problema_7.ite");
           //List<Int32> lstRecorrido = new List<int>();
            double gamma = 0.3;
            double learnRate = 0.5;
            int maxEpochs = 100000;
            lstRecorrido = new List<int>();

            Dictionary<string, double> dicStateToQ = TrainYResolver(cData, lstRecorrido.ToList<Int32>(), gamma, learnRate, maxEpochs);

            //Dictionary<string, double> dicStateToQ = Train(cData, lstRecorrido.ToList<Int32>(), gamma, learnRate, maxEpochs);

            //Resolver(dicStateToQ, cData, lstRecorrido.ToList<Int32>(), 50);

        }

        private Dictionary<string, double> Train(clsRLData cData, List<Int32> lstRecorrido, double gamma, double lrnRate, int maxEpochs)
        {
            Random rnd = new Random(100);
            Dictionary<string, double> dicStateToQ = new Dictionary<string, double>();
            double dblRecorridoCoste = 0;// CalcularCoste(lstRecorrido, cData);
            double dblRecorridoCosteMin = double.MaxValue;
            string strRecorridoMin = "";
            for (int epoch = 0; epoch < maxEpochs; ++epoch)
            {
                HashSet<Int32> hsNoAsignado = new HashSet<int>();
                List<Int32> lstNoAsignados = new List<int>();
                lstRecorrido = new List<int>();
                for (Int32 intI = 1; intI < cData.lstPuntosCoordenadas.Count; intI++)
                {
                    hsNoAsignado.Add(intI);
                    lstNoAsignados.Add(intI);
                }
                lstRecorrido.Add(0);
                while (lstNoAsignados.Count > 0)
                {
                    string strCurrentState = string.Join("|", lstRecorrido.ToArray());
                    double dblCosteOld = dblRecorridoCoste;
                    // Selecciona el punto a cambiar
                    Int32 intRecorridoIndex1 = rnd.Next(0, lstNoAsignados.Count - 1);
                    // Guarda el estado y el cambio que se va a realizar
                    lstRecorrido.Add(lstNoAsignados[intRecorridoIndex1]);
                    string strCurrentStatePlusAction = strCurrentState + "_" + lstNoAsignados[intRecorridoIndex1];
                    lstNoAsignados.RemoveAt(intRecorridoIndex1);
                    //Actualiza el Q
                    if (!dicStateToQ.ContainsKey(strCurrentStatePlusAction))
                        dicStateToQ[strCurrentStatePlusAction] = 0;
                    //Obtiene el maximo
                    strCurrentState = string.Join("|", lstRecorrido.ToArray());
                    double dblQMax = double.MinValue;
                    Boolean blnDetectado = false;
                    double dblReward = 0;
                    if (lstNoAsignados.Count > 0)
                    {
                        foreach (Int32 intPunto in lstNoAsignados)
                        {
                            string strNextNext = strCurrentState + "_" + intPunto;
                            double dblQ = 0;
                            if (dicStateToQ.ContainsKey(strNextNext))
                            {
                                dblQ = dicStateToQ[strNextNext];
                                if (dblQ > dblQMax)
                                    dblQMax = dblQ;
                                blnDetectado = true;
                            }
                        }
                    }
                    else
                    {
                        // Estan todos asignados por lo que obtiene el rewards
                        dblReward = -CalcularCoste(lstRecorrido, cData);
                        if (-dblReward < dblRecorridoCosteMin)
                        {
                            dblRecorridoCosteMin = -dblReward;
                            strRecorridoMin = strCurrentStatePlusAction;
                        }
                    }
                    if (!blnDetectado)
                        dblQMax = 0;
                    dicStateToQ[strCurrentStatePlusAction] = lrnRate * dicStateToQ[strCurrentStatePlusAction] + (1 - lrnRate) * (dblReward + gamma * dblQMax);
                }
            }
            Console.WriteLine("********************************");
            Console.WriteLine(strRecorridoMin + "->" + dblRecorridoCosteMin);
            Console.WriteLine("********************************");
            return dicStateToQ;
        }

        private Dictionary<string, double> TrainYResolver(clsRLData cData, List<Int32> lstRecorrido, double gamma, double lrnRate, int maxEpochs)
        {
            double dblEpsilon = 0.95;
            Random rnd = new Random(100);
            Dictionary<string, double> dicStateToQ = new Dictionary<string, double>();
            double dblRecorridoCoste = 0;// CalcularCoste(lstRecorrido, cData);
            double dblRecorridoCosteMin = double.MaxValue;
            string strRecorridoMin = "";
            for (int epoch = 0; epoch < maxEpochs; ++epoch)
            {
                HashSet<Int32> hsNoAsignado = new HashSet<int>();
                List<Int32> lstNoAsignados = new List<int>();
                lstRecorrido = new List<int>();
                for (Int32 intI = 1; intI < cData.lstPuntosCoordenadas.Count; intI++)
                {
                    hsNoAsignado.Add(intI);
                    lstNoAsignados.Add(intI);
                }
                lstRecorrido.Add(0);
                while (lstNoAsignados.Count > 0)
                {
                    string strCurrentState = string.Join("|", lstRecorrido.ToArray());
                    double dblCosteOld = dblRecorridoCoste;
                    // Comprueba si explora o explota
                    double dblRuleta = rnd.NextDouble();
                    Int32 intRecorridoIndex1 = 0;
                    if (dblRuleta <dblEpsilon )
                    {
                        // Explora
                        // Selecciona el punto a cambiar aleatoriamente
                        intRecorridoIndex1 = rnd.Next(0, lstNoAsignados.Count - 1);
                    }
                    else
                    {
                        // Explota
                        Boolean blnEnContrado = false;
                        double dblQMaxTest = double.MinValue;
                        Int32 intIndexMax = 0;
                        for(Int32 intI=0; intI<lstNoAsignados .Count; intI++)
                        {
                            string strStateTest = strCurrentState + "_" + lstNoAsignados[intI];
                            if (dicStateToQ .ContainsKey (strStateTest))
                            {
                                if (dicStateToQ[strStateTest] > dblQMaxTest)
                                { 
                                    dblQMaxTest = dicStateToQ[strStateTest];
                                    intIndexMax = intI;
                                }
                                blnEnContrado = true;
                            }
                        }
                        if (!blnEnContrado)
                            intRecorridoIndex1 = rnd.Next(0, lstNoAsignados.Count - 1);
                        else
                            intRecorridoIndex1 = intIndexMax;
                    }
                    
                    // Guarda el estado y el cambio que se va a realizar
                    lstRecorrido.Add(lstNoAsignados[intRecorridoIndex1]);
                    string strCurrentStatePlusAction = strCurrentState + "_" + lstNoAsignados[intRecorridoIndex1];
                    lstNoAsignados.RemoveAt(intRecorridoIndex1);
                    //Actualiza el Q
                    if (!dicStateToQ.ContainsKey(strCurrentStatePlusAction))
                        dicStateToQ[strCurrentStatePlusAction] = 0;
                    //Obtiene el maximo
                    strCurrentState = string.Join("|", lstRecorrido.ToArray());
                    double dblQMax = double.MinValue;
                    Boolean blnDetectado = false;
                    double dblReward = 0;
                    if (lstNoAsignados.Count > 0)
                    {
                        foreach (Int32 intPunto in lstNoAsignados)
                        {
                            string strNextNext = strCurrentState + "_" + intPunto;
                            double dblQ = 0;
                            if (dicStateToQ.ContainsKey(strNextNext))
                            {
                                dblQ = dicStateToQ[strNextNext];
                                if (dblQ > dblQMax)
                                    dblQMax = dblQ;
                                blnDetectado = true;
                            }
                        }
                    }
                    else
                    {
                        // Estan todos asignados por lo que obtiene el rewards
                        dblReward = -CalcularCoste(lstRecorrido, cData);
                        if (-dblReward < dblRecorridoCosteMin)
                        {
                            dblRecorridoCosteMin = -dblReward;
                            strRecorridoMin = strCurrentStatePlusAction;
                        }
                        
                    }
                    if (!blnDetectado)
                        dblQMax = 0;
                    dicStateToQ[strCurrentStatePlusAction] = lrnRate * dicStateToQ[strCurrentStatePlusAction] + (1 - lrnRate) * (dblReward + gamma * dblQMax);
                }
                Console.WriteLine(epoch + "->" + dblRecorridoCosteMin);
            }
            Console.WriteLine("********************************");
            Console.WriteLine(strRecorridoMin + "->" + dblRecorridoCosteMin);
            Console.WriteLine("********************************");
            return dicStateToQ;
        }


        private void Resolver(Dictionary<string, double> dicStateToQ, clsRLData cData, List<Int32> lstRecorridoInicial, Int32 intMaxIteraciones)
        {
            List<Int32> lstNoAsignados = new List<int>();
            for (Int32 intI = 1; intI < cData.lstPuntosCoordenadas.Count; intI++)
            {
                lstNoAsignados.Add(intI);
            }
            double dblCosteMaxGlobal = double.MinValue;
            Random rnd = new Random();
            double dblCosteActual = 0;
            Int32 intIndexLast = -1;
            lstRecorridoInicial.Add(0);
            while(lstNoAsignados .Count>0)
            {
                string strCurrentState = string.Join("|", lstRecorridoInicial.ToArray());
                double dblCosteMax = double.MinValue ;
                Boolean blnEncotrado = false;
                Int32 intAccionMin = -1;
                for (Int32 intAccion = 0; intAccion < lstNoAsignados.Count; intAccion++)
                {
                    if (dicStateToQ.ContainsKey(strCurrentState + "_" + lstNoAsignados[intAccion]))
                    {
                        double dblCoste = dicStateToQ[strCurrentState + "_" + lstNoAsignados[intAccion]];
                        if (dblCoste > dblCosteMax)
                        {
                            dblCosteMax = dblCoste;
                            intAccionMin = intAccion;
                            blnEncotrado = true;
                            dblCosteMaxGlobal = dblCosteMax;
                        }
                    }
                }
                // Realiza el cambio
                if (!blnEncotrado)
                    intAccionMin = rnd.Next(0, lstRecorridoInicial.Count - 1);

                lstRecorridoInicial.Add(lstNoAsignados [intAccionMin ]);
                lstNoAsignados.RemoveAt(intAccionMin);
                dblCosteActual = CalcularCoste(lstRecorridoInicial, cData);
                Console.WriteLine(string.Join("", lstRecorridoInicial.ToArray()) + "->" + dblCosteActual);
                intIndexLast = intAccionMin;
            }
            Console.WriteLine("Coste Global:" + dblCosteMaxGlobal );
        }



        private List<Int32> MoverIndiceAdelante(List<Int32> lstRecorrido, Int32 intRecorridoIndexAMover1, clsRLData cData)
        {
            // Calcula el punto siguiente
            Int32 intRecorridoIndexAMover2 = intRecorridoIndexAMover1 + 1;
            if (intRecorridoIndexAMover2 >= lstRecorrido.Count)
                intRecorridoIndexAMover2 = 0;
            Int32 intIndexAnterior1 = intRecorridoIndexAMover1 - 1;
            if (intIndexAnterior1 < 0)
                intIndexAnterior1 = lstRecorrido.Count - 1;
            Int32 intIndexPosterior2 = intRecorridoIndexAMover2 + 1;
            if (intIndexPosterior2 >= lstRecorrido.Count)
                intIndexPosterior2 = 0;
            // Realiza el cambio en el recorrido
            Int32 intPuntoAux = lstRecorrido[intRecorridoIndexAMover1];
            lstRecorrido[intRecorridoIndexAMover1] = lstRecorrido[intRecorridoIndexAMover2];
            lstRecorrido[intRecorridoIndexAMover2] = intPuntoAux;
            return lstRecorrido;
        }


        private (clsRLData cData, List<Int32> lstRecorrido) GenerarProblema(Int32 intXmax, Int32 intYmax, Int32 intNumeroPuntos, Boolean blnConvexo = false)
        {
            clsRLData cData = new clsRLData();
            Random rnd = new Random(100);
            Int32 intCentroX = Convert.ToInt32(intXmax / 2);
            Int32 intCentroY = Convert.ToInt32(intYmax / 2);
            Int32 intRadioMax = Convert.ToInt32((0.95 * Math.Min(intXmax, intYmax)) / 2);
            Int32 intRadioMin = Convert.ToInt32((0.25 * Math.Min(intXmax, intYmax)) / 2);
            List<Int32> lstRecorrido = new List<int>();
            while (cData.lstPuntosCoordenadas.Count < intNumeroPuntos)
            {
                // toma el angulo
                double dblAngulo = rnd.Next(0, 360);
                dblAngulo = dblAngulo / (2 * Math.PI);
                Int32 intRadio = intRadioMax;
                if (!blnConvexo)
                    intRadio = rnd.Next(intRadioMin, intRadioMax);
                double dblX = intCentroX + intRadio * Math.Cos(dblAngulo);
                double dblY = intCentroY + intRadio * Math.Sin(dblAngulo);
                clsRLDataCoordenadas cCoordenadas = new clsRLDataCoordenadas();
                cCoordenadas.intX = Convert.ToInt32(dblX);
                cCoordenadas.intY = Convert.ToInt32(dblY);
                lstRecorrido.Add(cData.lstPuntosCoordenadas.Count);
                cData.lstPuntosCoordenadas.Add(cCoordenadas);
            }
            // Calcula la matriz de distancias
            for (Int32 intI = 0; intI < lstRecorrido.Count; intI++)
            {
                for (Int32 intJ = intI + 1; intJ < lstRecorrido.Count; intJ++)
                {
                    Int32 intPunto1 = lstRecorrido[intI];
                    Int32 intPunto2 = lstRecorrido[intJ];
                    double dblDistancia = Math.Sqrt(((cData.lstPuntosCoordenadas[intPunto1].intX - cData.lstPuntosCoordenadas[intPunto2].intX) * (cData.lstPuntosCoordenadas[intPunto1].intX - cData.lstPuntosCoordenadas[intPunto2].intX)) + ((cData.lstPuntosCoordenadas[intPunto1].intY - cData.lstPuntosCoordenadas[intPunto2].intY) * (cData.lstPuntosCoordenadas[intPunto1].intY - cData.lstPuntosCoordenadas[intPunto2].intY)));
                    cData.dicParPuntosADistancia.Add(intPunto1 + "_" + intPunto2, dblDistancia);
                    cData.dicParPuntosADistancia.Add(intPunto2 + "_" + intPunto1, dblDistancia);
                }
            }

            return (cData, lstRecorrido);
        }


        private double CalcularCoste(List<Int32> lstRecorrido, clsRLData cData)
        {
            // Calcula el coste
            double dblCoste = 0;
            for (Int32 intI = 0; intI < lstRecorrido.Count; intI++)
            {
                if ((intI + 1) == lstRecorrido.Count)
                    dblCoste = dblCoste + cData.dicParPuntosADistancia[lstRecorrido[intI] + "_" + lstRecorrido[0]];
                else
                    dblCoste = dblCoste + cData.dicParPuntosADistancia[lstRecorrido[intI] + "_" + lstRecorrido[(intI + 1)]];
            }
            return dblCoste;
        }
    }
}
