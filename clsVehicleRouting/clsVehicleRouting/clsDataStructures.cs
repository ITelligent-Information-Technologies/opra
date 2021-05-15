using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsVehicleRouting
{


    class clsDataProblem
    {
        // Vehiculos
        public Int32 intVehiclesNumber; // Numero de vehiculos
        public Dictionary<Int32, double> dicIdVehicleCapacity = new Dictionary<int, double>(); // Capacidad de cada vehiculo (Id=1,...,intVehiclesNumber)
        public Dictionary<Int32, Int32> dicIdVehicleIdPuntoConsumoInicio = new Dictionary<int, int>(); // Indica el primer punto de consumo de una ruta.
        public Dictionary<Int32, Int32> dicIdVehicleIdPuntoConsumoFin = new Dictionary<int, int>(); // Indica el ultimo punto de consumo de una ruta.
        public Dictionary<Int32, double> dicIdVehicleDistanciaRecorrida = new Dictionary<int, double>(); // Distancia total recorrida por el vehiculo
        // Tiempo finalizacion=tiempo en ruta + tiempo en esperas + tiempo servicios
        public Dictionary<Int32, double> dicIdVehicleTiempoFinalizacion = new Dictionary<int, double>(); // Tiempo de finalizacion de la ruta en el vehiculo
        public Dictionary<Int32, double> dicIdVehicleTiempoEsperas = new Dictionary<int, double>(); // Tiempo total de las esperas del vehiculo por llegar antes de time windows
        public Dictionary<Int32, double> dicIdVehicleTiempoServicios = new Dictionary<int, double>(); // Tiempo total de las servicio del vehiculo por llegar antes de time windows
        public Dictionary<Int32, double> dicIdVehicleTiempoEnRuta = new Dictionary<int, double>(); // Tiempo total en ruta.
        // PUntos de consumo
        public Int32 intPuntosConsumoNumber;// Numero de puntos de consumo
        public Dictionary<Int32, clsDataPuntoConsumo> dicIdPuntoConsumoADatosPuntoConsumo = new Dictionary<int, clsDataPuntoConsumo>(); // Datos de cada punto de consumo


        // Matrices tiempo o puede ser tiempo incluyendo depot que se mete como CERO
        public Dictionary<string, double> dicIdPuntoAIdPuntoBATiempo = new Dictionary<string, double>(); // Entrada un string Id1_Id2 (ej. 3_4) y devuelve el tiempo entre esos puntos
        // Matriz de distancia entre dos puntos                                                                         
        public Dictionary<string, double> dicIdPuntoAIdPuntoBADistancia = new Dictionary<string, double>(); // Entrada un string Id1_Id2 (ej. 3_4) y devuelve el tiempo entre esos puntos

        // Listados ordenados de menor a mayor distancia para cada punto de consumo
        public Dictionary<Int32, List<Int32>> dicIdPuntoConsumoALstPuntoCosumosMenorDistancia = new Dictionary<int, List<int>>(); // Para cada punto de consumo da una lista de los otros puntos de menor a mayor distancia


    }

    class clsParameters
    {
        public double dblPorcentajeNumeroPuntoConsumoADestruirMin = 0.1; // el numero de puntos de consumo a destruir sera un aleatorio entre esta proporcion y el macio
        public double dblPorcentajeNumeroPuntoConsumoADestruirMax = 0.4; // el numero de puntos de consumo a destruir sera un aleatorio entre esta proporcion y el macio
        public Int32 intRegretLevelSolucionInicial = 3; // Nivel de regret a utiilizar para la solucion inicial
        public Boolean blnRepairRegretLevel3 = true; // Cosiderar regret-3 en repair
        public Boolean blnRepairRegretLevel2 = true; // Cosiderar regret-2 en repair
        public Boolean blnRepairRegretLevel1 = true; // Cosiderar regret-1 en repair
        public Int32 intNumIterations = 10000;
        public Int32 intNumSolucionesBacktrack = 15; // Numero de soluciones maximas para realizar backtrack
        public double dblRandomnessLevel = 5; // Nivel de randomnes para critical distroy mayor que 1 cuanto mas pequeño mas randomness
        public Boolean blnConsiderarTimeWindowSuperior = false; // Si es true considera un tiempo maximo para la entrega recogida
        // Adaptative layer
        public double dblAdaptativeLayerPesoNuevo = 0.4;// Indica el peso para las nuevas scores frente al peso anterior
        public double dblAdaptativeLayerPuntuacionSolucionMejorGlobal = 1; // valor una nueva mejor solucion global
        public double dblAdaptativeLayerPuntuacionSolucionMejorParcial = 0.4; // valor una nueva solucion que mejora la anterior
        public double dblAdaptativeLayerPuntuacionSolucionAceptadaQueNoMejora = 0.25; // valor una nueva solucion que empeora la anterior pero ha sido aceptada
        public double dblAdaptativeLayerPuntuacionSolucionRechazada = 0; // valor una nueva solucion que no mejora y no ha sido aceptada
        public Int32 intAdaptativeLayerNumeroIteracionParaActualizar = 100; // Numero iteracinoes para actualizar
        public Boolean blnParalelizar = true; // Si se quiere paralelizar entre el numero de procesadores disponibles
        // Funcion objetivo
        public double dblObjetivoPesoNoAsignados = 1; // Peso que se le da a los no asignados
        public double dblObjetivoPesoDistancia = 1; // Peso que se le da a la distancia recorrida
        public double dblObjetivoPesosTiempo = 1; // Peso que se le da al tiempo
        public bool blnTiempoTotal = true; // Considera el tiempo total (suma de los tiempos de las rutas) y no el tiempo de la ruta maxima (max(tiemposRutas)).
    }

    /// <summary>
    /// Esta clase guarda las soluciones minimas para hacer con posterioridad
    /// backtrack. Por un lado guarda la intNumSolucionesBacktrack mejores
    /// soluciones para volver a ellas y tambien guarda la mejor solucion
    /// hasta el momento.
    /// </summary>
    class clsBackTrackNew
    {
        Queue<clsDataStructuresRutas> _queRutasMin;//Cola con las rutas minimas para backtrack
        double _dblCosteMin; // Menor valor hasta el momento
        clsDataStructuresRutas _cRutaMinima; // Guarda la ruta minima
        Int32 _intNumSolucionesBacktrack = 0;
        public clsBackTrackNew(Int32 intNumSolucionesBacktrack)
        {
            _intNumSolucionesBacktrack = intNumSolucionesBacktrack;
            _queRutasMin = new Queue<clsDataStructuresRutas>(intNumSolucionesBacktrack);
            _dblCosteMin = -1;
        }

        public void AddRuta(clsDataStructuresRutas cRutas)
        {
            // Si se ha llegado al limite saca la ultima
            if (_queRutasMin.Count() >= _intNumSolucionesBacktrack)
                _queRutasMin.Dequeue();
            //Añade la nueva ruta copiandola
            _queRutasMin.Enqueue(clsObjectCopy.Clone<clsDataStructuresRutas>(cRutas));
            // Chequea que la nueva ruta es al menos igual o menor que la minima
            if (cRutas.dblDistanciaTotal > _dblCosteMin && _dblCosteMin >= 0)
                new Exception("La nueva ruta debe ser menor que la minima");
            // Actualiza el coste minimo
            _dblCosteMin = cRutas.dblDistanciaTotal;
        }
        public clsDataStructuresRutas GetRuta()
        {
            // Ojo si queda la ultima la guara como ruta minima
            if (_queRutasMin.Count() == 1)
            {
                _cRutaMinima = clsObjectCopy.Clone<clsDataStructuresRutas>(_queRutasMin.Peek());
                return _queRutasMin.Dequeue();
            }
            // Si queda mas de una ruta devuelve la siguiente en cola
            if (_queRutasMin.Count() > 0)
                return _queRutasMin.Dequeue();
            else
                return clsObjectCopy.Clone<clsDataStructuresRutas>(_cRutaMinima);
        }
        public clsDataStructuresRutas RutaMinima()
        {
            if (_queRutasMin.Count() > 0)
                return _queRutasMin.Peek();
            return _cRutaMinima;
        }

        public double CosteMinimo()
        {
            return _dblCosteMin;
        }
    }



    /// <summary>
    /// Esta clase guarda las soluciones minimas para hacer con posterioridad
    /// backtrack. Por un lado guarda la intNumSolucionesBacktrack mejores
    /// soluciones para volver a ellas y tambien guarda la mejor solucion
    /// hasta el momento.
    /// </summary>
    class clsBackTrack
    {
        Queue<clsDataStructuresRutas> _queRutasMin;//Cola con las rutas minimas para backtrack
        double _dblCosteMin; // Menor valor hasta el momento
        clsDataStructuresRutas _cRutaMinima; // Guarda la ruta minima
        Int32 _intNumSolucionesBacktrack = 0;
        public clsBackTrack(Int32 intNumSolucionesBacktrack)
        {
            _intNumSolucionesBacktrack = intNumSolucionesBacktrack;
            _queRutasMin = new Queue<clsDataStructuresRutas>(intNumSolucionesBacktrack);
            _dblCosteMin = -1;
        }

        public void AddRuta(clsDataStructuresRutas cRutas)
        {
            // Si se ha llegado al limite saca la ultima
            if (_queRutasMin.Count() >= _intNumSolucionesBacktrack)
                _queRutasMin.Dequeue();
            //Añade la nueva ruta copiandola
            _queRutasMin.Enqueue(clsObjectCopy.Clone<clsDataStructuresRutas>(cRutas));
            // Chequea que la nueva ruta es al menos igual o menor que la minima
            if (cRutas.dblDistanciaTotal > _dblCosteMin && _dblCosteMin >= 0)
                new Exception("La nueva ruta debe ser menor que la minima");
            // Actualiza el coste minimo
            _dblCosteMin = cRutas.dblDistanciaTotal;
        }
        public clsDataStructuresRutas GetRuta()
        {
            // Ojo si queda la ultima la guara como ruta minima
            if (_queRutasMin.Count() == 1)
            {
                _cRutaMinima = clsObjectCopy.Clone<clsDataStructuresRutas>(_queRutasMin.Peek());
                return _queRutasMin.Dequeue();
            }
            // Si queda mas de una ruta devuelve la siguiente en cola
            if (_queRutasMin.Count() > 0)
                return _queRutasMin.Dequeue();
            else
                return clsObjectCopy.Clone<clsDataStructuresRutas>(_cRutaMinima);
        }
        public clsDataStructuresRutas RutaMinima()
        {
            if (_queRutasMin.Count() > 0)
                return _queRutasMin.Peek();
            return _cRutaMinima;
        }

        public double CosteMinimo()
        {
            return _dblCosteMin;
        }
    }

    class clsDataAdaptativeLayer
    {
        Random rnd = new Random(100);
        public enum enumTipoResultado
        {
            SolucionMejorGlobal, // valor una nueva mejor solucion global
            SolucionMejorParcial,  // valor una nueva solucion que mejora la anterior
            SolucionAceptadaQueNoMejora, // valor una nueva solucion que empeora la anterior pero ha sido aceptada
            SolucionRechazada
        }
        Dictionary<string, double> dicOperadorPeso = new Dictionary<string, double>(); // Para cada operador guarda su peso que sera utilizado en la ruleta
        Dictionary<string, double> dicOperadorPuntuacion = new Dictionary<string, double>(); // Para cada operador guarda su score
        Dictionary<string, Int32> dicOperadorPuntuacionCuenta = new Dictionary<string, int>(); // Para cada operador guardar el numero de scores que le ha metido
        SortedList<double, string> slPesoOperador = new SortedList<double, string>();// Sorted list para la ruleta
        Int32 intNumeroDesdeLastUpdate = 0;

        public clsDataAdaptativeLayer(List<string> lstOperadores)
        {
            foreach (string strOperador in lstOperadores)
            {
                dicOperadorPeso.Add(strOperador, (double)1 / lstOperadores.Count);
                dicOperadorPuntuacionCuenta.Add(strOperador, 0);
                dicOperadorPuntuacion.Add(strOperador, 0);
                double dblValor = rnd.NextDouble() * 0.0001 + (1 / (double)lstOperadores.Count);
                slPesoOperador.Add(dblValor, strOperador);
            }
        }

        /// <summary>
        /// Cada vez que se utiliza un operador se indica aqui el resultado del mismo
        /// </summary>
        /// <param name="cParameters"></param>
        /// <param name="strOperador"></param>
        /// <param name="enuResultado"></param>
        public void AddOperadorResultado(clsParameters cParameters, string strOperador, enumTipoResultado enuResultado)
        {
            double dblValor = 0;
            if (enuResultado == enumTipoResultado.SolucionMejorGlobal)
                dblValor = cParameters.dblAdaptativeLayerPuntuacionSolucionMejorGlobal;
            else if (enuResultado == enumTipoResultado.SolucionMejorParcial)
                dblValor = cParameters.dblAdaptativeLayerPuntuacionSolucionMejorParcial;
            else if (enuResultado == enumTipoResultado.SolucionAceptadaQueNoMejora)
                dblValor = cParameters.dblAdaptativeLayerPuntuacionSolucionAceptadaQueNoMejora;
            else if (enuResultado == enumTipoResultado.SolucionRechazada)
                dblValor = cParameters.dblAdaptativeLayerPuntuacionSolucionRechazada;
            dicOperadorPuntuacion[strOperador] = dicOperadorPuntuacion[strOperador] + dblValor;
            dicOperadorPuntuacionCuenta[strOperador]++;
            intNumeroDesdeLastUpdate++;
            if (intNumeroDesdeLastUpdate >= cParameters.intAdaptativeLayerNumeroIteracionParaActualizar)
                ActualizarPesos(cParameters);
        }

        /// <summary>
        /// Genera una ruleta y obtiene el que correponda
        /// segun el peso de cada uno
        /// </summary>
        /// <returns></returns>
        public string ObtenerOperador()
        {

            double dblRand = rnd.NextDouble();
            double dblMin = 0;
            double dblMax = 0;
            foreach (KeyValuePair<double, string> kvPair in slPesoOperador)
            {
                dblMin = dblMax;
                dblMax = dblMax + kvPair.Key;
                if (dblRand > dblMin && dblRand <= dblMax)
                    return kvPair.Value;
            }
            new Exception("No encontrado operador");
            return null;
        }
        /// <summary>
        ///  Actualiza los pesos de los operadores cuando se ha llegado
        ///  al numero de iteraciones marcadas
        /// </summary>
        /// <param name="cParameters"></param>
        public void ActualizarPesos(clsParameters cParameters)
        {
            slPesoOperador = new SortedList<double, string>();
            intNumeroDesdeLastUpdate = 0;
            List<string> lstOperadores = dicOperadorPuntuacion.Keys.ToList();
            double dblPuntuacionTotal = 0;
            // Calcula la suma de las puntuacion medias
            foreach (KeyValuePair<string, double> kvPair in dicOperadorPuntuacion)
            {
                if (kvPair.Value > 0)
                    dblPuntuacionTotal = dblPuntuacionTotal + kvPair.Value / dicOperadorPuntuacionCuenta[kvPair.Key];
            }
            // Actualiza el valor de las puntuaciones
            foreach (string strOperador in lstOperadores)
            {
                // Puntuacion nueva
                double dblPuntuacionMediaNueva = 0;
                if (dicOperadorPuntuacion[strOperador] > 0)
                    dblPuntuacionMediaNueva = dicOperadorPuntuacion[strOperador] / dicOperadorPuntuacionCuenta[strOperador];
                // Actualiza el peso
                dicOperadorPeso[strOperador] = (1 - cParameters.dblAdaptativeLayerPesoNuevo) * dicOperadorPeso[strOperador] +
                    cParameters.dblAdaptativeLayerPesoNuevo * dblPuntuacionMediaNueva / dblPuntuacionTotal;
                slPesoOperador.Add(dicOperadorPeso[strOperador] + rnd.NextDouble() * .0001, strOperador);
                // Pone a cero la puntuacion
                dicOperadorPuntuacion[strOperador] = 0;
            }
        }

    }

    class clsParametersSimulatedAnnealing
    {
        public double dblRandomnessLevel = 5; // Nivel de randomnes para critical distroy mayor que 1 cuanto mas pequeño mas randomness
        public double dblTemperatureInicial; // Temperatura inicial esta temperatura se debe estimar
        public double dblTemperatureFinal; // Temperatura final esta temperatura se debe estimar
        private double dblTemperatureActual = -1; // Esta funcion guarda la temperatura actual
        public double dblProbabilityAceptanceBadSolution = 0.5; // Probabilidad de aceptar una mala solucion para el calculo temperatura inicial.
        public double dblPorcentajeEmpeoramientoBadSolution = 0.05; // La solucion inicial que empeorace este porcentaje podria ser aceptada con probabilidad dblProbabilityAceptanceBadSolution
        public double dblFractionInitialTemperatureAtTheEnd = 0.002; // Fraccion de la temperatura inicial que fija la temperatura final
        public double dblTemperatureDecreaseRate; // Este decrease rate se debe estimar.


        public void InicializarTemperaturas(clsParameters cParameters, double dblCosteSolucionIncial)
        {
            // Chequea que los datos sean correctos
            if (dblPorcentajeEmpeoramientoBadSolution < 0 || dblPorcentajeEmpeoramientoBadSolution > 1)
                new Exception("Porcentje de empeoramiento debe ser entre 0 y 1");
            if (dblProbabilityAceptanceBadSolution < 0 || dblProbabilityAceptanceBadSolution > 1)
                new Exception("Probabilidad de aceptar una mala solucion debe ser entre 0 y 1");
            // Estima la temperatura inicial
            dblTemperatureInicial = (-dblPorcentajeEmpeoramientoBadSolution * dblCosteSolucionIncial) / Math.Log(dblProbabilityAceptanceBadSolution);
            // Estima la temperatura final
            dblTemperatureFinal = dblTemperatureInicial * dblFractionInitialTemperatureAtTheEnd;
            // Estima el decrease rate par obtener el numero de iteraciones
            dblTemperatureDecreaseRate = Math.Exp((Math.Log(dblTemperatureFinal / dblTemperatureInicial)) / cParameters.intNumIterations);
            // Inicializa la temperatura inicial
            dblTemperatureActual = dblTemperatureInicial;
        }

        /// <summary>
        /// Esta funcion va bajando la temperatura cada vez
        /// </summary>
        public void BajarTemperatura()
        {
            dblTemperatureActual = dblTemperatureDecreaseRate * dblTemperatureActual;
        }

        /// <summary>
        /// Esta funcion devuelve la temperatura actual.
        /// </summary>
        /// <returns></returns>
        public double GetTemperatura()
        {
            return dblTemperatureActual;
        }


        /// <summary>
        /// Esta funcion indica si debe continuar(true)
        /// o se ha llegado al fin (false)
        /// </summary>
        /// <returns></returns>
        public Boolean Continuar()
        {
            if (dblTemperatureActual < dblTemperatureFinal)
                return false;
            else
                return true;
        }

    }


    class clsDataPuntoConsumo
    {
        public double dblDemand; // Demanda del punto de consumo
        public double dblLongitud; // Longitud punto de consumo
        public double dblLatitude; // Latitud del punto de consumo
        public double dblTimeWindowsMin; // Inicio ventana de tiempo para iniciar al recogida o entrega (antes obligaria a esperar)
        public double dblTimeWindowsMax; // Fin venta tiempo para recogida o entrega (despues no es posible)
        public double dblTiempoServicio; // Tiempo que se tarda en hacer entrega o recogida
        public Boolean[] blnVehiculosAptos;// Array con los vehiculos que el punto de consumo puede utilizar

    }

    [Serializable]
    class clsDataRutas
    {
        public double dblDistanciaTotal = 0;
        public Dictionary<Int32, double> dicIdVehiculoACapacidadAsignada = new Dictionary<int, double>(); // Para cada vehiculo guarda la capacidad asignada hasta el momento
        public Dictionary<Int32, clsDataRutasPuntoConsumo> dicIdPuntoConsumoAsignadosADatos = new Dictionary<int, clsDataRutasPuntoConsumo>(); // Para un punto de consumo da info sobre el (sigueinte, anterior y vehiculo)
        public Dictionary<Int32, Int32> dicIdVehiculoAIdPuntoConsumoAsignadosPrimeroEnRuta = new Dictionary<int, int>(); // Para un vehiculo indica el primer punto de consumo en la ruta
        public HashSet<Int32> hsIdPuntoConsumoNoAsignados = new HashSet<int>(); // Este hashset contiene puntos de consumo no asignados
        // Este diccionario tiene como key idPuntoConsumo_IdVehiculo y nos da el coste minimo de insertar idPuntoConsumo en ese vehiculo y donde se insertaria
        public Dictionary<string, clsDataRegretEdge> dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo = new Dictionary<string, clsDataRegretEdge>();

        /// <summary>
        /// Esta funcion destruye un punto de consumo asignado y lo paso
        /// a punto de consumo no asignado. Lo unico que no hace es 
        /// calcular su regret, para ello se utiliza  la funcion
        /// calcularregretnoasignados, que calcula el regret de 
        /// todos los puntos no asignados, por lo cual debe ser
        /// llamada despues de destruir todos los puntos
        /// </summary>
        /// <param name="cProblem"></param>
        /// <param name="intIdPuntoConsumo"></param>
        public void Remove(clsDataProblem cProblem, Int32 intIdPuntoConsumo)
        {
            clsDataRutasPuntoConsumo cPC = dicIdPuntoConsumoAsignadosADatos[intIdPuntoConsumo];
            Int32 intIdPcAnterior = cPC.intPuntoConsumoAnterior;
            Int32 intIdPcSiguiente = cPC.intPuntoConsumoSiguiente;
            Int32 intIdVehiculo = cPC.intIdVehiculo;
            // Al siguente le asigna el anterior como anterior
            dicIdPuntoConsumoAsignadosADatos[intIdPcSiguiente].intPuntoConsumoAnterior = intIdPcAnterior;
            // Pone el anterior y el siguiente
            dicIdPuntoConsumoAsignadosADatos[intIdPcAnterior].intPuntoConsumoSiguiente = intIdPcSiguiente;
            // Quita la capacidad asignada
            dicIdVehiculoACapacidadAsignada[intIdVehiculo] = dicIdVehiculoACapacidadAsignada[intIdVehiculo] - cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblDemand;
            // Quita la distancia de la distancia total
            dblDistanciaTotal = dblDistanciaTotal - (cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPcAnterior + "_" + intIdPuntoConsumo] + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumo + "_" + intIdPcSiguiente]);
            // Añade el coste del coneccion del punto antes y siguiente tras quitar el punto
            dblDistanciaTotal = dblDistanciaTotal + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPcAnterior + "_" + intIdPcSiguiente];
            // Comprueba si la debe modificar la priera operacion en maquina
            if (dicIdVehiculoAIdPuntoConsumoAsignadosPrimeroEnRuta[intIdVehiculo] == intIdPuntoConsumo)
                dicIdVehiculoAIdPuntoConsumoAsignadosPrimeroEnRuta[intIdVehiculo] = intIdPcAnterior;
            // Añade el punto consumo a los no asignados
            hsIdPuntoConsumoNoAsignados.Add(intIdPuntoConsumo);
            // Lo borra de asignado
            dicIdPuntoConsumoAsignadosADatos.Remove(intIdPuntoConsumo);
        }


        /// <summary>
        /// Esta fucion calcula para todos los puntos no asignados
        /// el coste minimo de insercion para cada punto y
        /// vehiculo y lo guarda en dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo
        /// </summary>
        /// <param name="cProblem"></param>
        public void CalcularCosteMinimoNoAsignados(clsDataProblem cProblem)
        {
            foreach (Int32 intIdPuntoConsumo in hsIdPuntoConsumoNoAsignados)
            {
                // Calcula el punto minimo de insercion con cada vehiculo
                for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
                {
                    dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumo + "_" + intV] = ObtenerInsercionMinimaParaPuntoConsumoYVehiculo(cProblem, intIdPuntoConsumo, intV);
                }

            }
        }

        public void Add(clsDataProblem cProblem, Int32 intIdPuntoConsumo, Int32 intIdVehiculo)
        {
            // Comprueba que el punto a insertar este en no asignado
            if (!hsIdPuntoConsumoNoAsignados.Contains(intIdPuntoConsumo))
                new Exception("El punto de consumo no esta entre los no asignados");
            // Se borra de los no asignados
            hsIdPuntoConsumoNoAsignados.Remove(intIdPuntoConsumo);
            // Se añade a los ya asignados
            clsDataRutasPuntoConsumo cPC = new clsDataRutasPuntoConsumo();
            cPC.intIdVehiculo = intIdVehiculo;
            //Arregla el anterior
            Int32 intIdPuntoConsumoAnteriorInsercion = dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumo + "_" + intIdVehiculo].intIdPuntoConsumoAnterior;
            cPC.intPuntoConsumoAnterior = intIdPuntoConsumoAnteriorInsercion;
            Int32 intIdSiguiente = 0;
            intIdSiguiente = dicIdPuntoConsumoAsignadosADatos[intIdPuntoConsumoAnteriorInsercion].intPuntoConsumoSiguiente;
            dicIdPuntoConsumoAsignadosADatos[intIdPuntoConsumoAnteriorInsercion].intPuntoConsumoSiguiente = intIdPuntoConsumo;
            // Resta el coste del anterior al siguiente que se acaba de eliminar
            dblDistanciaTotal = dblDistanciaTotal - cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnteriorInsercion + "_" + intIdSiguiente];
            //Arregla el siguiente
            cPC.intPuntoConsumoSiguiente = intIdSiguiente;
            dicIdPuntoConsumoAsignadosADatos[intIdSiguiente].intPuntoConsumoAnterior = intIdPuntoConsumo;
            // Añade el nuevo punto
            dicIdPuntoConsumoAsignadosADatos.Add(intIdPuntoConsumo, cPC);
            // Añade el coste del anterior->PuntoConsumo->siguiente
            dblDistanciaTotal = dblDistanciaTotal + cProblem.dicIdPuntoAIdPuntoBATiempo[cPC.intPuntoConsumoAnterior + "_" + intIdPuntoConsumo] + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumo + "_" + cPC.intPuntoConsumoSiguiente];
            // Actualiza los consumos asignados al vehiculo
            if (!dicIdVehiculoACapacidadAsignada.ContainsKey(intIdVehiculo))
                dicIdVehiculoACapacidadAsignada.Add(intIdVehiculo, 0);
            // Añade la demanda del punto asignado al vehiculo
            dicIdVehiculoACapacidadAsignada[intIdVehiculo] = dicIdVehiculoACapacidadAsignada[intIdVehiculo] + cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intIdPuntoConsumo].dblDemand;
            // Recalcula los valores para el regret respecto al vehiculo actual
            RegretRecalcularTrasInsersion(cProblem, intIdPuntoConsumo, intIdVehiculo);
        }

        private void RegretRecalcularTrasInsersion(clsDataProblem cProblem, Int32 intIdPuntoConsumo, Int32 intIdVehiculo)
        {
            foreach (Int32 intIdPuntoConsumoNoAsignado in hsIdPuntoConsumoNoAsignados)
            {
                double dblCoste = cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoNoAsignado + "_" + intIdPuntoConsumo];
                // Calcula la distancia si que une antes de intIdPuntoConsumo
                Int32 intIdAnterior = dicIdPuntoConsumoAsignadosADatos[intIdPuntoConsumo].intPuntoConsumoAnterior;
                double dblCosteAnterior = dblCoste;
                if (intIdAnterior < 0)
                    dblCosteAnterior = dblCosteAnterior + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoNoAsignado + "_" + 0];
                else
                    dblCosteAnterior = dblCosteAnterior + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoNoAsignado + "_" + intIdAnterior];
                // Calcula la distancia si que une antes de intIdPuntoConsumo
                Int32 intIdSiguiente = dicIdPuntoConsumoAsignadosADatos[intIdPuntoConsumo].intPuntoConsumoSiguiente;
                double dblCosteSiguiente = dblCoste;
                if (intIdSiguiente < 0)
                    dblCosteSiguiente = dblCosteSiguiente + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoNoAsignado + "_" + 0];
                else
                    dblCosteSiguiente = dblCosteSiguiente + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoNoAsignado + "_" + intIdSiguiente];
                // Obtiene el coste minimo
                double dblCosteMinimo = dblCosteAnterior;
                if (dblCosteAnterior > dblCosteSiguiente)
                {
                    dblCosteMinimo = dblCosteSiguiente;
                    intIdAnterior = intIdPuntoConsumo;
                }
                // Actualiza el valor del punto no asignado
                if (!dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo.ContainsKey(intIdPuntoConsumoNoAsignado + "_" + intIdVehiculo))
                    new Exception("diccionario sin datos");
                // Comprueba si el nuevo punto esta en el Edge del minimo
                Int32 intEdgeMinAnterior = dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumoNoAsignado + "_" + intIdVehiculo].intIdPuntoConsumoAnterior;
                Int32 intEdgeMinSiguiente = dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumoNoAsignado + "_" + intIdVehiculo].intIdPuntoConsumoSiguiente;
                double dblEdgeMinCoste = dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumoNoAsignado + "_" + intIdVehiculo].dblInsertionCost;

                Boolean blnCambia = false;
                // La insercion no esta en el edge minimo para este vehiculo y punto consumo por lo que si es menor lo cambia
                if ((intEdgeMinAnterior != intIdAnterior || intEdgeMinSiguiente != intIdSiguiente) && dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumoNoAsignado + "_" + intIdVehiculo].dblInsertionCost > dblCosteMinimo)
                    blnCambia = true;
                // La insercion  esta en el edge minimo para este vehiculo y ademas el coste nuevo es menor que el anterior por lo que lo cambia
                if ((intEdgeMinAnterior == intIdAnterior || intEdgeMinSiguiente == intIdSiguiente) && dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumoNoAsignado + "_" + intIdVehiculo].dblInsertionCost >= dblCosteMinimo)
                    blnCambia = true;
                // Realiza el cambio
                if (blnCambia)
                {
                    dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumoNoAsignado + "_" + intIdVehiculo].dblInsertionCost = dblCosteMinimo;
                    dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumoNoAsignado + "_" + intIdVehiculo].intIdPuntoConsumoSiguiente = intIdSiguiente;
                    dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumoNoAsignado + "_" + intIdVehiculo].intIdPuntoConsumoAnterior = intIdAnterior;
                }
                // La insercion  esta en el edge minimo para este vehiculo y ademas el coste nuevo es MAYOR que el anterior por lo que debe mirar toda la ruta
                clsDataRegretEdge cEdge = new clsDataRegretEdge();
                cEdge.dblInsertionCost = double.MaxValue;

                if ((intEdgeMinAnterior == intIdAnterior || intEdgeMinSiguiente == intIdSiguiente) && dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumoNoAsignado + "_" + intIdVehiculo].dblInsertionCost < dblCosteMinimo)
                {
                    // Debe recorrer todos los puntos de cosumo asignados a este vehiculo y calcular el coste de insercion para obtener el minimo
                    Int32 intIdAnteriorEnRuta = dicIdVehiculoAIdPuntoConsumoAsignadosPrimeroEnRuta[intIdVehiculo];

                    Boolean blnEnBucle = true;
                    while (blnEnBucle)
                    {
                        Int32 intIdSiguienteEnRuta = dicIdPuntoConsumoAsignadosADatos[intIdAnteriorEnRuta].intPuntoConsumoSiguiente;
                        double dblCosteInsertar = 0;
                        // Calcula el coste de insercion
                        if (intIdAnteriorEnRuta < 0)
                            dblCosteInsertar = cProblem.dicIdPuntoAIdPuntoBATiempo[0 + "_" + intIdPuntoConsumoNoAsignado];
                        else
                            dblCosteInsertar = cProblem.dicIdPuntoAIdPuntoBATiempo[intIdAnteriorEnRuta + "_" + intIdPuntoConsumoNoAsignado];

                        if (intIdSiguienteEnRuta < 0)
                            dblCosteInsertar = dblCosteInsertar + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoNoAsignado + "_" + 0];
                        else
                            dblCosteInsertar = dblCosteInsertar + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoNoAsignado + "_" + intIdSiguienteEnRuta];
                        // Comprueba si la nueva insercion minimiza el coste
                        if (dblCosteInsertar < cEdge.dblInsertionCost)
                        {
                            cEdge.dblInsertionCost = dblCosteInsertar;
                            cEdge.intIdPuntoConsumoAnterior = intIdAnteriorEnRuta;
                            cEdge.intIdPuntoConsumoSiguiente = intIdSiguienteEnRuta;
                        }
                        if (intIdSiguienteEnRuta < 0)
                            blnEnBucle = false;
                        intIdAnteriorEnRuta = intIdSiguienteEnRuta;
                    }// fin while
                    // Actualiza al minimo
                    dicIdPuntoConsumoIdVehiculoAEdgeCosteMinimo[intIdPuntoConsumoNoAsignado + "_" + intIdVehiculo] = cEdge;
                }

            }
        }

        /// <summary>
        /// Esta funcion recibe un punto de consumo no asignado y un vehiculo y calcula
        /// el punto de insercion de coste minimo y lo devuelve
        /// </summary>
        /// <param name="cProblem"></param>
        /// <param name="intIdPuntoConsumo"></param>
        /// <param name="intIdVehiculo"></param>
        private clsDataRegretEdge ObtenerInsercionMinimaParaPuntoConsumoYVehiculo(clsDataProblem cProblem, Int32 intIdPuntoConsumo, Int32 intIdVehiculo)
        {
            clsDataRegretEdge cEdge = new clsDataRegretEdge();
            Boolean blnEnBucle = true;
            Int32 intIdPuntoConsumoAnterior = dicIdVehiculoAIdPuntoConsumoAsignadosPrimeroEnRuta[intIdVehiculo];
            cEdge.dblInsertionCost = double.MaxValue;
            cEdge.intIdPuntoConsumoAnterior = -1;
            cEdge.intIdPuntoConsumoSiguiente = -1;
            Boolean blnMinimoEncontrado = false;
            while (blnEnBucle)
            {
                Int32 intIdPuntoConsumoSiguiente = dicIdPuntoConsumoAsignadosADatos[intIdPuntoConsumoAnterior].intPuntoConsumoSiguiente;
                // Calcula el coste de insertar entre los dos
                double dblCoste = cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumoAnterior + "_" + intIdPuntoConsumo] + cProblem.dicIdPuntoAIdPuntoBATiempo[intIdPuntoConsumo + "_" + intIdPuntoConsumoSiguiente];
                if (dblCoste < cEdge.dblInsertionCost)
                {
                    cEdge.dblInsertionCost = dblCoste;
                    cEdge.intIdPuntoConsumoAnterior = intIdPuntoConsumoAnterior;
                    cEdge.intIdPuntoConsumoSiguiente = intIdPuntoConsumoSiguiente;
                    blnMinimoEncontrado = true;
                }
                intIdPuntoConsumoAnterior = intIdPuntoConsumoSiguiente;
                if (intIdPuntoConsumoSiguiente < 0)
                    blnEnBucle = false;
            }
            if (!blnMinimoEncontrado)
                new Exception("Minimo no encontrado para punto y vehiculo");
            return cEdge;
        }

    }





    [Serializable]
    class clsDataRutasPuntoConsumo
    {
        public Int32 intPuntoConsumoAnterior = -1; // Punto de Consumo anterior en la ruta (-1 si no hay anterior)
        public Int32 intPuntoConsumoSiguiente = -1; // Punto de Consumo siguiente en la ruta (-1 si no hay siguiente)
        public Int32 intIdVehiculo = -1; // Vehiculo en el que esta el punto de consumo (-1 si no hay)
    }

    class clsDataRegretPuntoConsumo
    {
        // este diccionario guarda para cada IdVehiculo los datos de si se insertara el punto de consumo
        public Dictionary<Int32, clsDataRegretPuntoConsumoData> dicIdVehiculoADatos = new Dictionary<int, clsDataRegretPuntoConsumoData>();
        public Int32 intIdVehiculo1 = -1; // Id del vehiculo minimo
        public Int32 intIdVehiculo2 = -1; // Id del segundo minimo
        public Int32 intIdVehiculo3 = -1; // Id del tercer minimo
    }
    class clsDataRegretPuntoConsumoData
    {
        public Int32 intIdVehiculo; // Id del vehiculo
        public Int32 intIdPuntoConsumoAnterior; // Id del punto de consumo anterior donde se insertaria (el anterior)
        public double dblIncrementoObjetivo = double.MaxValue; // Incremento del objetivo despues de insertarlo
    }

    /// <summary>
    /// Esta clase define una conexion entre dos punto de consumos
    /// en un vehiculo (o ruta) y el coste que tendria el insertar
    /// un punto de consumo entre los dos.
    /// </summary>
    [Serializable]
    class clsDataRegretEdge
    {
        public Int32 intIdPuntoConsumoAnterior; // Id del punto de consumo anterior
        public Int32 intIdPuntoConsumoSiguiente; // Id del punto de consumo siguiente
        public double dblInsertionCost; // Coste de insercion del punto de consumo entre los dos anteriores
    }

}