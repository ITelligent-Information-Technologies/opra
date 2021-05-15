using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace clsVehicleRouting
{
    [Serializable]
    class clsDataStructuresRutasOld2
    {
        //public Stopwatch swTotal;
        //public Stopwatch swMedir;
        public Dictionary<Int32, clsDictionarySorted> dicIdPuntoConsumoSdEdgeCoste = new Dictionary<int, clsDictionarySorted>();
        public Dictionary<Int32, clsDictionarySorted> dicIdPuntoConsumoSdIdVehicleCoste = new Dictionary<int, clsDictionarySorted>();
        public clsDictionarySorted sdCritical = new clsDictionarySorted();// Guarda para cada idPuntoConsumo lo que se reduciria el coste si se quitase
        Dictionary<string, Int32> dicEdgeAIdVehicle = new Dictionary<string, int>();// Edge al vehiculo en el que esta
        public Dictionary<Int32, Int32> dicIdPuntoConsumoAIdVehicle = new Dictionary<int, int>(); // id punto consumo a vehicle
        Dictionary<string, Tuple<Int32, Int32>> dicEdgeAIdOperacionesAnteriorSiguiente = new Dictionary<string, Tuple<int, int>>();
        Dictionary<Int32, Int32> dicIdPuntoConsumoAIdAnterior = new Dictionary<int, int>();
        public Dictionary<Int32, Int32> dicIdPuntoConsumoAIdSiguiente = new Dictionary<int, int>();
        Dictionary<Int32, double> dicIdVehicleCapacidadUsada = new Dictionary<int, double>();
        public HashSet<Int32> hsIdPuntoConsumoNoAsignados = new HashSet<int>(); // Puntos de consumo no asignados
        List<Tuple<Int32, string>> lstUnDoIdPuntoConsumoDestroyEdge = new List<Tuple<int, string>>();// Va guardando los puntoconsumo que van siendo destroy en el orden de remove
        List<Tuple<Int32, string>> lstUnDoIdPuntoConsumoRepairEdge = new List<Tuple<int, string>>();// Va guardando los puntoconsumo que van repair en el orden de insercion        
        // Datos time windows
        Dictionary<Int32, double> dicIdPuntoConsumoTiempoInicio = new Dictionary<int, double>(); // Momento en que se inicia el punto de consumo
        Dictionary<Int32, double> dicIdPuntoConsumoTiempoFin = new Dictionary<int, double>(); // Momento en que se acaba el punto de consumo (inicio+tiempo servicio)
        Dictionary<Int32, double> dicIdPuntoConsumoHolguraTimeWindowsSuperior = new Dictionary<int, double>(); // Para un punto de consumo en una ruta dice cual es el incremento de tiempo maximo admisible si se inserta un punto antes que el
        Dictionary<Int32, double> dicIdVehicleHolguraMin = new Dictionary<int, double>(); // Para un vehiculo la holgura minima y maxima
        Dictionary<Int32, double> dicIdVehicleHolguraMax = new Dictionary<int, double>(); // Para un vehiculo la holgura minima y maxima

        public double dblDistanciaTotal = 0; // Distancia total recorrida
        public Int32 intPuntosNoAsignados = 0; // Numero de puntos que no han podido ser asignados
        public double dblCosteTotal = 0; // COste total de la solucion

        public clsDataStructuresRutasOld2(clsDataProblem cProblem)
        {
            // Introduce un punto de consumo por cada vehiculo con signo negativo y conectado con si mismo
            for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
            {
                string strEdge = -intV + "_" + -intV;
                dicIdPuntoConsumoAIdAnterior.Add(-intV, -intV);
                dicIdPuntoConsumoAIdSiguiente.Add(-intV, -intV);
                dicEdgeAIdOperacionesAnteriorSiguiente.Add(strEdge, new Tuple<int, int>(-intV, -intV));
                dicEdgeAIdVehicle.Add(strEdge, intV);
                dicIdPuntoConsumoAIdVehicle.Add(-intV, intV);
                dicIdVehicleCapacidadUsada.Add(intV, 0);
                dicIdPuntoConsumoTiempoInicio.Add(-intV, 0);
                dicIdPuntoConsumoTiempoFin.Add(-intV, 0);
                dicIdVehicleHolguraMax.Add(intV, double.MaxValue);
                dicIdVehicleHolguraMin.Add(intV, 0);
            }
            // Calcula los costes para elto de los res puntos de asignarlo a alguno de los puntos anteriores
            for (Int32 intIdPuntoConsumoOut = 1; intIdPuntoConsumoOut <= cProblem.intPuntosConsumoNumber; intIdPuntoConsumoOut++)
            {
                hsIdPuntoConsumoNoAsignados.Add(intIdPuntoConsumoOut);
                //Calcula para cada ruta/vehiculo
                for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
                {
                    string strEdge = -intV + "_" + -intV;
                    //Calcula el coste de añadir 
                    string strEdgeNew = -intV + "_" + intIdPuntoConsumoOut;
                    double dblCoste = 2 * cProblem.dicIdPuntoAIdPuntoBATiempo[strEdgeNew] + cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumoOut].dblTiempoServicio;
                    if (!dicIdPuntoConsumoSdEdgeCoste.ContainsKey(intIdPuntoConsumoOut))
                        dicIdPuntoConsumoSdEdgeCoste.Add(intIdPuntoConsumoOut, new clsDictionarySorted());
                    dblCoste = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].Add(strEdge, dblCoste);
                    //Añade el minimo coste para el vehiculo
                    if (!dicIdPuntoConsumoSdIdVehicleCoste.ContainsKey(intIdPuntoConsumoOut))
                        dicIdPuntoConsumoSdIdVehicleCoste.Add(intIdPuntoConsumoOut, new clsDictionarySorted());
                    dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Add(intV.ToString(), dblCoste);
                }
            }
        }

        public void Deshacer(clsDataProblem cProblem, clsParameters cParameters)
        {
            // Guarda los vehiculos de las rutas para despues recacular timewindows y holguras
            HashSet<Int32> hsIdVehicle = new HashSet<int>();
            // Deshace los puntos añadidos anteriormente
            for (Int32 intIndex = lstUnDoIdPuntoConsumoRepairEdge.Count - 1; intIndex >= 0; intIndex--)
            {
                Int32 intIdPuntoConsumo = lstUnDoIdPuntoConsumoRepairEdge[intIndex].Item1;

                string strEdge = lstUnDoIdPuntoConsumoRepairEdge[intIndex].Item2;
                if (!hsIdPuntoConsumoNoAsignados.Contains(intIdPuntoConsumo))
                {
                    Int32 intIdVehicle = dicIdPuntoConsumoAIdVehicle[intIdPuntoConsumo];
                    RemovePuntoConsumo(cProblem, cParameters, intIdPuntoConsumo, false);
                    if (!hsIdVehicle.Contains(intIdVehicle))
                        hsIdVehicle.Add(intIdVehicle);
                }
                else
                    new Exception("Erro punto no asignado y en undo");
            }
            for (Int32 intIndex = lstUnDoIdPuntoConsumoDestroyEdge.Count - 1; intIndex >= 0; intIndex--)
            {
                Int32 intIdPuntoConsumo = lstUnDoIdPuntoConsumoDestroyEdge[intIndex].Item1;
                string strEdge = lstUnDoIdPuntoConsumoDestroyEdge[intIndex].Item2;
                Int32 intIdVehicle = dicEdgeAIdVehicle[strEdge];
                // Añade pero no guarda deshacer ni calcula time windows ni comprueba errores de timewindows
                AddPuntoConsumoToEdge(cProblem, cParameters, intIdPuntoConsumo, strEdge, false, false);
                if (!hsIdVehicle.Contains(intIdVehicle))
                    hsIdVehicle.Add(intIdVehicle);
            }
            // Actualiza los vehiculos en cuanto a timewindows y holguras
            foreach (Int32 intIdVehicle in hsIdVehicle)
                CalcularTimeWindowsVehicle(cProblem, intIdVehicle);
        }

        /// <summary>
        /// Añade un punto de consumo a un edge calculando todos los datos
        /// </summary>
        /// <param name="cProblem"></param>
        /// <param name="cParameters"></param>
        /// <param name="intIdPuntoConsumo"></param>
        /// <param name="strEdge"></param>
        /// <param name="blnGuardarDeshacer"></param>
        public void AddPuntoConsumoToEdge(clsDataProblem cProblem, clsParameters cParameters, Int32 intIdPuntoConsumo, string strEdge, Boolean blnGuardarDeshacer, Boolean blnCalcularTimeWindowsYHolguras)
        {

            Int32 intIdPuntoConsumoAnterior = dicEdgeAIdOperacionesAnteriorSiguiente[strEdge].Item1;
            Int32 intIdPuntoConsumoSiguiente = dicEdgeAIdOperacionesAnteriorSiguiente[strEdge].Item2;
            Int32 intIdVehicle = dicEdgeAIdVehicle[strEdge];
            // Añade el punto de consumo al vehiculo
            if (dicIdPuntoConsumoAIdVehicle.ContainsKey(intIdPuntoConsumo))
                dicIdPuntoConsumoAIdVehicle[intIdPuntoConsumo] = intIdVehicle;
            else
                dicIdPuntoConsumoAIdVehicle.Add(intIdPuntoConsumo, intIdVehicle);
            // Guarda datos para deshacer
            if (blnGuardarDeshacer)
                lstUnDoIdPuntoConsumoRepairEdge.Add(new Tuple<int, string>(intIdPuntoConsumo, strEdge));
            // Añade la demanda del punto de consumo al vehiculo
            double dblDemand = cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblDemand;
            if (!dicIdVehicleCapacidadUsada.ContainsKey(intIdVehicle))
                dicIdVehicleCapacidadUsada.Add(intIdVehicle, 0);
            dicIdVehicleCapacidadUsada[intIdVehicle] = dicIdVehicleCapacidadUsada[intIdVehicle] + dblDemand;
            // Comprueba si se ha superado la cantidad maxima y da error si pasa
            if (dicIdVehicleCapacidadUsada[intIdVehicle] > cProblem.dicIdVehicleCapacity[intIdVehicle])
                new Exception("Capacidad superada");
            // Lo quita de no asignados
            hsIdPuntoConsumoNoAsignados.Remove(intIdPuntoConsumo);
            // Arregla anterior
            dicIdPuntoConsumoAIdAnterior[intIdPuntoConsumoSiguiente] = intIdPuntoConsumo;
            dicIdPuntoConsumoAIdAnterior[intIdPuntoConsumo] = intIdPuntoConsumoAnterior;
            // Arregla siguiente
            dicIdPuntoConsumoAIdSiguiente[intIdPuntoConsumoAnterior] = intIdPuntoConsumo;
            dicIdPuntoConsumoAIdSiguiente[intIdPuntoConsumo] = intIdPuntoConsumoSiguiente;
            // Quita el Edge actual
            dicEdgeAIdOperacionesAnteriorSiguiente.Remove(strEdge);
            dicEdgeAIdVehicle.Remove(strEdge);
            // Introduce los dos nuevos edges
            dicEdgeAIdOperacionesAnteriorSiguiente.Add(intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo, new Tuple<int, int>(intIdPuntoConsumoAnterior, intIdPuntoConsumo));
            dicEdgeAIdVehicle.Add(intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo, intIdVehicle);
            dicEdgeAIdOperacionesAnteriorSiguiente.Add(intIdPuntoConsumo + "_" + intIdPuntoConsumoSiguiente, new Tuple<int, int>(intIdPuntoConsumo, intIdPuntoConsumoSiguiente));
            dicEdgeAIdVehicle.Add(intIdPuntoConsumo + "_" + intIdPuntoConsumoSiguiente, intIdVehicle);
            // Actualiza el coste al introducir el nuevo punto de consumo
            double dblCosto = cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo] +
                cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumo + "_" + intIdPuntoConsumoSiguiente] -
                cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoSiguiente] +
                cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblTiempoServicio;// Se añade el tiempo de servicio
            dblDistanciaTotal = dblDistanciaTotal + dblCosto;
            // Actualiza cDSIdPuntoConsumoValorCosteQuitar para despues el critical
            RecalculateCritical(cProblem, intIdPuntoConsumo);
            RecalculateCritical(cProblem, intIdPuntoConsumoAnterior);
            RecalculateCritical(cProblem, intIdPuntoConsumoSiguiente);
            // Si es necesario recalcula el time windows superior
            if (cParameters.blnConsiderarTimeWindowSuperior && blnCalcularTimeWindowsYHolguras)
            {
                var valores = CalcularTimeWindowsSuperiorAddPuntoConsumo(cProblem, intIdPuntoConsumo, dblCosto);
                if (!dicIdVehicleHolguraMin.ContainsKey(intIdVehicle))
                {
                    dicIdVehicleHolguraMin.Add(intIdVehicle, valores.Item1);
                    dicIdVehicleHolguraMax.Add(intIdVehicle, valores.Item2);
                }
                else
                {
                    dicIdVehicleHolguraMin[intIdVehicle] = valores.Item1;
                    dicIdVehicleHolguraMax[intIdVehicle] = valores.Item2;
                }
            }
            // Va actualizando cada operacion 

            for (Int32 intIdPuntoConsumoOut = 1; intIdPuntoConsumoOut <= cProblem.intPuntosConsumoNumber; intIdPuntoConsumoOut++)
            {
                // Mira las distintas opciones que se pueden dar
                if (intIdPuntoConsumoOut == intIdPuntoConsumoAnterior)
                {
                    // El Out es el anterior del Edge
                    // En este caso realiza la conexio al nuevo edge
                    ObtenerCosteInsercionParaPuntoConsumoYEdge(cProblem, intIdPuntoConsumoOut, intIdPuntoConsumo + "_" + intIdPuntoConsumoSiguiente);
                }
                else if (intIdPuntoConsumoOut == intIdPuntoConsumoSiguiente)
                {
                    // Si el Out es el siguiente en Edge
                    // En este caso realiza la conexio al nuevo edge
                    ObtenerCosteInsercionParaPuntoConsumoYEdge(cProblem, intIdPuntoConsumoOut, intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo);
                }
                else if (intIdPuntoConsumoOut == intIdPuntoConsumo)
                {
                    // Si el Out es el Punto Consumo a insertar
                    // Quita el punto de consumo al edge y comprueba si hay que actulizar el minimo en el vehiculo
                    double dblCosteVehicleTmp = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].GetValue(intIdVehicle.ToString());
                    double dblCosteEdgeTmp = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].GetValue(strEdge);
                    dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].Remove(strEdge);
                    // Comprueba si el menos coste para el vehiculo es el dde strEdge
                    if (dblCosteEdgeTmp <= dblCosteVehicleTmp)
                    {
                        // Elimina el coste minimo 
                        dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Remove(intIdVehicle.ToString());
                        // Intenta recalcular el coste minimo
                        ObtenerMenorCosteParaPuntoConsumoYVehiculo(intIdPuntoConsumoOut, intIdVehicle);
                    }
                }
                else
                {
                    // Caso normal el Out no esta en Edge ni es el Punto de Consumo a insertar
                    ObtenerCosteInsercionParaPuntoConsumoYEdge(cProblem, intIdPuntoConsumoOut, intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo);
                    // En este caso realiza la conexio al nuevo edge
                    ObtenerCosteInsercionParaPuntoConsumoYEdge(cProblem, intIdPuntoConsumoOut, intIdPuntoConsumo + "_" + intIdPuntoConsumoSiguiente);
                    // Quita el punto de consumo al edge y comprueba si hay que actulizar el minimo en el vehiculo
                    double dblCosteVehicleTmp = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].GetValue(intIdVehicle.ToString());
                    double dblCosteEdgeTmp = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].GetValue(strEdge);
                    dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].Remove(strEdge);
                    // Comprueba si el menos coste para el vehiculo es el dde strEdge
                    if (dblCosteEdgeTmp <= dblCosteVehicleTmp)
                    {
                        // Elimina el coste minimo 
                        dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Remove(intIdVehicle.ToString());
                        // Intenta recalcular el coste minimo
                        ObtenerMenorCosteParaPuntoConsumoYVehiculo(intIdPuntoConsumoOut, intIdVehicle);
                    }
                }
            }

        }

        public void RemovePuntoConsumo(clsDataProblem cProblem, clsParameters cParameters, Int32 intIdPuntoConsumo, Boolean blnGuardarDeshacer)
        {
            Int32 intIdPuntoConsumoAnterior = dicIdPuntoConsumoAIdAnterior[intIdPuntoConsumo];
            Int32 intIdPuntoConsumoSiguiente = dicIdPuntoConsumoAIdSiguiente[intIdPuntoConsumo];
            string strEdge1 = intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo;
            string strEdge2 = intIdPuntoConsumo + "_" + intIdPuntoConsumoSiguiente;
            string strEdgeNew = intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoSiguiente;
            Int32 intIdVehicle = dicEdgeAIdVehicle[strEdge1];
            dicIdPuntoConsumoAIdVehicle.Remove(intIdPuntoConsumo);
            //Lo añade a la lista para UnDo (deshacer)
            if (blnGuardarDeshacer)
                lstUnDoIdPuntoConsumoDestroyEdge.Add(new Tuple<int, string>(intIdPuntoConsumo, strEdgeNew));
            // Lo añade a no asignado
            hsIdPuntoConsumoNoAsignados.Add(intIdPuntoConsumo);
            // Quita el Edge actual
            dicEdgeAIdOperacionesAnteriorSiguiente.Remove(strEdge1);
            dicEdgeAIdVehicle.Remove(strEdge1);
            dicEdgeAIdOperacionesAnteriorSiguiente.Remove(strEdge2);
            dicEdgeAIdVehicle.Remove(strEdge2);
            // Introduce los dos nuevos edges
            dicEdgeAIdOperacionesAnteriorSiguiente.Add(strEdgeNew, new Tuple<int, int>(intIdPuntoConsumoAnterior, intIdPuntoConsumoSiguiente));
            dicEdgeAIdVehicle.Add(strEdgeNew, intIdVehicle);
            // Pone bien el anterior (siguiente mirando al anterior)
            dicIdPuntoConsumoAIdAnterior.Remove(intIdPuntoConsumo);
            dicIdPuntoConsumoAIdAnterior[intIdPuntoConsumoSiguiente] = intIdPuntoConsumoAnterior;
            // Pone bien el siguiente
            dicIdPuntoConsumoAIdSiguiente.Remove(intIdPuntoConsumo);
            dicIdPuntoConsumoAIdSiguiente[intIdPuntoConsumoAnterior] = intIdPuntoConsumoSiguiente;
            // Añade el coste del punto de consumo removido al nuevo edge
            double dblCoste = cProblem.dicIdPuntoAIdPuntoBATiempo[strEdge1] +
                cProblem.dicIdPuntoAIdPuntoBATiempo[strEdge2] +
                cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblTiempoServicio -
                cProblem.dicIdPuntoAIdPuntoBATiempo[strEdgeNew];
            // Actualiza sdCritical para despues el critical
            // Actualiza cDSIdPuntoConsumoValorCosteQuitar para despues el critical
            sdCritical.Remove(intIdPuntoConsumo.ToString());
            RecalculateCritical(cProblem, intIdPuntoConsumoAnterior);
            RecalculateCritical(cProblem, intIdPuntoConsumoSiguiente);
            // Actualiza el coste total            
            dblDistanciaTotal = dblDistanciaTotal - dblCoste;
            // Actualiza la capacidad del vehiculo
            dicIdVehicleCapacidadUsada[intIdVehicle] = dicIdVehicleCapacidadUsada[intIdVehicle] - cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblDemand;
            // Va actualizando cada operacion            
            for (Int32 intIdPuntoConsumoOut = 1; intIdPuntoConsumoOut <= cProblem.intPuntosConsumoNumber; intIdPuntoConsumoOut++)
            {
                // Mira las distintas opciones que se pueden dar
                if (intIdPuntoConsumoOut == intIdPuntoConsumoAnterior)
                {
                    // El Out es el anterior del Edge
                    // En este caso realiza la conexio al nuevo edge
                    EliminarCosteInsercionParaPuntoConsumoYEdge(cProblem, intIdPuntoConsumoOut, intIdPuntoConsumo, intIdPuntoConsumoSiguiente, intIdVehicle);
                }
                else if (intIdPuntoConsumoOut == intIdPuntoConsumoSiguiente)
                {
                    // Si el Out es el siguiente en Edge
                    // En este caso realiza la conexio al nuevo edge
                    EliminarCosteInsercionParaPuntoConsumoYEdge(cProblem, intIdPuntoConsumoOut, intIdPuntoConsumoAnterior, intIdPuntoConsumo, intIdVehicle);
                }
                else if (intIdPuntoConsumoOut == intIdPuntoConsumo)
                {
                    // Si el Out es el Punto Consumo a insertar
                    //Comprueba el coste minimo para ese idPuntoConsumoOut y vehiculo
                    double dblCosteVehicleTmp = double.MaxValue;
                    if (dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].ExistsKey(intIdVehicle.ToString()))
                        dblCosteVehicleTmp = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].GetValue(intIdVehicle.ToString());
                    // Calcula el coste del nuevo punto
                    double dblCosteInsert = cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoOut] +
                        cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoOut + "_" + intIdPuntoConsumoSiguiente] +
                        cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumoOut].dblTiempoServicio -
                        cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoSiguiente];
                    // Inserta el valor del punto Out en el nuevo edge
                    dblCosteInsert = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].Add(intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoSiguiente, dblCosteInsert);
                    // Comprueba si el nuevo coste es el menor para el vehiculo en cuestion
                    if (dblCosteInsert <= dblCosteVehicleTmp)
                    {
                        if (dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].ExistsKey(intIdVehicle.ToString()))
                            dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Update(intIdVehicle.ToString(), dblCosteInsert);
                        else
                            dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Add(intIdVehicle.ToString(), dblCosteInsert);
                    }
                }
                else // Es el caso normal el Out esta es diferente del edge y del eliminado
                {
                    // Calcula el coste del punto Out con el nuevo Edge
                    double dblCosteInsert = cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoOut] +
                        cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoOut + "_" + intIdPuntoConsumoSiguiente] +
                        cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumoOut].dblTiempoServicio -
                        cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoSiguiente];
                    // Inserta el valor del punto Out en el nuevo edge
                    dblCosteInsert = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].Add(intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoSiguiente, dblCosteInsert);

                    // Obtiene el coste de Out con los dos edges que se eliminan
                    double dblCosteAnterior = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].GetValue(intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo);
                    double dblCosteSiguiente = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].GetValue(intIdPuntoConsumo + "_" + intIdPuntoConsumoSiguiente);
                    // Elimina los edges
                    dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].Remove(intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo);
                    dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].Remove(intIdPuntoConsumo + "_" + intIdPuntoConsumoSiguiente);
                    // Obtiene el coste actual minimo del vehiculo (que tiene que existir)
                    double dblCosteVehicleMin = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].GetValue(intIdVehicle.ToString());
                    // Si el coste de insercion es menor que el del vehiculo lo sustituye
                    if (dblCosteVehicleMin >= dblCosteInsert)
                        dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Update(intIdVehicle.ToString(), dblCosteInsert);
                    else if (dblCosteVehicleMin >= dblCosteAnterior || dblCosteVehicleMin >= dblCosteSiguiente)
                    {
                        // En este caso alguno de los dos eliminados es el menor, po rlo que habrá que eliminarlo y recalcular el nuevo
                        // Elimina el coste minimo 
                        dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Remove(intIdVehicle.ToString());
                        // Intenta recalcular el coste minimo
                        ObtenerMenorCosteParaPuntoConsumoYVehiculo(intIdPuntoConsumoOut, intIdVehicle);
                    }
                }
            }

        }

        private (double, double) CalcularTimeWindowsSuperiorAddPuntoConsumo(clsDataProblem cProblem, Int32 intIdPuntoConsumo, double dblCoste)
        {
            double dblHolguraMin = double.MaxValue;
            double dblHolguraMax = -1;
            // Obtiene el inicio del punto de consumo introducido
            Int32 intIdPuntoConsumoAnterior = dicIdPuntoConsumoAIdAnterior[intIdPuntoConsumo];
            // Calcula el principio y fin del nuevo punto de consumo
            dicIdPuntoConsumoTiempoInicio[intIdPuntoConsumo] = dicIdPuntoConsumoTiempoFin[intIdPuntoConsumoAnterior] + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo];
            dicIdPuntoConsumoTiempoFin[intIdPuntoConsumo] = dicIdPuntoConsumoTiempoInicio[intIdPuntoConsumo] + cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblTiempoServicio;
            // Calcula la holgura inicial
            dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo] = cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblTimeWindowsMax - dicIdPuntoConsumoTiempoInicio[intIdPuntoConsumo];
            if (dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo] < 0)
                new Exception("Error se ha superado time windows maximo");
            // Calcula la holgura de los siguientes al punto de consumo            
            Int32 intIdPuntoConsumoSiguiente = dicIdPuntoConsumoAIdSiguiente[intIdPuntoConsumo];
            while (intIdPuntoConsumoSiguiente > 0)
            {
                dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoSiguiente] = dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoSiguiente] - dblCoste;
                if (dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoSiguiente] < 0)
                    new Exception("Error se ha superado time windows maximo");
                if (dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoSiguiente] < dblHolguraMin)
                    dblHolguraMin = dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoSiguiente];
                if (dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoSiguiente] > dblHolguraMax)
                    dblHolguraMax = dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoSiguiente];
                dicIdPuntoConsumoTiempoInicio[intIdPuntoConsumoSiguiente] = dicIdPuntoConsumoTiempoInicio[intIdPuntoConsumoSiguiente] + dblCoste;
                dicIdPuntoConsumoTiempoFin[intIdPuntoConsumoSiguiente] = dicIdPuntoConsumoTiempoFin[intIdPuntoConsumoSiguiente] + dblCoste;
                intIdPuntoConsumoSiguiente = dicIdPuntoConsumoAIdSiguiente[intIdPuntoConsumoSiguiente];
            }
            // Calcula la holgura de los anteriores incluido el intIdPuntoConsumo
            double dblMinimo = dblHolguraMin;
            intIdPuntoConsumoAnterior = intIdPuntoConsumo;
            while (intIdPuntoConsumoAnterior > 0)
            {
                dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoAnterior] = Math.Min(dblHolguraMin, dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoAnterior]);
                if (dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoAnterior] < 0)
                    new Exception("Error se ha superado time windows maximo");
                if (dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoAnterior] < dblHolguraMin)
                    dblHolguraMin = dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoAnterior];
                if (dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoAnterior] > dblHolguraMax)
                    dblHolguraMax = dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoAnterior];
                intIdPuntoConsumoAnterior = dicIdPuntoConsumoAIdAnterior[intIdPuntoConsumoAnterior];
            }
            return (dblHolguraMin, dblHolguraMax);
        }



        /// <summary>
        /// Reliza el calculo de la holgura y tiempo inicio y fin
        /// para todos los puntos de consumo de un vehiculo.
        /// IMPORTANTE: Se supone que el primer punto de consumo
        /// es igual al ultimo y tienen inicio cero el fin el igual
        /// </summary>
        /// <param name="cProblem"></param>
        /// <param name="intIdVehicle"></param>
        /// <returns></returns>
        public void CalcularTimeWindowsVehicle(clsDataProblem cProblem, Int32 intIdVehicle)
        {
            double dblHolguraMin = double.MaxValue;
            double dblHolguraMax = -1;
            //-- Pasada hacia delante (parte del primer punto de consumo y va calculando tiempo inicio y fin de cada punto)
            // Toma los datos iniciales
            Int32 intIdPuntoConsumoInicio = cProblem.dicIdVehicleIdPuntoConsumoInicio[intIdVehicle];
            Int32 intIdPuntoConsumoFin = cProblem.dicIdVehicleIdPuntoConsumoFin[intIdVehicle];
            dicIdPuntoConsumoTiempoInicio[intIdPuntoConsumoInicio] = 0;
            dicIdPuntoConsumoTiempoFin[intIdPuntoConsumoInicio] = 0;
            dicIdVehicleHolguraMax[intIdVehicle] = double.MaxValue;
            dicIdVehicleHolguraMin[intIdVehicle] = 0;
            // Si no hay nada en la ruta
            Int32 intIdPuntoConsumo = dicIdPuntoConsumoAIdSiguiente[intIdPuntoConsumoInicio];
            if (intIdPuntoConsumo == intIdPuntoConsumoFin)
                return;
            // Calcula la holgura de los siguientes al punto de consumo            
            Boolean blnEnBucle = true;
            while (blnEnBucle)
            {
                Int32 intIdPuntoConsumoAnterior = dicIdPuntoConsumoAIdAnterior[intIdPuntoConsumo];
                // Obtiene el inicio y el fin
                dicIdPuntoConsumoTiempoInicio[intIdPuntoConsumo] = dicIdPuntoConsumoTiempoFin[intIdPuntoConsumoAnterior] + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo]; ;
                dicIdPuntoConsumoTiempoFin[intIdPuntoConsumo] = dicIdPuntoConsumoTiempoInicio[intIdPuntoConsumo] + cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblTiempoServicio;
                dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo] = cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblTimeWindowsMax - dicIdPuntoConsumoTiempoInicio[intIdPuntoConsumo];
                if (dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo] < 0)
                    new Exception("Error se ha superado time windows maximo");
                // Calcula las holguras
                if (dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo] < dblHolguraMin)
                    dblHolguraMin = dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo];
                if (dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo] > dblHolguraMax)
                    dblHolguraMax = dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo];
                // Toma el siguiente
                intIdPuntoConsumo = dicIdPuntoConsumoAIdSiguiente[intIdPuntoConsumo];
                // Si es el ultimo punto de consumo sale.
                if (intIdPuntoConsumo == intIdPuntoConsumoFin)
                    blnEnBucle = false;
            }
            // Guarda la holgura maxima y minima
            dicIdVehicleHolguraMin[intIdVehicle] = dblHolguraMin;
            dicIdVehicleHolguraMax[intIdVehicle] = dblHolguraMax;
            //-- Pasada hacia detras
            blnEnBucle = true;
            intIdPuntoConsumo = dicIdPuntoConsumoAIdAnterior[intIdPuntoConsumoFin];
            dblHolguraMin = double.MaxValue;
            while (blnEnBucle)
            {
                dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo] = Math.Min(dblHolguraMin, dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo]);
                // Calcula las holguras
                if (dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo] < dblHolguraMin)
                    dblHolguraMin = dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumo];
                // Obtiene el anterior
                intIdPuntoConsumo = dicIdPuntoConsumoAIdAnterior[intIdPuntoConsumo];
                if (intIdPuntoConsumo == intIdPuntoConsumoInicio)
                    blnEnBucle = false;
            }
        }


        public (Int32, string) GetIdPuntoConsumoYEdgeDeMinRegret(clsDataProblem cProblem, clsParameters cParameters, Int32 intRegretLevel)
        {
            double dblCosteMin = double.MaxValue;
            double dblCoste1Min = 0;
            double dblCosteMax = -1;
            Int32 intIdPuntoConsumoMax = 0;
            Int32 intIdPuntoConsumoMin = 0;
            foreach (Int32 intIdPuntoConsumoNoAsignado in hsIdPuntoConsumoNoAsignados)
            {
                var valor = ObtenerIndicePrimerVehiculoConCapacidad(cProblem, intIdPuntoConsumoNoAsignado);
                Int32 intIndice1 = valor.Item1;
                Int32 intIndice1Cuenta = valor.Item2; // Numero de rutas totales para este vehiculo
                if (intIndice1 < 0)
                    new Exception("Error no hay  ruta con capacidad");

                double dblCoste = 0;
                double dblCoste1 = 0;
                if (intRegretLevel == 1)
                {
                    dblCoste = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoNoAsignado].GetMinFirstValue(intIndice1);
                    dblCoste1 = dblCoste;
                }
                else if (intRegretLevel == 2)
                {
                    dblCoste1 = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoNoAsignado].GetMinFirstValue(intIndice1);
                    double dblCoste2 = 0;
                    if ((intIndice1 + 1) < intIndice1Cuenta)
                    {
                        dblCoste2 = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoNoAsignado].GetMinFirstValue(intIndice1 + 1);
                        dblCoste = dblCoste2 - dblCoste1;
                    }
                    else // Si no hay mas rutas deja regret1
                        dblCoste = dblCoste1;
                }
                else if (intRegretLevel == 3)
                {
                    dblCoste1 = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoNoAsignado].GetMinFirstValue(intIndice1);
                    double dblCoste2 = 0;
                    if ((intIndice1 + 1) < intIndice1Cuenta)
                    {
                        dblCoste2 = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoNoAsignado].GetMinFirstValue(intIndice1 + 1);
                        dblCoste = dblCoste2 - dblCoste1;
                    }
                    else // Si no hay mas rutas deja regret1
                        dblCoste = dblCoste1;

                    double dblCoste3 = 0;
                    if ((intIndice1 + 2) < intIndice1Cuenta)
                    {
                        dblCoste3 = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoNoAsignado].GetMinFirstValue(intIndice1 + 2);
                        dblCoste = dblCoste + dblCoste3 - dblCoste1;
                    }
                    else // Si no hay mas rutas deja regret1
                        dblCoste = dblCoste + dblCoste1;

                }
                else
                    new Exception("Regret level incorrecto");
                // Calcula los valores
                if (dblCoste < dblCosteMin && intRegretLevel == 1)
                {
                    dblCosteMin = dblCoste;
                    intIdPuntoConsumoMin = intIdPuntoConsumoNoAsignado;
                    dblCoste1Min = dblCoste1;
                }
                if (dblCoste > dblCosteMax && intRegretLevel > 1)
                {
                    dblCosteMax = dblCoste;
                    intIdPuntoConsumoMax = intIdPuntoConsumoNoAsignado;
                    dblCoste1Min = dblCoste1;
                }
            }
            // Obtiene el edge al que pertenece            
            if (intRegretLevel == 1)
            {
                string strEdgeMin = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoMin].GetKey(dblCoste1Min);
                return (intIdPuntoConsumoMin, strEdgeMin);
            }
            string strEdgeMax = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoMax].GetKey(dblCoste1Min);
            return (intIdPuntoConsumoMax, strEdgeMax);
        }

        /// <summary>
        /// Esta clase se llama si existen restricciones (ej. ventanas de tiempo) que requieran
        /// comprobar si estas restricciones se cumplen antes de seleccionar un punto de consumo
        /// a eliminar
        /// </summary>
        /// <param name="cProblem"></param>
        /// <param name="cParameters"></param>
        /// <param name="intRegretLevel"></param>
        /// <returns></returns>
        public (Int32, string) GetIdPuntoConsumoYEdgeDeMinRegretConRestricciones(clsDataProblem cProblem, clsParameters cParameters, Int32 intRegretLevel)
        {
            Boolean blnEncontrado = false;
            double dblMinCoste = double.MaxValue;
            Int32 intMinIdPuntoConsumo = -1;
            double dblCoste1Min = 0;
            double dblMaxCoste = -1;
            string strEdgeMax = "";
            string strEdgeMin = "";
            Int32 intMaxIdPuntoConsumo = 0;
            // Recorre uno a uno los puntos de consumos no asignados
            foreach (Int32 intIdPuntoConsumoNoAsignado in hsIdPuntoConsumoNoAsignados)
            {
                // Obtenemos para un punto de consumo sus regrets                
                List<Tuple<Int32, double, string>> lstRegret = ObtenerCostesValidosParaPuntoConsumo(cProblem, cParameters, intIdPuntoConsumoNoAsignado, intRegretLevel);
                // Solo se estudia si hay regrets si no hay quiere decir que ese punto no se puede meter en ninguna ruta
                if (lstRegret.Count > 0)
                {
                    // Calculamos los costes
                    double dblCoste = -1;
                    double dblCoste1 = -1;
                    double dblCoste2 = -1;
                    double dblCoste3 = -1;
                    // Calcula el regret
                    if (lstRegret.Count > 0)
                        dblCoste1 = lstRegret[0].Item2;
                    if (lstRegret.Count > 1)
                        dblCoste2 = lstRegret[1].Item2;
                    if (lstRegret.Count > 2)
                        dblCoste3 = lstRegret[2].Item2;
                    // Calculamos el regret
                    if (intRegretLevel == 1)
                        dblCoste = dblCoste1;
                    else if (intRegretLevel == 2)
                    {
                        if (dblCoste2 > -1)
                            dblCoste = dblCoste2 - dblCoste1;
                        else // Si no hay mas rutas deja regret1
                            dblCoste = dblCoste1;
                    }
                    else if (intRegretLevel == 3)
                    {
                        // Hay coste 2 pero no coste 3
                        if (dblCoste2 > -1 && dblCoste3 < 0)
                            dblCoste = dblCoste2 - dblCoste1;
                        else if (dblCoste2 > -1 && dblCoste3 > -1) // Hay coste 2 y 3
                            dblCoste = dblCoste3 - dblCoste1 + dblCoste2 - dblCoste1;
                        else // solo hay coste 1
                            dblCoste = dblCoste1;
                    }
                    else
                        new Exception("Regret level incorrecto");
                    // Calcula los valores
                    blnEncontrado = true;
                    if (dblCoste < dblMinCoste && intRegretLevel == 1)
                    {
                        dblMinCoste = dblCoste;
                        intMinIdPuntoConsumo = intIdPuntoConsumoNoAsignado;
                        dblCoste1Min = dblCoste1;
                        strEdgeMin = lstRegret[0].Item3;
                    }
                    if (dblCoste > dblMaxCoste && intRegretLevel > 1)
                    {
                        dblMaxCoste = dblCoste;
                        intMaxIdPuntoConsumo = intIdPuntoConsumoNoAsignado;
                        dblCoste1Min = dblCoste1;
                        strEdgeMax = lstRegret[0].Item3;
                    }
                }
            }
            // No se ha encontrado nada
            if (!blnEncontrado)
                return (-1, "");
            // Obtiene el edge al que pertenece            
            if (intRegretLevel == 1)
                return (intMinIdPuntoConsumo, strEdgeMin);
            return (intMaxIdPuntoConsumo, strEdgeMax);
        }

        public Int32 CountNoAsignados()
        {
            return hsIdPuntoConsumoNoAsignados.Count;
        }

        /// <summary>
        /// Esta funcion es llamada para inicializar el rehacer 
        /// y pone a cero las estructuras para guardar los
        /// destroy y repain
        /// </summary>
        public void DeshacerInicializar()
        {
            lstUnDoIdPuntoConsumoDestroyEdge = new List<Tuple<int, string>>();
            lstUnDoIdPuntoConsumoRepairEdge = new List<Tuple<int, string>>();
        }

        private (Int32, Int32) ObtenerIndicePrimerVehiculoConCapacidad(clsDataProblem cProblem, Int32 intIdPuntoConsumo)
        {
            Int32 intIndiceEnSdIdVehicleCoste = -1;
            Boolean blnEnBucle = true;
            Int32 intCuenta = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumo].Count();
            for (Int32 intI = 0; intI < intCuenta && blnEnBucle; intI++)
            {
                Int32 intIdVehicleMin = Convert.ToInt32(dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumo].GetMinFirstKey(intI));
                intIndiceEnSdIdVehicleCoste++;
                // Chequea si hay capacidad en este vehiculo
                if ((cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblDemand + dicIdVehicleCapacidadUsada[intIdVehicleMin]) <= cProblem.dicIdVehicleCapacity[intIdVehicleMin])
                {
                    // Chequea si hay holgura en este vehiculo para meter el punto de consumo
                    //if(dicIdPuntoConsumoSdIdVehicleCoste [intIdPuntoConsumo ].GetValue (intIdVehicleMin .ToString ()) <=dic))
                    blnEnBucle = false;
                }

            }
            if (blnEnBucle)
                return (-1, intCuenta);
            else
                return (intIndiceEnSdIdVehicleCoste, intCuenta);
        }


        /// <summary>
        /// Esta clase recibe un punto de consumo y comprueba que edges cumplen con las restricciones:
        /// -Capacidad del vehiculo
        /// -Vehiculo valido para ese punto de consumo
        /// -Al insertarlo en un vehiculo todos los puntos del vehiculo deben cumplir con el time windows maximo
        /// Esta funcion devuelve los vehiculos donde se puede insertar y el coste minimo de insercion en cada uno
        /// Ojo! si no hay ninguno devuelve cero
        /// </summary>
        /// <param name="cProblem"></param>
        /// <param name="clsParameters"></param>
        /// <param name="intIdPuntoConsumo"></param>
        /// <returns></returns>
        private List<Tuple<Int32, double, string>> ObtenerCostesValidosParaPuntoConsumo(clsDataProblem cProblem, clsParameters clsParameters, Int32 intIdPuntoConsumo, Int32 intRegretLevel)
        {
            List<Tuple<Int32, double, string>> lstResultadosMinimos = new List<Tuple<int, double, string>>();// intIdVehicle, dblCoste, strEdge
            Boolean blnEnBucle = true;
            HashSet<Int32> hsVehicleProcesado = new HashSet<int>();
            Int32 intCuenta = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumo].Count();
            for (Int32 intI = 0; intI < intCuenta && blnEnBucle; intI++)
            {
                string strEdge = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumo].GetMinFirstKey(intI);
                Int32 intIdVehicleEdge = dicEdgeAIdVehicle[strEdge];
                double dblCosteInsertar = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumo].GetMinFirstValue(intI);
                // Chequea si que el coste a insertar es menor que la holgura maxima
                if (dicIdVehicleHolguraMax[intIdVehicleEdge] >= dblCosteInsertar)
                {
                    // Comprueba si el vehiculo es apto para el puntoconsumo
                    if (!hsVehicleProcesado.Contains(intIdVehicleEdge) && cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].blnVehiculosAptos[intIdVehicleEdge])
                    {
                        // Comprueba si cumple por capacidad
                        if ((cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblDemand + dicIdVehicleCapacidadUsada[intIdVehicleEdge]) <= cProblem.dicIdVehicleCapacity[intIdVehicleEdge])
                        {
                            // Comprueba si al insertar puntoconsumo en edge puntoconsumo cumple con su timewindows maximo
                            Int32 intIdPuntoConsumoAnterior = dicEdgeAIdOperacionesAnteriorSiguiente[strEdge].Item1;
                            if (cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblTimeWindowsMax >= (dicIdPuntoConsumoTiempoFin[intIdPuntoConsumoAnterior] + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo]))
                            {
                                // Comprueba que al insertar no provoque que alguo de los posteriores deje de cumplir con el timewindowsmax
                                Int32 intIdPuntoConsumoSiguiente = dicEdgeAIdOperacionesAnteriorSiguiente[strEdge].Item2;
                                if (intIdPuntoConsumoSiguiente < 0 || dicIdPuntoConsumoHolguraTimeWindowsSuperior[intIdPuntoConsumoSiguiente] >= dblCosteInsertar)
                                {
                                    hsVehicleProcesado.Add(intIdVehicleEdge);
                                    // es posible insertar
                                    lstResultadosMinimos.Add(new Tuple<Int32, double, string>(intIdVehicleEdge, dblCosteInsertar, strEdge));
                                    if (lstResultadosMinimos.Count == intRegretLevel)
                                        blnEnBucle = false;
                                }
                            }

                        }
                    }
                }
            }
            return lstResultadosMinimos;
        }


        /// <summary>
        /// Recalcula el valor de critical para el punto pasado
        /// esto es calcula el coste que tendria quitar el punto
        /// pasado y lo mete en la estructura sdCritical
        /// </summary>
        /// <param name="cProblem"></param>
        /// <param name="intIdPuntoConsumo"></param>
        private void RecalculateCritical(clsDataProblem cProblem, Int32 intIdPuntoConsumo)
        {
            if (intIdPuntoConsumo < 0)
                return;
            Int32 intIdPuntoConsumoAnt = dicIdPuntoConsumoAIdAnterior[intIdPuntoConsumo];
            Int32 intIdPuntoConsumoSig = dicIdPuntoConsumoAIdSiguiente[intIdPuntoConsumo];
            // Calcula el critical cost del idpunto consumo
            double dblCoste = cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnt + "_" + intIdPuntoConsumoSig] -
                    cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnt + "_" + intIdPuntoConsumo] -
                    cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumo + "_" + intIdPuntoConsumoSig] -
                    cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblTiempoServicio; // se añade tambien el tiempo de servicio
            if (sdCritical.ExistsKey(intIdPuntoConsumo.ToString()))
                sdCritical.Update(intIdPuntoConsumo.ToString(), dblCoste);
            else
                sdCritical.Add(intIdPuntoConsumo.ToString(), dblCoste);
        }



        private void ObtenerMenorCosteParaPuntoConsumoYVehiculo(Int32 intIdPuntoConsumoOut, Int32 intIdVehicle)
        {
            // Este es el peor caso pues hay que revisar todos los de este coche para obtener el nuevo minimo ya  que el anterior minimo era el edge quitado
            Boolean blnEnBucle = true;
            Int32 intIteracionesCuenta = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].Count();
            for (Int32 intIndex = 0; (intIndex < intIteracionesCuenta && blnEnBucle); intIndex++)
            {
                // Si es el mismo vehiculo
                string strEdgeMinCost = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].GetMinFirstKey(intIndex);
                if (intIdVehicle == dicEdgeAIdVehicle[strEdgeMinCost])
                {
                    blnEnBucle = false;
                    double dblCoste = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].GetValue(strEdgeMinCost);
                    if (!dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].ExistsKey(intIdVehicle.ToString()))
                        dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Add(intIdVehicle.ToString(), dblCoste);
                    else
                        dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Update(intIdVehicle.ToString(), dblCoste);
                }
            }
            // Si no ha encontrado nada quita el vehiculo
            if (blnEnBucle)
            {
                if (dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].ExistsKey(intIdVehicle.ToString()))
                    dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Remove(intIdVehicle.ToString());
            }
        }

        /// <summary>
        /// Determina para un punto de consumo cuanto costaria insertarlo en un edge
        /// esto es el coste adicional de insercion
        /// </summary>
        /// <param name="cProblem"></param>
        /// <param name="intIdPuntoConsumoOut"></param>
        /// <param name="strEdge"></param>
        private void ObtenerCosteInsercionParaPuntoConsumoYEdge(clsDataProblem cProblem, Int32 intIdPuntoConsumoOut, string strEdge)
        {
            Int32 intIdPuntoConsumoAnterior = dicEdgeAIdOperacionesAnteriorSiguiente[strEdge].Item1;
            Int32 intIdPuntoConsumoSiguiente = dicEdgeAIdOperacionesAnteriorSiguiente[strEdge].Item2;
            Int32 intIdVehicle = dicEdgeAIdVehicle[strEdge];
            // Dado un edge y un punto de consumo calcula el coste de insertar ese punto de consumo
            double dblCosteInsertar = cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoOut] +
                cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoOut + "_" + intIdPuntoConsumoSiguiente] -
                cProblem.dicIdPuntoAIdPuntoBATiempo[strEdge] +
                cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumoOut].dblTiempoServicio; //añade el tiempo de servicio
            dblCosteInsertar = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].Add(strEdge, dblCosteInsertar);
            // Si ese coche no tiene coste minimo mete el coste de insertar
            if (!dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].ExistsKey(intIdVehicle.ToString()))
            {
                dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Add(intIdVehicle.ToString(), dblCosteInsertar);
                return;
            }
            // Si el coche ya tiene coste
            double dblCosteVehiculoMinimo = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].GetValue(intIdVehicle.ToString());
            // Si el nuevo coste es menor o igual que el actual lo sustituye y sale
            if (dblCosteVehiculoMinimo >= dblCosteInsertar)
            {
                dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].Update(intIdVehicle.ToString(), dblCosteInsertar);
                return;
            }
        }

        private void EliminarCosteInsercionParaPuntoConsumoYEdge(clsDataProblem cProblem, Int32 intIdPuntoConsumoOut, Int32 intIdPuntoConsumoAnterior, Int32 intIdPuntoConsumoSiguiente, Int32 intIdVehicle)
        {
            // Se calcula el coste de insercion de punto consumo out al edge que se va a quitar
            double dblCosteInsertar = dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].GetValue(intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoSiguiente);
            // Obtiene el coste minimo actual
            double dblCosteVehicleMin = dicIdPuntoConsumoSdIdVehicleCoste[intIdPuntoConsumoOut].GetValue(intIdVehicle.ToString());
            // Borra el edge
            dicIdPuntoConsumoSdEdgeCoste[intIdPuntoConsumoOut].Remove(intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumoSiguiente);
            // Si el valor del coste al edge a eliminar es el minimo se debe recalcular
            if (dblCosteVehicleMin >= dblCosteInsertar)
            {
                // Recalcular
                ObtenerMenorCosteParaPuntoConsumoYVehiculo(intIdPuntoConsumoOut, intIdVehicle);
            }

        }
    }
}
