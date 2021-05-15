using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    class clsBackTrackList
    {
        private Queue<clsDatosBackTrack> _queBackTrackList;
        private Int32 _intMaxBackTrackList;


        public clsBackTrackList(Int32 intMaxBackTrackList)
        {
            _queBackTrackList = new Queue<clsDatosBackTrack>();
            _intMaxBackTrackList = intMaxBackTrackList;
        }

        public void Add(clsDatosJobShop cData, clsDatosSchedule cSchedule, clsTabooList cTList, List<Tuple<Int32, Int32>> lstMoves, Tuple<Int32, Int32> tupLastSelectedMove, double dblMakespan)
        {
            clsDatosBackTrack cDatosBackTrack = new clsDatosBackTrack();
            cDatosBackTrack.cSchedule = clsObjectCopy.Clone<clsDatosSchedule>(cSchedule);
            // Quita el ultimo movimiento
            PairWiseMove(new Tuple<int, int>(tupLastSelectedMove.Item2, tupLastSelectedMove.Item1), cData, cDatosBackTrack.cSchedule);
            // Copia los movimientos 
            cDatosBackTrack.tupMove = tupLastSelectedMove;
            cDatosBackTrack.dblMakespan = dblMakespan;
            cDatosBackTrack.lstMoves = new List<Tuple<int, int>>();
            foreach (Tuple<Int32, Int32> tupMoves in lstMoves)
            {
                if (!tupMoves.Equals(tupLastSelectedMove))
                {
                    cDatosBackTrack.lstMoves.Add(tupMoves);
                }
            }
            // Copia el taboolist
            cDatosBackTrack.cTlist =clsObjectCopy .Clone <clsTabooList  > ( cTList);
            // Lo encola
            if (_queBackTrackList.Count >= _intMaxBackTrackList)
                _queBackTrackList.Dequeue();
            _queBackTrackList.Enqueue(cDatosBackTrack);
        }

        public clsDatosBackTrack Get()
        {
            return _queBackTrackList.Dequeue();

        }

        public Int32 Count()
        {
            return _queBackTrackList.Count;

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
