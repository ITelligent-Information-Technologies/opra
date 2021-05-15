using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    class clsTabuList
    {
        private Random _rnd = new Random(100);
        private Queue<string> _queTabu; // esta cola mantiene las firmas
        private HashSet<string> _hsTabu = new HashSet<string>(); // Este hashset mantiene los datos que estan en cola
         private Int32 _intTabuListSize; // Tamaño tabu list es un aleatorio entre intTabuListMin y intTabuListMax
        private clsFirmaTabuList _cFirma; // Firma de tabu list

        public clsTabuList( clsDatosParametros cParametros)
        {
            _intTabuListSize = _rnd.Next(cParametros .intTabuListMin , cParametros .intTabuListMax );
            _hsTabu = new HashSet<string>();
            _queTabu = new Queue<string>();
            _cFirma = new clsFirmaTabuList(cParametros.enuGuardarTabuList);
        }

        public Boolean Add(clsDatosSchedule cSchedule, clsDatosCambio cCambio )
        {

            // Obtiene la firma
            string strFirma = _cFirma.GenerarFirma(cSchedule.dblMakespan , cCambio);
            if (!_hsTabu.Contains(strFirma))
                _hsTabu.Add(strFirma);
            else
                return false;
            // Comprueba si debe sacar antes de meter y si es asi saca
            string strFirmaOut = "";
            if (_queTabu.Count() >= _intTabuListSize)
            {
                strFirmaOut = _queTabu.Dequeue();
                _hsTabu.Remove(strFirmaOut);
            }
            // Mete la nueva tupla
            _queTabu.Enqueue(strFirma);
            return true;
        }

        public Boolean Exist(double dblMakespan, clsDatosCambio cCambio)
        {
            string strFirma = _cFirma.GenerarFirma(dblMakespan, cCambio);
            return _hsTabu.Contains(strFirma);
        }

        public void Clear()
        {
            _hsTabu = new HashSet<string>();
            _queTabu = new Queue<string>();
        }


    }
}
