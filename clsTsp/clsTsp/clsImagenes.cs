using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Drawing;
using System.IO;


namespace clsTsp
{
    class clsImagenes
    {
        Bitmap _bmImagenBase;


        public double PintarMatrizDistancia(List<clsPunto> lstPuntos, Dictionary<string, double> dicParPuntosToDistancia, Int32 intWidth, Int32 intHeight, string strFilePathOutput)
        {
            // Calcula lo que hay que dejar en x en blanco
            Int32 intMargenX = Convert.ToInt32((double)(intWidth - lstPuntos.Count) / 2);
            Int32 intMargenY = Convert.ToInt32((double)(intHeight - lstPuntos.Count) / 2);
            double dblMultiplicadorMaximo = 1.05;

            double dblCoste = 0;
            Bitmap bm = new Bitmap(intWidth, intHeight);
            Image img = new Bitmap(bm);
            Graphics drawing = Graphics.FromImage(img);
            Brush br = new SolidBrush(Color.FromArgb(255, (byte)0, (byte)0, (byte)0));
            drawing.FillRectangle(br, 0, 0, img.Width, img.Height);
            // Calcula la distancia maxima y minima
            double dblDistanciaMin = double.MaxValue;
            double dblDistanciaMax = double.MinValue;
            for (Int32 intI = 0; intI < lstPuntos.Count; intI++)
            {
                for (Int32 intJ = intI + 1; intJ < lstPuntos.Count; intJ++)
                {
                    double dblDistancia = dicParPuntosToDistancia[intI + "_" + intJ];
                    dblDistanciaMax = Math.Max(dblDistancia, dblDistanciaMax);
                    dblDistanciaMin = Math.Min(dblDistancia, dblDistanciaMin);
                }
            }
            dblDistanciaMax = dblMultiplicadorMaximo * dblDistanciaMax;
            // Va pintando los cuadrados con el color que corresponda
            for (Int32 intI = 0; intI < lstPuntos.Count; intI++)
            {
                for (Int32 intJ = 0; intJ < lstPuntos.Count; intJ++)
                {
                    double dblDistancia = dicParPuntosToDistancia[intI + "_" + intJ];
                    if (intI == intJ)
                        dblDistancia = dblDistanciaMax;
                    // Normaliza la distancia
                    double dblDistanciaNormalizada = (dblDistancia - dblDistanciaMin) * (2 * 255) / (dblDistanciaMax - dblDistanciaMin);
                    Int16 intR = 0;
                    Int16 intG = 0;
                    Int16 intB = 0;
                    if (dblDistanciaNormalizada > 255)
                        intR = Convert.ToInt16(dblDistanciaNormalizada - 255);
                    else
                        intG = Convert.ToInt16(255 - dblDistanciaNormalizada);
                    if (intR == 0 && intG == 0)
                        intR = intG = 1;
                    Rectangle rec = new Rectangle(intMargenX + intI, intMargenY + intJ, 1, 1);
                    br = new SolidBrush(Color.FromArgb(255, (byte)intR, (byte)intG, (byte)intB));
                    drawing.FillRectangle(br, rec);
                }

            }
            drawing.Save();
            img.Save(strFilePathOutput);
            //bm.Save(strFilePathOutput);
            drawing.Dispose();
            bm.Dispose();
            return dblCoste;
        }


