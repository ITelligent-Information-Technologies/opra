using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    class clsTaboo
    {
        public clsDatosResultados JobShop(clsDatosJobShop cData, clsDatosSchedule cSchedule, clsDatosHorarios cDatosHorarios, Int32 intMaxTabooList, Int32 intMaxBackTrackList, Int32 intMaxIter)
        {
            clsDatosSchedule cScheduleMin;
            Boolean blnEnBucle = true;
            Int32 intCuenta = 0;
            // Genera la lista taboo
            clsTabooList cTlist = new clsTabooList(intMaxTabooList);
            // Genera la lista backtrack
            clsBackTrackList cBackTrackList = new clsBackTrackList(intMaxBackTrackList);
            // Genera la solucion Inicial
            // TODO
            // Calcula Bellman            
            clsBellman cBellman = new clsBellman();
            cBellman.CalcularBellman(cData, cSchedule);
            // Genera los movimientos
            List<Tuple<Int32, Int32>> lstMoves = MovesCandidates(cData, cSchedule);
            // Si no hay movimientos sale
            if (lstMoves.Count() == 0)
            {
                // Guarda el actual como minimo
                cScheduleMin = clsObjectCopy.Clone<clsDatosSchedule>(cSchedule);
                blnEnBucle = false;
            }
            // Comprueba si debe guardar en lista backtrack
            double dblMakespanMin = cSchedule.dblMakespan;
            Int32 intIter = 0;
            // Selecciona el movimiento
            while (blnEnBucle)
            {
                double dblMakespanOld = dblMakespanMin;
                intCuenta++;
                Console.WriteLine(intCuenta + " " + dblMakespanMin);
                // Selecciona el movimiento y calcula el resultado para ese movimiento
                MoveSelection(cData, cSchedule, lstMoves, cTlist, ref dblMakespanMin);
                // Si mejora el makespan pone las iteraciones a cero y mete en lista backtrack
                if (dblMakespanMin < dblMakespanOld)
                {
                    intIter = 0;
                    // Guarda el actual como minimo
                    cScheduleMin = clsObjectCopy.Clone<clsDatosSchedule>(cSchedule);
                    // Guarda en lista de backtrack
                    if (lstMoves.Count > 1)
                        cBackTrackList.Add(cData, cSchedule, cTlist, lstMoves, cSchedule.tupMove, cSchedule.dblMakespan);
                }
                else
                    intIter++;
                // Comprueba si ha llegado al fin de las interaciones
                if (intIter > intMaxIter)
                {
                    // Si no hay mas en la lista de backtracking sale
                    if (cBackTrackList.Count() == 0)
                        blnEnBucle = false;
                    else
                    {
                        // Realiza el backtrack
                        clsDatosBackTrack cBackTrack = cBackTrackList.Get();
                        cSchedule = clsObjectCopy.Clone<clsDatosSchedule>(cBackTrack.cSchedule);
                        cTlist = clsObjectCopy.Clone<clsTabooList>(cBackTrack.cTlist);
                        lstMoves = new List<Tuple<int, int>>();
                        foreach (Tuple<Int32, Int32> tupCopy in cBackTrack.lstMoves)
                            lstMoves.Add(tupCopy);
                    }
                    intIter = 0;
                }
                else
                {
                    // Genera los movimientos nuevos a partir del resultado anterior
                    lstMoves = MovesCandidates(cData, cSchedule);
                    // Si no hay movimiento sales
                    if (lstMoves.Count == 0)
                        blnEnBucle = false;
                }
            } // Fin while
              // Obtiene el horario
            clsHorarios cHorarios = new clsHorarios();
            cHorarios.ObtenerFechas(cDatosHorarios, cData, cSchedule, DateTime.Now);
            clsDatosResultados cResultados = new clsDatosResultados();
            cResultados.cData = cData;
            cResultados.cSchedule = cSchedule;
            return cResultados;
        }

        private void MoveSelection(clsDatosJobShop cData, clsDatosSchedule cSchedule, List<Tuple<Int32, Int32>> lstMoves, clsTabooList cTlist, ref double dblMakespanMinGlobal)
        {
            // Comprueba los movimientos U (unforbidden) y FP (forbidden but profitables)
            double dblMakespan = -1;
            double dblMakespanMin = double.MaxValue;
            Int32 intIdOperationMin1 = -1;
            Int32 intIdOperationMin2 = -1;
            HashSet<string> hsMovesFN = new HashSet<string>(); // Esta funcion se utiliza para guardar los non profitabe FN            
            foreach (Tuple<Int32, Int32> tupMove in lstMoves)
            {
                // Realiza el moviento para obtener el Cmax y vuelve al original
                MoveTest(tupMove, cData, cSchedule, true);
                dblMakespan = cSchedule.dblMakespan;
                // Comprueba si es un U move (permtido) o FP (forbidden but profitable)
                Boolean blnConsiderarMove = false;
                if (dblMakespan < dblMakespanMinGlobal)
                    blnConsiderarMove = true;
                else // Comprueba si es U
                {
                    if (!cTlist.Exist(tupMove.Item1, tupMove.Item2))
                        blnConsiderarMove = true;
                }
                // Comprueba si es U o FP y si mejora el anterior
                if (blnConsiderarMove && dblMakespan < dblMakespanMin)
                {
                    dblMakespanMin = dblMakespan;
                    intIdOperationMin1 = tupMove.Item1;
                    intIdOperationMin2 = tupMove.Item2;
                }
                // Si es FN lo guarda
                if (!blnConsiderarMove)
                    hsMovesFN.Add(tupMove.Item1 + "_" + tupMove.Item2);
            }
            // Si ha obtenido un minimo
            if (intIdOperationMin1 != -1)
            {
                Tuple<Int32, Int32> tupSelectedMove = new Tuple<int, int>(intIdOperationMin1, intIdOperationMin2);
                //PairWiseMove(tupSelectedMove, cData);
                // Realiza el cambio, obtiene los resultados y lo deja cambiado
                MoveTest(tupSelectedMove, cData, cSchedule, false);
                cTlist.Add(intIdOperationMin1, intIdOperationMin2);
            }
            else
            {
                // No hay ninguna operacion tipo U o FP por lo que hay que coger una FN (forbidden non profitable)
                string strSelectedMove = "";
                if (hsMovesFN.Count == 1)
                {
                    strSelectedMove = hsMovesFN.ElementAt(0);
                }
                else
                {
                    Boolean blnEnBucle = true;
                    Tuple<Int32, Int32> tupLastInList = cTlist.GetLastInTaboo();
                    while (blnEnBucle)
                    {
                        Tuple<Int32, Int32> tupOut = cTlist.Add(tupLastInList.Item1, tupLastInList.Item2);
                        // Si esta vacio el tupOut toma valor -1
                        if (tupOut.Item1 > 0)
                        {
                            // Si no esta en taboo
                            if (!cTlist.Exist(tupOut.Item1, tupOut.Item2))
                            {
                                string strMove = tupOut.Item1 + "_" + tupOut.Item2;
                                //Si es un FN entonces lo selecciona y sale
                                if (hsMovesFN.Contains(strMove))
                                {
                                    strSelectedMove = strMove;
                                    blnEnBucle = false;
                                }
                            }
                        }
                    }
                    // Si no hay movimiento seleccionado hay un error
                    if (strSelectedMove == "")
                        new Exception("Error al seleccionar movimiento");
                    string[] strSplit = strSelectedMove.Split('_');
                    intIdOperationMin1 = Convert.ToInt32(strSplit[0]);
                    intIdOperationMin2 = Convert.ToInt32(strSplit[1]);
                    Tuple<Int32, Int32> tupSelectedMove = new Tuple<int, int>(intIdOperationMin1, intIdOperationMin2);
                    //PairWiseMove(tupSelectedMove, cData);
                    // Realiza el cambio, obtiene los resultados y lo deja cambiado
                    MoveTest(tupSelectedMove, cData, cSchedule, false);
                    cTlist.Add(intIdOperationMin1, intIdOperationMin2);
                }
            }
            if (dblMakespanMin < dblMakespanMinGlobal)
                dblMakespanMinGlobal = dblMakespanMin;
        }

        /// <summary>
        /// Genera los potenciales movimientos segun Nowicki & Smutnicki 1996 pag 800
        /// Si es el primer bloque selecciona las dos ultimas.
        /// Si es un bloque intermedio selecciona las dos primeras y las dos ultimas.
        /// Si es el ultimo bloque selecciona las dos primeras operaciones
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="cResultadoBellman"></param>
        /// <returns></returns>
        private List<Tuple<Int32, Int32>> MovesCandidates(clsDatosJobShop cData, clsDatosSchedule cSchedule)
        {
            List<Tuple<Int32, Int32>> lstResultsCandidates = new List<Tuple<int, Int32>>();
            List<Int32> lstBlock = new List<int>();
            Int32 intIdMachineOld = -1;
            Boolean blnFirst = true;
            for (Int32 intI = cSchedule.lstIdOperationInCriticalPath.Count() - 1; intI >= 0; intI--)
            {
                Int32 intIdOperation = cSchedule.lstIdOperationInCriticalPath[intI];
                Int32 intIdMachine = cData.dicIdOperationIdMachine[intIdOperation];
                // Cambio bloque
                if (intIdMachine != intIdMachineOld && intIdMachineOld != -1)
                {
                    // Si es el primer bloque
                    if (blnFirst)
                    {
                        blnFirst = false;
                        // Swap las two operations
                        if (lstBlock.Count > 1)
                        {
                            Int32 intIdOperation1 = lstBlock[lstBlock.Count - 2];
                            Int32 intIdOperation2 = lstBlock[lstBlock.Count - 1];
                            lstResultsCandidates.Add(new Tuple<Int32, Int32>(intIdOperation1, intIdOperation2));
                        }
                    }
                    else // Es una operacion del centro
                    {
                        if (lstBlock.Count > 1)
                        {
                            // Cambia los dos primeros
                            Int32 intIdOperation1 = lstBlock[0];
                            Int32 intIdOperation2 = lstBlock[1];
                            lstResultsCandidates.Add(new Tuple<Int32, Int32>(intIdOperation1, intIdOperation2));
                            // Si hay 3 o mas cambia los dos ultimos
                            if (lstBlock.Count > 2)
                            {
                                intIdOperation1 = lstBlock[lstBlock.Count - 2];
                                intIdOperation2 = lstBlock[lstBlock.Count - 1];
                                lstResultsCandidates.Add(new Tuple<Int32, Int32>(intIdOperation1, intIdOperation2));
                            }
                        }
                    }
                    lstBlock = new List<int>();
                }
                lstBlock.Add(intIdOperation);
                intIdMachineOld = intIdMachine;
            }
            // Comprueba si hay que cambiar lo del ultimo bloque cambia las dos primeras
            if (lstBlock.Count > 1)
            {
                Int32 intIdOperation1 = lstBlock[0];
                Int32 intIdOperation2 = lstBlock[1];
                lstResultsCandidates.Add(new Tuple<Int32, Int32>(intIdOperation1, intIdOperation2));
            }
            return lstResultsCandidates;
        }


        private void MoveTest(Tuple<Int32, Int32> tupMove, clsDatosJobShop cData, clsDatosSchedule cSchedule, Boolean blnVolverOriginal)
        {
            // Hace el cambio de operaciones
            PairWiseMove(tupMove, cData, cSchedule);
            // Llama a Bellman
            clsBellman cBellman = new clsBellman();
            cBellman.CalcularBellman(cData, cSchedule);
            cSchedule.tupMove = tupMove;
            // VUelve los cambios al original (ver si esto hace falta o no)
            if (blnVolverOriginal)
                PairWiseMove(new Tuple<int, int>(tupMove.Item2, tupMove.Item1), cData, cSchedule);
        }

        private void PairWiseMove(Tuple<Int32, Int32> tupMove, clsDatosJobShop cData, clsDatosSchedule cSchedule)
        {
            Int32 intIdMachine = cData.dicIdOperationIdMachine[tupMove.Item1];
            if (intIdMachine != cData.dicIdOperationIdMachine[tupMove.Item2])
                new Exception("Error deberia ser la misma maquina");
            // Cambia en lst
            Int32 intPos1 = cSchedule.dicIdOperationPositionInLstOperations[tupMove.Item1];
            Int32 intPos2 = cSchedule.dicIdOperationPositionInLstOperations[tupMove.Item2];
            cSchedule.dicIdMachineLstOperations[intIdMachine][intPos1] = tupMove.Item2;
            cSchedule.dicIdMachineLstOperations[intIdMachine][intPos2] = tupMove.Item1;
            cSchedule.dicIdOperationPositionInLstOperations[tupMove.Item1] = intPos2;
            cSchedule.dicIdOperationPositionInLstOperations[tupMove.Item2] = intPos1;
            // Realiza los cambios en diccionarios  Next en Machine        
            Int32 intNext2 = cSchedule.dicIdOperationIdNextInMachine[tupMove.Item2];
            Int32 intPrevious1 = cSchedule.dicIdOperationIdPreviousInMachine[tupMove.Item1];

            cSchedule.dicIdOperationIdNextInMachine[tupMove.Item1] = intNext2;
            cSchedule.dicIdOperationIdNextInMachine[tupMove.Item2] = tupMove.Item1;
            if (intPrevious1 > -1)
                cSchedule.dicIdOperationIdNextInMachine[intPrevious1] = tupMove.Item2;
            // Realiza los cambios en diccionarios  Previous en Machine
            cSchedule.dicIdOperationIdPreviousInMachine[tupMove.Item1] = tupMove.Item2;
            cSchedule.dicIdOperationIdPreviousInMachine[tupMove.Item2] = intPrevious1;
            if (intNext2 > -1)
                cSchedule.dicIdOperationIdPreviousInMachine[intNext2] = tupMove.Item1;
        }


    }
}
