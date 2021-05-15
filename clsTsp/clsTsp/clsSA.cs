using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;


namespace clsTsp
{
    [Serializable]
    class clsPunto
    {
        public Int32 intX;
        public Int32 intY;
    }

    class clsSA
    {
        double dblCosteBest = double.MaxValue;

        public void GenerarProblemas(Int32 intNumeroCiudades, Int32 intNumeroProblemas, string strPathDirProblemas)
        {
            Int32 intWidth = 400;
            Int32 intHeight = 400;

            for (Int32 intI = 0; intI < intNumeroProblemas; intI++)
            {
                List<clsPunto> lstPuntos = GenerarProblema(intWidth, intHeight, intNumeroCiudades, false);
                string strPathProblema = strPathDirProblemas + intNumeroCiudades + "_" + (intI + 1) + ".ite";
                clsWriteObjectToFile.WriteToBinaryFile<List<clsPunto>>(strPathProblema, lstPuntos);
            }
        }

        public void GenerarIteraciones(string strPathProblema)
        {
            if (!File.Exists(strPathProblema))
                new Exception("Fichero no encontrado");
            List<clsPunto> lstPuntos = clsWriteObjectToFile.ReadFromBinaryFile<List<clsPunto>>(strPathProblema);
            string strPathProblemaOut = strPathProblema.Replace("Problemas", "iteraciones");
            strPathProblemaOut = strPathProblemaOut.Replace(".ite", ".csv");
            Optimizar(lstPuntos, strPathProblema, strPathProblemaOut);

        }

        


