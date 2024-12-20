using System;

namespace AgenciappHome.Models
{
    public partial class Event
    {
        public Guid Id { get; set; }
        public Client Client { get; set; }
        public Guid ClientId { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public Agency Agency { get; set; }
        public Guid AgencyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
