using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    class clsHorarios
    {
        public (Dictionary<Int32, DateTime> dicIdOperationFechaStart, Dictionary<Int32, DateTime> dicIdOperationFechaEnd) ObtenerFechas(clsDatosHorarios cHorarios, clsDatosJobShop cData, clsDatosSchedule cSchedule, DateTime dtmStartDatetime)
        {
            Int32[] intIdOperacionOrdenadoStart = new Int32[cSchedule.dicIdOperationStartTime.Count];
            Int32[] intIdOperacionOrdenadoEnd = new Int32[cSchedule.dicIdOperationStartTime.Count];
            double[] dblStartTime = new double[cSchedule.dicIdOperationStartTime.Count];
            double[] dblEndTime = new double[cSchedule.dicIdOperationStartTime.Count];
            Dictionary<Int32, double> dicIdOperationStart = new Dictionary<int, double>();
            Dictionary<Int32, double> dicIdOperationEnd = new Dictionary<int, double>();

            Int32 intCount = 0;
            foreach (KeyValuePair<Int32, double> kvPair in cSchedule.dicIdOperationStartTime)
            {
                dblStartTime[intCount] = kvPair.Value;
                dblEndTime[intCount] = kvPair.Value + cData.dicIdOperationTime[kvPair.Key];
                intIdOperacionOrdenadoStart[intCount] = kvPair.Key;
                intIdOperacionOrdenadoEnd[intCount] = kvPair.Key;
                dicIdOperationStart.Add(kvPair.Key, dblStartTime[intCount]);
                dicIdOperationEnd.Add(kvPair.Key, dblEndTime[intCount]);
                intCount++;
            }
            // Ordena los operaciones por su comienzo y  obtiene las fechas y horas de inicio
            Array.Sort(dblStartTime, intIdOperacionOrdenadoStart);
            // Ordena los operaciones por su comienzo y  obtiene las fechas y horas de fin
            Array.Sort(dblEndTime, intIdOperacionOrdenadoEnd);
            return RecorrerHorarios(cData, intIdOperacionOrdenadoStart, intIdOperacionOrdenadoEnd, cHorarios, dtmStartDatetime, cSchedule);
        }

        private (Dictionary<Int32, DateTime> dicIdOperationFechaStart, Dictionary<Int32, DateTime> dicIdOperationFechaEnd) RecorrerHorarios(clsDatosJobShop cData, Int32[] intIdOperacionOrdenadoStart, Int32[] intIdOperacionOrdenadoEnd, clsDatosHorarios cHorarios, DateTime dtmStartDatetime, clsDatosSchedule cSchedule)
        {
            Dictionary<Int32, DateTime> dicIdOperationFechaStart = new Dictionary<int, DateTime>();
            Dictionary<Int32, DateTime> dicIdOperationFechaEnd = new Dictionary<int, DateTime>();
            // Recorre el calendario y va encontrando los datos
            Boolean blnEnBucle = true;
            DateTime dtmFechaHoraStart = dtmStartDatetime;
            DateTime dtmFechaHoraEnd = dtmStartDatetime;
            DateTime dtmDiaEnCurso = dtmStartDatetime.Date;
            Int32 intWeekDay = (Int32)dtmDiaEnCurso.DayOfWeek;
            List<clsDatosHorariosHoras> lstHoras = new List<clsDatosHorariosHoras>();
            Dictionary<Int32, DateTime> dicIdOperationDate = new Dictionary<int, DateTime>();
            double dblTiempoContinuoInicio = 0;
            double dblTiempoContinuoFinal = 0;
            Int32 intIndexOrdenadoStart = 0;
            Int32 intIndexOrdenadoEnd = 0;
            // Va recorriendo en un bucle cada dia hasta que no queden mas operaciones por asignar
            while (blnEnBucle)
            {
                // Comprueba si el horario es especial
                if (cHorarios.dicFechasEspecialesHorarios.ContainsKey(dtmDiaEnCurso))
                    lstHoras = cHorarios.dicFechasEspecialesHorarios[dtmDiaEnCurso];
                else // Es horario normal (lunes 1, Martes 2, ...)
                {
                    if (!cHorarios.dicIdDiasSemanaHorarios.ContainsKey(intWeekDay))
                        new Exception("No hay horario para el dia de la semana " + intWeekDay);
                    else
                        lstHoras = cHorarios.dicIdDiasSemanaHorarios[intWeekDay];
                }
                // Recorre la lista
                foreach (clsDatosHorariosHoras cHoras in lstHoras)
                {
                    // No hay actividad ese dia no hay nada
                    if (cHoras.blnSinActividad)
                    {
                        if (lstHoras.Count > 1)
                            new Exception("Si no hay actividad no puede haber otro horario ese dia");
                    }
                    else // Si hay actividad
                    {
                        // Actualiza la fecha y hora inicio
                        dtmFechaHoraStart = dtmDiaEnCurso.AddHours(cHoras.dblHoraDesde);
                        dtmFechaHoraEnd = dtmDiaEnCurso.AddHours(cHoras.dblHoraHasta);
                        // Calcula este horario cuando acaba en tiempo continuo
                        dblTiempoContinuoFinal = dblTiempoContinuoInicio + (dtmFechaHoraEnd - dtmFechaHoraStart).TotalHours;
                        // Comprueba si alguna operacion se inicia en este intervalo y calcula su tiempo de inicio y lo asigna
                        Boolean blnEnBucleTmp = true;
                        while (blnEnBucleTmp && (intIndexOrdenadoStart < intIdOperacionOrdenadoStart.Length))
                        {
                            Int32 intIdOperacion = intIdOperacionOrdenadoStart[intIndexOrdenadoStart];
                            double dblStartTime = ConvertirTiemposAHoras(cSchedule.dicIdOperationStartTime[intIdOperacion], cData.enuTipoTiempo);
                            // El comienzo de la operacion tiene que estar entre TiempoContinuoIni y TiempoContinuoFinal
                            if (dblStartTime >= dblTiempoContinuoInicio && dblStartTime <= dblTiempoContinuoFinal)
                            {
                                // Señala la fecha y hora de inicio de la operacion
                                double dblHorasDiferencia = dblStartTime - dblTiempoContinuoInicio;
                                dicIdOperationFechaStart.Add(intIdOperacion, dtmFechaHoraStart.AddHours(dblHorasDiferencia));
                                intIndexOrdenadoStart++;
                            }
                            else
                                blnEnBucleTmp = false;
                        }
                        // Comprueba si alguna operacion se inicia en este intervalo y calcula su tiempo de inicio y lo asigna
                        blnEnBucleTmp = true;
                        while (blnEnBucleTmp && (intIndexOrdenadoEnd < intIdOperacionOrdenadoEnd.Length))
                        {
                            Int32 intIdOperacion = intIdOperacionOrdenadoEnd[intIndexOrdenadoEnd];
                            double dblEndTime = ConvertirTiemposAHoras(cSchedule.dicIdOperationEndTime[intIdOperacion], cData.enuTipoTiempo);
                            // El comienzo de la operacion tiene que estar entre TiempoContinuoIni y TiempoContinuoFinal
                            if (dblEndTime >= dblTiempoContinuoInicio && dblEndTime <= dblTiempoContinuoFinal)
                            {
                                // Señala la fecha y hora de inicio de la operacion
                                double dblHorasDiferencia = dblEndTime - dblTiempoContinuoInicio;
                                dicIdOperationFechaEnd.Add(intIdOperacion, dtmFechaHoraStart.AddHours(dblHorasDiferencia));
                                intIndexOrdenadoEnd++;
                            }
                            else
                                blnEnBucleTmp = false;
                        }
                    }
                    dblTiempoContinuoInicio = dblTiempoContinuoFinal;
                }
                dtmDiaEnCurso = dtmDiaEnCurso.AddDays(1);
                if (intIndexOrdenadoEnd >= intIdOperacionOrdenadoEnd.Length && intIndexOrdenadoStart >= intIdOperacionOrdenadoStart.Length)
                    blnEnBucle = false;
            }
            return (dicIdOperationFechaStart, dicIdOperationFechaEnd);
        }

        /// <summary>
        /// Convierte un determinado tipo de tiempo en horas
        /// </summary>
        /// <param name="dblTiempo"></param>
        /// <param name="enuTipoTiempo"></param>
        /// <returns></returns>
        private double ConvertirTiemposAHoras(double dblTiempo, TipoTiempo enuTipoTiempo)
        {
            double dblTiempoEnHoras = 0;
            if (enuTipoTiempo == TipoTiempo.UnidadEs30Segundos)
                dblTiempoEnHoras = dblTiempo / 120;
            else if (enuTipoTiempo == TipoTiempo.UnidadEs1Minuto)
                dblTiempoEnHoras = dblTiempo / 60;
            else if (enuTipoTiempo == TipoTiempo.UnidadEs15Minutos)
                dblTiempoEnHoras = dblTiempo / 4;
            else if (enuTipoTiempo == TipoTiempo.UnidadEs30Minutos)
                dblTiempoEnHoras = dblTiempo / 2;
            else if (enuTipoTiempo == TipoTiempo.UnidadEs45Minutos)
                dblTiempoEnHoras = dblTiempo * 3 / 4;
            else if (enuTipoTiempo == TipoTiempo.UnidadEs1Hora)
                dblTiempoEnHoras = dblTiempo;
            else
                new Exception("Camnbio de tipo tiempo no implementado");
            return dblTiempoEnHoras;
        }
    }
}

