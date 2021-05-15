using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

/// <summary>
/// Esta clase esta basada en PALNS de Victor Pillac
/// </summary>
namespace clsVehicleRouting
{
    class clsLNSVRPOld2
    {

        Random rnd = new Random(100);


        public clsDataStructuresRutas PararellLNSVR(clsDataProblem cProblem, clsParameters cParameters, clsParametersSimulatedAnnealing cParametersSimulatedAnnealing)
        {
            clsBackTrack cBackTrack = new clsBackTrack(cParameters.intNumSolucionesBacktrack);
            // Si se quiere paralelizar utiliza lambda
            if (cParameters.blnParalelizar)
            {
                // Toma el numero de procesadores
                Int32 intNumeroProcesadores = Environment.ProcessorCount;
                Parallel.For(0, intNumeroProcesadores, i =>
                {
                    //Llama a la funcion
                    LNSVRP(i, intNumeroProcesadores, cProblem, cParameters, cParametersSimulatedAnnealing, cBackTrack);
                });
            }
            else // Si no se paraleliza
                LNSVRP(1, 1, cProblem, cParameters, cParametersSimulatedAnnealing, cBackTrack);

            return cBackTrack.RutaMinima();

        }





        private void LNSVRP(Int32 intHilo, Int32 intHilosParalelos, clsDataProblem cProblem, clsParameters cParameters, clsParametersSimulatedAnnealing cParametersSimulatedAnnealing, clsBackTrack cBackTrack)
        {

            clsDataAdaptativeLayer cAdaptativeDestroy = new clsDataAdaptativeLayer(new List<string>() { "DestroyRandom", "DestroyCritical" });
            clsDataAdaptativeLayer cAdaptativeRepair = new clsDataAdaptativeLayer(new List<string>() { "RepairRegret1", "RepairRegret2", "RepairRegret3" });
            //clsDataRutas cRutasOptima = new clsDataRutas();
            Int32 intVueltas = 0;
            Int32 intCopias = 0;
            double dblTemperaturaInicial = 0; // temperatura incial simulated annealing
            double dblTemperaturaFinal = 0; //temperatura final simulated annealing
            double dblTemperaturaDecreaseRate = 0; // Decrease rate para temperatura Simlated Annleading
            double dblTemperaturaActual = 0; //Temperatura actual
            // Obtiene la solucion inicial
            clsDataStructuresRutas cRutas = SolucionInicial(cProblem, cParameters);
            // Inicializa las temperaturas
            cParametersSimulatedAnnealing.InicializarTemperaturas(cParameters, cRutas.dblDistanciaTotal);
            dblTemperaturaInicial = cParametersSimulatedAnnealing.dblTemperatureInicial;
            dblTemperaturaFinal = cParametersSimulatedAnnealing.dblTemperatureFinal;
            dblTemperaturaDecreaseRate = cParametersSimulatedAnnealing.dblTemperatureDecreaseRate;
            dblTemperaturaActual = dblTemperaturaInicial;
            // Numero de vueltas maximo sin cambio para backtrack
            cParameters.intNumIterations = cParameters.intNumIterations / intHilosParalelos;
            double dblMaxIteracionesParaBacktrack = (double)(cParameters.intNumIterations * intHilosParalelos) / (1.2 * (double)cParameters.intNumSolucionesBacktrack);
            Int32 intNumIteracionesBacktrack = 0;
            Boolean blnEnBucle = true;
            // Pruebas tiempos
            Stopwatch swDestroy = new Stopwatch();
            Stopwatch swRepair = new Stopwatch();
            Stopwatch swTotal = new Stopwatch();
            swTotal.Start();
            // Recorre hasta que se enfrie
            while ((dblTemperaturaActual > dblTemperaturaFinal) && blnEnBucle)
            {
                double dblCosteTotal = cRutas.dblCosteTotal;
                swDestroy.Start();
                // Selecciona el numero a destruir
                Int32 intNumeroADestruir = rnd.Next((Int32)(cParameters.dblPorcentajeNumeroPuntoConsumoADestruirMin * (double)cProblem.intPuntosConsumoNumber), (Int32)(cParameters.dblPorcentajeNumeroPuntoConsumoADestruirMax * (double)cProblem.intPuntosConsumoNumber));
                // Selecciona el algoritmo para destroy
                string strDestroy = cAdaptativeDestroy.ObtenerOperador();
                // Destruye
                if (strDestroy == "DestroyRandom")
                    DestroyRandom(cProblem, cParameters, cRutas, intNumeroADestruir);
                else if (strDestroy == "DestroyCritical")
                    DestroyCritical(cProblem, cParameters, cRutas, cParameters.dblRandomnessLevel, intNumeroADestruir);
                else
                    new Exception("Operador destroy no contemplado");
                swDestroy.Stop();

                // Construye
                swRepair.Start();
                string strRepair = cAdaptativeRepair.ObtenerOperador();
                Int32 intRegretValue = 1;
                if (strRepair == "RepairRegret1")
                    intRegretValue = 1;
                else if (strRepair == "RepairRegret2")
                    intRegretValue = 2;
                else if (strRepair == "RepairRegret3")
                    intRegretValue = 3;
                else
                    new Exception("Operador repair no contemplado");
                // Repara la solucion actual si no hay restricciones
                if (!cParameters.blnConsiderarTimeWindowSuperior)
                    Repair(cProblem, cParameters, cRutas, intRegretValue);
                else//Si hay que considerar restricciones
                    RepairConRestricciones(cProblem, cParameters, cRutas, intRegretValue);
                swRepair.Stop();
                // Comprueba si debe aceptar la nueva solucion o no
                if (cRutas.dblCosteTotal > dblCosteTotal)
                {
                    double dblSA = Math.Exp((dblCosteTotal - cRutas.dblCosteTotal) / dblTemperaturaActual);
                    // Si no se acepta la nueva solucion
                    if (dblSA < rnd.NextDouble())
                    {
                        cRutas.Deshacer(cProblem, cParameters);
                        cAdaptativeRepair.AddOperadorResultado(cParameters, strRepair, clsDataAdaptativeLayer.enumTipoResultado.SolucionRechazada);
                        cAdaptativeDestroy.AddOperadorResultado(cParameters, strDestroy, clsDataAdaptativeLayer.enumTipoResultado.SolucionRechazada);
                        intCopias++;
                    }
                    else
                    {
                        // Aunque es peor la acepta
                        cAdaptativeRepair.AddOperadorResultado(cParameters, strRepair, clsDataAdaptativeLayer.enumTipoResultado.SolucionAceptadaQueNoMejora);
                        cAdaptativeDestroy.AddOperadorResultado(cParameters, strDestroy, clsDataAdaptativeLayer.enumTipoResultado.SolucionAceptadaQueNoMejora);
                    }
                }
                else
                {
                    // Comprueba si la nueva solucion es mejor que la optima
                    if (cBackTrack.CosteMinimo() < 0 || (Convert.ToInt64(cRutas.dblDistanciaTotal) < Convert.ToInt64(cBackTrack.CosteMinimo())))
                    {
                        // Guarda la ruta para backtrack
                       cBackTrack.AddRuta(cRutas);
                        intNumIteracionesBacktrack = 0;
                        intCopias++;
                        // Es mejor
                        cAdaptativeRepair.AddOperadorResultado(cParameters, strRepair, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorGlobal);
                        cAdaptativeDestroy.AddOperadorResultado(cParameters, strDestroy, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorGlobal);
                        Console.WriteLine(DateTime .Now +"  "+ intHilo + " Minimo. " + intVueltas + "->"  + cBackTrack.CosteMinimo() + " Ruta:" + cRutas.dblDistanciaTotal + " Copias:" + intCopias + " Temp:" + cParametersSimulatedAnnealing.GetTemperatura());
                    }
                    else
                    {
                        // Es mejor que la anterior pero no optima
                        cAdaptativeRepair.AddOperadorResultado(cParameters, strRepair, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorParcial);
                        cAdaptativeDestroy.AddOperadorResultado(cParameters, strDestroy, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorParcial);
                    }
                }
                intVueltas++;
                // Comprueba si debe hacer backtrak
                if (intNumIteracionesBacktrack > dblMaxIteracionesParaBacktrack)
                {
                    // Tiempo
                    //swTotal.Stop();
                    TimeSpan tsDestroy = swDestroy.Elapsed;
                    TimeSpan tsRepair = swRepair.Elapsed;
                    TimeSpan tsTotal = swTotal.Elapsed;
                   // Console.WriteLine(cRutas.swTotal.Elapsed.TotalSeconds + " Medir:" + cRutas.swMedir.Elapsed.TotalSeconds);
                    //swTotal.Start();
                    if(intVueltas>5000)
                    Console.WriteLine("Total:" + tsTotal.TotalSeconds +" Destroy:" + tsDestroy.TotalSeconds + " Repair:" + tsRepair.TotalSeconds );
                    // Format and display the TimeSpan value.
                    //string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    //    ts.Hours, ts.Minutes, ts.Seconds,
                    //    ts.Milliseconds / 10);
                    


                    cRutas = cBackTrack.GetRuta();
                    if (cRutas == null)
                        blnEnBucle = false;
                    else
                    {
                        intNumIteracionesBacktrack = 0;
                        //cBackTrack.RutaMinima ();
                        Console.WriteLine(intHilo + " Backtrack " + intVueltas + "->" + cBackTrack.CosteMinimo() + " Ruta:" + cRutas.dblDistanciaTotal + " Copias:" + intCopias + " Temp:" + dblTemperaturaActual);
                    }
                }
                intNumIteracionesBacktrack++;
                // Baja la temperatura
                dblTemperaturaActual = dblTemperaturaDecreaseRate * dblTemperaturaActual;
            }
        }





