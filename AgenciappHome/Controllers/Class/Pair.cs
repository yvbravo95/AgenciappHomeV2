using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers.Class
{
    public class Pair<X,Y>
    {
        public Pair()
        {

        }
        public Pair(X obj1, Y obj2)
        {
            this.obj1 = obj1;
            this.obj2 = obj2;
        }
        public X obj1 { get; set; }
        public Y obj2 { get; set; }
    }
}
