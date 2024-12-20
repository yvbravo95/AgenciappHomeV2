using AgenciappHome.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agenciapp.Common.Services
{
    public interface IUserResolverService
    {
        User GetUser();
    }
    public class UserResolverService : IUserResolverService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly databaseContext _context;
        public UserResolverService(IHttpContextAccessor contextAccessor, databaseContext context)
        {
            _contextAccessor = contextAccessor;
            _context = context;
        }

        public User GetUser()
        {
            string userName = _contextAccessor.HttpContext.User?.Identity?.Name??"";
            return _context.User.FirstOrDefault(x => x.Username == userName);
        }
    }
}
