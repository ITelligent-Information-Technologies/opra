using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    /// <summary>
    /// Datos de un movimiento realizado
    /// segun neighborhood N6
    /// </summary>
    class clsDatosMovimiento
    {
        public Int32 intIdOperacionU; // Operacion U en el bloque
        public Int32 intIdOperacionV; // Operacion V en el bloque
        public Int32 intIdOperacionAnteriorU; //Operacoin anterior a U en maquina antes de cambio
        public Int32 intIdOperacionPosteriorV; //Operacion posterior a V en maquina antes de cambio
        public Int32 intIdOperacionAnteriorV; //Operacoin anterior a V en maquina antes de cambio
        public Int32 intIdOperacionPosteriorU; //Operacion posterior a U en maquina antes de cambio
        public Boolean blnEsForward; // Si el cambio es forward, si es false es backward
        public Boolean bleEsN6; // Di el cambio es N6 si no lo es entonces N1
        public double dblEstimatedMakespan; // Makespan estimado
    }
    /// <summary>
    /// Esta clase esta inspirada en el paper de Zhang
    /// A  very fast algorithm for the job shop scheduling problem
    /// e intenta resolver el JSP utilizando TabuSearch TS
    /// y Simulated Annealing SA
    /// </summary>
    class clsTSSA
    {
        private Random _rnd = new Random(100);
        public clsDatosSchedule  JobShop(clsDatosJobShop cData, clsDatosSchedule cSchedule, clsDatosParametros cParametros)
        {
            clsDatosSchedule cScheduleMin = new clsDatosSchedule();
            double dblTinicial = 2;
            double dblT = dblTinicial; // Temperatura inicial
            double dblDecreaseRate = 0.9;
            // Crea la clase de los Neighbourhood
            clsN6 cN6 = new clsN6();
            clsN1 cN1 = new clsN1();
            // Crea el stack para el backtrack
            clsMaxStack<clsDatosSchedule> cStackBackTrack = new clsMaxStack<clsDatosSchedule>(cParametros.intMaxStackBackTrack);
            // Genera la solucion Inicial
            // TODO
            // Clase de deteccio de ciclos
            clsCycle cCycle = new clsCycle(100, 2);
            // Calcula Bellman            
            clsBellman cBellman = new clsBellman();
            clsTabuList cTabuList = new clsTabuList(cParametros);
            clsFirmaTabuList cFirma = new clsFirmaTabuList(cParametros.enuGuardarTabuList);
            clsDatosMovimiento cMovimientoOld = new clsDatosMovimiento();
            double dblMakespanMin = double.MaxValue;
            // Parametros control bucles
            Boolean blnEnBucleBig = true;
            // Comienza a contar el tiempo e iteraciones
            DateTime dtmStart = DateTime.Now;
            Int32 intCuentaBucleBig = 0;
            clsSimulatedAnnealing cSA = new clsSimulatedAnnealing(cParametros, 10);
            // Bucle Big (maximo iteraciones y tiempos)
            while (blnEnBucleBig)
            {
                Int32 intCuentaBucleSmall = 0;
                Boolean blnEnBucleSmall = true;
                // Bucle small (iteraciones pequeñas)
                while (blnEnBucleSmall)
                {
                    intCuentaBucleBig++;
                    intCuentaBucleSmall++;
                    Boolean blnEsCiclo = false;
                    // Calcula el makespan utilizando bellman
                    (string strFirma, Boolean blnEsBucle, Boolean blnEsOptimo) = cBellman.CalcularBellman(cData, cSchedule);
                    // Si se ha producido un bucle (es decir en el grafo hay un bucle)
                    if (blnEsBucle)
                        new Exception("Error Bucle");
                    // Comprueba si hay un ciclo (se repiten los makespan con un patron)
                    blnEsCiclo = cCycle.CheckCycleAndAdd(cSchedule.dblMakespan);
                    if (blnEsCiclo)
                        Console.WriteLine("********************************* CICLO CICLO CICLO");
                    if (!blnEsCiclo)
                    {
                        // Compruba si ha mejorado
                        if (cSchedule.dblMakespan < dblMakespanMin || (cSchedule.dblMakespan > dblMakespanMin && cSA.Aceptar(cSchedule.dblMakespan, dblMakespanMin)))
                        {
                            if (cSchedule.dblMakespan < dblMakespanMin)
                            { 
                                dblMakespanMin = cSchedule.dblMakespan;
                                // Guarda el minimo hasta el momento
                                 cScheduleMin = clsObjectCopy.Clone<clsDatosSchedule>(cSchedule);
                            }
                            else
                                Console.WriteLine("****************************Aceptada INFERIOR");
                            // Hace una copia y lo guarda en backtrack
                            clsDatosSchedule cScheduleNew = clsObjectCopy.Clone<clsDatosSchedule>(cSchedule);
                            cStackBackTrack.Push(cScheduleNew);
                            intCuentaBucleSmall = 0;
                          
                        }
                        // Calcula Bellman desde el final
                        Dictionary<Int32, double> dicIdOperacionTiempoHastaFin = cBellman.CalcularBellmanFromEnd(cData, cSchedule);
                        // Genera los movimientos
                        clsDatosCambio cCambio = cN6.SeleccionarMovimiento(cParametros, strFirma, cTabuList, cData, cSchedule, dicIdOperacionTiempoHastaFin, dblMakespanMin);
                        // Hay un cambio potencial para realizar
                        if (cCambio.intIdOperacionU > -1)
                        {
                            if (cCambio.blnEsForward)
                            {
                                cTabuList.Add(cSchedule, cCambio);
                                cMovimientoOld = cN6.RealizarMovimientoForward(cData, cCambio.intIdOperacionU, cCambio.intIdOperacionV, cSchedule);
                            }
                            else
                            {
                                cTabuList.Add(cSchedule, cCambio);
                                cMovimientoOld = cN6.RealizarMovimientoBackward(cData, cCambio.intIdOperacionU, cCambio.intIdOperacionV, cSchedule);
                            }
                            cMovimientoOld.dblEstimatedMakespan = cCambio.dblMakespanEstimado;
                            Console.WriteLine(intCuentaBucleBig + "->" + cSchedule.dblMakespan + "->" + dblMakespanMin);
                        }
                        else
                        {
                            //Se utiliza N1
                            cMovimientoOld = cN1.N1SeleccionarYRealizarMovimiento(cData, cSchedule);
                            Console.WriteLine("*************************N1");
                        }
                    }// no es blnEsCiclo
                     // Comprueba si debe salir
                    if (blnEsCiclo || intCuentaBucleSmall > cParametros.intMaxIteracionesPorBucle || intCuentaBucleBig > cParametros.intMaxIteraciones)
                        blnEnBucleSmall = false;
                } // Bucle Small
                  // Siguiente en el stack
                  // Decrementa temperatura en cada iteracion
                cSA.DecrementarTemperatura();
                if (cStackBackTrack.Count > 0)
                    cSchedule = cStackBackTrack.Pop();
                else
                {
                    if (cParametros.blnSiNoHayParaBacktrackUtilizaN1)
                    {
                        Console.WriteLine("********************************************** SIN BACKTRAK UTILIZA N1");
                        //Se utiliza N1
                        cMovimientoOld = cN1.N1SeleccionarYRealizarMovimiento(cData, cSchedule);
                    }
                    else
                        blnEnBucleBig = false; // Si no hay mas sale
                }

                cTabuList = new clsTabuList(cParametros);
                cCycle.Clear();
                // Comprueba si debe finalizar por maximo de iteraciones o por tiempo
                if (intCuentaBucleBig > cParametros.intMaxIteraciones || (DateTime.Now - dtmStart).TotalSeconds > cParametros.intMaxSegundos)
                    blnEnBucleBig = false;
            } // Bucle Big
            double dblRE = (dblMakespanMin - cData.dblMakespanBest) / cData.dblMakespanBest;
            double dblSeconds = (DateTime.Now - dtmStart).TotalSeconds;
            Console.WriteLine("Makespan Obtenido: " + dblMakespanMin);
            Console.WriteLine("Tiempo (segundos): " + dblSeconds);
            Console.WriteLine("Resultado Respecto Mejor(%): " +(1- dblRE) * 100);
            return cScheduleMin;
        }



    }
}
