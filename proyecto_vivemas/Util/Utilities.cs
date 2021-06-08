using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace proyecto_vivemas.Util
{
    public class Utilities
    {
        private byte[] Clave = Encoding.ASCII.GetBytes("$!V1V3M4$%2ol9¨`");
        private byte[] IV = Encoding.ASCII.GetBytes("$e3%/=°!37$D&(q~ES");
        private const int UNI = 0, DIECI = 1, DECENA = 2, CENTENA = 3;
        private static string[,] _matriz = new string[CENTENA + 1, 10]
            {
                {null," uno", " dos", " tres", " cuatro", " cinco", " seis", " siete", " ocho", " nueve"},
                {" diez"," once"," doce"," trece"," catorce"," quince"," dieciseis"," diecisiete"," dieciocho"," diecinueve"},
                {null,null,null," treinta"," cuarenta"," cincuenta"," sesenta"," setenta"," ochenta"," noventa"},
                {null,null,null,null,null," quinientos",null," setecientos",null," novecientos"}
            };

        private const Char sub = (Char)26;
        //Cambiar acá si se quiere otro comportamiento en los métodos de clase
        public const String SeparadorDecimalSalidaDefault = "SOLES";
        public const String SeparadorDecimalSalidaDolares = "DOLAR AMERICANO";
        public const String MascaraSalidaDecimalDefault = "00'/100'";
        public const String MascaraSalidaDecimalDolares = "00'/100'";
        public const Int32 DecimalesDefault = 2;
        public const Boolean LetraCapitalDefault = true;
        public const Boolean ConvertirDecimalesDefault = false;
        public const Boolean ApocoparUnoParteEnteraDefault = false;
        public const Boolean ApocoparUnoParteDecimalDefault = false;

        #region numeros a letras
        public string EncodText(string text)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(text);
            byte[] encripted;
            RijndaelManaged cripto = new RijndaelManaged();
            using (MemoryStream ms = new MemoryStream(inputBytes.Length))
            {
                using (CryptoStream objCryptoStream = new CryptoStream(ms, cripto.CreateEncryptor(Clave, IV), CryptoStreamMode.Write))
                {
                    objCryptoStream.Write(inputBytes, 0, inputBytes.Length);
                    objCryptoStream.FlushFinalBlock();
                    objCryptoStream.Close();
                }
                encripted = ms.ToArray();
            }
            return Convert.ToBase64String(encripted);
        }

        public string SacarDescripcionPago(string idProforma, string numcuo, decimal montoPagado, decimal monto, string manzana, string lote, string proyecto)
        {
            if (numcuo.Equals("CASH"))
            {
                return "POR EL PAGO CASH DEL" + Math.Round(((montoPagado * 100) / monto), 2, MidpointRounding.AwayFromZero) + "% DEL VALOR CONTADO POR LA COMPRA DEL LOTE " + lote + " DE LA MANZANA " + manzana + " DEL PROYECTO " + proyecto;
            }
            else
            {
                return "POR EL PAGO DE LA LETRA NRO " + int.Parse(numcuo) + " SEGUN PROFORMA NRO " + idProforma + " POR LA COMPRA DEL LOTE " + lote + " DE LA MANZANA " + manzana + " DEL PROYECTO " + proyecto;
            }
        }
        public static String ToString(String Numero)
        {
            return Utilities.ToString(Numero, CultureInfo.CurrentCulture);
        }

        public static String ToStringDolares(String Numero)
        {
            return Utilities.ToStringDolares(Numero, CultureInfo.CurrentCulture);
        }

        public static String ToString(String Numero, CultureInfo ReferenciaCultural)
        {
            Double dNumero;
            if (Double.TryParse(Numero, NumberStyles.Float, ReferenciaCultural, out dNumero))
                return ToString(dNumero);
            else throw new ArgumentException("'" + Numero + "' no es un número válido.");
        }

        public static String ToStringDolares(String Numero, CultureInfo ReferenciaCultural)
        {
            Double dNumero;
            if (Double.TryParse(Numero, NumberStyles.Float, ReferenciaCultural, out dNumero))
                return ToStringDolares(dNumero);
            else throw new ArgumentException("'" + Numero + "' no es un número válido.");
        }

        public static String ToString(Double Numero)
        {
            return Convertir((Decimal)Numero, DecimalesDefault, SeparadorDecimalSalidaDefault, MascaraSalidaDecimalDefault, true, LetraCapitalDefault, ConvertirDecimalesDefault, ApocoparUnoParteEnteraDefault, ApocoparUnoParteDecimalDefault);
        }

        public static String ToStringDolares(Double Numero)
        {
            return Convertir((Decimal)Numero, DecimalesDefault, SeparadorDecimalSalidaDolares, MascaraSalidaDecimalDolares, true, LetraCapitalDefault, ConvertirDecimalesDefault, ApocoparUnoParteEnteraDefault, ApocoparUnoParteDecimalDefault);
        }

        private static String Convertir(Decimal Numero, Int32 Decimales, String SeparadorDecimalSalida, String MascaraSalidaDecimal, Boolean EsMascaraNumerica, Boolean LetraCapital, Boolean ConvertirDecimales, Boolean ApocoparUnoParteEntera, Boolean ApocoparUnoParteDecimal)
        {
            Int64 Num;
            Int32 terna, pos, centenaTerna, decenaTerna, unidadTerna, iTerna;
            String numcad, cadTerna;
            StringBuilder Resultado = new StringBuilder();

            Num = (Int64)Math.Abs(Numero);

            if (Num >= 1000000000000 || Num < 0) throw new ArgumentException("El número '" + Numero.ToString() + "' excedió los límites del conversor: [0;1.000.000.000.000)");
            if (Num == 0)
                Resultado.Append(" cero");
            else
            {
                numcad = Num.ToString();
                iTerna = 0;
                pos = numcad.Length;

                do //Se itera por las ternas de atrás para adelante
                {
                    iTerna++;
                    cadTerna = String.Empty;
                    if (pos >= 3)
                        terna = Int32.Parse(numcad.Substring(pos - 3, 3));
                    else
                        terna = Int32.Parse(numcad.Substring(0, pos));

                    centenaTerna = (Int32)(terna / 100);
                    decenaTerna = terna - centenaTerna * 100;
                    unidadTerna = (decenaTerna - (Int32)(decenaTerna / 10) * 10);

                    if ((decenaTerna > 0) && (decenaTerna < 10))
                        cadTerna = _matriz[UNI, unidadTerna] + cadTerna;
                    else if ((decenaTerna >= 10) && (decenaTerna < 20))
                        cadTerna = cadTerna + _matriz[DIECI, decenaTerna - (Int32)(decenaTerna / 10) * 10];
                    else if (decenaTerna == 20)
                        cadTerna = cadTerna + " veinte";
                    else if ((decenaTerna > 20) && (decenaTerna < 30))
                        cadTerna = " veinti" + _matriz[UNI, unidadTerna].Substring(1, _matriz[UNI, unidadTerna].Length - 1);
                    else if ((decenaTerna >= 30) && (decenaTerna < 100))
                        if (unidadTerna != 0)
                            cadTerna = _matriz[DECENA, (Int32)(decenaTerna / 10)] + " y" + _matriz[UNI, unidadTerna] + cadTerna;
                        else
                            cadTerna += _matriz[DECENA, (Int32)(decenaTerna / 10)];

                    switch (centenaTerna)
                    {
                        case 1:
                            if (decenaTerna > 0) cadTerna = " ciento" + cadTerna;
                            else cadTerna = " cien" + cadTerna;
                            break;
                        case 5:
                        case 7:
                        case 9:
                            cadTerna = _matriz[CENTENA, (Int32)(terna / 100)] + cadTerna;
                            break;
                        default:
                            if ((Int32)(terna / 100) > 1) cadTerna = _matriz[UNI, (Int32)(terna / 100)] + "cientos" + cadTerna;
                            break;
                    }
                    //Reemplazo el 'uno' por 'un' si no es en las únidades o si se solicító apocopar
                    if ((iTerna > 1 | ApocoparUnoParteEntera) && decenaTerna == 21)
                        cadTerna = cadTerna.Replace("veintiuno", "veintiun");
                    else if ((iTerna > 1 | ApocoparUnoParteEntera) && unidadTerna == 1 && decenaTerna != 11)
                        cadTerna = cadTerna.Substring(0, cadTerna.Length - 1);
                    //Acentúo 'dieciseís', 'veintidós', 'veintitrés' y 'veintiséis'
                    else if (decenaTerna == 16) cadTerna = cadTerna.Replace("dieciseis", "dieciseis");
                    else if (decenaTerna == 22) cadTerna = cadTerna.Replace("veintidos", "veintidos");
                    else if (decenaTerna == 23) cadTerna = cadTerna.Replace("veintitres", "veintitres");
                    else if (decenaTerna == 26) cadTerna = cadTerna.Replace("veintiseis", "veintiseis");
                    //Reemplazo 'uno' por 'un' si no es en las únidades o si se solicító apocopar (si _apocoparUnoParteEntera es verdadero) 

                    switch (iTerna)
                    {
                        case 3:
                            if (Num < 2000000) cadTerna += " millon";
                            else cadTerna += " millones";
                            break;
                        case 2:
                        case 4:
                            if (terna > 0) cadTerna += " mil";
                            break;
                    }
                    Resultado.Insert(0, cadTerna);
                    pos = pos - 3;
                } while (pos > 0);
            }
            //Se agregan los decimales si corresponde
            if (Decimales > 0)
            {
                Resultado.Append(" CON ");               
                Int32 EnteroDecimal = (Int32)Math.Round((Double)(Numero - (Int64)Numero) * Math.Pow(10, Decimales), 0);
                if (ConvertirDecimales)
                {
                    Boolean esMascaraDecimalDefault = MascaraSalidaDecimal == MascaraSalidaDecimalDefault;
                    Resultado.Append(Convertir((Decimal)EnteroDecimal, 0, null, null, EsMascaraNumerica, false, false, (ApocoparUnoParteDecimal && !EsMascaraNumerica/*&& !esMascaraDecimalDefault*/), false) + " "
                        + (EsMascaraNumerica ? "" : MascaraSalidaDecimal));
                }
                else
                    if (EsMascaraNumerica)
                {
                    Resultado.Append(EnteroDecimal.ToString(MascaraSalidaDecimal) + " ");
                    Resultado.Append(SeparadorDecimalSalida);
                }
                else Resultado.Append(EnteroDecimal.ToString() + " " + MascaraSalidaDecimal);
            }
            //Se pone la primer letra en mayúscula si corresponde y se retorna el resultado
            if (LetraCapital)
                return Resultado[1].ToString().ToUpper() + Resultado.ToString(2, Resultado.Length - 2).ToUpper();
            else
                return Resultado.ToString().Substring(1);
        }
        #endregion

        public static string reemplazarCaracteres(string texto)
        {
            if(texto == null)
            {
                return "CLIENTES VARIOS";
            }

            return texto.ToUpper().Replace("Á", "A")
                .Replace("É", "E")
                .Replace("Í", "I")
                .Replace("Ó", "O")
                .Replace("Ú", "U")
                .Replace("Ñ", "N")
                .Replace("Ä", "A")
                .Replace("Ë", "E")
                .Replace("Ï", "I")
                .Replace("Ö", "O")
                .Replace("Ü", "U");
        }
    }
}