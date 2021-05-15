using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    /// <summary>
    /// Esta clase se ocupa de obtener la firma que se utilizara para representar una solucion
    /// en Tabu List, hay diversos tipos implementados y se guardan en un enum
    /// de la clase parametros
    /// </summary>
    class clsFirmaTabuList
    {
        private TiposFirmaTabuList _enuTipoFirma;
        public clsFirmaTabuList(TiposFirmaTabuList enuTipoFirma)
        {
            _enuTipoFirma = enuTipoFirma;
        }

        public string GenerarFirma(double dblMakespan, clsDatosCambio cCambio)
        {
            string strFirma = "";
            if (_enuTipoFirma == TiposFirmaTabuList.SoloParUVYForwardBackward)
            {
                strFirma = cCambio.intIdOperacionU + "_" + cCambio.intIdOperacionV + "_" + cCambio.blnEsForward.ToString();
            }
            else if (_enuTipoFirma == TiposFirmaTabuList.SoloParUV)
            {
                strFirma = cCambio.intIdOperacionU + "_" + cCambio.intIdOperacionV;
            }
            if (_enuTipoFirma == TiposFirmaTabuList.SoloParUVYMakespan)
            {
                strFirma = cCambio.intIdOperacionU  + "_" + cCambio.intIdOperacionV  + "_" + dblMakespan;
            }
            else if (_enuTipoFirma == TiposFirmaTabuList.SoloParUVYMakespanYForwardBackward )
            {
                strFirma = cCambio.intIdOperacionU + "_" + cCambio.intIdOperacionV + "_" + dblMakespan+"_"+cCambio .blnEsForward .ToString ();
            }
            else
                new Exception("Tipo no implementado");

            return strFirma;
        }
    }
}
