using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    public enum TiposFirmaTabuList
    {
        SoloParUV, // Solo guarda el par U, V que se va a cambiar
        SoloParUVYMakespan, // Guarda el par U, V y el makespan antes del cambio
        CaminoUaV, // Guarda el cambio de U a V antes del camnbio-> U,SU,...,AV,V
        CaminoUaVYPosicionMaquinas, // Ademas del camino de U a V tambien la posicion de cada operacion en maquina (vale la de la primera maquina)
        ParUVYMakespanYCaminoCritico, // Guarda el par U,V, el makespan y el camino critico completo
        SoloParUVYForwardBackward, // Solo guarda el par U, V que se va a cambiar y si es Forward o Backward
        SoloParUVYMakespanYForwardBackward, // Guarda el par U, V y el makespan antes del cambio y si es Forward o Backward
        CaminoUaVYForwardBackward, // Guarda el cambio de U a V antes del camnbio-> U,SU,...,AV,V y si es Forward o Backward
        CaminoUaVYPosicionMaquinasYForwardBackward,// Ademas del camino de U a V tambien la posicion de cada operacion en maquina (vale la de la primera maquina) y si es Forward o Backward
        ParUVYMakespanYCaminoCriticoYForwardBackward // Guarda el par U,V, el makespan y el camino critico completo y si es Forward o Backward
    }
    class clsDatosParametros
    {
        public Int32 intMaxSegundos = 300; // Maximo de segundos si se alcanza finaliza (si se alcanza antes max iteraciones tambine finaliza)
        public Int32 intMaxIteraciones = 30000; // Iteraciones totales maximas
        public Int32 intMaxIteracionesPorBucle = 1000; // Numero maximo de iteraciones por bucle se pone a cero si se encuentra una solucion mejor
        public Int32 intMaxIteracionCalentamiento = 300; // Maximo de iteraciones inicial para calentamiento
        public Int32 intMaxStackBackTrack = 30; // Maximo de elementos en stack de back track
        public double dblSAProbabilidadAceptarSolucionInicial = 0.3; // Probabilidad de aceptar una solucion mala inicial tras el calentamiento
        public double dblSAProbabilidadAceptarSolucionFinal = 0.01; // Probabilidad de aceptar una solucion mala inicial tras el calentamiento
        public TiposFirmaTabuList enuGuardarTabuList = TiposFirmaTabuList.SoloParUVYMakespan; // Guarda la firma para representa una solucion en tabu
        public Int32 intTabuListMin; // Tamaño minimo busca tabu
        public Int32 intTabuListMax; // Tamaño maximo busca tabu
        public Boolean blnN6PermitirForbiddenNonProfitable = false; // Si en N6 permite los Forbidden Non Profitable (movimientos malos y prohibidos)
        public Boolean blnSiNoHayParaBacktrackUtilizaN1 = true; // Si no hay para backtrack y quedan iteraciones utiliza N1
        public clsDatosParametros(clsDatosJobShop cData)
        {
            // Se calcula el tamaño minimo y maximo de la lista tabu  segun paper de Zhang
            intTabuListMin = 10 + Convert.ToInt32((double)cData.intJobsCount / cData.intMachinesCount);
            intTabuListMax = intTabuListMin + 2;
        }
    }
}
