using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lpsolve55;


namespace clsVehicleRouting
{
    class clsVehicleRouting
    {
        [STAThread]
        /* unsafe is needed to make sure that these function are not relocated in memory by the CLR. If that would happen, a crash occurs */
        /* go to the project property page and in “configuration properties>build” set Allow Unsafe Code Blocks to True. */
        /* see http://msdn2.microsoft.com/en-US/library/chfa2zb8.aspx and http://msdn2.microsoft.com/en-US/library/t2yzs44b.aspx */
        private /* unsafe */ static void logfunc(IntPtr lp, int userhandle, string Buf)
        {
            System.Diagnostics.Debug.Write(Buf);
        }

        private /* unsafe */ static byte ctrlcfunc(IntPtr lp, int userhandle)
        {
            /* 'If set to true, then solve is aborted and returncode will indicate this. */
            return (0);
        }

        private /* unsafe */ static void msgfunc(int lp, int userhandle, lpsolve.lpsolve_msgmask message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        private static void ThreadProc(object filename)
        {
            IntPtr lp;
            lpsolve.lpsolve_return ret;
            double o;

            lp = lpsolve.read_LP((string)filename, 0, "");
            ret = lpsolve.solve(lp);
            o = lpsolve.get_objective(lp);
            //Debug.Assert(ret == lpsolve.lpsolve_return.OPTIMAL && Math.Round(o, 13) == 1779.4810350637485);
            lpsolve.delete_lp(lp);
        }
        private const Int64 BIGM = 100000000;

        static public void OptimizarRuta(clsDataProblem cProblem)
        {
            clsProblemaLp cLp = new clsProblemaLp();
            // Obtiene las variables que pueden no ser interesantes para reducir la dimension del problema
            HashSet<string> hsParesValidos = new HashSet<string>();
            hsParesValidos = ReducirDimensionPares(cProblem, 0.4);
            // Introduce la variables de decicion
            for (Int32 intP2 = 0; intP2 <= cProblem.intPuntosConsumoNumber; intP2++)
            {
                // Introduce cada origen y vehiculo
                for (Int32 intP1 = 0; intP1 <= cProblem.intPuntosConsumoNumber; intP1++)
                {
                    // No puede ser el mismo punto
                    if (intP1 != intP2 && hsParesValidos.Contains(intP1 + "_" + intP2))
                    {
                        // Cada vehiculo
                        for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
                        {
                            string strVariableName = "De:" + intP1 + "_A:" + intP2 + "_Vehiculo:" + intV;
                            if (!cLp.VariableExiste(strVariableName))
                                cLp.AddVariable(strVariableName, tipoVariable.Binarias);
                        }
                    }
                }
            }
            // Suma de las llegadas a un destino y vehiculo es igual a 1 solo vehiculo
            for (Int32 intP2 = 1; intP2 <= cProblem.intPuntosConsumoNumber; intP2++)
            {
                string strRestriccionName = "Llegada_solo_1_vehiculo_DESTINO:" + intP2;
                cLp.AddRestriccion(strRestriccionName, tipoRestriccion.EQ, 1);
                // Introduce cada origen y vehiculo
                for (Int32 intP1 = 0; intP1 <= cProblem.intPuntosConsumoNumber; intP1++)
                {
                    // No puede ser el mismo punto
                    if (intP1 != intP2 && hsParesValidos.Contains(intP1 + "_" + intP2))
                    {
                        // Cada vehiculo
                        for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
                        {
                            string strVariableName = "De:" + intP1 + "_A:" + intP2 + "_Vehiculo:" + intV;
                            cLp.AddRestriccionVariable(strRestriccionName, strVariableName, 1);
                        }
                    }
                }
            }
            // Suma de las salidas hacia un destino y vehiculo es igual a 1 solo vehiculo
            for (Int32 intP1 = 1; intP1 <= cProblem.intPuntosConsumoNumber; intP1++)
            {
                string strRestriccionName = "Salida_solo_1_vehiculo_SALIDA:" + intP1;
                cLp.AddRestriccion(strRestriccionName, tipoRestriccion.EQ, 1);
                // Introduce cada origen y vehiculo
                for (Int32 intP2 = 0; intP2 <= cProblem.intPuntosConsumoNumber; intP2++)
                {
                    // No puede ser el mismo punto
                    if (intP1 != intP2 && hsParesValidos.Contains(intP1 + "_" + intP2))
                    {
                        // Cada vehiculo
                        for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
                        {
                            string strVariableName = "De:" + intP1 + "_A:" + intP2 + "_Vehiculo:" + intV;
                            cLp.AddRestriccionVariable(strRestriccionName, strVariableName, 1);
                        }
                    }
                }
            }
            // Cada vehiculo debe salir de depot
            for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
            {
                string strRestriccionName = "Salida_depot_VEHICULO:" + intV;
                cLp.AddRestriccion(strRestriccionName, tipoRestriccion.EQ, 1);
                Int32 intP1 = 0;
                for (Int32 intP2 = 1; intP2 <= cProblem.intPuntosConsumoNumber; intP2++)
                {
                    // No puede ser el mismo punto
                    if (intP1 != intP2 && hsParesValidos.Contains(intP1 + "_" + intP2))
                    {
                        // Cada vehiculo
                        string strVariableName = "De:" + intP1 + "_A:" + intP2 + "_Vehiculo:" + intV;
                        cLp.AddRestriccionVariable(strRestriccionName, strVariableName, 1);
                    }
                }
            }
            // Cada vehiculo debe llegar al depot
            for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
            {
                string strRestriccionName = "Llegada_depot_VEHICULO:" + intV;
                cLp.AddRestriccion(strRestriccionName, tipoRestriccion.EQ, 1);
                Int32 intP2 = 0;
                // Introduce cada origen y vehiculo
                for (Int32 intP1 = 1; intP1 <= cProblem.intPuntosConsumoNumber; intP1++)
                {
                    // No puede ser el mismo punto
                    if (intP1 != intP2 && hsParesValidos.Contains(intP1 + "_" + intP2))
                    {
                        // Cada vehiculo
                        string strVariableName = "De:" + intP1 + "_A:" + intP2 + "_Vehiculo:" + intV;
                        cLp.AddRestriccionVariable(strRestriccionName, strVariableName, 1);
                    }
                }
            }
            //El mismo vehiculo que llega a un punto de consumo debe salir de ese punto de consumo
            // SiXiuk=SjXujk para todo u y k(vehiculos)
            for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
            {
                for (Int32 intP = 1; intP <= cProblem.intPuntosConsumoNumber; intP++)
                {
                    string strRestriccionName = "Llega_A:" + intP + "_mismo_VEHICULO:" + intV + "_sale_DE:" + intP;
                    cLp.AddRestriccion(strRestriccionName, tipoRestriccion.EQ, 0);
                    for (Int32 intP1 = 0; intP1 <= cProblem.intPuntosConsumoNumber; intP1++)
                    {
                        // No puede ser el mismo punto
                        if (intP1 != intP && (hsParesValidos.Contains(intP1 + "_" + intP) && hsParesValidos.Contains(intP + "_" + intP1)))
                        {
                            string strVariableName = "De:" + intP1 + "_A:" + intP + "_Vehiculo:" + intV;
                            cLp.AddRestriccionVariable(strRestriccionName, strVariableName, 1);
                            strVariableName = "De:" + intP + "_A:" + intP1 + "_Vehiculo:" + intV;
                            cLp.AddRestriccionVariable(strRestriccionName, strVariableName, -1);
                        }
                    }
                }
            }
            // Lo que llega a un punto menos lo que sale debe ser igual a la demanda si ese coche 
            // sirve ese punto
            for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
            {
                for (Int32 intP = 1; intP <= cProblem.intPuntosConsumoNumber; intP++)
                {
                    string strRestriccionName = "Llega_A:" + intP + "_Menos_sale_DE:" + intP + "_mismo_VEHICULO:" + intV;
                    cLp.AddRestriccion(strRestriccionName, tipoRestriccion.EQ, 0);
                    for (Int32 intP1 = 0; intP1 <= cProblem.intPuntosConsumoNumber; intP1++)
                    {
                        // No puede ser el mismo punto
                        if (intP1 != intP && (hsParesValidos.Contains(intP1 + "_" + intP)))
                        {
                            string strVariableName = "Cantidad_lleva_De:" + intP1 + "_A:" + intP + "_Vehiculo:" + intV;
                            if (!cLp.VariableExiste(strVariableName))
                                cLp.AddVariable(strVariableName, tipoVariable.RealMayorCero);
                            cLp.AddRestriccionVariable(strRestriccionName, strVariableName, 1);
                            strVariableName = "Cantidad_lleva_De:" + intP + "_A:" + intP1 + "_Vehiculo:" + intV;
                            if (!cLp.VariableExiste(strVariableName))
                                cLp.AddVariable(strVariableName, tipoVariable.RealMayorCero);
                            cLp.AddRestriccionVariable(strRestriccionName, strVariableName, -1);
                            strVariableName = "De:" + intP1 + "_A:" + intP + "_Vehiculo:" + intV;
                            cLp.AddRestriccionVariable(strRestriccionName, strVariableName, -cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intP].dblDemand);
                        }
                    }
                }
            }
            // La cantidad que llega a un punto de consumo desde otro punto debe ser mayor o igual a su demanda (o cero si ese transporte no se activa)
            // La cantidad que llega a un punto de consumo desde otro punto debe ser menor que lo que puede llevar el coche menos la demanda del pu
            for (Int32 intP1 = 0; intP1 <= cProblem.intPuntosConsumoNumber; intP1++)
            {
                for (Int32 intP2 = 1; intP2 <= cProblem.intPuntosConsumoNumber; intP2++)
                {
                    for (Int32 intV = 1; intV <= cProblem.intVehiclesNumber; intV++)
                    {
                        // No puede ser el mismo punto
                        if (intP1 != intP2 && hsParesValidos.Contains(intP1 + "_" + intP2))
                        {
                            // Lo que llega a un punto desde otro punto debe ser mayor o igual que la demanda de ese punto (o cero si no se activa)
                            string strRestriccionName = "Cantidad_llegada_VEHICULO:" + intV + "_DESDE:" + intP1 + "_A:" + intP2 + "_mayor_que_demanda";
                            cLp.AddRestriccion(strRestriccionName, tipoRestriccion.LE, 0);
                            string strVariableName = "De:" + intP1 + "_A:" + intP2 + "_Vehiculo:" + intV;
                            cLp.AddRestriccionVariable(strRestriccionName, strVariableName, cProblem.dicIdPuntoConsumoADatosPuntoConsumo[intP2].dblDemand);
                            strVariableName = "Cantidad_lleva_De:" + intP1 + "_A:" + intP2 + "_Vehiculo:" + intV;
                            cLp.AddRestriccionVariable(strRestriccionName, strVariableName, -1);
                            // Lo que llega a un punto desde otro punto debe ser menor que la capacidad del vehiculo menos lo que dejo en el punto anterior
                            strRestriccionName = "Cantidad_llegada_VEHICULO:" + intV + "_DESDE:" + intP1 + "_A:" + intP2 + "_menor_que_capacidad_vehiculo";
                            cLp.AddRestriccion(strRestriccionName, tipoRestriccion.LE, 0);
                            strVariableName = "Cantidad_lleva_De:" + intP1 + "_A:" + intP2 + "_Vehiculo:" + intV;
                            cLp.AddRestriccionVariable(strRestriccionName, strVariableName, 1);
                            strVariableName = "De:" + intP1 + "_A:" + intP2 + "_Vehiculo:" + intV;
                            cLp.AddRestriccionVariable(strRestriccionName, strVariableName, -cProblem.dicIdVehicleCapacity[intV]);
                        }
                    }
                }
            }

            // Funcion objetivo distancia Minimizar distancia maxima (cantidad_maxima_a_minizar) de cada vehiculo            
            for (Int32 intV = 1; intV < cProblem.intVehiclesNumber; intV++)
            {
                string strRestriccionName = "Distancia_maxima_recorrida_VEHICUO:" + intV;
                cLp.AddRestriccion(strRestriccionName ,tipoRestriccion.LE ,0 );
                string strVariableName = "Cantidad_maxima_a_minimizar";
                if (!cLp.VariableExiste(strVariableName))
                {
                    cLp.AddVariable(strVariableName, tipoVariable.RealMayorCero);
                    cLp.AddObjetivo(strVariableName ,1);
                }
                cLp.AddRestriccionVariable(strRestriccionName, strVariableName, -1);
                // Introduce cada origen y vehiculo
                for (Int32 intP1 = 0; intP1 <= cProblem.intPuntosConsumoNumber; intP1++)
                {
                    // Introduce el destino de cada vehiculo
                    for (Int32 intP2 = 0; intP2 <= cProblem.intPuntosConsumoNumber; intP2++)
                    {
                        // No puede ser el mismo punto
                        if (intP1 != intP2 && hsParesValidos.Contains(intP1 + "_" + intP2))
                        {
                            strVariableName = "De:" + intP1 + "_A:" + intP2 + "_Vehiculo:" + intV;
                            string strPuntos = intP1 + "_" + intP2;
                            cLp.AddRestriccionVariable (strRestriccionName , strVariableName, cProblem.dicIdPuntoAIdPuntoBATiempo[strPuntos]);
                        }
                    }
                }
            }
            
            clsParametros cParametros = new clsParametros();
            cParametros.intTimeoutSegundos = 3600*3;

            ResolverLP(cParametros, cLp);

        }

