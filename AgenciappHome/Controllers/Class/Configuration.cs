using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers.Class
{
    public static class Configuration
    {
        public static void setNotificationByAgency(databaseContext _context)
        {
            foreach (var agency in _context.Agency)
            {
                bool exist_notification = _context.NotificationByAgencies.Any(x => x.AgencyId == agency.AgencyId);
                if (!exist_notification)
                {
                    _context.NotificationByAgencies.Add(new NotificationByAgency { 
                        AgencyId = agency.AgencyId,
                    });
                }
            }
            _context.SaveChanges();
        }
    }
}