        public void PintarRecorridoSobreMatrizDistancia(List<clsPunto> lstPuntos, List<Int32> lstRecorrido, string strPathFileImagenBase, Int32 intWidth, Int32 intHeight, string strFilePathOutput)
        {
            // Si la imagen base esta vacia la carga
            if (_bmImagenBase == null)
                _bmImagenBase = new Bitmap(strPathFileImagenBase);

            Bitmap bm = new Bitmap(_bmImagenBase);
            // Calcula lo que hay que dejar en x en blanco
            Int32 intMargenX = Convert.ToInt32((double)(intWidth - lstPuntos.Count) / 2);
            Int32 intMargenY = Convert.ToInt32((double)(intHeight - lstPuntos.Count) / 2);

            // Pone el ultimo con el primero
            Int32 intX = intMargenX + lstRecorrido[lstRecorrido.Count - 1];
            Int32 intY = intMargenY + lstRecorrido[0];
            Color actualColor = bm.GetPixel(intX, intY);
            Color newColor = (Color.FromArgb(255, (byte)actualColor.R, (byte)actualColor.G, (byte)255));
            bm.SetPixel(intX, intY, newColor);

            for (Int32 intI = 1; intI < lstRecorrido.Count; intI++)
            {
                intX = intMargenX + lstRecorrido[intI - 1];
                intY = intMargenY + lstRecorrido[intI];
                actualColor = bm.GetPixel(intX, intY);
                newColor = (Color.FromArgb(255, (byte)actualColor.R, (byte)actualColor.G, (byte)255));
                bm.SetPixel(intX, intY, newColor);
            }
            Image img = new Bitmap(bm);
            img.Save(strFilePathOutput);
            //bm.Save(strFilePathOutput);
            bm.Dispose();

        }


        public double PintarRecorrido(List<clsPunto> lstPuntos, List<Int32> lstRecorrido, Int32 intWidth, Int32 intHeight, string strFilePathOutput, Boolean blnPintarSoloCiudades)
        {
            double dblCoste = 0;
            Bitmap bm = new Bitmap(intWidth, intHeight);
            Image img = new Bitmap(bm);
            Graphics drawing = Graphics.FromImage(img);
            drawing.FillRectangle(Brushes.White, 0, 0, img.Width, img.Height);
            Pen BlackPen = new Pen(Color.DarkGreen, 1);
            Pen RedPen = new Pen(Color.DarkGreen, 1);

            for (Int32 intI = 1; intI < lstRecorrido.Count; intI++)
            {
                Int32 intIndexAnt = lstRecorrido[intI - 1];
                Int32 intIndex = lstRecorrido[intI];
                float dblOldX = lstPuntos[intIndexAnt].intX;
                float dblOldY = lstPuntos[intIndexAnt].intY;
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
                    drawing.DrawLine(BlackPen, dblOldX, dblOldY, dblX, dblY);
                }
                dblCoste = dblCoste + Math.Sqrt((dblX - dblOldX) * (dblX - dblOldX) + (dblY - dblOldY) * (dblY - dblOldY));
            }
            // Cierra el loop
            if (lstPuntos.Count > 1)
            {
                Int32 intIndexAnt = lstRecorrido[lstPuntos.Count - 1];
                Int32 intIndex = lstRecorrido[0];

                float dblOldX = lstPuntos[intIndexAnt].intX;
                float dblOldY = lstPuntos[intIndexAnt].intY;
                float dblX = lstPuntos[intIndex].intX;
                float dblY = lstPuntos[intIndex].intY;
                if (!blnPintarSoloCiudades)
                {
                    drawing.DrawLine(BlackPen, dblOldX, dblOldY, dblX, dblY);
                }
                dblCoste = dblCoste + Math.Sqrt((dblX - dblOldX) * (dblX - dblOldX) + (dblY - dblOldY) * (dblY - dblOldY));
            }
            Font font = new Font("Century Gothic", 15);
            Brush brush = new SolidBrush(Color.Green);
            string strTexto = "ITelligent";
            drawing.DrawString(strTexto, font, brush, 10, 10);
            font = new Font("Eurostile", 10);
            string strCoste = Convert.ToInt32(dblCoste).ToString("N");//, CultureInfo.InvariantCulture);
            drawing.DrawString(strCoste, font, brush, 10, 40);
            drawing.Save();
            img.Save(strFilePathOutput);
            //bm.Save(strFilePathOutput);
            drawing.Dispose();
            bm.Dispose();
            return dblCoste;
        }

    }
}