        private static clsResultados ResolverLP(clsParametros cParametros, clsProblemaLp cLp)
        {
            DateTime dtmStart = DateTime.Now;
            lpsolve.Init(".");
            IntPtr lp;
            int release = 0, Major = 0, Minor = 0, build = 0;
            double[] Col;


            // Crea el LP
            // Se Obtiene el total de variables para el problema de optimizacion
            Int32 intTotalVariables = cLp.GetTotalVariables() + 1;
            // Se crea el programa de optimizacion
            lp = lpsolve.make_lp(0, intTotalVariables);
            // Pasa las variables al problema
            List<clsProblemaLpVariable> lstProblemaVariables = cLp.GetVariables();
            foreach (clsProblemaLpVariable cVariable in lstProblemaVariables)
            {
                lpsolve.set_col_name(lp, cVariable.VariableId + 1, cVariable.VariableNombre);
                if (cVariable.EnuTipoVariable == tipoVariable.Binarias)
                    lpsolve.set_binary(lp, cVariable.VariableId + 1, 1);
                else if (cVariable.EnuTipoVariable == tipoVariable.Entera)
                    lpsolve.set_int(lp, cVariable.VariableId + 1, 1);
                else if (cVariable.EnuTipoVariable == tipoVariable.Semicontinua)
                {
                    lpsolve.set_semicont(lp, cVariable.VariableId + 1, 1);
                    lpsolve.set_lowbo(lp, cVariable.VariableId + 1, cVariable.SemicontinuaMinimo);
                }
                else if (cVariable.EnuTipoVariable == tipoVariable.RealMayorCero)
                {
                    //lpsolve.set_int(lp, cVariable.VariableId + 1, true);
                }
                else
                {
                    new Exception("tipo de variable no contemplada");
                }
            }
            // Pasa las restricciones al problema
            List<clsProblemaLpRestriccion> lstProblemaRestricciones = cLp.GetRestricciones();
            foreach (clsProblemaLpRestriccion cRestriccion in lstProblemaRestricciones)
            {
                double[] dblRows = new double[intTotalVariables];
                // Añade las variables
                foreach (clsProblemaLpVariable cVariable in cRestriccion.LstRestriccionVariables)
                {
                    dblRows[(cVariable.VariableId + 1)] = cVariable.VariableCoeficiente;
                }

                if (cRestriccion.TipoRestriccion == tipoRestriccion.EQ)
                    lpsolve.add_constraint(lp, dblRows, lpsolve.lpsolve_constr_types.EQ, cRestriccion.Rhs);
                else if (cRestriccion.TipoRestriccion == tipoRestriccion.FR)
                    lpsolve.add_constraint(lp, dblRows, lpsolve.lpsolve_constr_types.FR, cRestriccion.Rhs);
                else if (cRestriccion.TipoRestriccion == tipoRestriccion.GE)
                    lpsolve.add_constraint(lp, dblRows, lpsolve.lpsolve_constr_types.GE, cRestriccion.Rhs);
                else if (cRestriccion.TipoRestriccion == tipoRestriccion.LE)
                    lpsolve.add_constraint(lp, dblRows, lpsolve.lpsolve_constr_types.LE, cRestriccion.Rhs);
                else
                    new Exception("Tipo de restriccion no contemplada");
            }
            // Pasa los objetivos al problema
            Dictionary<Int32, double> dicObjetivoIdVariableCoeficiente = cLp.GetObjetivos();
            double[] dblObjetivos = new double[intTotalVariables];
            foreach (KeyValuePair<Int32, double> kvPair in dicObjetivoIdVariableCoeficiente)
            {
                dblObjetivos[kvPair.Key + 1] = kvPair.Value;
            }

            // Si el objetivo es maximizar el numero de escaños con o sin minimizar votos
            //if (cParametros.enuTipoOptimizacion == tipoOptimizacion.MaximizarNumeroEscanos || cParametros.enuTipoOptimizacion == tipoOptimizacion.MaximizarNumeroEscanosYMinimizarTotalVotos)
            //  lpsolve.set_maxim(lp);
            // Crea la funcion objetivo                       
            lpsolve.set_obj_fn(lp, dblObjetivos);
            // Se añaden ciertas cosas que no se
            lpsolve.lp_solve_version(ref Major, ref Minor, ref release, ref build);
            /* let's first demonstrate the logfunc callback feature */
            lpsolve.put_logfunc(lp, new lpsolve.logfunc(logfunc), 0);
            //lpsolve.solve(lp); /* just to see that a message is send via the logfunc routine ... */
            /* ok, that is enough, no more callback */
            lpsolve.put_logfunc(lp, null, 0);
            /* Now redirect all output to a file */
            //lpsolve.set_outputfile(lp, "resultJaime5.txt");
            /* set an abort function. Again optional */
            lpsolve.put_abortfunc(lp, new lpsolve.ctrlcfunc(ctrlcfunc), 0);
            // Se para a los K segundos
            lpsolve.set_timeout(lp, cParametros.intTimeoutSegundos);
            // pay attention to the 1 base and ignored 0 column for constraints

            // Resuelve            
            lpsolve.solve(lp);
            // Obtiene los resultados
            clsResultados cResultados = new clsResultados();
            cResultados.dblObjetivo = lpsolve.get_objective(lp);
            double[] dblCol = new double[lpsolve.get_Ncolumns(lp)];
            lpsolve.get_variables(lp, dblCol);
            for (Int32 intCol = 0; intCol < dblCol.Length; intCol++)
            {
                if (dblCol[intCol] > 0)
                    cResultados.dicVariableValor.Add(lpsolve.get_col_name(lp, intCol + 1), dblCol[intCol]);
            }
            cResultados.intModelStatus = lpsolve.get_status(lp);
            cResultados.strModelStatus = lpsolve.get_statustext(lp, cResultados.intModelStatus);
            //lpsolve.print_objective(lp);
           // lpsolve.print_solution(lp, 1);
            //lpsolve.print_constraints(lp, 1);
            //lpsolve.print_duals(lp);
           // lpsolve.print_debugdump(lp, @"c:\borrar\dump.txt");
          
            DateTime dtmEnd = DateTime.Now;
            cResultados.dblSegundos = (dtmEnd - dtmStart).TotalSeconds;
            lpsolve.delete_lp(lp);

            return cResultados;
        }


