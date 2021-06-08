using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proyecto_vivemas.ViewModels
{
    public class DataMigracion
    {
        public List<FilaPago> filas { get; set; }       
    }

    public class DataMigracionContratos
    {
        public List<dataContrato> filas { get; set; }
    }

    public class dataContrato
    {
        public long idContrato { get; set; }
    }

    public class FilaPago
    {
        public int algo { get; set; }
        public string lote { get; set; }
        public string documento { get; set; }
        public int idProforma { get; set; }
        public double montoCash { get; set; }
        public double saldoFinal { get; set; }
        public double Saldocash { get; set; }

    }
}