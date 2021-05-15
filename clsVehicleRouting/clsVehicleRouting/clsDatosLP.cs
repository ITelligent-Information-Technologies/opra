using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clsVehicleRouting
{


    public class clsResultados
    {
        public string strEtiqueta; // Etiqueta con datos para pasar al cliente
        public double dblObjetivo; // Votos adicionales para el objetivo
        public double dblSegundos; // tiempo de procesamiento en segundos
        public Dictionary<string, double> dicVariableValor = new Dictionary<string, double>(); // Nombre de la variable y valor        
        public Int32 intModelStatus; // Status del modelo en codigo
        public string strModelStatus; // Status del modelo en texto
        public Dictionary<Int32, Int32> dicIdPartidoEscanosOld = new Dictionary<int, int>();
        public Dictionary<Int32, Int32> dicIdPartidoEscanosNew = new Dictionary<int, int>();
    }



    public class clsParametros
    {
        public Int32 intTimeoutSegundos = 30; // Tiempo maximo para buscar una solución por defecto 60 segundos
        public Boolean blnMinimizar = true; // SI hay que minimizar o maximizar
        public Boolean blnEntero = true; // Toma todaas las variable de decision como enteras

    }

    public class clsVariablesOptimizacion
    {
        private Dictionary<Int32, string> _dicIdColANombre = new Dictionary<Int32, string>();
        private Dictionary<string, Int32> _dicNombreAIdCol = new Dictionary<string, int>();
        private Int32 _intIdCol = 1;

        public Int32 AddVariableByName(string strName)
        {
            if (_dicNombreAIdCol.ContainsKey(strName))
                return _dicNombreAIdCol[strName];
            _dicNombreAIdCol.Add(strName, _intIdCol);
            _dicIdColANombre.Add(_intIdCol, strName);
            _intIdCol++;
            return _intIdCol - 1;
        }

        public Int32 GetVariableByName(string strName)
        {
            if (_dicNombreAIdCol.ContainsKey(strName))
                return _dicNombreAIdCol[strName];
            return -1;
        }
        public string GetVariableById(Int32 intId)
        {
            if (_dicIdColANombre.ContainsKey(intId))
                return _dicIdColANombre[intId];
            return null;
        }



    }

    public enum tipoRestriccion
    {
        LE, //Less or Equal
        EQ, // Equal
        GE, // Great or equal        
        FR //
    }

    public enum tipoVariable
    {
        Entera,
        Semicontinua,
        Binarias,
        Libre,
        RealMayorCero
    }


    public class clsProblemaLp
    {
        private Dictionary<string, Int32> _dicRestriccionId = new Dictionary<string, int>(); // Nombre de la restriccion a ID
        private Dictionary<Int32, string> _dicIdRestriccion = new Dictionary<Int32, string>(); // Id a nombre de la restriccion
        private Dictionary<string, Int32> _dicVariableId = new Dictionary<string, int>(); // Nombre de la variable a ID
        private Dictionary<Int32, string> _dicIdVariable = new Dictionary<int, string>(); // id dela restriccion a nombre
        private Dictionary<Int32, tipoVariable> _dicIdVariableTipo = new Dictionary<int, tipoVariable>(); // Id variable a su tipo
        private Dictionary<Int32, double> _dicIdVariableSemicontinuaMinimo = new Dictionary<int, double>(); // Si la variable es simcontiua el minimo si no 0
        //  private HashSet<Int32> _hsIdVariablesEnteras = new HashSet<int>(); // Id variable para aquellas que son entereas
        private List<clsProblemaLpRestriccion> _lstRestricciones = new List<clsProblemaLpRestriccion>(); // Restricciones
        private Dictionary<Int32, double> _dicObjetivoIdVariableCoeficiente = new Dictionary<int, double>(); // Guarda las variables objetivos


        public Int32 GetTotalVariables()
        {
            return (_dicVariableId.Count);
        }

        /// <summary>
        /// Añade una variable a la funcion objetivo
        /// </summary>
        /// <param name="strVariableNombre"></param>
        /// <param name="dblCoeficiente"></param>
        public void AddObjetivo(string strVariableNombre, double dblCoeficiente)
        {
            if (!_dicVariableId.ContainsKey(strVariableNombre))
                new Exception("Variable pasada no existe");
            Int32 intVariableId = _dicVariableId[strVariableNombre];
            if (_dicObjetivoIdVariableCoeficiente.ContainsKey(intVariableId))
                new Exception("Ya se ha pasado un coefciente para esa variable de la funcion objetivo");
            _dicObjetivoIdVariableCoeficiente.Add(intVariableId, dblCoeficiente);
        }

        /// <summary>
        ///  Devuelve la funcion objetivo (id variable y coeficiente)
        /// </summary>
        /// <returns></returns>
        public Dictionary<Int32, double> GetObjetivos()
        {
            return _dicObjetivoIdVariableCoeficiente;
        }

        /// <summary>
        /// Se añade una nueva restriccion al problema
        /// </summary>
        /// <param name="strRestriccionNombre">Nombre de la restriccion</param>
        /// <param name="enuRestriccionTipo">Tipo de restriccion</param>
        /// <param name="dblRestriccionRhs">Right Hand Side de la restriccion</param>
        public void AddRestriccion(string strRestriccionNombre, tipoRestriccion enuRestriccionTipo, double dblRestriccionRhs)
        {
            if (!_dicRestriccionId.ContainsKey(strRestriccionNombre))
            {
                clsProblemaLpRestriccion cRestriccion = new clsProblemaLpRestriccion();
                cRestriccion.Rhs = dblRestriccionRhs;
                cRestriccion.TipoRestriccion = enuRestriccionTipo;
                cRestriccion.RestriccionNombre = strRestriccionNombre;
                _lstRestricciones.Add(cRestriccion);
                _dicRestriccionId.Add(strRestriccionNombre, _lstRestricciones.Count - 1);
                _dicIdRestriccion.Add(_lstRestricciones.Count - 1, strRestriccionNombre);
            }
            else
                new Exception("restriccion ya existe");
        }

        /// <summary>
        /// Comprueba si un nombre de variable ya ha sido incluido
        /// </summary>
        /// <param name="strVariableNombre"></param>
        /// <returns></returns>
        public bool VariableExiste(string strVariableNombre)
        {
            if (!_dicVariableId.ContainsKey(strVariableNombre))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Comprueba si un nombre de restriccion ya ha sido incluido
        /// </summary>
        /// <param name="strRestriccionNombre"></param>
        /// <returns></returns>
        public bool RestriccionExiste(string strRestriccionNombre)
        {
            if (!_dicRestriccionId.ContainsKey(strRestriccionNombre))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Añade una nueva variable, siempre que no este
        /// </summary>
        /// <param name="strVariableNombre">Nombre de la variables</param>
        /// <param name="blnVariableEsEntera">Si la variable es entera</param>
        public void AddVariable(string strVariableNombre, tipoVariable enumTipoVariable, double dblSemicontinuaMinimo = 0)
        {
            if (!_dicVariableId.ContainsKey(strVariableNombre))
            {
                _dicVariableId.Add(strVariableNombre, _dicVariableId.Count);
                _dicIdVariableTipo.Add(_dicIdVariable.Count, enumTipoVariable);
                _dicIdVariableSemicontinuaMinimo.Add(_dicIdVariable.Count, dblSemicontinuaMinimo);
                _dicIdVariable.Add(_dicIdVariable.Count, strVariableNombre);
            }
            else
                new Exception("Variable ya existe");
        }

        /// <summary>
        /// Añade una variable a una restriccion, ojo no se puede añadir la misma variable dos veces
        /// la variable se añade a la parte izquierda de la restriccion
        /// </summary>
        /// <param name="strRestriccionNombre">Nombhre de la restriccion a la que se añade la variable</param>
        /// <param name="strVariableNombre">Nombre de la variable que se añade</param>
        /// <param name="dblVariableCoeficiente">Coeficiente dela variable (en la parte izquierda)</param>
        public void AddRestriccionVariable(string strRestriccionNombre, string strVariableNombre, double dblVariableCoeficiente)
        {
            // La restriccion tiene que existir previamente
            if (!_dicRestriccionId.ContainsKey(strRestriccionNombre))
                new Exception("El nombre de restriccion no existe");
            // La variable tiene que existir previamente
            if (!_dicVariableId.ContainsKey(strVariableNombre))
                new Exception("El nombre de la variable no existe");
            Int32 intRestriccionId = _dicRestriccionId[strRestriccionNombre];
            Int32 intVariableId = _dicVariableId[strVariableNombre];
            _lstRestricciones[intRestriccionId].AddVariable(intVariableId, strVariableNombre, _dicIdVariableTipo[intVariableId], _dicIdVariableSemicontinuaMinimo[intVariableId], dblVariableCoeficiente);
        }

        public List<clsProblemaLpRestriccion> GetRestricciones()
        {
            List<clsProblemaLpRestriccion> lstProblemaRestricciones = new List<clsProblemaLpRestriccion>();
            foreach (KeyValuePair<Int32, string> kvPair in _dicIdRestriccion)
            {
                clsProblemaLpRestriccion cRestriccion = new clsProblemaLpRestriccion();
                cRestriccion.Rhs = _lstRestricciones[kvPair.Key].Rhs;
                cRestriccion.RestriccionNombre = _lstRestricciones[kvPair.Key].RestriccionNombre;
                cRestriccion.TipoRestriccion = _lstRestricciones[kvPair.Key].TipoRestriccion;
                cRestriccion.LstRestriccionVariables = _lstRestricciones[kvPair.Key].LstRestriccionVariables;
                lstProblemaRestricciones.Add(cRestriccion);
            }
            return lstProblemaRestricciones;
        }


        /// <summary>
        /// Obtiene todas las variables del problema en una lista
        /// </summary>
        /// <returns></returns>
        public List<clsProblemaLpVariable> GetVariables()
        {
            List<clsProblemaLpVariable> lstProblemaVariables = new List<clsProblemaLpVariable>();
            foreach (KeyValuePair<Int32, string> kvPair in _dicIdVariable)
            {
                clsProblemaLpVariable cVariable = new clsProblemaLpVariable();
                cVariable.VariableId = kvPair.Key;
                cVariable.VariableNombre = kvPair.Value;
                cVariable.EnuTipoVariable = _dicIdVariableTipo[kvPair.Key];
                cVariable.SemicontinuaMinimo = _dicIdVariableSemicontinuaMinimo[kvPair.Key];
                lstProblemaVariables.Add(cVariable);
            }
            return lstProblemaVariables;
        }

        public List<clsProblemaLpVariable> GetRestriccionVariables(string strRestriccionNombre)
        {
            if (!_dicRestriccionId.ContainsKey(strRestriccionNombre))
                new Exception("Restriccion no encontrada");
            return _lstRestricciones[_dicRestriccionId[strRestriccionNombre]].LstRestriccionVariables;
        }
    }

    public class clsProblemaLpRestriccion
    {
        private tipoRestriccion _enuTipoRestriccion = tipoRestriccion.LE; // Tipo de restriccion LessEqual, ...
        private double _dblRhs = 0; // Right hand side de la restriccion
        private List<clsProblemaLpVariable> _lstRestriccionVariables = new List<clsProblemaLpVariable>(); // Listado de variables para esta restriccion
        private string _strRestriccionNombre = ""; // Nombre de la restriccion

        public string RestriccionNombre
        {
            get { return _strRestriccionNombre; }
            set { _strRestriccionNombre = value; }
        }

        internal List<clsProblemaLpVariable> LstRestriccionVariables
        {
            get { return _lstRestriccionVariables; }
            set { _lstRestriccionVariables = value; }
        }

        public tipoRestriccion TipoRestriccion
        {
            get { return _enuTipoRestriccion; }
            set { _enuTipoRestriccion = value; }
        }

        public double Rhs
        {
            get { return _dblRhs; }
            set { _dblRhs = value; }
        }

        public void AddVariable(Int32 intVariableId, string strVariableNombre, tipoVariable enuVariableTipo, double dblSemicontinuaMinimo, double dblVariableCoeficiente)
        {
            clsProblemaLpVariable cVariable = new clsProblemaLpVariable();
            cVariable.VariableId = intVariableId;
            cVariable.VariableNombre = strVariableNombre;
            cVariable.EnuTipoVariable = enuVariableTipo;
            cVariable.VariableCoeficiente = dblVariableCoeficiente;
            cVariable.SemicontinuaMinimo = dblSemicontinuaMinimo;
            _lstRestriccionVariables.Add(cVariable);
        }

    }

    public class clsProblemaLpVariable
    {
        private string _strVariableNombre; // Nombre de la variable
        //private Boolean _blnVariableEsEntera; // Si la variable es entera
        private tipoVariable _enuTipoVariable; // Tipo de variable: entera, binaria,...
        private double dblSemicontinuaMinimo; // Valor minimo de la variable seimcontinua

        public double SemicontinuaMinimo
        {
            get { return dblSemicontinuaMinimo; }
            set { dblSemicontinuaMinimo = value; }
        }

        private Int32 _intVariableId; // Id de la variable viene de _dicVariableId
        private double _dblVariableCoeficiente; // Coeficiente de la variable

        public tipoVariable EnuTipoVariable
        {
            get { return _enuTipoVariable; }
            set { _enuTipoVariable = value; }
        }

        public string VariableNombre
        {
            get { return _strVariableNombre; }
            set { _strVariableNombre = value; }
        }

        //public Boolean VariableEsEntera
        //{
        //    get { return _blnVariableEsEntera; }
        //    set { _blnVariableEsEntera = value; }
        //}

        public Int32 VariableId
        {
            get { return _intVariableId; }
            set { _intVariableId = value; }
        }


        public double VariableCoeficiente
        {
            get { return _dblVariableCoeficiente; }
            set { _dblVariableCoeficiente = value; }
        }

    }

}
