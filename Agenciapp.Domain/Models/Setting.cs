using System;
using System.ComponentModel.DataAnnotations;
using AgenciappHome.Models;

namespace Agenciapp.Domain.Models
{
    public class Setting
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public Agency Agency { get; set; }  
        public string Value { get; set; }       
    }
}