using AgenciappHome.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Agenciapp.Domain.Models
{
    public class Cheque
    {
        protected Cheque()
        {

        }
        public Cheque(string number, ManifiestoPasaporte manifiestoPasaporte, ServicioConsular servicioConsular)
        {
            Number = number;
            ManifiestoPasaporte = manifiestoPasaporte;
            CreatedAt = DateTime.UtcNow;
            ServicioConsular = servicioConsular;
        }

        [Key] public Guid Id { get; protected set; }
        public string Number { get; protected set; }
        public ManifiestoPasaporte ManifiestoPasaporte { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public ServicioConsular ServicioConsular { get; protected set; }
        public decimal Amount
        {
            get
            {
                decimal costo = ManifiestoPasaporte.Passports.Where(x => x.Minorista != null || x.AgencyTransferida != null).Sum(x => x.costoProveedor);
                costo += ManifiestoPasaporte.Passports.Where(x => x.Minorista == null && x.AgencyTransferida == null).Sum(x => x.costo);
                return costo;
            }
        }
        public string AmountText { get { return NumberToText((int)Amount); } }

        protected string NumberToText(int n)
        {
            if (n < 0)
                return "Minus " + NumberToText(-n);
            else if (n == 0)
                return "";
            else if (n <= 19)
                return new string[] {"One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight",
         "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
         "Seventeen", "Eighteen", "Nineteen"}[n - 1] + " ";
            else if (n <= 99)
                return new string[] {"Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy",
         "Eighty", "Ninety"}[n / 10 - 2] + " " + NumberToText(n % 10);
            else if (n <= 199)
                return "One Hundred " + NumberToText(n % 100);
            else if (n <= 999)
                return NumberToText(n / 100) + "Hundreds " + NumberToText(n % 100);
            else if (n <= 1999)
                return "One Thousand " + NumberToText(n % 1000);
            else if (n <= 999999)
                return NumberToText(n / 1000) + "Thousands " + NumberToText(n % 1000);
            else if (n <= 1999999)
                return "One Million " + NumberToText(n % 1000000);
            else if (n <= 999999999)
                return NumberToText(n / 1000000) + "Millions " + NumberToText(n % 1000000);
            else if (n <= 1999999999)
                return "One Billion " + NumberToText(n % 1000000000);
            else
                return NumberToText(n / 1000000000) + "Billions " + NumberToText(n % 1000000000);
        }
    }

    public class ChequeProrroga : Cheque
    {
        protected ChequeProrroga()
        {

        }
        public ChequeProrroga(string number, ManifiestoPasaporte manifiestoPasaporte, ServicioConsular servicioConsular) : base(number, manifiestoPasaporte, servicioConsular)
        {
        }
    }

    public class ChequeProrrogaDoble : Cheque
    {
        protected ChequeProrrogaDoble()
        {
        }
        public ChequeProrrogaDoble(string number, ManifiestoPasaporte manifiestoPasaporte, ServicioConsular servicioConsular) : base(number, manifiestoPasaporte, servicioConsular)
        {
        }
    }

    public class ChequeRenovacion : Cheque
    {
        protected ChequeRenovacion()
        {

        }
        public ChequeRenovacion(string number, ManifiestoPasaporte manifiestoPasaporte, ServicioConsular servicioConsular, bool control) : base(number, manifiestoPasaporte, servicioConsular)
        {
            Control = control;
        }
        public bool Control { get; set; }
        public decimal Amount
        {
            get
            {
                decimal costo = ManifiestoPasaporte.Passports.Where(x => x.Minorista != null || x.AgencyTransferida != null).Sum(x => x.costoProveedor);
                costo += ManifiestoPasaporte.Passports.Where(x => x.Minorista == null && x.AgencyTransferida == null).Sum(x => x.costo);
                
                return Control ? ManifiestoPasaporte.Passports.Count * 5 : costo - ManifiestoPasaporte.Passports.Count * 5;
            }
        }
        public string AmountText { get { return NumberToText((int)Amount); } }

    }

    public class ChequePrimeraVez : Cheque
    {
        protected ChequePrimeraVez()
        {

        }
        public ChequePrimeraVez(string number, ManifiestoPasaporte manifiestoPasaporte, ServicioConsular servicioConsular, Passport passport, bool control) : base(number, manifiestoPasaporte, servicioConsular)
        {
            Passport = passport;
            Control = control;
        }

        public Passport Passport { get; private set; }
        public bool Control { get; set; }
        public decimal Amount { get { return Control ? 5 : 20; } }
        public string AmountText { get { return NumberToText((int)Amount); } }


    }

    public class ChequeHE11 : Cheque
    {
        protected ChequeHE11()
        {

        }
        public ChequeHE11(string number, ManifiestoPasaporte manifiestoPasaporte, ServicioConsular servicioConsular, Passport passport) : base(number, manifiestoPasaporte, servicioConsular)
        {
            Passport = passport;
        }
        public Passport Passport { get; private set; }
        public decimal Amount { get { return 40; } }
        public string AmountText { get { return NumberToText((int)Amount); } }

    }

    public class ChequeOtorgamiento : Cheque
    {
        protected ChequeOtorgamiento()
        {

        }
        public ChequeOtorgamiento(string number, ManifiestoPasaporte manifiestoPasaporte, ServicioConsular servicioConsular, Passport passport) : base(number, manifiestoPasaporte, servicioConsular)
        {
            Passport = passport;
        }
        public Passport Passport { get; private set; }
        public decimal Amount
        {
            get
            {
                decimal costo = Passport.costo;
                if(Passport.Minorista != null || Passport.AgencyTransferida != null)
                    costo = Passport.costoProveedor;

                if (Passport.ServicioConsular == ServicioConsular.HE11)
                    return costo - 40;
                else if (Passport.ServicioConsular == ServicioConsular.PrimerVez)
                    return 180;
                else if (Passport.ServicioConsular == ServicioConsular.PrimerVez2)
                    return 140;
                else
                    return 0;
            }
        }
        public string AmountText { get { return NumberToText((int)Amount); } }

    }
}
