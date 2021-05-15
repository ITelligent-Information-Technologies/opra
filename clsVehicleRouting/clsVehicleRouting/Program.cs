using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsVehicleRouting
{
    class Program
    {
        static void Main(string[] args)
        {
            // Testear selection algorithm
            //TestLinearTimeSelectionAlgorithm();
            //TestearLPVRP();
            TetearLSNVRP();

            return;

            // Testear uso de memoria
            System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            long processAllocatedMemory = currentProcess.WorkingSet64;
            //            So that's process.

            //To get specific object allocation you probably can use GC to check initial memory, then allocate an object and finally to check memory again:

            // Check initial memory
            var memoryStart = System.GC.GetTotalMemory(true);


            // Check memory after allocation
            List<clsDictionarySorted> lstDic = new List<clsDictionarySorted>();
            for (Int32 intI = 0; intI < 10000; intI++)
            {
                clsDictionarySorted cDS = new clsDictionarySorted();
                for (Int32 intJ = 0; intJ < 100; intJ++)
                    cDS.Add(intJ.ToString(), intJ);
                lstDic.Add(cDS);
            }
            var memoryEnd = System.GC.GetTotalMemory(true);
            // Memoria en megabytes
            Console.WriteLine((memoryEnd - memoryStart) / 1000000);
            return;
            //SortedList<double, Int32> slTest = new SortedList<double, Int32>();
            SortedDictionary<Int64, Int32> slTest = new SortedDictionary<Int64, int>();
            Dictionary<Int32, Int64> dicReverse = new Dictionary<int, Int64>();

            Random rnd = new Random(100);
            Int32 intMaxIteraciones = 250000;
            DateTime dtmStart = DateTime.Now;
            double dblMin = double.MaxValue;
            for (Int32 intI = 1; intI < intMaxIteraciones; intI++)
            {
                double dblValor = intI * 1000000000000000 + rnd.NextDouble() * rnd.NextDouble();
                if (dblValor < dblMin)
                    dblMin = dblValor;
                Int64 intValor = Convert.ToInt64(dblValor);
                slTest.Add(intValor, intI);
                // Boolean blnAus= slTest.ContainsValue(intI);
                dicReverse.Add(intI, intValor);

                Int32 intKey = intI - 50;
                Int64 intValorOld = 0;
                if (intKey >= 0)
                {

                    Int64 intValue = slTest.ElementAt(0).Key;
                    intValorOld = dicReverse[intKey];
                    intKey = slTest[intValorOld];
                    if (intKey != (intI - 50))
                        new Exception("eeror");
                }


            }
            DateTime dtmEnd = DateTime.Now;
            Console.WriteLine((dtmEnd - dtmStart).TotalSeconds);

        }


        static void TetearLSNVRP()
        {
            clsLNSVRP cVR = new clsLNSVRP();
            //clsLNSVRPOld2 cVR = new clsLNSVRPOld2();
            clsDataProblem cProblem = GenerarProblema(500, 10);
            //clsDataRutas cRutas = cVR.SolucionInicial(cProblem, 3);
            clsParameters cParameters = new clsParameters();
            cParameters.intNumIterations = 10000;
            cParameters.blnParalelizar = false;
            cParameters.blnConsiderarTimeWindowSuperior = true;
            cParameters.intRegretLevelSolucionInicial = 3;
            clsParametersSimulatedAnnealing cParametersSimulatedAnnealing = new clsParametersSimulatedAnnealing();
            DateTime dtStart = DateTime.Now;
            clsBackTrack cBackTrack = new clsBackTrack(cParameters.intNumSolucionesBacktrack);
            clsDataStructuresRutas cRutas = cVR.PararellLNSVR(cProblem, cParameters, cParametersSimulatedAnnealing);
            //clsDataStructuresRutasOld2 cRutas = cVR.PararellLNSVR(cProblem, cParameters, cParametersSimulatedAnnealing);
            DateTime dtEnd = DateTime.Now;
            Console.WriteLine((dtEnd - dtStart).TotalSeconds);
            // Lo guarda en fichero para despues dibujarlo
            StringBuilder sbTexto = new StringBuilder();
            string strSeparador = "";
            for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
            {
                strSeparador = strSeparador + ";";
                Boolean blnEnBucle = true;
                Int32 intPC = -intV;
                Int32 intPCInicial = -intV;
                while (blnEnBucle)
                {
                    clsDataPuntoConsumo cPC;
                    if (intPC < 0)
                        cPC = cProblem.dicIdPuntoConsumoADatosPuntoConsumo[0];
                    else
                        cPC = cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPC];
                    sbTexto.Append(cPC.dblLongitud + strSeparador + cPC.dblLatitude + Environment.NewLine);

                    Int32 intSiguiente = cRutas.dicIdPuntoConsumoAIdSiguiente[intPC];
                    if (intSiguiente < 0)
                    {
                        // Cierra el bucle
                        if (intPCInicial < 0)
                            cPC = cProblem.dicIdPuntoConsumoADatosPuntoConsumo[0];
                        else
                            cPC = cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPCInicial];
                        sbTexto.Append(cPC.dblLongitud + strSeparador + cPC.dblLatitude + Environment.NewLine);
                        blnEnBucle = false;
                    }
                    intPC = intSiguiente;
                }
            }
            string strTexto = sbTexto.ToString().Trim();
        }

        static void TestearLPVRP()
        {
            clsDataProblem cProblem = GenerarProblema(40, 3);
            clsVehicleRouting.OptimizarRuta(cProblem);
        }

        static void TestLinearTimeSelectionAlgorithm()
        {
            int[] arr = { 12, 3, 5, 7, 4, 19, 26 };
            int n = arr.Length, k = 3;
            Console.WriteLine("K'th smallest element is "
                + LinearTimeSelectionAlgorithm.kthSmallest(arr, 0, n - 1, k));
        }

        static clsDataProblem GenerarProblema(Int32 intNumPuntosConsumo, Int32 intVehiclesNumber)
        {
            Random rnd = new Random(100);
            clsDataProblem cProblem = new clsDataProblem();
            cProblem.intPuntosConsumoNumber = intNumPuntosConsumo;
            cProblem.intVehiclesNumber = intVehiclesNumber;
            // Obtiene la demanda de cada punto de consumo
            double dblDemandTotal = 0;
            for (Int32 intPC = 0; intPC <= cProblem.intPuntosConsumoNumber; intPC++)
            {
                clsDataPuntoConsumo cPC = new clsDataPuntoConsumo();
                if (intPC == 0)
                {
                    cPC.dblLongitud = 10000;
                    cPC.dblLatitude = 10000;
                    cPC.dblDemand = 0;
                }
                else
                {
                    cPC.dblLongitud = rnd.Next(1, 20000);
                    cPC.dblLatitude = rnd.Next(1, 20000);
                    cPC.dblDemand = rnd.Next(1, 20);
                }
                dblDemandTotal = dblDemandTotal + cPC.dblDemand;
                cProblem.dicIdPuntoConsumoADatosPuntoConsumo.Add(intPC, cPC);
            }
            // Capacidad vehicles
            dblDemandTotal = dblDemandTotal * 1.2; // Se incrementa la demanda en un % para que a los coche les sobre algo de capacidad            
            for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
            {
                cProblem.dicIdVehicleCapacity.Add(intV, Math.Ceiling(dblDemandTotal / cProblem.intVehiclesNumber));
                cProblem.dicIdVehicleIdPuntoConsumoInicio.Add(intV, -intV);
                cProblem.dicIdVehicleIdPuntoConsumoFin.Add(intV, -intV);
            }
            // Matriz tiempo (velocidad 24 km/h que corresponden con 400 metros/minuto)
            for (Int32 intPC1 = 0; intPC1 <= cProblem.intPuntosConsumoNumber; intPC1++)
            {
                clsDataPuntoConsumo cPc1 = cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPC1];
                List<Int32> lstPc2 = new List<int>();
                List<double> lstDistancia = new List<double>();
                for (Int32 intPC2 = 0; intPC2 <= cProblem.intPuntosConsumoNumber; intPC2++)
                {
                    clsDataPuntoConsumo cPc2 = cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPC2];
                    if (intPC1 != intPC2)
                    {
                        string strPoints = intPC1 + "_" + intPC2;
                        double dblDistancia = Math.Sqrt(Math.Pow(cPc1.dblLongitud - cPc2.dblLongitud, 2) + Math.Pow(cPc1.dblLatitude - cPc2.dblLatitude, 2));
                        double dblMinutos = dblDistancia / 400;
                        cProblem.dicIdPuntoAIdPuntoBATiempo.Add(strPoints, dblMinutos);
                        cProblem.dicIdPuntoAIdPuntoBADistancia.Add(strPoints, dblDistancia);
                        lstPc2.Add(intPC2);
                        lstDistancia.Add(dblDistancia);
                    }
                    else
                    {
                        cProblem.dicIdPuntoAIdPuntoBATiempo.Add(intPC1 + "_" + intPC2, 0);
                        cProblem.dicIdPuntoAIdPuntoBADistancia.Add(intPC1 + "_" + intPC2, 0);
                    }
                }
                // Lo pasa a array y lo ordena
                Int32[] intPuntoConsumos = lstPc2.ToArray();
                double[] dblDistancias = lstDistancia.ToArray();
                Array.Sort(dblDistancias, intPuntoConsumos);
                lstPc2 = new List<int>();
                for (Int32 intI = 0; intI < intPuntoConsumos.Length; intI++)
                    lstPc2.Add(intPuntoConsumos[intI]);
                cProblem.dicIdPuntoConsumoALstPuntoCosumosMenorDistancia.Add(intPC1, lstPc2);
            }
            // Pone el depot como negativo para cada vehiculo
            double dblDistanciaTotalDepot = 0;
            for (Int32 intPC = 0; intPC <= cProblem.intPuntosConsumoNumber; intPC++)
            {                
                for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
                {
                    string strPoints = intPC + "_" + 0;
                    double dblDistancia = cProblem.dicIdPuntoAIdPuntoBATiempo[strPoints];
                    double dblMinutos = dblDistancia / 400;
                    cProblem.dicIdPuntoAIdPuntoBATiempo.Add(intPC + "_" + -intV, dblMinutos);
                    cProblem.dicIdPuntoAIdPuntoBADistancia.Add(intPC + "_" + -intV, dblDistancia);
                    // Mete el inverso                    
                    cProblem.dicIdPuntoAIdPuntoBATiempo.Add(-intV + "_" + intPC, dblMinutos);
                    cProblem.dicIdPuntoAIdPuntoBADistancia.Add(-intV + "_" + intPC, dblDistancia);
                    if (intPC == 0)
                    {                         
                        cProblem.dicIdPuntoAIdPuntoBATiempo.Add(-intV + "_" + -intV, 0);
                        cProblem.dicIdPuntoAIdPuntoBADistancia.Add(-intV + "_" + -intV,0);
                    }
                }
                dblDistanciaTotalDepot = 10+dblDistanciaTotalDepot + cProblem.dicIdPuntoAIdPuntoBATiempo[intPC + "_" + 0];
            }
            // Estima los comienzos y fin de cada punto
            double dblPromedioVehiculo = dblDistanciaTotalDepot / cProblem.intVehiclesNumber;
            double dblTercio = dblPromedioVehiculo / 1;
            for (Int32 intPC = 1; intPC <= cProblem.intPuntosConsumoNumber; intPC++)
            {
                double dblRand = rnd.NextDouble();
                // Tiempo de servicio (tiempo en realizar entrega o recogida quitando el tiempo de transporte)
                cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPC].dblTiempoServicio = 10;// + 5 * dblRand;
                // el 33% debe estar en el primer tercio en las 4 primeras horas el resto en las 8 horas
                if (dblRand < 0.333)
                {
                    cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPC].dblTimeWindowsMin = 0;
                    cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPC].dblTimeWindowsMax = Convert.ToInt32(dblTercio/2);
                }
                else
                {
                    cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPC].dblTimeWindowsMin = 0;
                    cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPC].dblTimeWindowsMax = Convert.ToInt32 (dblTercio);
                }
                // Introduce los coches validos o no OJO VER ABAJO PUES NO ESTA ACTIVO
                cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPC].blnVehiculosAptos = new Boolean[intVehiclesNumber + 1];
                for (Int32 intV = 1; intV <= intVehiclesNumber; intV++)
                {
                    dblRand = rnd.NextDouble();
                    // no lo activa
                    dblRand = 1;
                    if (dblRand < 0.05)
                        cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPC].blnVehiculosAptos[intV] = false;
                    else
                        cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intPC].blnVehiculosAptos[intV] = true;
                }
            }
            return cProblem;
        }
    }
}