        /// <summary>
        /// Esta funcion selecciona de forma aleatoria intNumeroADestruir
        /// puntos de consumos asignados y los elemina de la asignacion
        /// pasandolos a no asignados. Ademas actualiza el coste minimo
        /// de insercion para esos puntos que quedan como pendientes de asignar
        /// </summary>
        /// <param name="cProblem"></param>
        /// <param name="cRutas"></param>
        /// <param name="intNumeroADestruir"></param>
        private void DestroyRandom(clsDataProblem cProblem, clsParameters cParameters, clsDataStructuresRutas cRutas, Int32 intNumeroADestruir)
        {
            // Pone a cero todo
            // cRutas.hsIdPuntoConsumoNoAsignados = new HashSet<int>();
            cRutas.DeshacerInicializar();
            // Selecciona intNumADestruir
            Random rnd = new Random(100);
            HashSet<Int32> hsDestruidas = new HashSet<int>();
            HashSet<Int32> hsIdVehicle = new HashSet<int>();
            while (hsDestruidas.Count < intNumeroADestruir)
            {
                // Seleccionar una nueva para destruir
                Int32 intId = rnd.Next(1, cProblem.intPuntosConsumoNumber);
                // Destruye
                if (!hsDestruidas.Contains(intId) && !cRutas.hsIdPuntoConsumoNoAsignados.Contains(intId))
                {
                    Int32 intIdVehicle = cRutas.dicIdPuntoConsumoAIdVehicle[intId];
                    hsDestruidas.Add(intId);
                    cRutas.RemovePuntoConsumo(cProblem, cParameters, intId, true);
                    if (!hsIdVehicle.Contains(intIdVehicle))
          hsIdVehicle.Add(intIdVehicle);
                }
            }
            // Recalcula los tiempos y holguras de las rutas modificadas
            foreach (Int32 intIdVehicle in hsIdVehicle)
                cRutas.CalcularTimeWindowsVehicle(cProblem, intIdVehicle);
            // Recalcula los tiempos de los no asignados a los edges de las rutas para todos los vehiculos
            // Comienza reinicializnado los coste
            // Recorre cada vehiculo
            // añade el coste desde cada no asignado a cada edge del vehiculo

        }

