using System;

namespace Agenciapp.Service.ICubiqServices.Models
{
    public class ItemContainerModel
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}