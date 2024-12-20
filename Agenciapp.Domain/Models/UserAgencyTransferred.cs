using System;

namespace AgenciappHome.Models
{
    public class UserAgencyTransferred
    {
        public UserAgencyTransferred()
        {

        }
        public UserAgencyTransferred(Agency agency, User user)
        {
            Agency = agency;
            User = user;
        }
        public Guid AgencyId { get; set; }
        public Guid UserId { get; set; }
        public Agency Agency { get; set; }
        public User User { get; set; }
    }
}
