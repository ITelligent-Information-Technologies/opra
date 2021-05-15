using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsScheduling
{
    class Program
    {
        static void Main(string[] args)

        {
            //TestJobShop();
            TestJobShopTSSA();


        }

        static void TestJobShopTSSA()
        {
            // Guarda los resultados
            clsDatosResultados cResultados = new clsDatosResultados();
            // Genera un horario de trabajo
            clsDatosHorarios cDatosHorarios = new clsDatosHorarios();
            List<clsDatosHorariosHoras> lstHoras = new List<clsDatosHorariosHoras>();
            clsDatosHorariosHoras cHoras = new clsDatosHorariosHoras();
            cHoras.dblHoraDesde = 9;
            cHoras.dblHoraHasta = 17;
            lstHoras.Add(cHoras);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(1, lstHoras);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(2, lstHoras);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(3, lstHoras);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(4, lstHoras);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(5, lstHoras);
            clsDatosHorariosHoras cHorariosFiesta = new clsDatosHorariosHoras();
            cHorariosFiesta.blnSinActividad = true;
            List<clsDatosHorariosHoras> lstHorasFinSemana = new List<clsDatosHorariosHoras>();
            lstHorasFinSemana.Add(cHorariosFiesta);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(6, lstHorasFinSemana);
            cDatosHorarios.dicIdDiasSemanaHorarios.Add(7, lstHorasFinSemana);
            // Mete un festivo
            cDatosHorarios.dicFechasEspecialesHorarios.Add(Convert.ToDateTime("02/01/2020"), lstHorasFinSemana);

            //--- Taillard 20 15 1
            string strJson = System.IO.File.ReadAllText(@"D:\AAdatos\Itelligent\Recursos\Scheduling\ProblemasBenchmarks\Taillard\JobShop\ITelligent\Json\tai20_15_1.json");
            cResultados.cData = JsonConvert.DeserializeObject<clsDatosJobShop>(strJson);
            clsSolucionInicial cSInicial = new clsSolucionInicial();
            clsDatosSchedule cSchedule = cSInicial.OrdenarPorLongitudTrabajos(cResultados.cData);
            ////--- FIN TAILLARD 15 20 1

            // Calcula el bellman para la solucion actual
            clsBellman cBellman = new clsBellman();
            cBellman.CalcularBellman(cResultados.cData, cSchedule);

            // Pruba a genera el schedule con un horario
            clsHorarios cHorarios = new clsHorarios();
            (cResultados.dicIdOperacionFechaStart, cResultados.dicIdOperacionFechaEnd) = cHorarios.ObtenerFechas(cDatosHorarios, cResultados.cData, cSchedule, Convert.ToDateTime("1/1/2020"));


            //-- Problema 3x3
            //clsBenchmark cBenchmark = new clsBenchmark();
            //clsDatosSchedule cSchedule = new clsDatosSchedule(); 
            //clsDatosJobShop cData = cBenchmark.CargarProblema(@"D:\AAdatos\Itelligent\Recursos\Scheduling\ProblemasBenchmarks\Test", @"test_3_3", cSchedule);
            //---FIn problema 3x3

            //-------- elementos para reciclar
            //clsDatosJobShop cData = cBenchmark.CargarProblema(@"d:\AAdatos\Itelligent\Recursos\Scheduling\ProblemasBenchmarks\Taillard\JobShop\ITelligent", @"tai20_15", cSchedule);
            //Serializar no cambiar nada
            // string output = JsonConvert.SerializeObject(cData);
            //System.IO.File.WriteAllText(@"D:\AAdatos\Itelligent\Recursos\Scheduling\ProblemasBenchmarks\Taillard\JobShop\ITelligent\Json\tai20_15_1.json", output);
            // //Deserializar
            //clsDatosJobShop deserialized = JsonConvert.DeserializeObject<clsDatosJobShop>(output);
            //------- fin elementos para reciclar

            // TODO

            // Llama al algortimo de resolucion
            clsDatosParametros cParametros = new clsDatosParametros(cResultados.cData);
            //1386
            cParametros.intMaxStackBackTrack = 30; //30
            cParametros.intMaxIteraciones = 100000; //100000
            cParametros.intMaxIteracionesPorBucle = 2500; //2500
            cParametros.intTabuListMin = 300; //300
            cParametros.intTabuListMax = cParametros.intTabuListMin;
            cParametros.enuGuardarTabuList = TiposFirmaTabuList.SoloParUV; // SoloParUV
            clsTSSA cTssa = new clsTSSA();

            clsDatosSchedule cScheduleMin = cTssa.JobShop(cResultados.cData, cSchedule, cParametros);
            // Obtiene la fechas para el Schedule minmimo
            (cResultados.dicIdOperacionFechaStart, cResultados.dicIdOperacionFechaEnd) = cHorarios.ObtenerFechas(cDatosHorarios, cResultados.cData, cSchedule, Convert.ToDateTime("1/1/2020"));
            // Guarda resultado en disco
            //clsWriteObjectToFile.WriteToBinaryFile<clsDatosResultados>(@"C:\AAdatos\Itelligent\Recursos\Scheduling\ProblemasBenchmarks\Taillard\JobShop\ITelligent\tai20_15_fin.ite", cResultados);
        }



    }
}
