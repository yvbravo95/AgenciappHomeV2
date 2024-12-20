using System;

namespace AgenciappHome.Models.Request
{
    public class AduanaModel
    {
        public Guid? Id { get; set; }
        public string Articulo { get; set; }
        public string UM { get; set; }
        public string Cantidad { get; set; }
        public string Valor { get; set; }
        public string Observaciones { get; set; }
        public bool EnvioAduana { get; set; }
    }
}
