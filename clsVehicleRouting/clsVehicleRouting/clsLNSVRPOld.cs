using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Esta clase esta basada en PALNS de Victor Pillac
/// </summary>
namespace clsVehicleRouting
{
    class clsLNSVRPOld
    {

        Random rnd = new Random(100);


       



        //public clsDataStructuresRutas LNSVRPNew(Int32 intHilo, clsDataProblem cProblem, clsParameters cParameters, clsParametersSimulatedAnnealing cParametersSimulatedAnnealing, clsBackTrack cBackTrack)
        //{

        //    clsDataAdaptativeLayer cAdaptativeDestroy = new clsDataAdaptativeLayer(new List<string>() { "DestroyRandom", "DestroyCritical" });
        //    clsDataAdaptativeLayer cAdaptativeRepair = new clsDataAdaptativeLayer(new List<string>() { "RepairRegret1", "RepairRegret2", "RepairRegret3" });
        //    clsDataRutas cRutasOptima = new clsDataRutas();
        //    Int32 intVueltas = 0;
        //    Int32 intCopias = 0;
        //    // Obtiene la solucion inicial
        //    clsDataStructuresRutas cRutas = SolucionInicialNew(cProblem, cParameters.intRegretLevelSolucionInicial);
        //    // Inicializa las temperaturas
        //    cParametersSimulatedAnnealing.InicializarTemperaturas(cParameters, cRutas.dblCostoTotal);
        //    // Numero de vueltas maximo sin cambio para backtrack
        //    double dblMaxIteracionesParaBacktrack = (double)cParameters.intNumIterations / (1.2 * (double)cParameters.intNumSolucionesBacktrack);
        //    Int32 intNumIteracionesBacktrack = 0;