        private static HashSet<string> ReducirDimensionPares(clsDataProblem cProblem, double dblPorcentajeMatener)
        {
            HashSet<string> hsParesValidos = new HashSet<string>();
            double[] dblDistancias = new double[cProblem.intPuntosConsumoNumber];
            string[] strPuntos = new string[cProblem.intPuntosConsumoNumber];
            Int32 intIndex = 0;
            Int32 intP1;
            Int32 intP2;
            if (dblPorcentajeMatener < 0 || dblPorcentajeMatener > 1)
                new Exception("Porcentaje no puede ser menor que cero o mayor que uno");
            // Decide cuantos pares deja para los pares que empiezan o terminan en cero
            Int32 intMantenerCero = Convert.ToInt32(dblPorcentajeMatener * cProblem.intPuntosConsumoNumber);
            intMantenerCero = Math.Min(cProblem.intPuntosConsumoNumber, intMantenerCero * cProblem.intVehiclesNumber);
            // Decide cuantos pares deja para los pares que NO empiezan o terminan en cero
            Int32 intMantenerNoCero = Convert.ToInt32(dblPorcentajeMatener * cProblem.intPuntosConsumoNumber);
            intMantenerNoCero = Math.Min(cProblem.intPuntosConsumoNumber, intMantenerNoCero);
            for (intP1 = 1; intP1 <= cProblem.intPuntosConsumoNumber; intP1++)
            {
                dblDistancias = new double[cProblem.intPuntosConsumoNumber];
                strPuntos = new string[cProblem.intPuntosConsumoNumber];
                intIndex = 0;
                for (intP2 = 1; intP2 <= cProblem.intPuntosConsumoNumber; intP2++)
                {
                    if (intP1 != intP2)
                    {
                        strPuntos[intIndex] = intP1 + "_" + intP2;
                        dblDistancias[intIndex] = cProblem.dicIdPuntoAIdPuntoBATiempo[strPuntos[intIndex]];
                        intIndex++;
                    }
                }
                Array.Sort(dblDistancias, strPuntos);
                for (Int32 intI = 0; intI < intMantenerNoCero; intI++)
                {
                    hsParesValidos.Add(strPuntos[intI]);
                }
            }

            // Toma las que salen del depot
            dblDistancias = new double[cProblem.intPuntosConsumoNumber];
            strPuntos = new string[cProblem.intPuntosConsumoNumber];
            intIndex = 0;
            intP1 = 0;
            for (intP2 = 1; intP2 <= cProblem.intPuntosConsumoNumber; intP2++)
            {
                if (intP1 != intP2)
                {
                    strPuntos[intIndex] = intP1 + "_" + intP2;
                    dblDistancias[intIndex] = cProblem.dicIdPuntoAIdPuntoBATiempo[strPuntos[intIndex]];
                    intIndex++;
                }
            }
            Array.Sort(dblDistancias, strPuntos);
            for (Int32 intI = 0; intI < intMantenerCero; intI++)
            {
                hsParesValidos.Add(strPuntos[intI]);
            }
            // Toma las que entran en el depot
            dblDistancias = new double[cProblem.intPuntosConsumoNumber];
            strPuntos = new string[cProblem.intPuntosConsumoNumber];
            intIndex = 0;
            intP2 = 0;
            for (intP1 = 1; intP1 <= cProblem.intPuntosConsumoNumber; intP1++)
            {
                if (intP1 != intP2)
                {
                    strPuntos[intIndex] = intP1 + "_" + intP2;
                    dblDistancias[intIndex] = cProblem.dicIdPuntoAIdPuntoBATiempo[strPuntos[intIndex]];
                    intIndex++;
                }
            }
            Array.Sort(dblDistancias, strPuntos);
            for (Int32 intI = 0; intI < intMantenerCero; intI++)
            {
                hsParesValidos.Add(strPuntos[intI]);
            }


            return hsParesValidos;
        }

    }
}
