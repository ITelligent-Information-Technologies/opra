using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsTsp
{
    [Serializable ]
    class clsRLData
    {
        // Dado dos puntos EJ. 1_5 nos devuelve su distancia
        public Dictionary<string, double> dicParPuntosADistancia = new Dictionary<string, double>();
        // Cada punto se define por dos coordenadas x, y
        public List<clsRLDataCoordenadas> lstPuntosCoordenadas = new List<clsRLDataCoordenadas>();

    }
    [Serializable ]
    class clsRLDataCoordenadas
    {
        public Int32 intX;
        public Int32 intY;
    }


    class clsRL
    {
        public void RLTest()
        {
            (clsRLData cData, List<Int32> lstRecorrido) = GenerarProblema(500, 500, 9);
           // clsWriteObjectToFile.WriteToBinaryFile<clsRLData>(@"C:\vbDll\TSP\problema_9.ite",cData );
            double gamma = 0.3;
            double learnRate = 0.5;
            int maxEpochs = 500000;

            Dictionary<string, double> dicStateToQ = Train(cData, lstRecorrido.ToList<Int32>(), gamma, learnRate, maxEpochs);

            Resolver(dicStateToQ, cData, lstRecorrido.ToList<Int32>(), 50);

        }

        private Dictionary<string, double> Train(clsRLData cData, List<Int32> lstRecorrido, double gamma, double lrnRate, int maxEpochs)
        {
            Random rnd = new Random(100);
            Dictionary<string, double> dicStateToQ = new Dictionary<string, double>();
            double dblRecorridoCoste = CalcularCoste(lstRecorrido, cData);
            double dblRecorridoCosteMin = double.MaxValue;
            string strRecorridoMin="";
            for (int epoch = 0; epoch < maxEpochs; ++epoch)
            {
                string strCurrentState = string.Join("|", lstRecorrido.ToArray());
                double dblCosteOld = dblRecorridoCoste;
                // Selecciona el punto a cambiar
                Int32 intRecorridoIndex1 = rnd.Next(0, cData.lstPuntosCoordenadas.Count - 1);
                // Guarda el estado y el cambio que se va a realizar
                string strCurrentStatePlusAction = strCurrentState + "_" + lstRecorrido[intRecorridoIndex1];
                // Realiza el cambio
                lstRecorrido = MoverIndiceAdelante(lstRecorrido, intRecorridoIndex1, cData);
                string strNextState = string.Join("|", lstRecorrido.ToArray());
                dblRecorridoCoste = CalcularCoste(lstRecorrido, cData);
                if (dblRecorridoCosteMin > dblRecorridoCoste)
                {
                    dblRecorridoCosteMin = dblRecorridoCoste;
                    strRecorridoMin = string.Join("|", lstRecorrido.ToArray());
                }

                // Estima el coste de potenciales cambios
                double dblCosteMin = double.MaxValue;
                double dblQNNMin = double.MaxValue;
                for (Int32 intIndex1 = 0; intIndex1 < lstRecorrido.Count; intIndex1++)
                {
                    //List<Int32> lstRecorrido = cData.lstRecorrido.ToList<Int32>();
                    List<Int32> lstRecorridoAux = MoverIndiceAdelante(lstRecorrido.ToList<Int32>(), intIndex1, cData);
                    string strNextNextState = string.Join("|", lstRecorridoAux.ToArray());
                    double dblCosteNextNext = CalcularCoste(lstRecorridoAux, cData);
                    double dblIncremento = dblCosteNextNext - dblRecorridoCoste;
                    if (dicStateToQ.ContainsKey(strNextNextState))
                    {
                        if (dicStateToQ[strNextNextState] < 0)
                            dicStateToQ[strNextNextState] = dicStateToQ[strNextNextState] * gamma;
                        else
                            dicStateToQ[strNextNextState] =dicStateToQ[strNextNextState] / gamma ;
                        dblIncremento = dblIncremento + dicStateToQ[strNextNextState];                        
                    }
                    if (dblIncremento < dblQNNMin)
                        dblQNNMin = dblIncremento;
                }
                if (!dicStateToQ.ContainsKey(strNextState))
                    dicStateToQ.Add(strNextState, dblQNNMin);
                else 
                dicStateToQ[strNextState] = ((1 - lrnRate) * dicStateToQ[strNextState]) + (lrnRate * (dblQNNMin));

                if (dicStateToQ[strNextState] < 0)
                    dblQNNMin = dicStateToQ[strNextState] *gamma;
                else
                    dblQNNMin =dicStateToQ[strNextState] / gamma ;
                if (!dicStateToQ.ContainsKey(strCurrentStatePlusAction))
                    dicStateToQ.Add(strCurrentStatePlusAction,0);
                dicStateToQ[strCurrentStatePlusAction] = ((1 - lrnRate) * dicStateToQ[strCurrentStatePlusAction]) + (lrnRate *((dblRecorridoCoste -dblCosteOld )+ dblQNNMin) );
                
            }
            Console.WriteLine("********************************");
            Console.WriteLine(strRecorridoMin + "->" + dblRecorridoCosteMin);
            Console.WriteLine("********************************");
            return dicStateToQ;
        }



        private void Resolver(Dictionary<string, double> dicStateToQ, clsRLData cData, List<Int32> lstRecorridoInicial, Int32 intMaxIteraciones)
        {
            double dblCosteMinGlobal = double.MaxValue;
            Random rnd = new Random();
            double dblCosteActual = 0;
            Int32 intIndexLast = -1;
            for (Int32 intIte = 0; intIte < intMaxIteraciones; intIte++)
            {
                string strCurrentState = string.Join("|", lstRecorridoInicial.ToArray());
                double dblCosteMin = double.MaxValue;
                Boolean blnEncotrado = false;
                Int32 intAccionMin = -1;
                for (Int32 intAccion = 0; intAccion < lstRecorridoInicial.Count; intAccion++)
                {
                    if (dicStateToQ.ContainsKey(strCurrentState + "_" + intAccion))
                    {
                        double dblCoste = dicStateToQ[strCurrentState + "_" + intAccion];
                        if (dblCoste < dblCosteMin && intAccion != intIndexLast)
                        {
                            dblCosteMin = dblCoste;
                            intAccionMin = intAccion;
                            blnEncotrado = true;
                        }
                    }
                }
                // Realiza el cambio
                if (!blnEncotrado)
                    intAccionMin = rnd.Next(0, lstRecorridoInicial.Count - 1);

                lstRecorridoInicial = MoverIndiceAdelante(lstRecorridoInicial, intAccionMin, cData);
                dblCosteActual = CalcularCoste(lstRecorridoInicial, cData);
                if (dblCosteActual < dblCosteMinGlobal)
                    dblCosteMinGlobal = dblCosteActual;
                Console.WriteLine(string.Join("", lstRecorridoInicial.ToArray()) + "->" + dblCosteActual);
                intIndexLast = intAccionMin;
            }
            Console.WriteLine("Coste Global:" + dblCosteMinGlobal);
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
