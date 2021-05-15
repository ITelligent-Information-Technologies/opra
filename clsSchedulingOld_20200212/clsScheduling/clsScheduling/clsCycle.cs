using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    /// <summary>
    /// Esta clase comprueba si la trayectoria
    /// de las soluciones vuelven siempre a las 
    /// mismas esto es si se forman ciclos.
    /// Basicamente supongamos que vamos probando
    /// distintas soluciones y vamos obteniendo 
    /// distintos makespan (sean C dichos makespan):
    /// C1, C2, ..., Cn
    /// Si hubiese un ciclo se tendria que ir 
    /// repitiendo el mismo valor del 
    /// makespan con una determinada frecuencia
    /// esta clase comprueba si eso sucede.
    /// Por ejemplo si el makespan se repite con 
    /// frecuencia 4 tendriamos algo como esto:
    /// ...34,24,35,23,67,34,24,35,23,67,34,24,...
    /// </summary>
    class clsCycle
    {
        private Queue<double> _queMakespan; // Va guardando en la cola los makespan que van llegando
        private Dictionary<double, Int32> _dicMakespanToIndex;  // Guarda el makespan y el indice de dicho makespan
        private Int32 _intNumeroFrecuencias = 100; // Numero de frecuencias que mira para ver si se repite
        private Int32 _intNumeroRepeticiones = 2; // Este numero por la frecuencia es lo que se mira para detras
        private Int32 _intLastIndex = 0; // Ultimo index metido
        private Int32 _intActualCuentaCiclo = 0; // Esta variable lleva una cuenta de las veces hasta el momento que el mismo makespan
        private Int32 _intActualFrecuencia = -1; // Esta variable es la frecuencia en que se repite el makespan actual (el numero de veces repetido en Cuenta)

        public clsCycle(Int32 intNumeroFrecuencias = 100, Int32 intNumeroRepeticiones = 2)
        {
            _intNumeroFrecuencias = intNumeroFrecuencias;
            _intNumeroRepeticiones = intNumeroRepeticiones;
            _queMakespan = new Queue<double>();
            _dicMakespanToIndex = new Dictionary<double, int>();
        }

        public void Clear()
        {
            _queMakespan = new Queue<double>();
            _dicMakespanToIndex = new Dictionary<double, int>();
            _intActualCuentaCiclo = 0;
            _intActualFrecuencia = -1;
        }

        /// <summary>
        /// Chequea si hay un ciclo, es decir si el mismo 
        /// makespan se va repitiendo con una determinada
        /// frecuencia ejemplo:
        /// 34,24,25,98,34,24,25,98,34,24,...
        /// En el anterior con una frecuencia 4 se va repitiendo
        /// el mismo valor.
        /// </summary>
        /// <param name="dblMakespan"></param>
        /// <returns></returns>
        public Boolean CheckCycleAndAdd(double dblMakespan)
        {
            // Añade los valores
            _intLastIndex++;
            if (_queMakespan.Count >= _intNumeroFrecuencias)
            { 
              double  dblMakespanOut = _queMakespan.Dequeue();
                if(_dicMakespanToIndex .ContainsKey (dblMakespanOut))
                {
                    if (_intLastIndex - _dicMakespanToIndex[dblMakespanOut] >= _intNumeroFrecuencias)
                        _dicMakespanToIndex.Remove(dblMakespanOut);

                }
            }
            _queMakespan.Enqueue(dblMakespan);
            // Si ya hay una frecuencia anterior comprueba si se obtiene ese makespan a esa frecuencia
            if (_intActualFrecuencia > 0)
            {
                // Se repite el makespan a la frecuencia establecida
                if (_queMakespan.ElementAt(_queMakespan.Count- _intActualFrecuencia) == dblMakespan)
                {
                    _intActualCuentaCiclo++;
                    // Si se ha superado el numero de veces entonces hay ciclo
                    if (_intActualCuentaCiclo >= (_intNumeroFrecuencias * _intNumeroRepeticiones))
                        return true;
                    // Si no se ha superado sale diciendo que todavia no hay ciclo pero puede haberlo mas adelante
                    return false;
                }
            }
            // Hay que recalcular la frecuencia
            if (_dicMakespanToIndex.ContainsKey(dblMakespan))
            { 
                _intActualFrecuencia = _intLastIndex  - _dicMakespanToIndex[dblMakespan];
                _intActualCuentaCiclo=1;
                _dicMakespanToIndex[dblMakespan] = _intLastIndex;
            }
            else
            {
                _intActualCuentaCiclo = 0;
                _intActualFrecuencia = -1;
                _dicMakespanToIndex.Add(dblMakespan, _intLastIndex);
            }
            return false;
        }
    }
}
