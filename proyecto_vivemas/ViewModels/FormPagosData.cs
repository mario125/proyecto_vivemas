using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proyecto_vivemas.ViewModels
{
    public class FormPagosData
    {
        public List<TiposPagoModel> listTiposPagoModel { get; set; }
        public List<BancosOrigenModel> listBancosOrigenModel { get; set; }
        public List<BancosDestinoModel> listBancosDestinoModel { get; set; }
        public List<CuentasBancoDestinoModel> listCuentasBancoDestinoModel { get; set; }
    }

    public class TiposPagoModel
    {
        public string idTipoPago { get; set; }
        public string descripcionTipoPago { get; set; }
    }

    public class BancosOrigenModel
    {
        public string idBancoOrigen { get; set; }
        public string descripcionBancoOrigen { get; set; }
    }

    public class BancosDestinoModel
    {
        public string idBancoDestino { get; set; }
        public string descripcionBancoDestino { get; set; }
    }

    public class CuentasBancoDestinoModel
    {
        public int idCuentaBancoDestino { get; set; }
        public string nroCuentaBancoDestino { get; set; }
    }

    public class DatosPagoCalculo
    {
        public long idContrato { get; set; }
        public decimal montoPago { get; set; }
        public decimal? montoDescuento { get; set; }
        public decimal? montoMora { get; set; }
        public List<LetrasPagadas> cuotasPagadas { get; set; }
    }

    public class LetrasPagadas
    {
        public string nroCuota { get; set; }
        public string fechaVencimientoCuota { get; set; }
        public decimal montoPagado { get; set; }
        public decimal montoPendiente { get; set; }
        public decimal montoDescuento { get; set; }
        public decimal montoPagadoDescuento { get; set; }
        public string descripcionPago { get; set; }
    }

    public class PagoModelo
    {
        public long? bancoDestino { get; set; }
        public string bancoOrigen { get; set; }
        public string fechaDeposito { get; set; }
        public long? idContrato { get; set; }
        public long? idMetodoPago { get; set; }
        public long? idEvento { get; set; }
        public decimal montoPago { get; set; }
        public long? nroCuentaBancoDestino { get; set; }
        public string nroCuentaBancoOrigen { get; set; }
        public string nroOperacion { get; set; }
        public decimal? montoMora { get; set; }
        public decimal? montoDescuento { get; set; }

        public List<DetallePagos> detallePagos { get; set; }
    }

    public class DetallePagos
    {
        public string idPago { get; set; }
        public string descripcion { get; set; }
    }

    public class EventoModel
    {
        public long eventoContratoId { get; set; }
        public string eventoDescripcion { get; set; }
        public string eventoFecha { get; set; }
        public decimal eventoMonto { get; set; }
        public List<EventoMetodoContacto> eventoMetodosContacto { get; set; }
    }

    public class EventoMetodoContacto
    {
        public long contactoId { get; set; }
    }
}