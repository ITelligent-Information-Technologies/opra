using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    [Serializable ]
    class clsTabooList
    {
        private Queue<Tuple<Int32, Int32>> _queTaboo; // esta cola mantiene las tuplas
        private Dictionary<string, Int32> _dicTaboo; // este diccionario guarda los datos de las tuplas (t1_t2) como string para busqueda rapida
        private Int32 _intTabooListMax; // Numero maximo de elementos en lista
        private Int32 _intLastInList1; // Guarda el ultimo valor en la lista (el mas reciente)
        private Int32 _intLastInList2; // Guarda el ultimo valor en la lista (el mas reciente)
        public clsTabooList(Int32 intTabooListMax)
        {
            _queTaboo = new Queue<Tuple<int, int>>(intTabooListMax);
            _dicTaboo = new Dictionary<string, int>();
            _intTabooListMax = intTabooListMax;
        }

       

        public Tuple<Int32, Int32> Add(Int32 intIdOperation1, Int32 intIdOperation2)
        {
            string strTuplaIn = intIdOperation1 + "_" + intIdOperation2;
            Tuple<Int32, Int32> tupOut=new Tuple<int, int> (-1,-1);
            // Comprueba si debe sacar antes de meter y si es asi saca
            if (_queTaboo.Count() >= _intTabooListMax)
            {
                tupOut = _queTaboo.Dequeue();
                string strTuplaOut = tupOut.Item1 + "_" + tupOut.Item2;
                if (!_dicTaboo.ContainsKey(strTuplaOut))
                    new Exception("Movimiento no encontrado");
                // Como puede haber tuplas repetidas en TabooList va eliminando del diccionaro y solo si es cero la saca
                _dicTaboo[strTuplaOut]--;
                if (_dicTaboo[strTuplaOut] == 0)
                    _dicTaboo.Remove(strTuplaOut);
            }
            // Mete la nueva tupla
            _queTaboo.Enqueue(new Tuple<Int32, Int32>(intIdOperation1, intIdOperation2));
            // Mete en el diccionario de busqueda
            if (_dicTaboo.ContainsKey(strTuplaIn))
                _dicTaboo[strTuplaIn]++;
            else
                _dicTaboo.Add(strTuplaIn, 1);
            // Guarda como el ultimo metido
            _intLastInList1 = intIdOperation1;
            _intLastInList2 = intIdOperation2;
            // Devuelve el que saca si lo hay
            return tupOut;
        }

        public Boolean Exist(Int32 intIdOperation1, Int32 intIdOpeartion2)
        {
            return _dicTaboo.ContainsKey(intIdOperation1 + "_" + intIdOpeartion2);
        }

        public Tuple<Int32, Int32> GetLastInTaboo()
        {
            return new Tuple<int, int>(_intLastInList1, _intLastInList2);
        }


    }
}
