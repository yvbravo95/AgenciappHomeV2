using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers.Class
{
    public class AsignarPermisosUser
    {
        private databaseContext _context;

        public AsignarPermisosUser()
        {
            _context = new databaseContext();
        }

        public  async Task AddsAsync(Guid id, databaseContext db)
        {
            _context = db;

            var user = _context.User.Include(x => x.AccessListUsers).FirstOrDefault(x => x.UserId == id);

            if (user.Type == "Empleado")
            {
               await PermisosEmpleadoAsync(user);
            }
            else if(user.Type == "EmpleadoCuba")
            {
               await PermisosEmpleadoCubaAsync(user);
            }
        }

        private  async Task PermisosEmpleadoAsync(User user)
        {
           /* List<AccessListUser> aux = new List<AccessListUser>();
            //createPermiso(_context.AccessLists.FirstOrDefault(x => x.Controller == "formbuilder" && x.Action == "create"), user);
            //createPermiso(_context.AccessLists.FirstOrDefault(x => x.Controller == "clients" && x.Action == "exportclient"), user);
            //createPermiso(_context.AccessLists.FirstOrDefault(x => x.Controller == "wholesalers" && x.Action == "create"), user);
            //createPermiso(_context.AccessLists.FirstOrDefault(x => x.Controller == "wholesalers" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "facturas" && x.Action == "index"), user);
            //createPermiso(_context.AccessLists.FirstOrDefault(x => x.Controller == "minorista" && x.Action == "index"), user);
            //createPermiso(_context.AccessLists.FirstOrDefault(x => x.Controller == "minorista" && x.Action == "preciostramites"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "ticket" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "home" && x.Action == "indexempleado"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "shippings" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "ordernew" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "servicios" && x.Action == "create"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "rechargue" && x.Action == "create"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "shippings" && x.Action == "scanqr"), user);
            //createPermiso(_context.AccessLists.FirstOrDefault(x => x.Controller == "servicioxcobrars" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "serviciosxPagar" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "clients" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "remesas" && x.Action == "createremesa"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "remesas" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "ticket" && x.Action == "create"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "ordernew" && x.Action == "create"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "enviomaritimo" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "bills" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "servicios" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "contacts" && x.Action == "create"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "contacts" && x.Action == "index"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "carriers" && x.Action == "create"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "shippings" && x.Action == "create"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "enviomaritimo" && x.Action == "create"), user);
            await createPermisoAsync(_context.AccessLists.FirstOrDefault(x => x.Controller == "rechargue" && x.Action == "index"), user);
           */
        }

        private async Task PermisosEmpleadoCubaAsync(User user)
        {
            await createPermisoAsync( user, "empleadocuba", "ordenesenviadas");
            await createPermisoAsync( user, "empleadocuba", "ordenesrevisadas");
            await createPermisoAsync( user, "empleadocuba", "ordenesentregadas");
            await createPermisoAsync( user, "remesas", "index");
            await createPermisoAsync( user, "remesas", "details");
            await createPermisoAsync( user, "home", "indexempleado");

        }

        private async Task<AccessListUser> createPermisoAsync(User user, string controller, string action)
        {
            var accesslist = _context.AccessLists.FirstOrDefault(x => x.Controller == controller && x.Action == action);

            AccessListUser aux = new AccessListUser();
            aux.AccessListId = Guid.NewGuid();
            aux.accessList = accesslist;
            aux.AccessListId = accesslist.AccessListId;
            aux.AccessListUserId = user.UserId;
            aux.user = user;
            _context.AccessListUsers.Add(aux);
            await _context.SaveChangesAsync();
            return aux;
        }
    }
}