        public void Optimizar(List<clsPunto> lstPuntos, string strPathFileIn, string strPathFileOut)
        {
            StringBuilder sbFile = new StringBuilder();
            sbFile.Append("strSolucion;dblCosteSolucion;strSolucionAnterior;dblCosteSolucionAnterior;strAccionMinimoCoste;dblCosteAccionMinimoCoste" + Environment.NewLine);
            Random rnd = new Random();
            double dblTini = 1000;
            double dblAlpha = 0.998;
            double dblTfin = 1;
            double dblT = dblTini;
            Int32 intNumIteracionesPorTemperatura = 100;
            Int32 intCuenta = 0;
            double dblCosteOld = 0;
            double dblNumItera = Math.Log(dblTfin / dblTini) / Math.Log(dblAlpha);
            Console.WriteLine("Numero Interaciones:" + dblNumItera);
            intCuenta++;
            List<clsPunto> lstPuntosBest = new List<clsPunto>();
            // Hace el recorrido inicial
            List<Int32> lstRecorrido = new List<int>();
            for (Int32 intI = 0; intI < lstPuntos.Count; intI++)
                lstRecorrido.Add(intI);
            double dblCoste = CalcularDistanciaRecorrido(lstPuntos, lstRecorrido);
            // Realiza la busqueda utilizando SA
            while (dblT > dblTfin)
            {
                // Realiza un conjunto de interaciones intermedias
                for (Int32 intK = 0; intK < intNumIteracionesPorTemperatura; intK++)
                {
                    // Propone la ciudad a cambiar
                    Int32 intIndexCambiar1 = rnd.Next(0, lstRecorrido.Count - 1);
                    Int32 intIndexCambiar2 = intIndexCambiar1 + 1;
                    if (intIndexCambiar2 >= lstPuntos.Count)
                        intIndexCambiar2 = 0;
                    // realiza el cambio
                    sbFile.Append(GenerarLineaCambio(intIndexCambiar1, intIndexCambiar2, lstPuntos, ref lstRecorrido, ref dblCoste));

                    // Comprueba si es el mejor
                    if (dblCoste < dblCosteBest)
                    {
                        dblCosteBest = dblCoste;
                        Console.WriteLine(intCuenta + "->" + dblCosteBest);
                        //lstPuntosBest = new List<clsPunto>();
                        //foreach (clsPunto cPuntoAux in lstPuntos)
                        //    lstPuntosBest.Add(cPuntoAux);

                    }
                    double dblIncremento = dblCoste - dblCosteOld;
                    double dblExp = Math.Exp(-dblIncremento / dblT);
                    double dblRandom = rnd.NextDouble();
                    // Lo acepta
                    if (dblExp > dblRandom)
                    {
                        dblCosteOld = dblCoste;
                    }
                    else // no lo acepta
                    {
                        // Vuelve para detras
                        dblCoste = CambiarYCoste(dblCoste, intIndexCambiar1, intIndexCambiar2, lstPuntos, ref lstRecorrido);
                    }

                    intCuenta++;
                    dblCosteOld = dblCoste;
                }
                Console.WriteLine("Antes Quitar Cruz->" + dblCoste);
                string strLineas = QuitarUnaCruz(lstPuntos, ref dblCoste, ref lstRecorrido);
                if (strLineas != "")
                    sbFile.Append(strLineas);
                Console.WriteLine("Despues Quitar Cruz->" + dblCoste);
                dblT = dblAlpha * dblT;
                // lstPuntos = new List<clsPunto>();
                //foreach (clsPunto cPuntoAux in lstPuntosBest)
                //    lstPuntos.Add(cPuntoAux);
                //dblCosteOld = dblCosteBest;
            }
            Console.WriteLine(intCuenta + "->" + dblCoste);
            Console.WriteLine("comienza quitar cruces");
            // Quita el resto de las cruces
            Boolean blnEnBucle = true;
            while (blnEnBucle)
            {
                string strLineas = QuitarUnaCruz(lstPuntos, ref dblCoste, ref lstRecorrido);
                if (strLineas != "")
                    sbFile.Append(strLineas);
                else
                    blnEnBucle = false;
            }
            Console.WriteLine(intCuenta + "->" + dblCoste);
            // Guarda el fichero en disco
            File.WriteAllText(strPathFileOut, sbFile.ToString().Trim(), Encoding.GetEncoding("windows-1252"));
            // Guarda la imagen del recorrido final
            strPathFileOut = strPathFileOut.Replace(".csv", ".jpg");
            //PintarRecorrido(lstPuntos, lstRecorrido, 500, 500, strPathFileOut, false);
        }



    
        private string GenerarLineaCambio(Int32 intIndexCambiar1, Int32 intIndexCambiar2, List<clsPunto> lstPuntos, ref List<Int32> lstRecorrido, ref double dblCoste)
        {
            StringBuilder sbLine = new StringBuilder();
            // realiza el cambio
            string strSolucionOld = string.Join("|", lstRecorrido.ToArray());
            double dblCosteOld = dblCoste;
            dblCoste = CambiarYCoste(dblCoste, intIndexCambiar1, intIndexCambiar2, lstPuntos, ref lstRecorrido);
            string strSolucion = string.Join("|", lstRecorrido.ToArray());
            sbLine.Append(strSolucion + ";" + dblCoste + ";" + strSolucionOld + ";" + dblCosteOld + ";");
            // Genera los potenciales cambios
            double dblCosteMin = double.MaxValue;
            string strAccionMin = "";
            for (Int32 intIndex = 0; intIndex < (lstRecorrido.Count - 1); intIndex++)
            {
                Int32 intIndex1 = intIndex;
                Int32 intIndex2 = intIndex + 1;
                string strAccion = lstRecorrido[intIndex1] + "_" + lstRecorrido[intIndex2];
                double dblCosteAux = CambiarYCoste(dblCoste, intIndex1, intIndex2, lstPuntos, ref lstRecorrido);
                if (dblCosteAux < dblCosteMin)
                {
                    dblCosteMin = dblCosteAux;
                    strAccionMin = strAccion;
                }
                // Deshace el cambio
                dblCoste = CambiarYCoste(dblCosteAux, intIndex1, intIndex2, lstPuntos, ref lstRecorrido);
            }
            sbLine.Append(strAccionMin + ";" + dblCosteMin + Environment.NewLine);
            return sbLine.ToString();
        }


