using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsVehicleRouting
{[Serializable ]
    class clsDictionarySorted
    {
        Random rnd = new Random(100);
        double dblMultiplicador = 0.0001;        
        Dictionary<string, double > dicInverse = new Dictionary<string, double>(); // Este guarda el contrario de Key a valor (valorNew)
        SortedDictionary<double, string> sdDirect = new SortedDictionary<double, string>(); // Diccionario ordenado de valor (valorNew) a key        

        public double  Add(string strKey, double dblValor)
        {
            double dblValorOld = dblValor;
            // Comprueba que la key no exista
            if (dicInverse.ContainsKey(strKey))
                new Exception("La key introducida ya existe");
            // Si el intValue ya esta le va añadiendo un valor hasta que consiga que no este
            while (sdDirect.ContainsKey(dblValor))
                dblValor = dblValor + rnd.NextDouble () *dblMultiplicador ;           
            // Ya tiene un intValue que es unico lo añade
            sdDirect.Add(dblValor, strKey);
            dicInverse.Add(strKey, dblValor);
            return dblValor;
        }

        public double  Update(string strKey, double dblValor)
        {
            double dblValorOld = dblValor;
            // Comprueba que la key no exista
            if (!dicInverse.ContainsKey(strKey))
                new Exception("La key introducida ya existe");
            double dblValueOld = dicInverse[strKey];
            // Si el intValue ya esta le va añadiendo un valor hasta que consiga que no este
            while (sdDirect.ContainsKey(dblValor))
                dblValor = dblValor + rnd.NextDouble() * dblMultiplicador;            
            // Añade el nuevo valor
            dicInverse[strKey] = dblValor;
            sdDirect.Remove(dblValueOld);
            sdDirect.Add(dblValor, strKey);
            return dblValor;
        }

        public void Remove(string strKey)
        {
            // Comprueba que la key no exista
            if (!dicInverse.ContainsKey(strKey))
                new Exception("La key introducida ya existe");
            double  dblValueOld = dicInverse[strKey];
            dicInverse.Remove(strKey);
            sdDirect.Remove(dblValueOld);
        }

        /// <summary>
        /// Comprueba si la key existe
        /// </summary>
        /// <param name="strKey"></param>
        /// <returns></returns>
        public Boolean  ExistsKey(string strKey)
        {
            // Comprueba que la key no exista
            if (!dicInverse.ContainsKey(strKey))
                return false;
            return true;
        }

        /// <summary>
        /// Comprueba si el valor existe.
        /// </summary>
        /// <param name="dblValor"></param>
        /// <returns></returns>
        public Boolean  ExistsValue(double dblValor)
        {
            // Comprueba que la key no exista
            if (!sdDirect.ContainsKey(dblValor))
                return false;
            return true;            
        }

        public double GetValue(string strKey)
        {
            // Comprueba que la key no exista
            if (!dicInverse.ContainsKey(strKey))
                new Exception("La key introducida ya existe");
            double dblValueOld = dicInverse[strKey];            
            return dblValueOld;
        }

        public string GetKey(double dblValor)
        {            
            // Comprueba que la key no exista
            if (!sdDirect.ContainsKey(dblValor))
                new Exception("El valor introducido no existe");
            return sdDirect[dblValor];            
        }

        public double GetMinFirstValue(Int32 intIndex)
        {
            double dblValor = sdDirect.ElementAt(intIndex).Key;
                       return dblValor;
        }

        public string GetMinFirstKey(Int32 intIndex)
        {
            return sdDirect.ElementAt(intIndex).Value;

        }

        public Int32 Count()
        {
            return sdDirect.Count();
        }

    }
}
