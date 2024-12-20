using System;

namespace Agenciapp.Service.IClientServices.Models
{
    public class AddNoteModel
    {
        public Guid ClientId { get; set; }
        public string Note { get; set; }
    }
}