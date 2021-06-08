using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proyecto_vivemas.ViewModels
{
    public class DocumentoVentaModelo
    {
        public long? documento_cliente_id { get; set; }
        public long documento_moneda_id { get; set; }
        public long documento_banco_id { get; set; }
        public long documento_cuenta_id { get; set; }
        public long documento_tipodocumentoventa_id { get; set; }
        public string documento_descripcion { get; set; }//
        public long documento_tipometodopago_id { get; set; }
        public string documento_nrooperacion { get; set; }
        public string documento_documentoreferencia { get; set; }/// 
        public string documento_fechaemisionreferencia { get; set; }
        public string documento_motivo { get; set; }/// 
        public string documento_tiponota { get; set; }/// 
        public string documento_fechadeposito { get; set; }
        public string documento_empresa_documento { get; set; }//
        public string documento_empresa_nombre { get; set; }
        public string documento_empresa_direccion { get; set; }
        public string documento_empresa_numeroContacto { get; set; }
        public string documento_empresa_correo { get; set; }
        public string documento_cliente_nombre { get; set; }
        public string documento_cliente_nroDocumento { get; set; }
        public string documento_cliente_direccion { get; set; }
        public string documento_fechaEmision { get; set; }
        public string documento_fechaVencimiento { get; set; }
        public string documento_moneda_descripcion { get; set; }//
        public string documento_qrcodeValue { get; set; }//
        public string documento_serie { get; set; }
        public decimal documento_total { get; set; }
        public decimal documento_subtotal { get; set; }
        public decimal documento_igv { get; set; }
        public string documento_digestValue { get; set; }
        public string documento_montoletras { get; set; }
        public string moneda_simbolo { get; set; }
        public string transaccion_fechadeposito { get; set; }
        public string transaccion_nrooperacion { get; set; }
        public string transaccion_banco { get; set; }
        public string transaccion_cuenta { get; set; }
        public string documento_fecha_emision { get; set; }
        public List<DocumentoDetalleModelo> detalleVenta { get; set; }
    }

    public class DocumentoDetalleModelo
    {
        public decimal documentoDetalle_cantidad { get; set; }
        public string documentoDetalle_unidad { get; set; }
        public string documentoDetalle_codigo { get; set; }
        public string documentoDetalle_descripcion { get; set; }
        public decimal documentoDetalle_valorUnitario { get; set; }
        public decimal documentoDetalle_total { get; set; }
    }
}