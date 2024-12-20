using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public string Path { get; set; }
        public string FileType { get; set; }
    }

    public class AttachmentPassport: Attachment
    {
        public Passport Passport { get; set; }
        public string Description { get; set; }
    }

    public class AttachmentOrder : Attachment
    {
        public Order Order { get; set; }
        public string Description { get; set; }
    }

    public class AttachmentReclamo : Attachment
    {
        public Reclamo Reclamo { get; set; }
        public string Description { get; set; }
    }

    public class AttachmentServicio: Attachment
    {
        public Servicio Servicio { get; set; }
        public string Description { get; set; }
    }
    
}
