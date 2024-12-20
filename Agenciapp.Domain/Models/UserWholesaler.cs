using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    //Clase para establecer la relacion muchos a muchos.
    //Con el objetivo de que un empleado en cuba solo vea los tramites
    //relacionados con los mayoristas(wholesaler) que tiene asignado.
    public class UserWholesaler
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid WholesalerId { get; set; }
        public Wholesaler Wholesaler { get; set; }
    }
}
