using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proyecto_vivemas.ViewModels
{
    public class DataReportes
    {
        public long? idProyecto { get; set; }
        public long idTipoTransaccion { get; set; }
        public long? idContrato { get; set; }
        public long idMetodoPago { get; set; }
        public long idBancoDestino { get; set; }
        public long idNroCuentaBancoDestino { get; set; }
        public string fechaDesde { get; set; }
        public string fechaHasta { get; set; }
    }

    public class DataReportesEventoDetalle
    {
        public string codigo { get; set; }
      
    }

    public class DataReportePagos
    {
        public string cuota_numeracion { get; set; }
        public string cuota_fechavencimiento { get; set; }
        public decimal? cuota_monto { get; set; }
        public decimal? transaccion_monto { get; set; }
        public decimal? pago_montodescuento { get; set; }
        public string fecha_pago { get; set; }
        public string cuota_estado { get; set; }
    }

    public class DataGetClientes
    {
        public string documento { get; set; }
        public int proyecto { get; set; }
        public int lote { get; set; }
    }
    public class DataGetClientesEventoDetail
    {
        public string documento { get; set; }
        public int proyecto { get; set; }
        public int lote { get; set; }
    }

    public class DataDocumentoElectronico
    {
        
        public string serie { get; set; }
        public string fechaInicio { get; set; }
        public string fechaFin { get; set; }


    }

    //class Person
    //{
    //    private string _name;  // the name field
    //    public string Name    // the Name property
    //    {
    //        get => _name;
    //        set => _name = value;
    //    }
    //}
}