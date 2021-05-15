using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    class clsN1
    {
        private Random _rnd = new Random(75);

        /// <summary>
        /// Realiza el movimiento N1 para ello se toma uno de lo bloques
        /// criticos al azar y despues se toma dentro de ese bloque
        /// se toma una operacoin al azar y se intercambia con la siguiente o anterior.
        /// Sea U  la operacion seleccionada al azar dentro de 
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="cSchedule"></param>
        /// <returns></returns>
        public clsDatosMovimiento N1SeleccionarYRealizarMovimiento(clsDatosJobShop cData, clsDatosSchedule cSchedule)
        {
            // Selecciona el bucle critico
            Int32 intCriticalBlockIndex = _rnd.Next(0, cSchedule.lstCriticalBlocks.Count - 1);
            if (cSchedule.lstCriticalBlocks[intCriticalBlockIndex].Count < 2)
                new Exception("Error numero en bloque critico menor que 2");
            // Selecciona la operacion V
            Int32 intIdOperacionVIndex = _rnd.Next(0, cSchedule.lstCriticalBlocks[intCriticalBlockIndex].Count - 1);
            // Si la operacion U es la primera del bloque seleccionar la siguiente como v
            Int32 intIdOperacionUIndex = 1;
            if (intIdOperacionVIndex == 0)
                intIdOperacionUIndex = 1;
            else if (intIdOperacionVIndex == cSchedule.lstCriticalBlocks[intCriticalBlockIndex].Count - 1)
            {
                // Si la operacion V es la ultima del bloque toma la U como la V y la U como la siguiente
                intIdOperacionUIndex = intIdOperacionVIndex ;
                intIdOperacionVIndex = intIdOperacionUIndex - 1;
            }
            else
            {
                // Si la operacion V es intermedia pone la U aleatoriamente delante o detras
                if (_rnd.NextDouble() > 0.5)
                    intIdOperacionUIndex = intIdOperacionVIndex+1;
                else
                {
                    intIdOperacionUIndex = intIdOperacionVIndex;
                    intIdOperacionVIndex = intIdOperacionUIndex - 1;
                }
            }
            // Realiza los cambios
            Int32 intIdU = cSchedule.lstCriticalBlocks[intCriticalBlockIndex][intIdOperacionUIndex];
            Int32 intIdV = cSchedule.lstCriticalBlocks[intCriticalBlockIndex][intIdOperacionVIndex];
            //Sustituye una operacion por la otra la U va antes que la V
            clsDatosMovimiento cMovimiento = new clsDatosMovimiento();
            cMovimiento.bleEsN6 = false;
            cMovimiento.intIdOperacionU = intIdU;
            cMovimiento.intIdOperacionAnteriorU = cSchedule.dicIdOperationIdPreviousInMachine[intIdU];
            cMovimiento.intIdOperacionPosteriorU = cSchedule.dicIdOperationIdNextInMachine[intIdU];
            cMovimiento.intIdOperacionV = intIdV;
            cMovimiento.intIdOperacionAnteriorV = cSchedule.dicIdOperationIdPreviousInMachine[intIdV];
            cMovimiento.intIdOperacionPosteriorV = cSchedule.dicIdOperationIdNextInMachine[intIdV];
            // Realiza el cambio de la primera y ultima en maquina
            Int32 intIdMachine = cData.dicIdOperationIdMachine[intIdU];
            if (cMovimiento.intIdOperacionPosteriorV == -1)
                cSchedule.dicIdMachineIdOperationLast[intIdMachine] = intIdU;
            if (cMovimiento.intIdOperacionAnteriorU == -1)
                cSchedule.dicIdMachineIdOperationFirst[intIdMachine] = cMovimiento.intIdOperacionPosteriorU;

            // Realiza el cambio en AU y V
            if (cMovimiento.intIdOperacionAnteriorU > -1)
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionAnteriorU] = cMovimiento.intIdOperacionV;
            cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionV] = cMovimiento.intIdOperacionAnteriorU;

            if (cMovimiento.intIdOperacionPosteriorU == cMovimiento.intIdOperacionV && cMovimiento.intIdOperacionAnteriorV == cMovimiento.intIdOperacionU)
            {
                // Realiza el cambio en PU y V
                cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionU] = cMovimiento.intIdOperacionV;
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionV] = cMovimiento.intIdOperacionU;
            }
            else
            {
                if (cMovimiento.intIdOperacionPosteriorU == cMovimiento.intIdOperacionAnteriorV)
                {

                }
                // Realiza el cambio en PU y V
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionV] = cMovimiento.intIdOperacionPosteriorU;
                if (cMovimiento.intIdOperacionPosteriorU > -1)
                    cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionPosteriorU] = cMovimiento.intIdOperacionV;
                // Realiza el cambio en AV y U
                if (cMovimiento.intIdOperacionAnteriorV > -1)
                    cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionAnteriorV] = cMovimiento.intIdOperacionU;
                cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionU] = cMovimiento.intIdOperacionAnteriorV;
            }
            // Realiza el cambio PV y U
            cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionU] = cMovimiento.intIdOperacionPosteriorV;
            if (cMovimiento.intIdOperacionPosteriorV > -1)
                cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionPosteriorV] = cMovimiento.intIdOperacionU;
            return cMovimiento;

        }

    }
}
