using System.Collections.Generic;

namespace AgenciappHome.Controllers.Class
{
    public class UpdateStatusRequest {
        public string shippingCode { get; set; }
        public int status { get; set; }
    }
}