        public void OptimizarOld(Int32 intRun)
        {
            Random rnd = new Random();
            Int32 intWidth = 1920;
            Int32 intHeight = 1080;
            Int32 intNumCiudades = 150;

            double dblTini = 1000;
            double dblAlpha = 0.999;
            double dblTfin = 1;
            double dblT = dblTini;
            Int32 intCuenta = 0;
            List<clsPunto> lstPuntos = new List<clsPunto>();
            string strPathProblema = @"c:\borrar\SA\problema.ite";
            string strFilePathOutput = @"c:\borrar\SA\" + intCuenta.ToString() + ".jpg";
            if (System.IO.File.Exists(strPathProblema))
            {
                lstPuntos = clsWriteObjectToFile.ReadFromBinaryFile<List<clsPunto>>(strPathProblema);
                dblCosteBest = PintarYGuardar(lstPuntos, intWidth, intHeight, strFilePathOutput, false);
            }
            else
            {
                lstPuntos = GenerarProblema(intWidth, intHeight, intNumCiudades, false);
                //while (QuitarCruces(ref lstPuntos, dblCosteBest, intWidth, intHeight, @"c:\borrar\SA\", ref intCuenta)) ;
                clsWriteObjectToFile.WriteToBinaryFile<List<clsPunto>>(strPathProblema, lstPuntos);
                PintarYGuardar(lstPuntos, intWidth, intHeight, strFilePathOutput, true);
                intCuenta++;
            }
            strFilePathOutput = @"c:\borrar\SA\" + intCuenta.ToString() + ".jpg";
            double dblCosteOld = PintarYGuardar(lstPuntos, intWidth, intHeight, strFilePathOutput, false);
            intCuenta++;
            double dblCoste = 0;
            List<clsPunto> lstPuntosBest = new List<clsPunto>();
            while (dblT > dblTfin)
            {
                //for (Int32 intK = 0; intK < 100; intK++)
                //{
                //    // Propone la ciudad a cambiar
                Int32 intCiudadCambiar1 = rnd.Next(0, lstPuntos.Count - 1);
                Int32 intCiudadCambiar2 = intCiudadCambiar1;
                while (intCiudadCambiar1 == intCiudadCambiar2)
                {
                    intCiudadCambiar2 = rnd.Next(0, lstPuntos.Count - 1);
                }
                // realiza el cambio
                Cambiar(intCiudadCambiar1, intCiudadCambiar2, ref lstPuntos);

                //strFilePathOutput = @"c:\borrar\SA\" + ("0000000" + intRun.ToString()).PadRight(3) + "_" + ("0000000" + intCuenta.ToString()).PadRight(5) + ".jpg";
                strFilePathOutput = @"c:\borrar\SA\" + intCuenta.ToString() + ".jpg";
                dblCoste = PintarYGuardar(lstPuntos, intWidth, intHeight, strFilePathOutput, false);

                if (dblCoste < dblCosteBest)
                {
                    dblCosteBest = dblCoste;
                    Console.WriteLine(intCuenta + "->" + dblCosteBest);
                    lstPuntosBest = new List<clsPunto>();
                    foreach (clsPunto cPuntoAux in lstPuntos)
                        lstPuntosBest.Add(cPuntoAux);
                    clsWriteObjectToFile.WriteToBinaryFile<List<clsPunto>>(strPathProblema, lstPuntosBest);
                }
                double dblIncremento = dblCoste - dblCosteOld;
                double dblExp = Math.Exp(-dblIncremento / dblT);
                double dblRandom = rnd.NextDouble();
                // Lo acepta
                if (dblExp > dblRandom)
                {
                    dblCosteOld = dblCoste;
                }
                else // no lo acepta
                {
                    // Vuelve para detras
                    Cambiar(intCiudadCambiar1, intCiudadCambiar2, ref lstPuntos);
                }

                intCuenta++;
                //}
                dblT = dblAlpha * dblT;
                // lstPuntos = new List<clsPunto>();
                //foreach (clsPunto cPuntoAux in lstPuntosBest)
                //    lstPuntos.Add(cPuntoAux);
                //dblCosteOld = dblCosteBest;
            }

            while (QuitarCruces(ref lstPuntosBest, dblCosteBest, intWidth, intHeight, @"c:\borrar\SA\", ref intCuenta)) ;
            clsWriteObjectToFile.WriteToBinaryFile<List<clsPunto>>(strPathProblema, lstPuntosBest);
        }

        private double CambiarYCoste(double dblCosteActual, Int32 intIndexCambiar1, Int32 intIndexCambiar2, List<clsPunto> lstPuntos, ref List<Int32> lstRecorrido)
        {
            //Se supone 0->1->2->3 y tras el cambio 0->2->1->3
            Int32 intIndexCambiar0 = intIndexCambiar1 - 1;
            if (intIndexCambiar0 < 0)
                intIndexCambiar0 = lstPuntos.Count - 1;
            Int32 intIndexCambiar3 = intIndexCambiar2 + 1;
            if (intIndexCambiar3 > (lstPuntos.Count - 1))
                intIndexCambiar3 = 0;
            // Calcular distancias
            double dblD01 = CalcularDistancia(lstRecorrido[intIndexCambiar0], lstRecorrido[intIndexCambiar1], lstPuntos);
            double dblD02 = CalcularDistancia(lstRecorrido[intIndexCambiar0], lstRecorrido[intIndexCambiar2], lstPuntos);
            double dblD13 = CalcularDistancia(lstRecorrido[intIndexCambiar1], lstRecorrido[intIndexCambiar3], lstPuntos);
            double dblD23 = CalcularDistancia(lstRecorrido[intIndexCambiar2], lstRecorrido[intIndexCambiar3], lstPuntos);
            // Cambiar los puntos
            Int32 intPunto1 = lstRecorrido[intIndexCambiar1];
            lstRecorrido[intIndexCambiar1] = lstRecorrido[intIndexCambiar2];
            lstRecorrido[intIndexCambiar2] = intPunto1;
            //Devuelve coste
            return (dblCosteActual - dblD01 - dblD23 + dblD02 + dblD13);
        }

        private double CalcularDistanciaRecorrido(List<clsPunto> lstPuntos, List<Int32> lstRecorrido)
        {
            double dblDistancia = 0;
            for (Int32 intIndex = 0; intIndex < (lstRecorrido.Count - 1); intIndex++)
            {
                dblDistancia = dblDistancia + CalcularDistancia(lstRecorrido[intIndex], lstRecorrido[intIndex + 1], lstPuntos);
            }
            dblDistancia = dblDistancia + CalcularDistancia(lstRecorrido[lstRecorrido.Count - 1], lstRecorrido[0], lstPuntos);
            return dblDistancia;
        }

        private double CalcularDistancia(Int32 intIndex1, Int32 intIndex2, List<clsPunto> lstPuntos)
        {
            double dblDistancia = Math.Sqrt(((lstPuntos[intIndex1].intX - lstPuntos[intIndex2].intX) * (lstPuntos[intIndex1].intX - lstPuntos[intIndex2].intX)) + ((lstPuntos[intIndex1].intY - lstPuntos[intIndex2].intY) * (lstPuntos[intIndex1].intY - lstPuntos[intIndex2].intY)));
            return dblDistancia;
        }

        private void Cambiar(Int32 intIndexCambiar1, Int32 intIndexCambiar2, ref List<clsPunto> lstPuntos)
        {

            clsPunto cPunto1 = new clsPunto();
            cPunto1.intX = lstPuntos[intIndexCambiar1].intX;
            cPunto1.intY = lstPuntos[intIndexCambiar1].intY;
            lstPuntos[intIndexCambiar1].intX = lstPuntos[intIndexCambiar2].intX;
            lstPuntos[intIndexCambiar1].intY = lstPuntos[intIndexCambiar2].intY;
            lstPuntos[intIndexCambiar2].intX = cPunto1.intX;
            lstPuntos[intIndexCambiar2].intY = cPunto1.intY;

        }



        private List<clsPunto> GenerarProblema(Int32 intXmax, Int32 intYmax, Int32 intNumeroPuntos, Boolean blnConvexo = false)
        {
            List<clsPunto> lstPuntos = new List<clsPunto>();
            Random rnd = new Random(100);
            Int32 intCentroX = Convert.ToInt32(intXmax / 2);
            Int32 intCentroY = Convert.ToInt32(intYmax / 2);
            Int32 intRadioMax = Convert.ToInt32((0.95 * Math.Min(intXmax, intYmax)) / 2);
            Int32 intRadioMin = Convert.ToInt32((0.25 * Math.Min(intXmax, intYmax)) / 2);
            while (lstPuntos.Count < intNumeroPuntos)
            {
                // toma el angulo
                double dblAngulo = rnd.Next(0, 360);
                dblAngulo = dblAngulo / (2 * Math.PI);
                Int32 intRadio = intRadioMax;
                if (!blnConvexo)
                    intRadio = rnd.Next(intRadioMin, intRadioMax);
                double dblX = intCentroX + intRadio * Math.Cos(dblAngulo);
                double dblY = intCentroY + intRadio * Math.Sin(dblAngulo);
                clsPunto cPunto = new clsPunto();
                cPunto.intX = Convert.ToInt32(dblX);
                cPunto.intY = Convert.ToInt32(dblY);
                lstPuntos.Add(cPunto);
            }

            return lstPuntos;
        }


        private string QuitarUnaCruz(List<clsPunto> lstPuntos, ref double dblCoste, ref List<Int32> lstRecorrido)
        {
            StringBuilder sbLineas = new StringBuilder();
            Boolean blnHaMejorado = false;
            //B->E y C->F cambiarlo por B->C y E->F y darle la vuelta a E->...->C pasa a C->...->E
            for (Int32 intI = 0; (intI < lstRecorrido.Count) && !blnHaMejorado; intI++)
            {
                for (Int32 intJ = intI + 2; (intJ < lstRecorrido.Count) && !blnHaMejorado; intJ++)
                {
                    Int32 intB = intI;
                    Int32 intE = intB + 1;
                    if (intE >= lstPuntos.Count)
                        intE = 0;
                    Int32 intC = intJ;
                    if (intC >= lstPuntos.Count)
                        intC = 0;
                    Int32 intF = intC + 1;
                    if (intF >= lstPuntos.Count)
                        intF = 0;
                    // Para que tenga sentido D(BE)+D(CF)>D(BC)+D(EF)
                    double dblBE = CalcularDistancia(lstRecorrido[intB], lstRecorrido[intE], lstPuntos);
                    double dblCF = CalcularDistancia(lstRecorrido[intC], lstRecorrido[intF], lstPuntos);
                    double dblBC = CalcularDistancia(lstRecorrido[intB], lstRecorrido[intC], lstPuntos);
                    double dblEF = CalcularDistancia(lstRecorrido[intE], lstRecorrido[intF], lstPuntos);
                    if ((dblBE + dblCF) > (dblBC + dblEF))
                    {
                        //QuitarCruceCambiarPuntos(intB, intE, intC, intF, ref lstPuntos);
                        Int32 intIni = intE;
                        Int32 intFin = intC;
                        while (intFin > intIni)
                        {
                            for (Int32 intIndex = intIni; intIndex < intFin; intIndex++)
                            {
                                // Va cambiando intIndex por el siguiente
                                string strLinea = GenerarLineaCambio(intIndex, intIndex + 1, lstPuntos, ref lstRecorrido, ref dblCoste);
                                sbLineas.Append(strLinea);
                            }
                            intFin--;
                        }
                        blnHaMejorado = true;
                    }
                }
            }
            return sbLineas.ToString();
        }


        private Boolean QuitarCruces(ref List<clsPunto> lstPuntos, double dblCoste, Int32 intWidth, Int32 intHeight, string strPathDirOutput, ref Int32 intCuenta)
        {
            Boolean blnHaMejorado = false;
            //B->E y C->F cambiarlo por B->C y E->F y darle la vuelta a E->...->C pasa a C->...->E
            for (Int32 intI = 0; intI < lstPuntos.Count; intI++)
            {
                for (Int32 intJ = intI + 2; intJ < lstPuntos.Count; intJ++)
                {
                    Int32 intB = intI;
                    Int32 intE = intB + 1;
                    if (intE >= lstPuntos.Count)
                        intE = 0;
                    Int32 intC = intJ;
                    if (intC >= lstPuntos.Count)
                        intC = 0;
                    Int32 intF = intC + 1;
                    if (intF >= lstPuntos.Count)
                        intF = 0;
                    // Para que tenga sentido D(BE)+D(CF)>D(BC)+D(EF)
                    double dblBE = Distancia(lstPuntos[intB], lstPuntos[intE]);
                    double dblCF = Distancia(lstPuntos[intC], lstPuntos[intF]);
                    double dblBC = Distancia(lstPuntos[intB], lstPuntos[intC]);
                    double dblEF = Distancia(lstPuntos[intE], lstPuntos[intF]);
                    if ((dblBE + dblCF) > (dblBC + dblEF))
                    {
                        string strFilePathOutput = strPathDirOutput + intCuenta + ".jpg";
                        //intCuenta++;
                        //dblCoste = PintarYGuardar(lstPuntos, intWidth, intHeight, strFilePathOutput, true, intB, intC);
                        QuitarCruceCambiarPuntos(intB, intE, intC, intF, ref lstPuntos);
                        strFilePathOutput = strPathDirOutput + intCuenta + ".jpg";
                        dblCoste = PintarYGuardar(lstPuntos, intWidth, intHeight, strFilePathOutput, false, true, intB, intE);
                        blnHaMejorado = true;
                        intCuenta++;
                    }

                }
            }
            return blnHaMejorado;
        }

        private void QuitarCruceCambiarPuntos(Int32 intB, Int32 intE, Int32 intC, Int32 intF, ref List<clsPunto> lstPuntos)
        {
            Int32 intSalto = intC - intE;
            if (intSalto < 0)
            {

            }
            Int32 intCuenta = 0;
            for (Int32 intI = intE; intI <= (intC - intCuenta); intI++)
            {
                clsPunto cPunto = new clsPunto();
                cPunto.intX = lstPuntos[intI].intX;
                cPunto.intY = lstPuntos[intI].intY;
                lstPuntos[intI] = lstPuntos[intC - intCuenta];
                lstPuntos[intC - intCuenta] = cPunto;
                intCuenta++;
            }

        }
        private double Distancia(clsPunto cPunto1, clsPunto cPunto2)
        {
            double dblDistancia = Math.Sqrt((cPunto1.intX - cPunto2.intX) * (cPunto1.intX - cPunto2.intX) + (cPunto1.intY - cPunto2.intY) * (cPunto1.intY - cPunto2.intY));
            return dblDistancia;
        }

        private double PintarYGuardar(List<clsPunto> lstPuntos, Int32 intWidth, Int32 intHeight, string strFilePathOutput, Boolean blnPintarSoloCiudades, Boolean blnEsQuitarCruce = false, Int32 intIndexCruceIni1 = -1, Int32 intIndexCruceIni2 = -1)
        {
            double dblCoste = 0;
            Bitmap bm = new Bitmap(intWidth, intHeight);
            Image img = new Bitmap(bm);
            Graphics drawing = Graphics.FromImage(img);
            drawing.FillRectangle(Brushes.White, 0, 0, img.Width, img.Height);
            Pen BlackPen = new Pen(Color.DarkGreen, 1);
            Pen RedPen = new Pen(Color.DarkGreen, 1);

            for (Int32 intIndex = 1; intIndex < lstPuntos.Count; intIndex++)
            {
                float dblOldX = lstPuntos[intIndex - 1].intX;
                float dblOldY = lstPuntos[intIndex - 1].intY;
                float dblX = lstPuntos[intIndex].intX;
                float dblY = lstPuntos[intIndex].intY;
                Rectangle rec = new Rectangle(Convert.ToInt32(dblX - 3), Convert.ToInt32(dblY - 3), 6, 6);
                drawing.FillRectangle(Brushes.DarkGray, rec);
                if (intIndex == 1)
                {
                    rec = new Rectangle(Convert.ToInt32(dblOldX - 3), Convert.ToInt32(dblOldY - 3), 6, 6);
                    drawing.FillRectangle(Brushes.DarkGray, rec);
                }
                if (!blnPintarSoloCiudades)
                {
                    if (intIndexCruceIni1 == (intIndex - 1) || intIndexCruceIni2 == (intIndex - 1))
                        drawing.DrawLine(RedPen, dblOldX, dblOldY, dblX, dblY);
                    else
                        drawing.DrawLine(BlackPen, dblOldX, dblOldY, dblX, dblY);
                }
                dblCoste = dblCoste + Math.Sqrt((dblX - dblOldX) * (dblX - dblOldX) + (dblY - dblOldY) * (dblY - dblOldY));
            }
            // Cierra el loop
            if (lstPuntos.Count > 1)
            {
                float dblOldX = lstPuntos[lstPuntos.Count - 1].intX;
                float dblOldY = lstPuntos[lstPuntos.Count - 1].intY;
                float dblX = lstPuntos[0].intX;
                float dblY = lstPuntos[0].intY;
                if (!blnPintarSoloCiudades)
                {
                    if (intIndexCruceIni1 == (lstPuntos.Count - 1) || intIndexCruceIni2 == (lstPuntos.Count - 1))
                        drawing.DrawLine(RedPen, dblOldX, dblOldY, dblX, dblY);
                    else
                        drawing.DrawLine(BlackPen, dblOldX, dblOldY, dblX, dblY);
                }
                dblCoste = dblCoste + Math.Sqrt((dblX - dblOldX) * (dblX - dblOldX) + (dblY - dblOldY) * (dblY - dblOldY));
            }
            Font font = new Font("Century Gothic", 40);
            Brush brush = new SolidBrush(Color.Green);
            string strTexto = "ITelligent";
            if (blnEsQuitarCruce)
                strTexto = strTexto + " (Quitando Cruces)";
            drawing.DrawString(strTexto, font, brush, 100, 50);
            font = new Font("Eurostile", 20);
            string strCoste = Convert.ToInt32(dblCoste).ToString("N");//, CultureInfo.InvariantCulture);
            drawing.DrawString(strCoste, font, brush, 100, 120);
            drawing.Save();
            if (dblCoste < dblCosteBest || blnEsQuitarCruce)
                img.Save(strFilePathOutput);
            //bm.Save(strFilePathOutput);
            drawing.Dispose();
            bm.Dispose();
            return dblCoste;

        }

    }



}



