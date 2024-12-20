using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class Phone
    {
        public Guid PhoneId { get; set; }
        public Guid ReferenceId { get; set; }
        public string Type { get; set; }
        public bool Current { get; set; }

        private string _number = string.Empty;
        public string Number 
        { 
            get
            {
                return _number;
            }
            set 
            {
                _number = GetOnlyNumbers(value);
            } 
        } 
        public string carrier { get; set; }
        public string sms_email { get; set; }

        private string GetOnlyNumbers(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            string aux = string.Empty;
            foreach (var item in value)
            {
                if (Char.IsDigit(item))
                    aux += item;
            }
            return aux;
        }
    }
}