        private void DestroyCritical(clsDataProblem cProblem, clsParameters cParameters, clsDataStructuresRutas cRutas, double dblRandomnessLevel, Int32 intNumeroADestruir)
        {
            HashSet<Int32> hsIdVehicle = new HashSet<int>();
            // Numero de operaciones a destruir
            double dblRandom = rnd.NextDouble();
            dblRandom = Math.Pow(dblRandom, dblRandomnessLevel);
            dblRandom = cProblem.intPuntosConsumoNumber * dblRandom;
            cRutas.DeshacerInicializar();
            // Selecciona los intNumADestruir con menor valor
            Boolean blnEnBucle = true;
            Int32 intCuenta = cRutas.sdCritical.Count();
            for (Int32 intI = 0; intI < intNumeroADestruir && blnEnBucle; intI++)
            {
                if (intI < intCuenta)
                {
                    Int32 intIdPuntoConsumo = Convert.ToInt32(cRutas.sdCritical.GetMinFirstKey(intI));
                    Int32 intIdVehicle = cRutas.dicIdPuntoConsumoAIdVehicle[intIdPuntoConsumo];
                    cRutas.RemovePuntoConsumo(cProblem, cParameters,intIdPuntoConsumo , true);
                    if (!hsIdVehicle.Contains(intIdVehicle))
                        hsIdVehicle.Add(intIdVehicle);
                }
                else
                    blnEnBucle = false;
            }
            // Recalcula los tiempos y holguras de las rutas modificadas
            foreach (Int32 intIdVehicle in hsIdVehicle)
                cRutas.CalcularTimeWindowsVehicle(cProblem, intIdVehicle);
        }



