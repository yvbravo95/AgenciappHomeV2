using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public abstract class AttachmentTicket
    {
        [Key]
        public Guid IdDocument { get; set; }
        public String type { get; set; }
        public string Description { get; set; }
        public Guid TicketForeignKey { get; set; }
        public Ticket Ticket { get; set; }
    }

    public class ImageAttachment : AttachmentTicket
    {
        public byte[] value { get; set; }
    }

    public class DocumentAttachment : AttachmentTicket
    {
        public string filename { get; set; }
    }
}
