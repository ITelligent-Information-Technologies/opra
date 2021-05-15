using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    class clsSolucionInicial
    {
        /// <summary>
        /// Esta funcion crea una solucion inicial que va añadiendo
        /// iterativamente las primeras operaciones de los trabajos
        /// despues las segundas, ... Las operaciones que se van a 
        /// insertar (ej. las segundas operaciones de los trabajos)
        /// se insertan en el orden de mayor a menor longitud de los 
        /// trabajos, asi el trabajo mas largo inserta primero        /// 
        /// </summary>
        /// <param name="cDatos"></param>
        /// <returns></returns>
        public clsDatosSchedule OrdenarPorLongitudTrabajos(clsDatosJobShop cDatos)
        {
            clsDatosSchedule cSchedule = new clsDatosSchedule();
            // Determina cual es el trabajo mas largo
            Int32[] intIdJobsTimeMax = ObtenerTrabajosMasLargos(cDatos);
            // Va insertando las operaciones de cada trabajo en el orden de mas largo a menos 
            Dictionary<Int32, Int32> dicIdJobToIdOperacionLast = new Dictionary<int, Int32>();
            Dictionary<Int32, Int32> dicIdMachineToIdOperacionLast = new Dictionary<int, int>();
            // Carga el diccionario con valores inciales
            for (Int32 intIndex = 0; intIndex < intIdJobsTimeMax.Length; intIndex++)
            {
                Int32 intIdJob = intIdJobsTimeMax[intIndex];
                Int32 intIdOperacion = cDatos.dicIdJobIdOperationFirst[intIdJob];
                Int32 intIdMachine = cDatos.dicIdOperationIdMachine[intIdOperacion];
                // Si la maquina es nueva
                if (!dicIdMachineToIdOperacionLast.ContainsKey(intIdMachine))
                {
                    cSchedule.dicIdOperationIdNextInMachine.Add(intIdOperacion, -1);
                    cSchedule.dicIdOperationIdPreviousInMachine.Add(intIdOperacion, -1);
                }
                else // Si ya hay una operacion de esa maquina
                {
                    cSchedule.dicIdOperationIdNextInMachine.Add(intIdOperacion, -1);
                    cSchedule.dicIdOperationIdNextInMachine[dicIdMachineToIdOperacionLast[intIdMachine]]= intIdOperacion;
                    cSchedule.dicIdOperationIdPreviousInMachine.Add(intIdOperacion, dicIdMachineToIdOperacionLast[intIdMachine]);
                }

                dicIdJobToIdOperacionLast.Add(intIdJob, intIdOperacion);
                if (cSchedule.dicIdMachineIdOperationFirst.ContainsKey(intIdMachine))
                {
                    cSchedule.dicIdMachineIdOperationLast[intIdMachine] = intIdOperacion;
                    dicIdMachineToIdOperacionLast[intIdMachine] = intIdOperacion;
                }
                else
                {
                    cSchedule.dicIdMachineIdOperationFirst.Add(intIdMachine, intIdOperacion);
                    cSchedule.dicIdMachineIdOperationLast.Add(intIdMachine, intIdOperacion);
                    dicIdMachineToIdOperacionLast.Add(intIdMachine, intIdOperacion);
                }
            }
            // Continua metiendo las siguientes
            Boolean blnEnGranBucle = true;
            while (blnEnGranBucle)
            {
                Boolean blnTodasAcabadas = true;
                for (Int32 intIndex = 0; intIndex < intIdJobsTimeMax.Length; intIndex++)
                {
                    Int32 intIdJob = intIdJobsTimeMax[intIndex];
                    Int32 intIdOperacionLastInJob = dicIdJobToIdOperacionLast[intIdJob];
                    Int32 intIdOperacion = cDatos.dicIdOperationIdNextInJob[intIdOperacionLastInJob];
                    if (intIdOperacion > 0)
                    {
                        blnTodasAcabadas = false;
                        Int32 intIdMachine = cDatos.dicIdOperationIdMachine[intIdOperacion];
                        if (dicIdMachineToIdOperacionLast.ContainsKey(intIdMachine))
                        {
                            Int32 intIdOperacionLast = dicIdMachineToIdOperacionLast[intIdMachine];
                            cSchedule.dicIdOperationIdNextInMachine[intIdOperacionLast] = intIdOperacion;
                            cSchedule.dicIdOperationIdPreviousInMachine.Add(intIdOperacion, intIdOperacionLast);
                            cSchedule.dicIdMachineIdOperationLast[intIdMachine] = intIdOperacion;
                        }
                        else
                        {
                            cSchedule.dicIdOperationIdPreviousInMachine.Add(intIdOperacion, -1);
                            cSchedule.dicIdMachineIdOperationFirst.Add(intIdMachine, intIdOperacion);
                            cSchedule.dicIdMachineIdOperationLast.Add(intIdMachine, intIdOperacion);
                        }
                        cSchedule.dicIdOperationIdNextInMachine.Add(intIdOperacion, -1);
                        dicIdJobToIdOperacionLast[intIdJob] = intIdOperacion;
                        dicIdMachineToIdOperacionLast[intIdMachine] = intIdOperacion;
                    }
                }
                blnEnGranBucle = !blnTodasAcabadas;
            }


            return cSchedule;
        }

        public clsDatosSchedule INSA(clsDatosJobShop cDatos)
        {
            clsDatosSchedule cSchedule = new clsDatosSchedule();
            // Determina cual es el trabajo mas largo
            Int32[] intIdJobTimeMax = ObtenerTrabajosMasLargos(cDatos);
            // Inserta las operaciones del trabajo mas largo
            Int32 intIdOperacion = cDatos.dicIdJobIdOperationFirst[intIdJobTimeMax[0]];
            Boolean blnEnBucle = true;
            Dictionary<Int32, Int32> dicIdMachineLastIdOperacion = new Dictionary<int, int>();
            while (blnEnBucle)
            {
                Int32 intIdMachine = cDatos.dicIdOperationIdMachine[intIdOperacion];
                if (!cSchedule.dicIdMachineIdOperationFirst.ContainsKey(intIdMachine))
                {
                    cSchedule.dicIdMachineIdOperationFirst.Add(intIdMachine, intIdOperacion);
                    cSchedule.dicIdMachineIdOperationLast.Add(intIdMachine, intIdOperacion);
                    cSchedule.dicIdOperationIdNextInMachine.Add(intIdOperacion, -1);
                    cSchedule.dicIdOperationIdPreviousInMachine.Add(intIdOperacion, -1);
                    dicIdMachineLastIdOperacion.Add(intIdMachine, intIdOperacion);
                }
                else // No es la primera operacion en esa maquina
                {
                    cSchedule.dicIdMachineIdOperationLast[intIdMachine] = intIdOperacion;
                    Int32 intIdOperacionLast = dicIdMachineLastIdOperacion[intIdMachine];
                    dicIdMachineLastIdOperacion[intIdMachine] = intIdOperacion;
                    cSchedule.dicIdOperationIdNextInMachine[intIdOperacionLast] = intIdOperacion;
                    cSchedule.dicIdOperationIdNextInMachine.Add(intIdOperacion, -1);
                    cSchedule.dicIdOperationIdPreviousInMachine.Add(intIdOperacion, intIdOperacionLast);
                }
                intIdOperacion = cDatos.dicIdOperationIdNextInJob[intIdOperacion];
                if (intIdOperacion < 0)
                    blnEnBucle = false;
            }
            // Ordena las operaciones no  asignadas de mayor a menor
            //Int32[] intIdOperationOrdered = OrdenarOperacionesMenorAMayorTiempo(cDatos, intIdJobTimeMax);
            //// Va tomando cada operacion no asignada ordenada de menor a mayor y las va insertando
            //for (Int32 intIndex = 0; intIndex < intIdOperationOrdered.Length; intIndex++)
            //{
            //    Int32 intIdOperacion = intIdOperationOrdered[intIndex];
            //    // Realiza la insercion

            //}

            return cSchedule;
        }

        private Int32[] OrdenarOperacionesMenorAMayorTiempo(clsDatosJobShop cDatos, Int32 intIdJobTimeMax)
        {
            double[] dblOperationTime = new double[cDatos.dicIdOperationTime.Count];
            Int32[] intIdOperation = new int[cDatos.dicIdOperationTime.Count];
            Int32 intCuenta = 0;
            foreach (KeyValuePair<Int32, double> kvPair in cDatos.dicIdOperationTime)
            {
                if (cDatos.dicIdOperationIdMachine[kvPair.Key] != intIdJobTimeMax)
                {
                    intIdOperation[intCuenta] = kvPair.Key;
                    dblOperationTime[intCuenta] = kvPair.Value;
                    intCuenta++;
                }
            }
            Array.Sort(dblOperationTime, intIdOperation);
            return intIdOperation;
        }
        private Int32[] ObtenerTrabajosMasLargos(clsDatosJobShop cDatos)
        {
            Int32 intIdJobMax = -1;
            double dblJobMax = double.MinValue;
            Int32[] intIdJob = new Int32[cDatos.dicIdJobIdOperationFirst.Count];
            double[] dblJobTime = new double[cDatos.dicIdJobIdOperationFirst.Count];
            Dictionary<Int32, double> dicIdJobTime = new Dictionary<int, double>();
            Int32 intCuenta = 0;
            foreach (KeyValuePair<Int32, Int32> kvPair in cDatos.dicIdJobIdOperationFirst)
            {
                Int32 intIdOperacion = kvPair.Value;
                // añade el tiempo de la primera operacion del trabajo
                dicIdJobTime.Add(kvPair.Key, cDatos.dicIdOperationTime[kvPair.Value]);
                intIdJob[intCuenta] = kvPair.Key;
                dblJobTime[intCuenta] = cDatos.dicIdOperationTime[kvPair.Value];

                Boolean blnEnBucle = true;
                while (blnEnBucle)
                {
                    intIdOperacion = cDatos.dicIdOperationIdNextInJob[intIdOperacion];
                    if (intIdOperacion > -1)
                    {
                        dblJobTime[intCuenta] = dblJobTime[intCuenta] + cDatos.dicIdOperationTime[intIdOperacion];
                        dicIdJobTime[kvPair.Key] = dicIdJobTime[kvPair.Key] + cDatos.dicIdOperationTime[intIdOperacion];
                    }
                    else
                        blnEnBucle = false;
                }
                if (dblJobTime[intCuenta] > dblJobMax)
                {
                    dblJobMax = dblJobTime[intCuenta];
                    intIdJobMax = kvPair.Key;
                }
                intCuenta++;
            }
            // Ordena de menor a mayor  por tiempo de procesamiento
            Array.Sort(dblJobTime, intIdJob);
            // Le da la vuelta
            Array.Reverse(intIdJob);
            return intIdJob;
        }
    }
}