        /// <summary>
        /// Esta funcion realiza el Repair para un regret pasado. Esta funcion
        /// no tiene en cuenta las restricciones. Basicamente comprueba
        /// cual es el punto de consumo con mayor (menor) regret
        /// y lo va asignando. NO TIENE EN CUENTA RESTRICCIONES
        /// </summary>
        /// <param name="cProblem"></param>
        /// <param name="cParameters"></param>
        /// <param name="cRutas"></param>
        /// <param name="intRegretLevel"></param>
        private void Repair(clsDataProblem cProblem, clsParameters cParameters, clsDataStructuresRutas cRutas, Int32 intRegretLevel)
        {
            // Bucle para insertar todos los puntos de consumo
            Boolean blnEnBucle = true;
            while (blnEnBucle)
            {
                // Para cada punto de consumo no asignado obtiene el idPuntoConsumo y edge para insertar con menor regret 3
                var valores = cRutas.GetIdPuntoConsumoYEdgeDeMinRegret(cProblem, cParameters, intRegretLevel);
                // Se inserta el valor minimo, en el edge 
                cRutas.AddPuntoConsumoToEdge(cProblem, cParameters, valores.Item1, valores.Item2, true,true );
                // Obtiene el numero que falta por asignar y si no quedan sale
                if (cRutas.CountNoAsignados() == 0)
                    blnEnBucle = false;
            }
        }

        /// <summary>
        /// Esta funcion realiza el repair con restricciones.
        /// Obtiene el punto asignado de mayor (menor) regret
        /// que no este todavía asignado y lo va insertando
        /// teniendo en cuenta que sea factible
        /// </summary>
        /// <param name="cProblem"></param>
        /// <param name="cParameters"></param>
        /// <param name="cRutas"></param>
        /// <param name="intRegretLevel"></param>
        private void RepairConRestricciones(clsDataProblem cProblem, clsParameters cParameters, clsDataStructuresRutas cRutas, Int32 intRegretLevel)
        {
            // Bucle para insertar todos los puntos de consumo
            Boolean blnEnBucle = true;
            while (blnEnBucle)
            {
                // Para cada punto de consumo no asignado obtiene el idPuntoConsumo y edge para insertar con menor regret 3
                var valores = cRutas.GetIdPuntoConsumoYEdgeDeMinRegretConRestricciones(cProblem, cParameters, intRegretLevel);
                if (valores.Item1 > -1)
                {
                    // Se inserta el valor minimo, en el edge 
                    cRutas.AddPuntoConsumoToEdge(cProblem, cParameters, valores.Item1, valores.Item2, true, true);
                    // Obtiene el numero que falta por asignar y si no quedan sale
                    if (cRutas.CountNoAsignados() == 0)
                        blnEnBucle = false;
                }
                else
                    blnEnBucle = false; // No hay ninguno factible por lo que sale.
            }
            // Calcula el coste total
            cRutas.intPuntosNoAsignados = cRutas.hsIdPuntoConsumoNoAsignados.Count;
            cRutas.dblCosteTotal = cRutas.dblDistanciaTotal + 10000 * cRutas.intPuntosNoAsignados;
        }


        /// <summary>
        /// Esta funcion genera una solucion inicial basada en un nivel de regret
        /// comprueba que punto de consumo tiene el menor regret level a las operaciones
        /// asignadas y siempre que haya capacidad en esa ruta la asigna a esa ruta
        /// y continua con la siguiente operacion no asignada hasta que no quedan 
        /// operaciones por asignar
        /// </summary>
        /// <param name="cProblem">datos del problema</param>
        /// <param name="intRegretLevel">regret level</param>
        /// <returns></returns>
        private clsDataStructuresRutas SolucionInicial(clsDataProblem cProblem, clsParameters cParameters)
        {
            Int32 intRegretLevel = cParameters.intRegretLevelSolucionInicial;
            // Regret level debe estar entre 1 y 3
            if (intRegretLevel < 1 || intRegretLevel > 3)
                new Exception("Regret level no adecuado");
            // Pone a cero las rutas
            clsDataStructuresRutas cRutas = new clsDataStructuresRutas(cProblem);
            // Repara la solucion actual si no hay restricciones
            if (!cParameters.blnConsiderarTimeWindowSuperior)
                Repair(cProblem, cParameters, cRutas, cParameters.intRegretLevelSolucionInicial);
            else//Si hay que considerar restricciones
                RepairConRestricciones(cProblem, cParameters, cRutas, cParameters.intRegretLevelSolucionInicial);
            return cRutas;
        }


    }
}
