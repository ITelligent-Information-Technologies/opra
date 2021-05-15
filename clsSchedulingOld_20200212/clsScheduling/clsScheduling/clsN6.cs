using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    class clsDatosCambio
    {
        public Int32 intIdOperacionU = -1; //Operacion anterior para el cambio
        public Int32 intIdOperacionV = -1;//Opearcion posetrior para el cambio
        public double dblMakespanEstimado = -1; // Makespan estimado si se realiza el cambio
        public Boolean blnEsForward; // Si el cambio es forward o backward
        public string strPathUaV; // Guarda e un string el path de U a V antes del cambio
        public Int32 intPosicionMauquinaU; // Guarda la posicion en maquina de la operacion U antes del cambio (0,1,...)
    }


    /// <summary>
    /// Esta clase realiza el movimiento del vencindario N6
    /// Basicamente dado un bloque que comienza por una operacion
    /// U y termina en una operacion V. Se pueden realizar dos
    /// cambios:
    /// Forward: La operacion U pasa a despues de la V
    /// Backward: La operacion V se coloca antes de la U
    /// Esta clase estima el makespan que se producira si se
    /// realiza alguno de estos cambios y selecciona aquel
    /// cambio que sin ser tabu reduzca mas el maskepan.
    /// </summary>
    class clsN6
    {
        private Random _rnd = new Random(100);
        /// <summary>
        /// Esta funcion dado los bloques criticos
        /// selecciona aquel bloque que si se cambian
        /// la primera y ultima operacion mas reduce
        /// el makespan. Además deben ser movimientos
        /// que no esten en la lista tabu
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="cSchedule"></param>
        /// <param name="dicIdOperacionTiempoHastaFin"></param>
        /// <returns></returns>
        public clsDatosCambio SeleccionarMovimiento(clsDatosParametros cParametros, string strFirma, clsTabuList cTabuList, clsDatosJobShop cData, clsDatosSchedule cSchedule, Dictionary<Int32, double> dicIdOperacionTiempoHastaFin, double dblMakespanMinGlobal)
        {
            clsDatosCambio cCambioMin = new clsDatosCambio();
            cCambioMin.dblMakespanEstimado = double.MaxValue;
            Int32 intIndexMin = -1;
            Int32 intIdOperacionV = -1;
            Int32 intIdOperacionU = -1;
            List<clsDatosCambio> lstForbiddenNonProfitable = new List<clsDatosCambio>();// movimientos prohibidos y que no reducen el makespan
            List<clsDatosCambio> lstProfitable = new List<clsDatosCambio>();// movimientos que pueden mejorar el makespan
            List<clsDatosCambio> lstNonTabu = new List<clsDatosCambio>();// movimientos que pueden mejorar el makespan
            StringBuilder sbForward = new StringBuilder(); // Guarda las operaciones del path del cambio forward
            StringBuilder sbBackward = new StringBuilder(); // Guarda las operaciones del path del cambio backward
            //Va comprobando el makespan de cada bloque
            for (Int32 intIndex = 0; intIndex < cSchedule.lstCriticalBlocks.Count; intIndex++)
            {
                intIdOperacionV = cSchedule.lstCriticalBlocks[intIndex][0];
                sbForward = new StringBuilder();
                sbForward.Append(intIdOperacionV);
                // Forward Va probando a intercambiar cada operacion U con la V (prueba todas las del bloque), la V es la ultima operacoin del bloque
                for (Int32 intIndexU = 1; intIndexU < cSchedule.lstCriticalBlocks[intIndex].Count; intIndexU++)
                {
                    intIdOperacionU = cSchedule.lstCriticalBlocks[intIndex][intIndexU];
                    sbForward.Append("_" + intIdOperacionU);
                    // Chequean si se dan las condiciiones para un bucle
                    if (!EsBucleForward(cData, intIdOperacionU, intIdOperacionV, dicIdOperacionTiempoHastaFin))
                    {
                        double dblMakespan = 0;
                        dblMakespan = EstimarMakespanCambioForward(cSchedule.lstCriticalBlocks[intIndex], intIndexU, cData, cSchedule, dicIdOperacionTiempoHastaFin);
                        clsDatosCambio cCambio = new clsDatosCambio();
                        cCambio.blnEsForward = true;
                        cCambio.intIdOperacionU = intIdOperacionU;
                        cCambio.intIdOperacionV = intIdOperacionV;
                        cCambio.dblMakespanEstimado = dblMakespan;
                        cCambio.strPathUaV = sbForward.ToString();
                        // si es tabu
                        if (cTabuList.Exist(cSchedule.dblMakespan, cCambio))
                        {
                            // Aunque sea tabu se espera que mejore el makespan por lo que se considera                            
                            if (dblMakespan < dblMakespanMinGlobal)
                            {

                                lstProfitable.Add(cCambio);
                                // esto guarda el minimo hasta el momento
                                if (dblMakespan < cCambioMin.dblMakespanEstimado)
                                {
                                    cCambioMin.dblMakespanEstimado = dblMakespan;
                                    intIndexMin = intIndex;
                                    cCambioMin.intIdOperacionU = intIdOperacionU;
                                    cCambioMin.intIdOperacionV = intIdOperacionV;
                                    cCambioMin.blnEsForward = true;
                                    cCambioMin.strPathUaV = sbForward.ToString();
                                }
                            }
                            else// No mejora el makespan
                                lstForbiddenNonProfitable.Add(cCambio);
                        }
                        else
                        {
                            lstNonTabu.Add(cCambio);
                            // Esto es para ir guardando el minimo
                            if (dblMakespan < cCambioMin.dblMakespanEstimado)
                            {
                                cCambioMin.dblMakespanEstimado = dblMakespan;
                                intIndexMin = intIndex;
                                cCambioMin.intIdOperacionU = intIdOperacionU;
                                cCambioMin.intIdOperacionV = intIdOperacionV;
                                cCambioMin.blnEsForward = true;
                                cCambioMin.strPathUaV = sbForward.ToString();
                            }
                        }
                    }
                }
                // Solo si hay mas de dos operacoines se hace el backward si hay una sola es lo mismo que el forward
                if (cSchedule.lstCriticalBlocks[intIndex].Count > 2)
                {
                    intIdOperacionU = cSchedule.lstCriticalBlocks[intIndex][cSchedule.lstCriticalBlocks[intIndex].Count - 1];
                    sbBackward = new StringBuilder();
                    sbBackward.Append(intIdOperacionU);
                    // Backward Va probando a intercambiar cada operacion V con la U  (prueba todas las del bloque), la U es la primera operacoin del bloque
                    // Ojo! la U,V (es decir una al lado de la otra) ya ha sido probada en el bloque anterior, por tanto no hay que ponerla
                    for (Int32 intIndexV = cSchedule.lstCriticalBlocks[intIndex].Count - 2; intIndexV >= 0; intIndexV--)
                    {
                        double dblMakespan = 0;
                        dblMakespan = EstimarMakespanCambioBackward(cSchedule.lstCriticalBlocks[intIndex], cData, intIndexV, cSchedule, dicIdOperacionTiempoHastaFin);
                        intIdOperacionV = cSchedule.lstCriticalBlocks[intIndex][intIndexV];
                        sbBackward.Append("_" + intIdOperacionV);
                        if (!EsBucleBackward(cData, intIdOperacionU, intIdOperacionV, cSchedule))
                        {
                            // Comprueba si el cambio backward es posible
                            clsDatosCambio cCambio = new clsDatosCambio();
                            cCambio.blnEsForward = false;
                            cCambio.intIdOperacionU = intIdOperacionU;
                            cCambio.intIdOperacionV = intIdOperacionV;
                            cCambio.dblMakespanEstimado = dblMakespan;
                            cCambio.strPathUaV = sbBackward.ToString();
                            if (cTabuList.Exist(cSchedule.dblMakespan, cCambio))
                            {
                                // Prueba cambiar forward
                                if (dblMakespan < dblMakespanMinGlobal)
                                {
                                    lstProfitable.Add(cCambio);
                                    //Esto guarda el minimo
                                    if (dblMakespan < cCambioMin.dblMakespanEstimado)
                                    {
                                        cCambioMin.dblMakespanEstimado = dblMakespan;
                                        intIndexMin = intIndex;
                                        cCambioMin.intIdOperacionU = intIdOperacionU;
                                        cCambioMin.intIdOperacionV = intIdOperacionV;
                                        cCambioMin.blnEsForward = false;
                                        cCambioMin.strPathUaV = sbBackward.ToString();
                                    }
                                }
                                else
                                    lstForbiddenNonProfitable.Add(cCambio);
                            }
                            else
                            {
                                lstNonTabu.Add(cCambio);
                                if (dblMakespan < cCambioMin.dblMakespanEstimado)
                                {
                                    cCambioMin.dblMakespanEstimado = dblMakespan;
                                    intIndexMin = intIndex;
                                    cCambioMin.intIdOperacionU = intIdOperacionU;
                                    cCambioMin.intIdOperacionV = intIdOperacionV;
                                    cCambioMin.blnEsForward = false;
                                    cCambioMin.strPathUaV = sbBackward.ToString();
                                }
                            }
                        }
                    }
                }
            }
            // Si la unica opcion son Forbidden Non Profitable selecciona uno de ellos
            if (cCambioMin.intIdOperacionU < 0 && lstForbiddenNonProfitable.Count > 0 && cParametros .blnN6PermitirForbiddenNonProfitable )
            {
                return SeleccionarForbiddenNonProfitable(lstForbiddenNonProfitable);
            }
            return cCambioMin;
            //return SeleccionarForbiddenNonProfitable(lstProfitable);// cCambioMin;
        }

        /// <summary>
        /// En caso de que la unica opcion sea un movimiento Forbidden Non Profitable
        /// esta funcion selecciona cual de ellos realizar.
        /// </summary>
        /// <param name="lstForbiddenNonProfitable"></param>
        /// <returns></returns>
        private clsDatosCambio SeleccionarForbiddenNonProfitable(List<clsDatosCambio> lstForbiddenNonProfitable)
        {
            //Selecciona aleatoriamente sin quitar del tabu
            Int32 intIndex = 0;
            if (lstForbiddenNonProfitable.Count > 1)
                intIndex = _rnd.Next(0, lstForbiddenNonProfitable.Count - 1);
            return lstForbiddenNonProfitable[intIndex];

        }

        /// <summary>
        /// Sea un cambio Forward de U y V y sea STU el siguietne en trabajo 
        /// a U. Si se cumple L(V,n) > L(STU, n) No puede haber bucle
        /// Siendo L(V,N) el tiempo desde el final a V.
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="intIdOperacionU"></param>
        /// <param name="intIdOperacionV"></param>
        /// <param name=""></param>
        /// <param name="dicIdOperacionTiempoHastaFin"></param>
        /// <returns></returns>
        private Boolean EsBucleForward(clsDatosJobShop cData, Int32 intIdOperacionU, Int32 intIdOperacionV, Dictionary<Int32, double> dicIdOperacionTiempoHastaFin)
        {
            // Chequea que L(V,n)>= L(STU,n) no puede haber bucle si no se cumple si puede haberlo
            Int32 intSTU = cData.dicIdOperationIdNextInJob[intIdOperacionU];
            if (intSTU > -1 && dicIdOperacionTiempoHastaFin[intIdOperacionV] < dicIdOperacionTiempoHastaFin[intSTU])
                return true;
            return false;
        }

        /// <summary>
        /// Sea un cambio Backward de U y V y sea ATV el Anterior a V en trabajo 
        /// Si se cumple L(0,U)+ TP(U) > L(0,ATV) + TP(ATV) No puede haber bucle
        /// Siendo L(0,U) el tiempo de comienzo de U y siendo TP(U) tiempo de 
        /// procesamiento de U.
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="intIdOperacionU"></param>
        /// <param name="intIdOperacionV"></param>
        /// <param name=""></param>
        /// <param name="dicIdOperacionTiempoHastaFin"></param>
        /// <returns></returns>
        private Boolean EsBucleBackward(clsDatosJobShop cData, Int32 intIdOperacionU, Int32 intIdOperacionV, clsDatosSchedule cSchedule)
        {
            // Chequea que  no puede haber bucle si no se cumple si puede haberlo
            Int32 intATV = cData.dicIdOperationIdPreviousInJob[intIdOperacionV];
            if (intATV > -1 && (cSchedule.dicIdOperationEndTime[intIdOperacionU] < cSchedule.dicIdOperationEndTime[intATV]))
                return true;
            return false;
        }

        /// <summary>
        /// Esta funcion realiza el cambio forward del bloque seleccionado
        /// para ello sea U la primera operacion del bloque y V la ultima
        /// sea la siguiente secuencia en maquina:
        /// AU->U->SU...AV->V->SV, realiza los cambios para obtener
        /// AU->SU...AV->V->U->SU
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="lstBlock"></param>
        /// <param name="cSchedule"></param>
        /// <returns></returns>
        public clsDatosMovimiento RealizarMovimientoForward(clsDatosJobShop cData, Int32 intIdOperacionU, Int32 intIdOperacionV, clsDatosSchedule cSchedule)
        {
            clsDatosMovimiento cMovimiento = new clsDatosMovimiento();
            cMovimiento.blnEsForward = true;
            cMovimiento.bleEsN6 = true;
            //Primera operacion del bloque esta operacion pasa al final del bloque
            Int32 intIdU = intIdOperacionU;
            cMovimiento.intIdOperacionU = intIdU;
            // Ultima operacion del bloque
            Int32 intIdV = intIdOperacionV;
            cMovimiento.intIdOperacionV = intIdV;
            // Anterior en maquina a la U y posterior
            cMovimiento.intIdOperacionAnteriorU = cSchedule.dicIdOperationIdPreviousInMachine[intIdU];
            cMovimiento.intIdOperacionPosteriorU = cSchedule.dicIdOperationIdNextInMachine[intIdU];
            // Posterior en maquina a la V y posterior
            cMovimiento.intIdOperacionAnteriorV = cSchedule.dicIdOperationIdPreviousInMachine[intIdV];
            cMovimiento.intIdOperacionPosteriorV = cSchedule.dicIdOperationIdNextInMachine[intIdV];
            // Realiza el cambio de la primera y ultima en maquina
            Int32 intIdMachine = cData.dicIdOperationIdMachine[intIdU];
            if (cMovimiento.intIdOperacionPosteriorV == -1)
                cSchedule.dicIdMachineIdOperationLast[intIdMachine] = intIdU;
            if (cMovimiento.intIdOperacionAnteriorU == -1)
                cSchedule.dicIdMachineIdOperationFirst[intIdMachine] = cMovimiento.intIdOperacionPosteriorU;
            // V->U
            cSchedule.dicIdOperationIdNextInMachine[intIdV] = intIdU;
            // V<-U
            cSchedule.dicIdOperationIdPreviousInMachine[intIdU] = intIdV;
            // U->SV
            cSchedule.dicIdOperationIdNextInMachine[intIdU] = cMovimiento.intIdOperacionPosteriorV;
            // U<-SV
            if (cMovimiento.intIdOperacionPosteriorV > -1)
                cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionPosteriorV] = intIdU;
            // Si AU->U->V-SV (la V antes del cambio va despues de la U)
            if (cMovimiento.intIdOperacionU == cMovimiento.intIdOperacionAnteriorV)
            {
                // AU->V
                if (cMovimiento.intIdOperacionAnteriorU > -1)
                    cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionAnteriorU] = cMovimiento.intIdOperacionV;
                // AU<-V
                cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionV] = cMovimiento.intIdOperacionAnteriorU;
            }
            else // Otros casos
            {
                // AU->SU
                if (cMovimiento.intIdOperacionAnteriorU > -1)
                    cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionAnteriorU] = cMovimiento.intIdOperacionPosteriorU;
                // AU<-SU
                cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionPosteriorU] = cMovimiento.intIdOperacionAnteriorU;

            }

            return cMovimiento;
        }

        /// <summary>
        /// Esta funcion realiza el cambio backward del bloque seleccionado
        /// para ello sea U la primera operacion del bloque y V la ultima
        /// sea la siguiente secuencia en maquina:
        /// AU->U->SU...AV->V->SV, realiza los cambios para obtener
        /// AU->V->U->SU...AV->SU
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="lstBlock"></param>
        /// <param name="cSchedule"></param>
        /// <returns></returns>
        public clsDatosMovimiento RealizarMovimientoBackward(clsDatosJobShop cData, Int32 intIdOperacionU, Int32 intIdOperacionV, clsDatosSchedule cSchedule)
        {
            clsDatosMovimiento cMovimiento = new clsDatosMovimiento();
            cMovimiento.blnEsForward = false;
            cMovimiento.bleEsN6 = true;
            //Primera operacion del bloque 
            Int32 intIdU = intIdOperacionU;
            cMovimiento.intIdOperacionU = intIdU;
            // Ultima operacion del bloque
            Int32 intIdV = intIdOperacionV;
            cMovimiento.intIdOperacionV = intIdV;
            // Anterior en maquina a la U y posterior
            cMovimiento.intIdOperacionAnteriorU = cSchedule.dicIdOperationIdPreviousInMachine[intIdU];
            cMovimiento.intIdOperacionPosteriorU = cSchedule.dicIdOperationIdNextInMachine[intIdU];
            // Posterior en maquina a la V y posterior
            cMovimiento.intIdOperacionAnteriorV = cSchedule.dicIdOperationIdPreviousInMachine[intIdV];
            cMovimiento.intIdOperacionPosteriorV = cSchedule.dicIdOperationIdNextInMachine[intIdV];


            // Realiza el cambio de la primera y ultima en maquina
            Int32 intIdMachine = cData.dicIdOperationIdMachine[intIdU];
            if (cMovimiento.intIdOperacionPosteriorV == -1)
                cSchedule.dicIdMachineIdOperationLast[intIdMachine] = cMovimiento.intIdOperacionAnteriorV;
            if (cMovimiento.intIdOperacionAnteriorU == -1)
                cSchedule.dicIdMachineIdOperationFirst[intIdMachine] = cMovimiento.intIdOperacionV;

            // Realiza AU->V
            if (cMovimiento.intIdOperacionAnteriorU > -1)
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionAnteriorU] = cMovimiento.intIdOperacionV;
            // Realiza AU<-V
            cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionV] = cMovimiento.intIdOperacionAnteriorU;
            // Realiza V->U
            cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionV] = cMovimiento.intIdOperacionU;
            // Realiza V<-U
            cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionU] = cMovimiento.intIdOperacionV;
            // Si AU->U->SU=AV->V->SV
            if (cMovimiento.intIdOperacionPosteriorU == cMovimiento.intIdOperacionAnteriorV)
            {
                // Realiza AV->SV
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionAnteriorV] = cMovimiento.intIdOperacionPosteriorV;
                // Realiza AV<-SV
                if (cMovimiento.intIdOperacionPosteriorV > -1)
                    cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionPosteriorV] = cMovimiento.intIdOperacionAnteriorV;
            }
            else if (cMovimiento.intIdOperacionU == cMovimiento.intIdOperacionAnteriorV)
            {
                // SI AU->U->V->SV
                // Realiza U->SV
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionU] = cMovimiento.intIdOperacionPosteriorV;
                // Realiza U<-SV
                if (cMovimiento.intIdOperacionPosteriorV > -1)
                    cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionPosteriorV] = cMovimiento.intIdOperacionU;
            }
            else // EL resto AU->U->SU...AV->V->SV
            {
                // Realiza AV->SV
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionAnteriorV] = cMovimiento.intIdOperacionPosteriorV;
                // Realiza AV<-SV
                if (cMovimiento.intIdOperacionPosteriorV > -1)
                    cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionPosteriorV] = cMovimiento.intIdOperacionAnteriorV;
            }

            return cMovimiento;
        }

        /// <summary>
        /// Sean U->L1->...Lk->V operaciones consecutivas
        /// en una maquina y que ademas estan en un camino
        /// critico. 
        /// Esta funcion estima el MAKESPAN que se obtendria
        /// si se realiza el siguiente cambio:
        /// L1->...->Lk->V->U
        /// </summary>
        /// <param name="lstBlock"></param>
        /// <param name="intIndexInBlockOperationU"></param>
        /// <param name="cData"></param>
        /// <param name="cSchedule"></param>
        /// <param name="dicIdOperacionTiempoHastaFin"></param>
        /// <returns></returns>
        private double EstimarMakespanCambioForward(List<Int32> lstBlock, Int32 intIndexInBlockOperationU, clsDatosJobShop cData, clsDatosSchedule cSchedule, Dictionary<Int32, double> dicIdOperacionTiempoHastaFin)
        {
            Dictionary<Int32, double> dicIdOperacionInicio = new Dictionary<int, double>();
            Dictionary<Int32, double> dicIdOperacionFin = new Dictionary<int, double>();
            Dictionary<Int32, double> dicIdOperacionMakespan = new Dictionary<int, double>();
            double dblMakespanMax = double.MinValue;
            Int32 intIdAnteriorTrabajo = 0;
            double dblTrabajoIni = 0;
            double dblMachineIni = 0;
            double dblMakespan = 0;
            // Obtiene la operacion U y la V y comprueba que no sean la misma
            Int32 intIdU = lstBlock[intIndexInBlockOperationU];
            Int32 intIdV = lstBlock[0];
            if (intIdU == intIdV)
                new Exception("Error operacion U y V son iguales");
            // Calcula el tiempo de U a End una vez que U pase al final
            Int32 intIdSiguienteMaquina = cSchedule.dicIdOperationIdNextInMachine[intIdV]; // Siguiente a U una vez realizado el cambio (es la siguiente a V)
            Int32 intIdSiguienteTrabajo = cData.dicIdOperationIdNextInJob[intIdU]; //Siguiente a U que es la misma que antes del cambio
            double dblTrabajoEnd = cData.dicIdOperationTime[intIdU];
            if (intIdSiguienteTrabajo > -1)
                dblTrabajoEnd = dicIdOperacionTiempoHastaFin[intIdSiguienteTrabajo] + cData.dicIdOperationTime[intIdU];
            else { }
            double dblMachineEnd = cData.dicIdOperationTime[intIdU];
            if (intIdSiguienteMaquina > -1)
                dblMachineEnd = dicIdOperacionTiempoHastaFin[intIdSiguienteMaquina] + cData.dicIdOperationTime[intIdU];
            else { }
            dicIdOperacionFin.Add(intIdU, Math.Max(dblTrabajoEnd, dblMachineEnd));
            intIdSiguienteMaquina = intIdU;
            // Operacion anterior a la primera en el bloque
            Int32 intIdAnteriorMaquina = cSchedule.dicIdOperationIdPreviousInMachine[intIdU];
            if (intIdAnteriorMaquina > -1)
                dicIdOperacionInicio.Add(intIdAnteriorMaquina, cSchedule.dicIdOperationStartTime[intIdAnteriorMaquina]);
            else
            { }

            // Obtiene el resto de las operaciones tanto 0 a I, como I a N
            for (Int32 intIni = intIndexInBlockOperationU - 1; intIni >= 0; intIni--)
            {
                //--- Obtiene de 0,IdOpearcion
                intIdAnteriorTrabajo = cData.dicIdOperationIdPreviousInJob[lstBlock[intIni]];
                dblTrabajoIni = 0;
                if (intIdAnteriorTrabajo > -1)
                    dblTrabajoIni = cSchedule.dicIdOperationEndTime[intIdAnteriorTrabajo];
                else { }
                dblMachineIni = 0;
                if (intIdAnteriorMaquina > -1)
                    dblMachineIni = dicIdOperacionInicio[intIdAnteriorMaquina] + cData.dicIdOperationTime[intIdAnteriorMaquina];
                else { }
                dicIdOperacionInicio.Add(lstBlock[intIni], Math.Max(dblTrabajoIni, dblMachineIni));
                intIdAnteriorMaquina = lstBlock[intIni];
                //--- Fin 0, idoperacion

                //--- Obtiene de idOperacion a N.
                // Indice desde el final al principio                
                Int32 intFin = intIndexInBlockOperationU - 1 - intIni;
                dblMachineEnd = cData.dicIdOperationTime[lstBlock[intFin]];
                if (intIdSiguienteMaquina > -1)
                    dblMachineEnd = dicIdOperacionFin[intIdSiguienteMaquina] + cData.dicIdOperationTime[lstBlock[intFin]];
                else { }
                dblTrabajoEnd = cData.dicIdOperationTime[lstBlock[intFin]];
                intIdSiguienteTrabajo = cData.dicIdOperationIdNextInJob[lstBlock[intFin]];
                if (intIdSiguienteTrabajo > -1)


                    dblTrabajoEnd = dicIdOperacionTiempoHastaFin[intIdSiguienteTrabajo] + cData.dicIdOperationTime[lstBlock[intFin]];
                else { }
                dicIdOperacionFin.Add(lstBlock[intFin], Math.Max(dblTrabajoEnd, dblMachineEnd));
                intIdSiguienteMaquina = lstBlock[intFin];
                //--- Fin idOpearcion to N

                // Obtiene el makespan maximo
                dblMakespan = double.MaxValue;
                if (dicIdOperacionInicio.ContainsKey(lstBlock[intFin]) && dicIdOperacionFin.ContainsKey(lstBlock[intFin]))
                {
                    dblMakespan = dicIdOperacionInicio[lstBlock[intFin]] + dicIdOperacionFin[lstBlock[intFin]];
                    dicIdOperacionMakespan.Add(lstBlock[intFin], dblMakespan);
                    if (dblMakespanMax < dblMakespan)
                        dblMakespanMax = dblMakespan;
                }
                if (dicIdOperacionInicio.ContainsKey(lstBlock[intIni]) && dicIdOperacionFin.ContainsKey(lstBlock[intIni]) && !dicIdOperacionMakespan.ContainsKey(lstBlock[intIni]))
                {
                    dblMakespan = dicIdOperacionInicio[lstBlock[intIni]] + dicIdOperacionFin[lstBlock[intIni]];
                    dicIdOperacionMakespan.Add(lstBlock[intIni], dblMakespan);
                    if (dblMakespanMax < dblMakespan)
                        dblMakespanMax = dblMakespan;
                }
            }
            // Calcula el tiempo ini para la operacion U (la primera del bloque que ahora es la ultima)
            intIdAnteriorTrabajo = cData.dicIdOperationIdPreviousInJob[intIdU];
            dblTrabajoIni = 0;
            if (intIdAnteriorTrabajo > -1)
                dblTrabajoIni = cSchedule.dicIdOperationEndTime[intIdAnteriorTrabajo];
            else { }
            dblMachineIni = 0;
            if (intIdAnteriorMaquina > -1)
                dblMachineIni = dicIdOperacionInicio[intIdAnteriorMaquina] + cData.dicIdOperationTime[intIdAnteriorMaquina];
            else { }
            dicIdOperacionInicio.Add(intIdU, Math.Max(dblTrabajoIni, dblMachineIni));
            dblMakespan = dicIdOperacionInicio[intIdU] + dicIdOperacionFin[intIdU];
            dicIdOperacionMakespan.Add(intIdU, dblMakespan);
            if (dblMakespanMax < dblMakespan)
                dblMakespanMax = dblMakespan;
            // Devuelve el makespan minimo
            return dblMakespanMax;


        }



        /// <summary>
        /// Sean U->L1->...Lk->V operaciones consecutivas
        /// en una maquina y que ademas estan en un camino
        /// critico. 
        /// Esta funcion estima el MAKESPAN que se obtendria
        /// si se realiza el siguiente cambio:
        /// V->U->L1...->Lk
        /// </summary>
        /// <param name="lstBlock"></param>
        /// <param name="cData"></param>
        /// <param name="cSchedule"></param>
        /// <param name="dicIdOperacionTiempoHastaFin"></param>
        private double EstimarMakespanCambioBackward(List<Int32> lstBlock, clsDatosJobShop cData, Int32 intIndexInBlockOperationV, clsDatosSchedule cSchedule, Dictionary<Int32, double> dicIdOperacionTiempoHastaFin)
        {
            Dictionary<Int32, double> dicIdOperacionInicio = new Dictionary<int, double>();
            Dictionary<Int32, double> dicIdOperacionFin = new Dictionary<int, double>();
            Dictionary<Int32, double> dicIdOperacionMakespan = new Dictionary<int, double>();
            double dblMakespanMax = double.MinValue;
            double dblTrabajoIni = 0;
            double dblMachineIni = 0;
            double dblMakespan = 0;
            double dblTrabajoFin = 0;
            double dblMachineFin = 0;
            Int32 intIdSiguienteTrabajo = 0;
            Int32 intIdU = lstBlock[lstBlock.Count - 1];
            Int32 intIdV = lstBlock[intIndexInBlockOperationV];
            // Calcula el tiempo de 0 a V
            Int32 intIdAnteriorMaquina = cSchedule.dicIdOperationIdPreviousInMachine[intIdU]; // Anterior a U por maquina (depues del cambio la anterior a V)
            Int32 intIdAnteriorTrabajo = cData.dicIdOperationIdPreviousInJob[intIdV]; //Anterior por trabajo a V
            if (intIdAnteriorMaquina > -1)
                dblMachineIni = cSchedule.dicIdOperationEndTime[intIdAnteriorMaquina];
            if (intIdAnteriorTrabajo > -1)
                dblTrabajoIni = cSchedule.dicIdOperationEndTime[intIdAnteriorTrabajo];
            dicIdOperacionInicio.Add(intIdV, Math.Max(dblTrabajoIni, dblMachineIni));
            intIdAnteriorMaquina = intIdV;
            Int32 intIdSiguienteMaquina = cSchedule.dicIdOperationIdNextInMachine[intIdV];
            if (intIdSiguienteMaquina > -1)
                dicIdOperacionFin.Add(intIdSiguienteMaquina, dicIdOperacionTiempoHastaFin[intIdSiguienteMaquina]);
            // Obtiene el resto de las operaciones tanto 0 a I, como I a N
            for (Int32 intIni = lstBlock.Count - 1; intIni >= intIndexInBlockOperationV + 1; intIni--)
            {
                //--- Obtiene de 0,IdOpearcion
                intIdAnteriorTrabajo = cData.dicIdOperationIdPreviousInJob[lstBlock[intIni]];
                dblTrabajoIni = 0;
                if (intIdAnteriorTrabajo > -1)
                    dblTrabajoIni = cSchedule.dicIdOperationEndTime[intIdAnteriorTrabajo];
                dblMachineIni = 0;
                if (intIdAnteriorMaquina > -1)
                    dblMachineIni = dicIdOperacionInicio[intIdAnteriorMaquina] + cData.dicIdOperationTime[intIdAnteriorMaquina];
                dicIdOperacionInicio.Add(lstBlock[intIni], Math.Max(dblTrabajoIni, dblMachineIni));
                intIdAnteriorMaquina = lstBlock[intIni];
                //--- Fin 0, idoperacion

                //--- Obtiene de idOperacion a N.
                // Indice desde el final al principio                
                Int32 intFin = lstBlock.Count - intIni + intIndexInBlockOperationV;
                dblMachineFin = cData.dicIdOperationTime[lstBlock[intFin]];
                if (intIdSiguienteMaquina > -1)
                    dblMachineFin = dicIdOperacionFin[intIdSiguienteMaquina] + cData.dicIdOperationTime[lstBlock[intFin]];
                double dblTrabajoEnd = cData.dicIdOperationTime[lstBlock[intFin]];
                intIdSiguienteTrabajo = cData.dicIdOperationIdNextInJob[lstBlock[intFin]];
                if (intIdSiguienteTrabajo > -1)
                    dblTrabajoEnd = dicIdOperacionTiempoHastaFin[intIdSiguienteTrabajo] + cData.dicIdOperationTime[lstBlock[intFin]];
                dicIdOperacionFin.Add(lstBlock[intFin], Math.Max(dblTrabajoEnd, dblMachineFin));
                intIdSiguienteMaquina = lstBlock[intFin];
                //--- Fin idOpearcion to N

                // Obtiene el makespan maximo
                dblMakespan = double.MaxValue;
                if (dicIdOperacionInicio.ContainsKey(lstBlock[intFin]) && dicIdOperacionFin.ContainsKey(lstBlock[intFin]))
                {
                    dblMakespan = dicIdOperacionInicio[lstBlock[intFin]] + dicIdOperacionFin[lstBlock[intFin]];
                    dicIdOperacionMakespan.Add(lstBlock[intFin], dblMakespan);
                    if (dblMakespanMax < dblMakespan)
                        dblMakespanMax = dblMakespan;
                }
                if (dicIdOperacionInicio.ContainsKey(lstBlock[intIni]) && dicIdOperacionFin.ContainsKey(lstBlock[intIni]) && !dicIdOperacionMakespan.ContainsKey(lstBlock[intIni]))
                {
                    dblMakespan = dicIdOperacionInicio[lstBlock[intIni]] + dicIdOperacionFin[lstBlock[intIni]];
                    dicIdOperacionMakespan.Add(lstBlock[intIni], dblMakespan);
                    if (dblMakespanMax < dblMakespan)
                        dblMakespanMax = dblMakespan;
                }
            }
            // Calcula el tiempo fin para la operacion V (la ultima del bloque que ahora seria la primera)
            intIdSiguienteTrabajo = cData.dicIdOperationIdNextInJob[intIdV];
            dblTrabajoFin = cData.dicIdOperationTime[intIdV];
            if (intIdSiguienteTrabajo > -1)
                dblTrabajoFin = dicIdOperacionTiempoHastaFin[intIdSiguienteTrabajo] + cData.dicIdOperationTime[intIdV];
            dblMachineFin = cData.dicIdOperationTime[intIdV];
            if (intIdSiguienteMaquina > -1)
                dblMachineFin = dicIdOperacionFin[intIdSiguienteMaquina] + cData.dicIdOperationTime[intIdV];
            dicIdOperacionFin.Add(intIdV, Math.Max(dblTrabajoFin, dblMachineFin));
            dblMakespan = dicIdOperacionInicio[intIdV] + dicIdOperacionFin[intIdV];
            dicIdOperacionMakespan.Add(intIdV, dblMakespan);
            if (dblMakespanMax < dblMakespan)
                dblMakespanMax = dblMakespan;
            // Devuelve el makespan minimo
            return dblMakespanMax;
        }


        /// <summary>
        /// Esta funcion deshace el ultimo cambio forward realizado
        /// dicho cambio esta guardado en cMoimiento y lo aplica
        /// en sentido contrario, dejando cSchedule como estaba
        /// antes del cambio.
        /// </summary>
        /// <param name="cMovimiento"></param>
        /// <param name="cData"></param>
        /// <param name="cSchedule"></param>
        public void DeshacerMovimientoForwardBorrar(clsDatosMovimiento cMovimiento, clsDatosJobShop cData, clsDatosSchedule cSchedule)
        {
            // Realiza el cambio de la primera y ultima en maquina
            Int32 intIdMachine = cData.dicIdOperationIdMachine[cMovimiento.intIdOperacionU];
            if (cMovimiento.intIdOperacionPosteriorV == -1)
                cSchedule.dicIdMachineIdOperationLast[intIdMachine] = cMovimiento.intIdOperacionV;
            if (cMovimiento.intIdOperacionAnteriorU == -1)
                cSchedule.dicIdMachineIdOperationFirst[intIdMachine] = cMovimiento.intIdOperacionU;

            // Realiza el cambio en la lista next previous
            if (cMovimiento.intIdOperacionAnteriorU > -1)
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionAnteriorU] = cMovimiento.intIdOperacionU;
            if (cMovimiento.intIdOperacionPosteriorV > -1)
                cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionPosteriorV] = cMovimiento.intIdOperacionV;

            cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionAnteriorV] = cMovimiento.intIdOperacionV;
            cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionPosteriorU] = cMovimiento.intIdOperacionU;

            cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionV] = cMovimiento.intIdOperacionPosteriorV;
            cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionV] = cMovimiento.intIdOperacionAnteriorV;

            cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionU] = cMovimiento.intIdOperacionPosteriorU;
            cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionU] = cMovimiento.intIdOperacionAnteriorU;

        }

        /// <summary>
        /// Esta funcion deshace el ultimo cambio realizado
        /// dicho cambio esta guardado en cMovimiento y lo aplica
        /// en sentido contrario, dejando cSchedule como estaba
        /// antes del cambio.
        /// </summary>
        /// <param name="cMovimiento"></param>
        /// <param name="cData"></param>
        /// <param name="cSchedule"></param>
        public void DeshacerMovimiento(clsDatosMovimiento cMovimiento, clsDatosJobShop cData, clsDatosSchedule cSchedule)
        {
            // Realiza el cambio de la primera y ultima en maquina
            Int32 intIdMachine = cData.dicIdOperationIdMachine[cMovimiento.intIdOperacionU];
            if (cMovimiento.intIdOperacionPosteriorV == -1)
                cSchedule.dicIdMachineIdOperationLast[intIdMachine] = cMovimiento.intIdOperacionV;
            if (cMovimiento.intIdOperacionAnteriorU == -1)
                cSchedule.dicIdMachineIdOperationFirst[intIdMachine] = cMovimiento.intIdOperacionU;

            // AU->U
            if (cMovimiento.intIdOperacionAnteriorU > -1)
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionAnteriorU] = cMovimiento.intIdOperacionU;
            // AU<-U
            cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionU] = cMovimiento.intIdOperacionAnteriorU;

            //V->SV
            cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionV] = cMovimiento.intIdOperacionPosteriorV;
            // V<-SV
            if (cMovimiento.intIdOperacionPosteriorV > -1)
                cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionPosteriorV] = cMovimiento.intIdOperacionV;

            // U->V la siguiente a U es V
            if (cMovimiento.intIdOperacionPosteriorU == cMovimiento.intIdOperacionV)
            {
                // U->V
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionU] = cMovimiento.intIdOperacionV;
                // U<-V
                cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionV] = cMovimiento.intIdOperacionU;
            }
            else
            {
                // U->SU
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionU] = cMovimiento.intIdOperacionPosteriorU;
                // U<-SU
                cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionPosteriorU] = cMovimiento.intIdOperacionU;
                // AV->V
                cSchedule.dicIdOperationIdNextInMachine[cMovimiento.intIdOperacionAnteriorV] = cMovimiento.intIdOperacionV;
                // AV<-V
                cSchedule.dicIdOperationIdPreviousInMachine[cMovimiento.intIdOperacionV] = cMovimiento.intIdOperacionAnteriorV;
            }






        }

    }
}
