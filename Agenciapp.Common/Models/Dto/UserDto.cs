using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
    }
}
