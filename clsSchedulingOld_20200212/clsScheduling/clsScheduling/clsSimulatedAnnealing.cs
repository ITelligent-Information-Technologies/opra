using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    /// <summary>
    /// Esta clase contiene el motor de decision de
    /// Simulated Annealing
    /// </summary>
    class clsSimulatedAnnealing
    {
        private Random _rnd = new Random(100);
        private double _dblT; // Temperatura actual
        private double _dblTo; // Temperatura inicial
        private double _dblDecrementoTemperatura; // Decrementos de la temperatura
        public clsSimulatedAnnealing(clsDatosParametros cParametros, double dblIcrementoMakespanMedio)
        {
            Int32 intNumeroInteraciones = Convert.ToInt32(((double)cParametros.intMaxIteraciones / cParametros.intMaxIteracionesPorBucle));
            _dblTo = (-dblIcrementoMakespanMedio) / System.Math.Log(cParametros.dblSAProbabilidadAceptarSolucionInicial);
            _dblDecrementoTemperatura = ((-dblIcrementoMakespanMedio) / (System.Math.Log(cParametros.dblSAProbabilidadAceptarSolucionFinal))) / _dblTo;
            _dblDecrementoTemperatura = System.Math.Pow(_dblDecrementoTemperatura, ((double)1 / intNumeroInteraciones));
            _dblT = _dblTo;
        }

        public void DecrementarTemperatura()
        {
            _dblT = _dblDecrementoTemperatura * _dblT;
        }
        public Boolean Aceptar(double dblMakespan, double dblMakespanMin)
        {
            double dblRandom = _rnd.NextDouble();
            double dblUmbral = Math.Exp(-(dblMakespan - dblMakespanMin) / _dblT);
            if (dblRandom < dblUmbral)
                return true;
            return false;
        }
    }
}