        //    while (cParametersSimulatedAnnealing.Continuar())
        //    {
        //        double dblCostoTotalOld = cRutas.dblCostoTotal;
        //        // Selecciona el numero a destruir
        //        Int32 intNumeroADestruir = rnd.Next((Int32)(cParameters.dblPorcentajeNumeroPuntoConsumoADestruirMin * (double)cProblem.intPuntosConsumoNumber), (Int32)(cParameters.dblPorcentajeNumeroPuntoConsumoADestruirMax * (double)cProblem.intPuntosConsumoNumber));
        //        // Selecciona el algoritmo para destroy
        //        string strDestroy = cAdaptativeDestroy.ObtenerOperador();
        //        // Destruye
        //        if (strDestroy == "DestroyRandom")
        //            DestroyRandomNew(cProblem, cRutas, intNumeroADestruir);
        //        else if (strDestroy == "DestroyCritical")
        //            DestroyCriticalNew(cProblem, cRutas, cParameters.dblRandomnessLevel, intNumeroADestruir);
        //        else
        //            new Exception("Operador destroy no contemplado");
        //        //    // Construye
        //        string strRepair = cAdaptativeRepair.ObtenerOperador();
        //        Int32 intRegretValue = 1;
        //        if (strRepair == "RepairRegret1")
        //            intRegretValue = 1;
        //        else if (strRepair == "RepairRegret2")
        //            intRegretValue = 2;
        //        else if (strRepair == "RepairRegret3")
        //            intRegretValue = 3;
        //        else
        //            new Exception("Operador repair no contemplado");
        //        // Repara la solucion actual
        //        RepairNew(cProblem, cRutas, intRegretValue);
        //        // Comprueba si debe aceptar la nueva solucion o no
        //        if (cRutas.dblCostoTotal > dblCostoTotalOld)
        //        {
        //            double dblSA = Math.Exp((dblCostoTotalOld - cRutas.dblCostoTotal) / cParametersSimulatedAnnealing.GetTemperatura());
        //            // Si no se acepta la nueva solucion
        //            if (dblSA < rnd.NextDouble())
        //            {
        //                cRutas.Deshacer(cProblem);
        //                cAdaptativeRepair.AddOperadorResultado(cParameters, strRepair, clsDataAdaptativeLayer.enumTipoResultado.SolucionRechazada);
        //                cAdaptativeDestroy.AddOperadorResultado(cParameters, strDestroy, clsDataAdaptativeLayer.enumTipoResultado.SolucionRechazada);
        //                intCopias++;
        //            }
        //            else
        //            {
        //                // Aunque es peor la acepta
        //                cAdaptativeRepair.AddOperadorResultado(cParameters, strRepair, clsDataAdaptativeLayer.enumTipoResultado.SolucionAceptadaQueNoMejora);
        //                cAdaptativeDestroy.AddOperadorResultado(cParameters, strDestroy, clsDataAdaptativeLayer.enumTipoResultado.SolucionAceptadaQueNoMejora);
        //            }
        //        }
        //        else
        //        {
        //            // Comprueba si la nueva solucion es mejor que la optima
        //            if (cBackTrack.CosteMinimo() < 0 || (Convert.ToInt64(cRutas.dblCostoTotal) < Convert.ToInt64(cBackTrack.CosteMinimo())))
        //            {
        //                // Guarda la ruta para backtrack
        //                cBackTrack.AddRuta(cRutas);
        //                intNumIteracionesBacktrack = 0;
        //                intCopias++;
        //                // Aunque es peor la acepta
        //                cAdaptativeRepair.AddOperadorResultado(cParameters, strRepair, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorGlobal);
        //                cAdaptativeDestroy.AddOperadorResultado(cParameters, strDestroy, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorGlobal);
        //                Console.WriteLine(intHilo + " Minimo. " + intVueltas + "->" + cBackTrack.CosteMinimo() + " Ruta:" + cRutas.dblCostoTotal + " Copias:" + intCopias + " Temp:" + cParametersSimulatedAnnealing.GetTemperatura());
        //            }
        //            else
        //            {
        //                // Es mejor que la anterior pero no optima
        //                cAdaptativeRepair.AddOperadorResultado(cParameters, strRepair, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorParcial);
        //                cAdaptativeDestroy.AddOperadorResultado(cParameters, strDestroy, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorParcial);
        //            }
        //        }
        //        intVueltas++;
        //        // Comprueba si debe hacer backtrak
        //        if (intNumIteracionesBacktrack > dblMaxIteracionesParaBacktrack)
        //        {
        //            cRutas = cBackTrack.GetRuta();
        //            intNumIteracionesBacktrack = 0;
        //            //cBackTrack.RutaMinima ();
        //            Console.WriteLine(intHilo + " Backtrack " + intVueltas + "->" + cBackTrack.CosteMinimo() + " Ruta:" + cRutas.dblCostoTotal + " Copias:" + intCopias + " Temp:" + cParametersSimulatedAnnealing.GetTemperatura());
        //        }
        //        intNumIteracionesBacktrack++;
        //        // Baja la temperatura
        //        cParametersSimulatedAnnealing.BajarTemperatura();
        //    }
        //    // Devuelve la ruta optima
        //    return cBackTrack.RutaMinima();
        //}


