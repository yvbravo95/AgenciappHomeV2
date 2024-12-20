using AgenciappHome.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers.Class
{
    public class CustomAuthorizeAttribute:AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        private string role;

        public CustomAuthorizeAttribute(string role)
        {
            this.role = role;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var service = (IAuthorizationService)context.HttpContext.RequestServices.GetService(typeof(IAuthorizationService));

            var roleRequirement = new RoleRequirement(this.role);
            var result = await service.AuthorizeAsync(context.HttpContext.User, null, roleRequirement);
            if (!result.Succeeded)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    public class RoleRequirement : IAuthorizationRequirement
    {
        public RoleRequirement(string role)
        {
            this.Role = role;
        }

        public string Role { get; private set; }
    }

    public class CustomRequirementHandler : AuthorizationHandler<RoleRequirement>
    {
        private readonly databaseContext _context;
        // Usa IoC (inyección de dependencias) para incluir
        // los servicios que necesitas para validar el rol.
        public CustomRequirementHandler(databaseContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            
            // añade tu lógica aquí, puede ser obtener los roles y usuarios (JOIN) con EF.
            //  o validarlos contra un servicio

            if (requirement.Role == "ADMIN")
            {
                var t = _context.User.Where(x => x.Type == "Administrador").ToList();
                context.Succeed(requirement);
            }
        }
    }
}
