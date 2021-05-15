using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    class clsBellman
    {
        Random _rnd = new Random(100);

        /// <summary>
        /// Esta funcion realiza un bellman desde el final hacia el comienzo
        /// obteniendo el tiempo que le queda a una operacion por llegar al final
        /// por su camino mas largo. Esto es para la operacion k la funcion devuelve
        /// el tiempo desde k (incluido su propio tiempo de procesamiento) hasta el final
        /// por el camino mas largo. Así si k pertenece al caminio critico y el makespan
        /// es M entonces el tiempo de comienzo de K + el tiempo obtenido por esta funcion
        /// sera igual al valor M.
        /// </summary>
        /// <param name="cDatos"></param>
        /// <param name="cSchedule"></param>
        /// <returns></returns>
        public Dictionary<Int32, double> CalcularBellmanFromEnd(clsDatosJobShop cDatos, clsDatosSchedule cSchedule)
        {
            // Inicializa diccionarios auxiliares
            Dictionary<Int32, double> dicIdOperacionListaTrabajo = new Dictionary<int, double>(); // Tiempo de la operacion por trabajo
            Dictionary<Int32, double> dicIdOperacionListaMaquina = new Dictionary<int, double>(); // Tiempo de la operacion por maquina
            Dictionary<Int32, double> dicIdOperacionTiempoHastaFin = new Dictionary<int, double>();// Tiempo desde el comienzo operacion hasta el final 
            Queue<Int32> queExplotacion = new Queue<int>(); // Lista de operaciones que estan listas pero no hemmos mirado sus siguientes
            Int32 intIteraciones = 0;
            // Toma las semillas que son las ULTIMAS operaciones en maquinas y que no tengan anteriores en trabajo
            foreach (KeyValuePair<Int32, Int32> kvPair in cSchedule.dicIdMachineIdOperationLast)
            {
                // Comprueba si la ULTIMA operacion la siguiente en maquina etsa vacia
                Int32 intIdOperacion = kvPair.Value;
                dicIdOperacionListaMaquina.Add(intIdOperacion, cDatos.dicIdOperationTime[intIdOperacion]);
                if (cDatos.dicIdOperationIdNextInJob[intIdOperacion] == -1)
                {
                    queExplotacion.Enqueue(intIdOperacion);
                    dicIdOperacionListaTrabajo.Add(intIdOperacion, cDatos.dicIdOperationTime[intIdOperacion]);
                    dicIdOperacionTiempoHastaFin.Add(intIdOperacion, cDatos.dicIdOperationTime[intIdOperacion]);
                }
                else
                {

                }
            }
            // Explota las operaciones en cola
            while (queExplotacion.Count() > 0)
            {
                Int32 intIdOperation = queExplotacion.Dequeue();
                intIteraciones++;
                // Comprueba si la anterior en  job esta lista
                Int32 intIdOperationPreviousJob = cDatos.dicIdOperationIdPreviousInJob[intIdOperation];
                if (intIdOperationPreviousJob > -1)
                {
                    dicIdOperacionListaTrabajo.Add(intIdOperationPreviousJob, dicIdOperacionTiempoHastaFin[intIdOperation] + cDatos.dicIdOperationTime[intIdOperationPreviousJob]);
                    // Comprueba si ya esta lista por maquina ya que es la ultima de la maquina
                    if (cSchedule.dicIdOperationIdNextInMachine[intIdOperationPreviousJob] == -1)
                    {
                        dicIdOperacionTiempoHastaFin.Add(intIdOperationPreviousJob, dicIdOperacionListaTrabajo[intIdOperationPreviousJob]);
                        queExplotacion.Enqueue(intIdOperationPreviousJob);
                    }
                    else if (dicIdOperacionListaMaquina.ContainsKey(intIdOperationPreviousJob))
                    {
                        // Comprueba si ya esta lista por maquina no siendo la primera por maquina
                        dicIdOperacionTiempoHastaFin.Add(intIdOperationPreviousJob, Math.Max(dicIdOperacionListaMaquina[intIdOperationPreviousJob], dicIdOperacionListaTrabajo[intIdOperationPreviousJob]));
                        queExplotacion.Enqueue(intIdOperationPreviousJob);
                    }
                }
                // Comprueba si la anterior en  machine esta lista
                Int32 intIdOperationPreviousMachine = cSchedule.dicIdOperationIdPreviousInMachine[intIdOperation];
                if (intIdOperationPreviousMachine > -1)
                {
                    dicIdOperacionListaMaquina.Add(intIdOperationPreviousMachine, dicIdOperacionTiempoHastaFin[intIdOperation] + cDatos.dicIdOperationTime[intIdOperationPreviousMachine]);
                    // Comprueba si ya esta lista por trabajo ya que es la ultima del trabajo
                    if (cDatos.dicIdOperationIdNextInJob[intIdOperationPreviousMachine] == -1)
                    {
                        dicIdOperacionTiempoHastaFin.Add(intIdOperationPreviousMachine, dicIdOperacionListaMaquina[intIdOperationPreviousMachine]);
                        queExplotacion.Enqueue(intIdOperationPreviousMachine);
                    }
                    else if (dicIdOperacionListaTrabajo.ContainsKey(intIdOperationPreviousMachine))
                    {
                        // Comprueba si ya esta lista por maquina no siendo la primera por maquina
                        dicIdOperacionTiempoHastaFin.Add(intIdOperationPreviousMachine, Math.Max(dicIdOperacionListaMaquina[intIdOperationPreviousMachine], dicIdOperacionListaTrabajo[intIdOperationPreviousMachine]));
                        queExplotacion.Enqueue(intIdOperationPreviousMachine);
                    }
                }
            }
            return dicIdOperacionTiempoHastaFin;
        }


        public (string strFirma, Boolean blnEsCiclo, Boolean blnEsOptimo) CalcularBellman(clsDatosJobShop cDatos, clsDatosSchedule cSchedule)
        {
            Boolean blnEsOptimo = false;
            //StringBuilder Para Signature Tabu list
            StringBuilder sbSignature = new StringBuilder();
            // Pone a cero los valores de Bellman
            cSchedule.ReinicializarDatosBellman();
            // Comienza
            Dictionary<Int32, Int32> dicIdMachineNextLstPosition = new Dictionary<int, int>();
            Queue<Int32> queExplotacion = new Queue<int>(); // Lista de operaciones que estan listas pero no hemmos mirado sus siguientes
            HashSet<Int32> hsIdOperacionesReady = new HashSet<int>(); // Hash con las operaciones que estan listas (tienen las anteriores calculadas)
            Int32 intIteraciones = 0;
            double dblMakespanMax = -1;
            Int32 intIdOperationMax = -1;
            // Toma las semillas que son las primeras operaciones en maquinas y que no tengan anteriores en trabajo
            foreach (KeyValuePair<Int32, Int32> kvPair in cSchedule.dicIdMachineIdOperationFirst)
            {
                // Comprueba si la operacion esta lista
                Int32 intIdOperacion = kvPair.Value;
                if (cDatos.dicIdOperationIdPreviousInJob[intIdOperacion] == -1)
                {
                    queExplotacion.Enqueue(intIdOperacion);
                    hsIdOperacionesReady.Add(intIdOperacion);
                    cSchedule.dicIdOperationStartTime.Add(intIdOperacion, 0);
                    cSchedule.dicIdOperationEndTime.Add(intIdOperacion, cDatos.dicIdOperationTime[intIdOperacion]);
                    
                    cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperacion, -1);
                    if (cSchedule.dicIdOperationEndTime[intIdOperacion] > dblMakespanMax)
                    {
                        dblMakespanMax = cSchedule.dicIdOperationEndTime[intIdOperacion];
                        intIdOperationMax = intIdOperacion;
                    }
                }
            }
            // Explota las operaciones en cola
            while (queExplotacion.Count() > 0)
            {
                Int32 intIdOperation = queExplotacion.Dequeue();
                intIteraciones++;
                // Comprueba si la siguiente en  job esta lista
                Int32 intIdOperationNextJob = cDatos.dicIdOperationIdNextInJob[intIdOperation];
                if (intIdOperationNextJob != -1)
                {
                    Int32 intIdPreviousInMachine = cSchedule.dicIdOperationIdPreviousInMachine[intIdOperationNextJob];
                    if (!hsIdOperacionesReady.Contains(intIdOperationNextJob) && (intIdPreviousInMachine == -1 || hsIdOperacionesReady.Contains(intIdPreviousInMachine)))
                    {
                        queExplotacion.Enqueue(intIdOperationNextJob);
                        hsIdOperacionesReady.Add(intIdOperationNextJob);
                        // Finaliza mas tarde la anterior por Job
                        if (intIdPreviousInMachine == -1 || cSchedule.dicIdOperationEndTime[intIdOperation] > cSchedule.dicIdOperationEndTime[intIdPreviousInMachine])
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdOperation]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdOperation] + cDatos.dicIdOperationTime[intIdOperationNextJob]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextJob, intIdOperation);
                        }
                        else // Finaliz mas tarde por maquina
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdPreviousInMachine]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextJob, cSchedule.dicIdOperationEndTime[intIdPreviousInMachine] + cDatos.dicIdOperationTime[intIdOperationNextJob]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextJob, intIdPreviousInMachine);
                        }
                        if (cSchedule.dicIdOperationEndTime[intIdOperationNextJob] > dblMakespanMax)
                        {
                            dblMakespanMax = cSchedule.dicIdOperationEndTime[intIdOperationNextJob];
                            intIdOperationMax = intIdOperationNextJob;
                        }
                    }
                }
                // Comprueba si la siguiente en maquina esta lista
                Int32 intIdOperationNextMachine = cSchedule.dicIdOperationIdNextInMachine[intIdOperation];
                if (intIdOperationNextMachine != -1)
                {
                    Int32 intIdPreviousInJob = cDatos.dicIdOperationIdPreviousInJob[intIdOperationNextMachine];
                    if (!hsIdOperacionesReady.Contains(intIdOperationNextMachine) && (intIdPreviousInJob == -1 || hsIdOperacionesReady.Contains(intIdPreviousInJob)))
                    {
                        queExplotacion.Enqueue(intIdOperationNextMachine);
                        hsIdOperacionesReady.Add(intIdOperationNextMachine);
                        // Finaliza mas tarde la anterior por maquina
                        if (intIdPreviousInJob == -1 || cSchedule.dicIdOperationEndTime[intIdOperation] > cSchedule.dicIdOperationEndTime[intIdPreviousInJob])
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdOperation]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdOperation] + cDatos.dicIdOperationTime[intIdOperationNextMachine]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextMachine, intIdOperation);
                        }
                        else // Finaliz mas tarde por job
                        {
                            cSchedule.dicIdOperationStartTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdPreviousInJob]);
                            cSchedule.dicIdOperationEndTime.Add(intIdOperationNextMachine, cSchedule.dicIdOperationEndTime[intIdPreviousInJob] + cDatos.dicIdOperationTime[intIdOperationNextMachine]);
                            cSchedule.dicIdOperationPreviousCritialPath.Add(intIdOperationNextMachine, intIdPreviousInJob);
                        }
                        if (cSchedule.dicIdOperationEndTime[intIdOperationNextMachine] > dblMakespanMax)
                        {
                            dblMakespanMax = cSchedule.dicIdOperationEndTime[intIdOperationNextMachine];
                            intIdOperationMax = intIdOperationNextMachine;
                        }
                    }
                }
            }
            // Si no ha recorrido todas hay BUCLE
            if (cSchedule.dicIdOperationStartTime.Count != cDatos.dicIdOperationTime.Count)
                return ("", true,false);
            cSchedule.dblMakespan = dblMakespanMax;
            // TODO AQUI SE PODRIA QUITAR EL CALCULO DEL MAKESPAN Y HACERLO EN EL BUCLE DE BELLMAN END
            // Obtiene el camino critico
            Boolean blnEnGranBucle = true;
            cSchedule.lstIdOperationInCriticalPath.Add(intIdOperationMax);
            List<Int32> lstBlock = new List<int>();
            Int32 intIdMachineOld = -1;
            Int32 intMaximoOperacionEnBloques = Int32.MinValue;
            while (blnEnGranBucle)
            {
                Int32 intIdMachine = cDatos.dicIdOperationIdMachine[intIdOperationMax];
                // Añade al critical block
                if (intIdMachine == intIdMachineOld || intIdMachineOld == -1)
                    lstBlock.Add(intIdOperationMax);
                else
                {
                    // Guarda el maximo de operaciones que tiene el bloque mas grande
                    if (lstBlock.Count > intMaximoOperacionEnBloques)
                        intMaximoOperacionEnBloques = lstBlock.Count;
                    // Si el bloque tiene al menos dos operaciones lo guarda.
                    if (lstBlock.Count >= 2)
                        cSchedule.lstCriticalBlocks.Add(lstBlock);
                    lstBlock = new List<int>();
                    lstBlock.Add(intIdOperationMax);
                }
                // Pasa a la operacion anterior
                if (cSchedule.dicIdOperationPreviousCritialPath[intIdOperationMax] == -1)
                    blnEnGranBucle = false;
                else
                {
                    intIdOperationMax = cSchedule.dicIdOperationPreviousCritialPath[intIdOperationMax];
                    cSchedule.lstIdOperationInCriticalPath.Add(intIdOperationMax);
                    sbSignature.Append("_" + intIdOperationMax);
                }
                intIdMachineOld = intIdMachine;
            }
            if (lstBlock.Count > 2)
                cSchedule.lstCriticalBlocks.Add(lstBlock);
            // Si hay un solo bloque todo el caminio critico esta sobre la misma maquina y es optimaç
            if (cSchedule.lstCriticalBlocks.Count == 1)
                blnEsOptimo = true;
            // Si el camino critico esta formado por un solo trabajo es optima
            if (intMaximoOperacionEnBloques == 1)
                blnEsOptimo = true;

            // return (cSchedule.dblMakespan.ToString("0.########") + sbSignature.ToString(), false,blnEsOptimo );
            return (cSchedule.dblMakespan.ToString("0.########") , false, blnEsOptimo);
        }

        public void EstimarBellmanParaUnCambioForward(Int32 intIdBlock, Int32 intIdOperationBlockMoverForward)
        {

        }

    }
}