        public clsDataRutas LNSVRPOld(clsDataProblem cProblem, clsParameters cParameters, clsParametersSimulatedAnnealing cParametersSimulatedAnnealing)
        {

            clsDataAdaptativeLayer cAdaptativeDestroy = new clsDataAdaptativeLayer(new List<string>() { "DestroyRandom", "DestroyCritical" });
            clsDataAdaptativeLayer cAdaptativeRepair = new clsDataAdaptativeLayer(new List<string>() { "RepairRegret1", "RepairRegret2", "RepairRegret3" });
            clsDataRutas cRutasOptima = new clsDataRutas();
            Int32 intVueltas = 0;
            Int32 intCopias = 0;
            // Obtiene la solucion inicial
            clsDataRutas cRutas = SolucionInicial(cProblem, cParameters.intRegretLevelSolucionInicial);
            // Inicializa las temperaturas
            cParametersSimulatedAnnealing.InicializarTemperaturas(cParameters, cRutas.dblDistanciaTotal);
            // Repite un numero de iteraciones
            double dblCosteMin = double.MaxValue;

            while (cParametersSimulatedAnnealing.Continuar())
            {
                // TODO: mejorar esto para no tener que copiar todo
                // Copia la ruta actual
                clsDataRutas cRutasOld = clsObjectCopy.Clone<clsDataRutas>(cRutas);
                // Selecciona el numero a destruir
                Int32 intNumeroADestruir = rnd.Next((Int32)(cParameters.dblPorcentajeNumeroPuntoConsumoADestruirMin * (double)cProblem.intPuntosConsumoNumber), (Int32)(cParameters.dblPorcentajeNumeroPuntoConsumoADestruirMax * (double)cProblem.intPuntosConsumoNumber));
                // Selecciona el algoritmo para destroy
                string strDestroy = cAdaptativeDestroy.ObtenerOperador();
                // Destruye
                if (strDestroy == "DestroyRandom")
                    DestroyRandomOld(cProblem, cRutas, intNumeroADestruir);
                //if (strDestroy == "DestroyCritial")
                //    //DestroyCriticalOld(cProblem, cRutas, cParameters.dblRandomnessLevel, intNumeroADestruir);
                //// Construye
                string strRepair = cAdaptativeRepair.ObtenerOperador();
                Int32 intRegretValue = 1;
                //if (strRepair == "RepairRegret1")
                //    intRegretValue = 1;
                //else if (strRepair == "RepairRegret2")
                //    intRegretValue = 2;
                //else if (strRepair == "RepairRegret3")
                //    intRegretValue = 3;
                //else
                //    new Exception("Operador repair no contemplado");

                RepairOld(cProblem, cRutas, intRegretValue);
                // Comprueba si debe aceptar la nueva solucion o no
                if (cRutas.dblDistanciaTotal > cRutasOld.dblDistanciaTotal)
                {
                    double dblSA = Math.Exp((cRutasOld.dblDistanciaTotal - cRutas.dblDistanciaTotal) / cParametersSimulatedAnnealing.GetTemperatura());
                    // Si no se acepta la nueva solucion
                    if (dblSA < rnd.NextDouble())
                    {
                        //TODO: SOlo copiar lo que ha cambiado y no todo.
                        cRutas = clsObjectCopy.Clone<clsDataRutas>(cRutasOld);
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
                    if (cRutas.dblDistanciaTotal < dblCosteMin)
                    {
                        dblCosteMin = cRutas.dblDistanciaTotal;
                        cRutasOptima = clsObjectCopy.Clone<clsDataRutas>(cRutas);
                        intCopias++;
                        // Aunque es peor la acepta
                        cAdaptativeRepair.AddOperadorResultado(cParameters, strRepair, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorGlobal);
                        cAdaptativeDestroy.AddOperadorResultado(cParameters, strDestroy, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorGlobal);
                    }
                    else
                    {
                        // Es mejor que la anterior pero no optima
                        cAdaptativeRepair.AddOperadorResultado(cParameters, strRepair, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorParcial);
                        cAdaptativeDestroy.AddOperadorResultado(cParameters, strDestroy, clsDataAdaptativeLayer.enumTipoResultado.SolucionMejorParcial);
                    }
                }
                Console.WriteLine(intVueltas + "->" + dblCosteMin + " Copias:" + intCopias + " Temp:" + cParametersSimulatedAnnealing.GetTemperatura());
                intVueltas++;
                // Actualiza los scores de los destroy

                // Baja la temperatura
                cParametersSimulatedAnnealing.BajarTemperatura();
            }
            return cRutasOptima;
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
        private void DestroyRandomOld(clsDataProblem cProblem, clsDataRutas cRutas, Int32 intNumeroADestruir)
        {
            // Pone a cero todo
            cRutas.hsIdPuntoConsumoNoAsignados = new HashSet<int>();

            // Selecciona intNumADestruir
            Random rnd = new Random(100);
            HashSet<Int32> hsDestruidas = new HashSet<int>();

            while (hsDestruidas.Count < intNumeroADestruir)
            {
                // Seleccionar una nueva para destruir
                Int32 intId = rnd.Next(1, cProblem.intPuntosConsumoNumber);
                // Destruye
                if (!hsDestruidas.Contains(intId))
                {
                    hsDestruidas.Add(intId);
                    cRutas.Remove(cProblem, intId);
                }
            }
            // Obtiene el regret de todas las destruidas
            cRutas.CalcularCosteMinimoNoAsignados(cProblem);
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
        private void DestroyRandomNew(clsDataProblem cProblem,clsParameters cParameters, clsDataStructuresRutas cRutas, Int32 intNumeroADestruir)
        {
            // Pone a cero todo
            cRutas.hsIdPuntoConsumoNoAsignados = new HashSet<int>();
            cRutas.DeshacerInicializar();

            // Selecciona intNumADestruir
            Random rnd = new Random(100);
            HashSet<Int32> hsDestruidas = new HashSet<int>();

            while (hsDestruidas.Count < intNumeroADestruir)
            {
                // Seleccionar una nueva para destruir
                Int32 intId = rnd.Next(1, cProblem.intPuntosConsumoNumber);
                // Destruye
                if (!hsDestruidas.Contains(intId))
                {
                    hsDestruidas.Add(intId);
                    cRutas.RemovePuntoConsumo(cProblem,cParameters , intId, true);
                }
            }
        }

        private void DestroyCriticalNew(clsDataProblem cProblem, clsParameters cParameters, clsDataStructuresRutas cRutas, double dblRandomnessLevel, Int32 intNumeroADestruir)
        {
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
                    cRutas.RemovePuntoConsumo(cProblem,cParameters , Convert.ToInt32(cRutas.sdCritical.GetMinFirstKey(intI)), true);
                }
                else
                    blnEnBucle = false;
            }
        }


        private void DestroyCriticalOld(clsDataProblem cProblem, clsDataRutas cRutas, double dblRandomnessLevel, Int32 intNumeroADestruir)
        {
            //TODO: Hacerlo en el ADD y Remove para no tener que crearlo cada vez (sortedlist)
            // Numero de operaciones a destruir
            double dblRandom = rnd.NextDouble();
            dblRandom = Math.Pow(dblRandom, dblRandomnessLevel);
            dblRandom = cProblem.intPuntosConsumoNumber * dblRandom;

            SortedList<double, Int32> slCostIdPuntoConsumo = new SortedList<double, int>();
            for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
            {
                Int32 intIdPuntoConsumo = cRutas.dicIdVehiculoAIdPuntoConsumoAsignadosPrimeroEnRuta[intV];
                // Se toma el siguiente pues el primero es el depot
                intIdPuntoConsumo = cRutas.dicIdPuntoConsumoAsignadosADatos[intIdPuntoConsumo].intPuntoConsumoSiguiente;
                Boolean blnEnBucle = true;
                while (blnEnBucle)
                {
                    Int32 intIdPuntoConsumoAnterior = cRutas.dicIdPuntoConsumoAsignadosADatos[intIdPuntoConsumo].intPuntoConsumoAnterior;
                    Int32 intIdPuntoConsumoSiguiente = cRutas.dicIdPuntoConsumoAsignadosADatos[intIdPuntoConsumo].intPuntoConsumoSiguiente;
                    double dblAhorroCosteDestroy = cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoSiguiente] -
                        cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo] -
                        cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumo + "_" + intIdPuntoConsumoSiguiente] +
                        0.00001 * rnd.NextDouble();// Se añade un valor aleatorio para que no haya dos costes iguales
                    slCostIdPuntoConsumo.Add(dblAhorroCosteDestroy, intIdPuntoConsumo);
                    intIdPuntoConsumo = intIdPuntoConsumoSiguiente;
                    if (intIdPuntoConsumo < 0)
                        blnEnBucle = false;
                }
            }
            // Selecciona los intNumADestruir con menor valor
            for (Int32 intI = 0; intI < intNumeroADestruir; intI++)
            {
                cRutas.Remove(cProblem, slCostIdPuntoConsumo.ElementAt(intI).Value);
            }
            // Obtiene el regret de todas las destruidas
            cRutas.CalcularCosteMinimoNoAsignados(cProblem);
        }

        private void DestroyRelated(clsDataRutas cRutas)
        {

        }

        private void RepairOld(clsDataProblem cProblem, clsDataRutas cRutas, Int32 intRegretLevel)
        {
            // Para cada punto de consumo no asignado obtiene el vehiculo de menor regret 
            List<Int32> lstIdPuntoConsumoNoAsignados = cRutas.hsIdPuntoConsumoNoAsignados.ToList();
            foreach (Int32 intIdPuntoConsumo in lstIdPuntoConsumoNoAsignados)
            {
                // Obtien el vehiculo de menor regret para el punto de consumo pasado
                var values = RepairRegretOld(cProblem, cRutas, intRegretLevel, intIdPuntoConsumo);

                // Se inserta el valor minimo
                cRutas.Add(cProblem, intIdPuntoConsumo, values.Item2);
            }

        }


        private void RepairNew(clsDataProblem cProblem,clsParameters cParameters, clsDataStructuresRutas cRutas, Int32 intRegretLevel)
        {
            // Bucle para insertar todos los puntos de consumo
            Boolean blnEnBucle = true;
            while (blnEnBucle)
            {
                // Para cada punto de consumo no asignado obtiene el idPuntoConsumo y edge para insertar con menor regret 3
                var valores = cRutas.GetIdPuntoConsumoYEdgeDeMinRegret(cProblem,cParameters , intRegretLevel);
                // Se inserta el valor minimo, en el edge 
                cRutas.AddPuntoConsumoToEdge(cProblem,cParameters , valores.Item1, valores.Item2, true,true);
                // Obtiene el numero que falta por asignar y si no quedan sale
                if (cRutas.CountNoAsignados() == 0)
                    blnEnBucle = false;
            }
        }

        //        private (double, Int32, double) RepairRegretNew(clsDataProblem cProblem, clsDataStructuresRutas  cRutas, Int32 intRegretLevel, Int32 intIdPuntoConsumo)
        //        {
        //            // El regret level debe ser entre 1 y 3
        //            if (intRegretLevel < 1 || intRegretLevel > 3)
        //                new Exception("Regret level no adecuado");
        //            // EL punto de consumo no puede estar en una ruta (sin es regret normal)
        //            if (!cRutas.hsIdPuntoConsumoNoAsignados.Contains(intIdPuntoConsumo))
        //                new Exception("Punto de consumo ya asignado a ruta");
        ////obtiene el numero de vehiculos con datos

        //            // Calcula y devuelve el regret
        //            if (intRegretLevel == 1)
        //                return (dblObjetivo1, intIdVehiculo1, dblObjetivo1);
        //            else if (intRegretLevel == 2)
        //                return (dblObjetivo2 - dblObjetivo1, intIdVehiculo1, dblObjetivo1);
        //            // Es regret 3
        //            return (dblObjetivo2 - dblObjetivo1 + dblObjetivo3 - dblObjetivo1, intIdVehiculo1, dblObjetivo1);
        //        }



        /// <summary>
        /// Obtiene el regret hasta nivel 3 para un punto de consumo pasado
        /// </summary>
        /// <param name="cProblem">Datos del problema</param>
        /// <param name="cRutas">Datos de las rutas con puntos de consumos asignados y no</param>
        /// <param name="intRegretLevel">Regret level que se quiere calcular 1, 2 o 3</param>
        /// <param name="intIdPuntoConsumo">Punto de consumo no asignado para el que se calcula el regret</param>
        /// <returns> Primer valor el regret para el nivel que se ha indicado, segundo valor el punto de conumo para el que
        /// se ha obtenido ese regret, tercer valor el regret 1 por si hay que desempatar</returns>
        private (double, Int32, double) RepairRegretOld(clsDataProblem cProblem, clsDataRutas cRutas, Int32 intRegretLevel, Int32 intIdPuntoConsumo)
        {
            // El regret level debe ser entre 1 y 3
            if (intRegretLevel < 1 || intRegretLevel > 3)
                new Exception("Regret level no adecuado");
            // EL punto de consumo no puede estar en una ruta (sin es regret normal)
            if (cRutas.dicIdPuntoConsumoAsignadosADatos.ContainsKey(intIdPuntoConsumo) || !cRutas.hsIdPuntoConsumoNoAsignados.Contains(intIdPuntoConsumo))
                new Exception("Punto de consumo ya asignado a ruta");
            // Calcula para el punto de consumo las mejores posiciones
            double dblObjetivo1 = double.MaxValue;
            double dblObjetivo2 = double.MaxValue;
            double dblObjetivo3 = double.MaxValue;
            Int32 intIdVehiculo1 = -1;
            Int32 intIdVehiculo2 = -1;
            Int32 intIdVehiculo3 = -1;
            // Recorre cada vehiculo y calcula los tres valores menores
            for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
            {
                // Comprueba que el vehiculo tiene capacidad para incluir la demanda del punto de consumo estudiado
                if ((cRutas.dicIdVehiculoACapacidadAsignada[intV] + cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblDemand) <= cProblem.dicIdVehicleCapacity[intV])
                {
                    double dblObjetivo = cRutas.dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumo + "_" + intV].dblInsertionCost;
                    if (dblObjetivo < dblObjetivo3 && dblObjetivo > dblObjetivo2)
                    {
                        dblObjetivo3 = dblObjetivo;
                        intIdVehiculo3 = intV;
                    }
                    if (dblObjetivo < dblObjetivo2 && dblObjetivo > dblObjetivo1)
                    {
                        dblObjetivo3 = dblObjetivo2;
                        intIdVehiculo3 = intIdVehiculo2;
                        dblObjetivo2 = dblObjetivo;
                        intIdVehiculo2 = intV;
                    }
                    if (dblObjetivo <= dblObjetivo1)
                    {
                        dblObjetivo3 = dblObjetivo2;
                        intIdVehiculo3 = intIdVehiculo2;
                        dblObjetivo2 = dblObjetivo1;
                        intIdVehiculo2 = intV;
                        dblObjetivo1 = dblObjetivo;
                        intIdVehiculo1 = intV;
                    }
                }
            }// fin for
            // Calcula y devuelve el regret
            if (intRegretLevel == 1)
                return (dblObjetivo1, intIdVehiculo1, dblObjetivo1);
            else if (intRegretLevel == 2)
                return (dblObjetivo2 - dblObjetivo1, intIdVehiculo1, dblObjetivo1);
            // Es regret 3
            return (dblObjetivo2 - dblObjetivo1 + dblObjetivo3 - dblObjetivo1, intIdVehiculo1, dblObjetivo1);
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
        public clsDataRutas SolucionInicial(clsDataProblem cProblem, Int32 intRegretLevel)
        {
            // Regret level debe estar entre 1 y 3
            if (intRegretLevel < 1 || intRegretLevel > 3)
                new Exception("Regret level no adecuado");
            // Pone a cero las rutas
            clsDataRutas cRutas = new clsDataRutas();
            // Añade todos los punto de consumos a no asignados y calcula las distancias a cada vehiculo (al nodo cero)
            for (Int32 intPC = 1; intPC <= cProblem.intPuntosConsumoNumber; intPC++)
            {
                cRutas.hsIdPuntoConsumoNoAsignados.Add(intPC);
                // Carga la las distancias de cada punto no asignado al depto (idPuntoConsumo=0) para cada vehiculo
                for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
                {
                    clsDataRegretEdge cEdge = new clsDataRegretEdge();
                    cEdge.intIdPuntoConsumoAnterior = -intV;
                    cEdge.intIdPuntoConsumoSiguiente = -intV;
                    cEdge.dblInsertionCost = 2 * cProblem.dicIdPuntoAIdPuntoBATiempo[0 + "_" + intPC];
                    cRutas.dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo.Add(intPC + "_" + intV, cEdge);
                }
            }
            // Inicializa todas las rutas con el Depot (intPuntoConsumo=0)
            for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
            {
                // El depot se pone como el intV en negativo uno por cada vehiculo
                clsDataRutasPuntoConsumo cPC = new clsDataRutasPuntoConsumo();
                cPC.intIdVehiculo = intV;
                cPC.intPuntoConsumoAnterior = -intV;
                cPC.intPuntoConsumoSiguiente = -intV;
                cRutas.dicIdPuntoConsumoAsignadosADatos.Add(-intV, cPC);
                cRutas.dicIdVehiculoACapacidadAsignada.Add(intV, 0);
                cRutas.dicIdVehiculoAIdPuntoConsumoAsignadosPrimeroEnRuta.Add(intV, -intV);
            }
            // Bucle para insertar todos los puntos de consumo
            Boolean blnEnBucle = true;
            while (blnEnBucle)
            {
                double dblRegretMin = double.MaxValue;
                double dblReretMin1 = double.MaxValue;
                Int32 intIdPuntoConsumoMin = -1;
                Int32 intIdVehiculoMin = -1;
                // Para cada punto de consumo no asignado obtiene el vehiculo de menor regret 3
                foreach (Int32 intIdPuntoConsumo in cRutas.hsIdPuntoConsumoNoAsignados)
                {
                    // Obtien el vehiculo de menor regret para el punto de consumo pasado
                    var values = RepairRegretOld(cProblem, cRutas, intRegretLevel, intIdPuntoConsumo);
                    // Se queda con el punto de consumo que minimice el regret de todos los puntos y todos los vehiculos
                    if (values.Item1 < dblRegretMin || (values.Item1 == dblRegretMin && values.Item3 < dblReretMin1))
                    {
                        intIdPuntoConsumoMin = intIdPuntoConsumo;
                        intIdVehiculoMin = values.Item2;
                        dblRegretMin = values.Item1;
                        dblReretMin1 = values.Item3;
                    }
                }
                // Se inserta el valor minimo
                cRutas.Add(cProblem, intIdPuntoConsumoMin, intIdVehiculoMin);
                if (cRutas.hsIdPuntoConsumoNoAsignados.Count == 0)
                    blnEnBucle = false;
            }
            return cRutas;
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
        public clsDataStructuresRutas SolucionInicialNew(clsDataProblem cProblem, clsParameters cParameters, Int32 intRegretLevel)
        {
            // Regret level debe estar entre 1 y 3
            if (intRegretLevel < 1 || intRegretLevel > 3)
                new Exception("Regret level no adecuado");
            // Pone a cero las rutas
            clsDataStructuresRutas cRutas = new clsDataStructuresRutas(cProblem);
            // Bucle para insertar todos los puntos de consumo
            Boolean blnEnBucle = true;
            while (blnEnBucle)
            {
                // Para cada punto de consumo no asignado obtiene el idPuntoConsumo y edge para insertar con menor regret 3
                var valores = cRutas.GetIdPuntoConsumoYEdgeDeMinRegret(cProblem,cParameters , intRegretLevel);
                // Se inserta el valor minimo, en el edge 
                cRutas.AddPuntoConsumoToEdge(cProblem,cParameters , valores.Item1, valores.Item2, true,true);
                // Obtiene el numero que falta por asignar y si no quedan sale
                if (cRutas.CountNoAsignados() == 0)
                    blnEnBucle = false;
            }
            return cRutas;
        }


    }
}
