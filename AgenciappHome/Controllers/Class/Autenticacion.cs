using AgenciappHome.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers.Class
{

    public class AgenciaAuthorize
    {
        private static readonly Guid _agencyHMPaquete = Guid.Parse("3B920E34-46CC-4230-B588-DFB988E68FB4");
        public enum TypeAutorize
        {
            None,
            Administrador,
            Agencia,
            Empleado,
            EmpleadoCuba,
            DistributorCuba,
            PrincipalDistributor
        }

        public static bool Autorize(ClaimsPrincipal User, databaseContext _context, string[] Roles, string controller, string action)
        {
            var user = _context.User.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
            if (Roles.Contains(user.Type))
            {
                var accessListuser = _context.AccessListUsers.Include(x => x.accessList).Include(x => x.user).Where(x => x.UserId == user.UserId);
                if (accessListuser.Count() == 0)
                {
                    return true;
                }
                else
                {
                    //var accessList = _context.AccessLists.Where(x => x.Controller.ToLower() == controller.ToLower() && x.Action.ToLower() == action.ToLower()).ToList();
                    var aux = accessListuser.Where(x => x.accessList.Controller.ToLower() == controller.ToLower() && x.accessList.Action.ToLower() == action.ToLower()).Any();
                    if (aux)
                    {
                        return true;
                    }
                }

            }
            return false;
        }
        [Authorize]
        public static TypeAutorize getRole(ClaimsPrincipal User, databaseContext _context)
        {
            var user = _context.User.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
            if (user != null)
            {
                if (user.Type.Equals("Administrador"))
                    return TypeAutorize.Administrador;
                else if (user.Type.Equals("Agencia"))
                    return TypeAutorize.Agencia;
                else if (user.Type.Equals("Empleado"))
                    return TypeAutorize.Empleado;
                else if (user.Type.Equals("EmpleadoCuba"))
                    return TypeAutorize.EmpleadoCuba;
                else if (user.Type.Equals("DistributorCuba"))
                    return TypeAutorize.DistributorCuba;
                else if (user.Type.Equals("PrincipalDistributor"))
                    return TypeAutorize.PrincipalDistributor;
            }

            return TypeAutorize.None;
        }

        public static User getUser(ClaimsPrincipal User, databaseContext _context)
        {
            return _context.User.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
        }

        public static Agency getAgency(ClaimsPrincipal User, databaseContext _context)
        {
            var user = _context.User.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
            return _context.Agency.Where(x => x.AgencyId == user.AgencyId).FirstOrDefault();
        }

        public static async Task<bool> addPermissionsDefaults(databaseContext context, Guid userId)
        {
            try
            {
                var user = await context.User.Include(x => x.AccessListUsers).FirstOrDefaultAsync(x => x.UserId == userId);
                if (user == null)
                {
                    return false;
                }
                if (user.Type == "DistributorCuba" || user.Type == "EmpleadoCuba")
                {
                    if (user.AccessListUsers.Any())
                        context.RemoveRange(user.AccessListUsers);
                    List<Pair<string, string>> access;
                    if (user.AgencyId.Equals(_agencyHMPaquete))
                    {
                        access = new List<Pair<string, string>> {
                            new Pair<string, string>{obj1= "home",obj2="indexempleado"},
                            new Pair<string, string>{obj1= "orders",obj2="details"},
                            new Pair<string, string>{obj1= "orders",obj2="entregarorder"},
                            new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesentregadas"},
                            new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesrecibidas"},
                        };
                    }
                    else
                    {
                        access = new List<Pair<string, string>> {
                            new Pair<string, string>{obj1= "home",obj2="indexempleado"},
                            new Pair<string, string>{obj1= "orders",obj2="details"},
                            new Pair<string, string>{obj1= "orders",obj2="entregarorder"},
                            new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesentregadas"},
                            new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesenviadas"},
                            new Pair<string, string>{obj1= "empleadocuba",obj2="combos"},
                            new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesrevisadas"},
                            new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesrecibidas"},
                            new Pair<string, string>{obj1= "remesas",obj2="index"},
                            new Pair<string, string>{obj1= "remesas",obj2="details"},
                            new Pair<string, string>{obj1= "remesas",obj2="entregarremesa"},
                            new Pair<string, string>{obj1= "shippings",obj2="index"},
                            new Pair<string, string>{obj1= "shippings",obj2="details"},
                        };
                    }

                    foreach (var item in access)
                    {
                        var aux = new AccessListUser
                        {
                            UserId = userId,
                            AccessListUserId = Guid.NewGuid(),
                            accessList = context.AccessLists.FirstOrDefault(x => x.Controller.ToLower() == item.obj1 && x.Action.ToLower() == item.obj2)
                        };
                        context.Add(aux);
                    }

                    await context.SaveChangesAsync();
                }
                else if (user.Type == "PrincipalDistributor")
                {
                    if (user.AccessListUsers.Any())
                        context.RemoveRange(user.AccessListUsers);
                    List<Pair<string, string>> access;
                    if (user.AgencyId.Equals(_agencyHMPaquete))
                    {
                        access = new List<Pair<string, string>> {
                        new Pair<string, string>{obj1= "home",obj2="indexempleado"},
                        new Pair<string, string>{obj1= "orders",obj2="details"},
                        new Pair<string, string>{obj1= "orders",obj2="entregarorder"},
                        new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesdespachadas"},
                        new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesrecibidas"},
                        new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesentregadas"},
                        new Pair<string, string>{obj1= "shippings",obj2="index"},
                        new Pair<string, string>{obj1= "shippings",obj2="details"} };
                    }
                    else
                    {
                        access = new List<Pair<string, string>>
                            {
                                new Pair<string, string> { obj1 = "home", obj2 = "indexempleado" },
                                new Pair<string, string> { obj1 = "orders", obj2 = "details" },
                                new Pair<string, string> { obj1 = "orders", obj2 = "entregarorder" },
                                new Pair<string, string> { obj1 = "empleadocuba", obj2 = "ordenesdespachadas" },
                                new Pair<string, string> { obj1 = "empleadocuba", obj2 = "ordenesrevisadas" },
                                new Pair<string, string> { obj1 = "empleadocuba", obj2 = "ordenesrecibidas" },
                                new Pair<string, string> { obj1 = "empleadocuba", obj2 = "ordenesentregadas" },
                                new Pair<string, string> { obj1 = "empleadocuba", obj2 = "combos" },
                                new Pair<string, string> { obj1 = "remesas", obj2 = "index" },
                                new Pair<string, string> { obj1 = "remesas", obj2 = "details" },
                                new Pair<string, string> { obj1 = "remesas", obj2 = "entregarremesa" },
                                new Pair<string, string> { obj1 = "shippings", obj2 = "index" },
                                new Pair<string, string> { obj1 = "shippings", obj2 = "details" }
                            };
                    }

                    foreach (var item in access)
                    {
                        var aux = new AccessListUser
                        {
                            UserId = userId,
                            AccessListUserId = Guid.NewGuid(),
                            accessList = context.AccessLists.FirstOrDefault(x => x.Controller.ToLower() == item.obj1 && x.Action.ToLower() == item.obj2)
                        };
                        context.Add(aux);
                    }

                    await context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return false;
            }
        }

        public static async Task InitAccessList(databaseContext context)
        {
            List<Pair<string, string>> access = new List<Pair<string, string>> {
                new Pair<string, string>{obj1= "home",obj2="indexempleado"},
                new Pair<string, string>{obj1= "formbuilder",obj2="create"},
                new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesdespachadas"},
                new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesenviadas"},
                new Pair<string, string>{obj1= "clients",obj2="exportclient"},
                new Pair<string, string>{obj1= "wholesalers",obj2="create"},
                new Pair<string, string>{obj1= "facturas",obj2="index"},
                new Pair<string, string>{obj1= "offices",obj2="index"},
                new Pair<string, string>{obj1= "home",obj2="index"},
                new Pair<string, string>{obj1= "remesas",obj2="index"},
                new Pair<string, string>{obj1= "analytics",obj2="sale"},
                new Pair<string, string>{obj1= "wholesalers",obj2="index"},
                new Pair<string, string>{obj1= "minorista",obj2="index"},
                new Pair<string, string>{obj1= "users",obj2="create"},
                new Pair<string, string>{obj1= "ticket",obj2="index"},
                new Pair<string, string>{obj1= "shippings",obj2="index"},
                new Pair<string, string>{obj1= "remesas",obj2="details"},
                new Pair<string, string>{obj1= "clients",obj2="importclient"},
                new Pair<string, string>{obj1= "minorista",obj2="preciostramites"},
                new Pair<string, string>{obj1= "ordernew",obj2="index"},
                new Pair<string, string>{obj1= "servicios",obj2="create"},
                new Pair<string, string>{obj1= "rechargue",obj2="create"},
                new Pair<string, string>{obj1= "offices",obj2="create"},
                new Pair<string, string>{obj1= "carriers",obj2="index"},
                new Pair<string, string>{obj1= "shippings",obj2="scanqr"},
                new Pair<string, string>{obj1= "servicioxcobrars",obj2="index"},
                new Pair<string, string>{obj1= "remesas",obj2="createremesa"},
                new Pair<string, string>{obj1= "orders",obj2="details"},
                new Pair<string, string>{obj1= "serviciosxpagar",obj2="index"},
                new Pair<string, string>{obj1= "formbuilder",obj2="index"},
                new Pair<string, string>{obj1= "contacts",obj2="importcontact"},
                new Pair<string, string>{obj1= "analytics",obj2="transactions"},
                new Pair<string, string>{obj1= "ticket",obj2="create"},
                new Pair<string, string>{obj1= "clients",obj2="index"},
                new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesrevisadas"},
                new Pair<string, string>{obj1= "ordernew",obj2="create"},
                new Pair<string, string>{obj1= "passport",obj2="details"},
                new Pair<string, string>{obj1= "enviomaritimo",obj2="index"},
                new Pair<string, string>{obj1= "ordernew",obj2="tiendas"},
                new Pair<string, string>{obj1= "bills",obj2="index"},
                new Pair<string, string>{obj1= "passport",obj2="index"},
                new Pair<string, string>{obj1= "servicios",obj2="index"},
                new Pair<string, string>{obj1= "orders",obj2="entregarorder"},
                new Pair<string, string>{obj1= "contacts",obj2="create"},
                new Pair<string, string>{obj1= "contacts",obj2="index"},
                new Pair<string, string>{obj1= "passport",obj2="create"},
                new Pair<string, string>{obj1= "shippings",obj2="create"},
                new Pair<string, string>{obj1= "empleadocuba",obj2="ordenesentregadas"},
                new Pair<string, string>{obj1= "enviomaritimo",obj2="create"},
                new Pair<string, string>{obj1= "users",obj2="index"},
                new Pair<string, string>{obj1= "carriers",obj2="create"},
                new Pair<string, string>{obj1= "ordernew",obj2="combos"},
                new Pair<string, string>{obj1= "rechargue",obj2="index"},
                new Pair<string, string>{obj1= "orders",obj2="importorders"},
                new Pair<string, string>{obj1= "ticket",obj2="importticket"},
            };

            var accessBd = await context.AccessLists.ToListAsync();
            foreach (var item in access)
            {
                if (!accessBd.Any(x => x.Controller.ToLower() == item.obj1 && x.Action.ToLower() == item.obj2))
                {
                    context.AccessLists.Add(new AccessList
                    {
                        AccessListId = Guid.NewGuid(),
                        Action = item.obj2,
                        Controller = item.obj1
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }



    public class AgenciaAuthorizeAttribute : ActionFilterAttribute
    {
        private string Users;
        private databaseContext contexto;

        public AgenciaAuthorizeAttribute(string Users)
        {
            this.Users = Users;
            contexto = new databaseContext();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var t = contexto.User.Where(x => x.Username == "admin").FirstOrDefault();
            }
            catch
            {

            }

        }


    }

}
