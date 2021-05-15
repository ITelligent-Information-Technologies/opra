using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    class clsDatosOperacionesPrevias
    {
        public Int32 intIdPreviaMaquina = 0;
        public Int32 intIdPreviaTrabajo = 0;
    }
    class clsTesting
    {
        /// <summary>
        /// Esta clase testea si desde el comienzo de la maquina se llega hasta el final
        /// y si desde el final de cada maquina se llega al principio
        /// utilizando la estructura de datos
        /// </summary>
        /// <param name="cSchedule"></param>
        public Boolean TestMachinePaths(clsDatosSchedule cSchedule, clsDatosJobShop cData)
        {
            Boolean blnOk = true;
            Int32 intOperacionesCuenta = 0;
            // Chequea las maquinas forward
            foreach (KeyValuePair<Int32, Int32> kvPair in cSchedule.dicIdMachineIdOperationFirst)
            {
                Boolean blnEnBucle = true;
                Int32 intIdMachine = kvPair.Key;
                Int32 intIdOperation = kvPair.Value;
                //          Console.WriteLine("++++Forward Test Machine: " + intIdMachine);
                while (blnEnBucle)
                {
                    if (cData.dicIdOperationIdMachine[intIdOperation] != intIdMachine)
                    {
                        blnOk = false;
                        Console.WriteLine("Error la operacion no pertenece a la maquina");
                    }
                    Int32 intIdOperationNext = cSchedule.dicIdOperationIdNextInMachine[intIdOperation];
                    intOperacionesCuenta++;
                    if (intIdOperationNext > -1)
                    {
                        intIdOperation = intIdOperationNext;
                    }
                    else
                        blnEnBucle = false;
                    //Console.WriteLine(intIdOperation);
                }
                if (intIdOperation == cSchedule.dicIdMachineIdOperationLast[intIdMachine])
                {
                    //            Console.WriteLine                        ("OK");
                }
                else
                {
                    Console.WriteLine("Error ultima maquina no es la misma");
                    blnOk = false;
                }
            }
            // Comprueba si estan todos las operaciones
            if (intOperacionesCuenta != cData.dicIdOperationIdMachine.Count)
            {
                Console.WriteLine("Error numero de operaciones en forward incorrecta");
                blnOk = false;
            }
            intOperacionesCuenta = 0;
            // Fin chequear maquinas forward

            // Chequea las maquinas backward
            foreach (KeyValuePair<Int32, Int32> kvPair in cSchedule.dicIdMachineIdOperationLast)
            {
                Boolean blnEnBucle = true;
                Int32 intIdMachine = kvPair.Key;
                Int32 intIdOperation = kvPair.Value;
                //Console.WriteLine("++++Backward Test Machine: " + intIdMachine);
                while (blnEnBucle)
                {
                    Int32 intIdOperationPrevious = cSchedule.dicIdOperationIdPreviousInMachine[intIdOperation];
                    intOperacionesCuenta++;
                    if (intIdOperationPrevious > -1)
                    {
                        intIdOperation = intIdOperationPrevious;
                    }
                    else
                        blnEnBucle = false;
                }
                if (intIdOperation == cSchedule.dicIdMachineIdOperationFirst[intIdMachine])
                {
                    //  Console.WriteLine("OK");
                }
                else
                {
                    Console.WriteLine("Error primera maquina no es la misma");
                    blnOk = false;
                }
            }
            // Comprueba si estan todos las operaciones
            if (intOperacionesCuenta != cData.dicIdOperationIdMachine.Count)
            {
                Console.WriteLine("Error numero de operaciones en forward incorrecta");
                blnOk = false;
            }
            // Fin chequear maquinas backward
            Console.WriteLine("*************************Correcto: " + blnOk);
            return blnOk;
        }

        /// <summary>
        /// Esta funcion se ocupa de comprobar si en un schedule forma un ciclo
        /// basicamente realiza lo mismo que bellman pero utilzando una 
        /// forma mucho mas lenta que chequea varias veces lo mismo
        /// </summary>
        /// <param name="cSchedule"></param>
        /// <param name="cData"></param>
        /// <returns></returns>
        public Boolean TestCiclos(clsDatosSchedule cSchedule, clsDatosJobShop cData)
        {
            Dictionary<Int32, clsDatosOperacionesPrevias> dicIdOperacionToPrevias = new Dictionary<int, clsDatosOperacionesPrevias>();
            // crea las operaciones para todo el diccionario
            foreach (KeyValuePair <Int32, double> kvPair in cData .dicIdOperationTime )
            {
                clsDatosOperacionesPrevias cOp = new clsDatosOperacionesPrevias();
                dicIdOperacionToPrevias.Add(kvPair.Key, cOp);
            }
            // Obtiene las primeras en maquina
            HashSet<Int32> hsInterseccion = new HashSet<int>();
            foreach (KeyValuePair<Int32, Int32> kvPair in cSchedule.dicIdMachineIdOperationFirst)
            {
                dicIdOperacionToPrevias[kvPair.Value].intIdPreviaMaquina = -1;
            }

            // Obtiene las primeras en trabajo
            foreach (KeyValuePair<Int32, Int32> kvPair in cData.dicIdJobIdOperationFirst)
            {
                
                    dicIdOperacionToPrevias[kvPair.Value].intIdPreviaTrabajo = -1;
                if (dicIdOperacionToPrevias[kvPair.Value].intIdPreviaMaquina != 0 && dicIdOperacionToPrevias[kvPair.Value ].intIdPreviaTrabajo != 0)
                    hsInterseccion.Add(kvPair.Value);

            }
            if (hsInterseccion.Count == 0)
            {
                Console.WriteLine("No hay operaciones inciales en maquina y trabajo");
                return false;
            }

            // Genera un bucle para ir comprobando las que tienen anteriores y actualizando
            Boolean blnEnBucleGrande = true;
            Int32 intVueltas = 0;
            while (blnEnBucleGrande)
            {
                blnEnBucleGrande = false;
                // Maquinas
                foreach (KeyValuePair<Int32, Int32> kvPair in cSchedule.dicIdMachineIdOperationFirst)
                {
                    Boolean blnEnBucle = true;
                    Int32 intIdMachine = kvPair.Key;
                    Int32 intIdOperation = kvPair.Value;
                    blnEnBucleGrande = false;
                    Boolean blnEncontradasTodas = true;
                    while (blnEnBucle)
                    {
                        Int32 intIdOperationNext = cSchedule.dicIdOperationIdNextInMachine[intIdOperation];
                        if (intIdOperationNext > -1)
                        {
                            if (dicIdOperacionToPrevias[intIdOperation].intIdPreviaMaquina != 0 && dicIdOperacionToPrevias[intIdOperation].intIdPreviaTrabajo != 0)
                                dicIdOperacionToPrevias[intIdOperationNext].intIdPreviaMaquina = intIdOperation;
                            else
                                blnEncontradasTodas = false;
                        }
                        else
                            blnEnBucle = false;
                        intIdOperation = intIdOperationNext;
                    }
                    // SI no se han encontrado todas debe continuar en bucle
                    if (!blnEncontradasTodas)
                        blnEnBucleGrande = true;
                    
                }
                // Trabajos
                // Maquinas
                foreach (KeyValuePair<Int32, Int32> kvPair in cData .dicIdJobIdOperationFirst )
                {
                    Boolean blnEnBucle = true;
                    Int32 intIdJob = kvPair.Key;
                    Int32 intIdOperation = kvPair.Value;
                    Boolean blnEncontradasTodas = true;
                    while (blnEnBucle)
                    {
                        Int32 intIdOperationNext = cData.dicIdOperationIdNextInJob [intIdOperation];
                        if (intIdOperationNext > -1)
                        {
                            if (dicIdOperacionToPrevias[intIdOperation].intIdPreviaMaquina != 0 && dicIdOperacionToPrevias[intIdOperation].intIdPreviaTrabajo != 0)
                                dicIdOperacionToPrevias[intIdOperationNext].intIdPreviaTrabajo  = intIdOperation;
                            else
                                blnEncontradasTodas = false;
                        }
                        else
                            blnEnBucle = false;
                        intIdOperation = intIdOperationNext;
                    }
                    // SI no se han encontrado todas debe continuar en bucle
                    if (!blnEncontradasTodas)
                        blnEnBucleGrande = true;
                }
                intVueltas++;
                if(intVueltas >(cData .dicIdOperationTime.Count *1.1))
                {
                    Console.WriteLine("No se ha podido acabar");
                    return false;
                }
            }

            return true;
        }
    }
}
