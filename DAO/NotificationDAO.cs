using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Team8SSISMobile.Models;

namespace Team8SSISMobile.DAO
{
    public class NotificationDAO
    {
        private readonly SSISContext context;

        public NotificationDAO()
        {
            this.context = new SSISContext();
        }

        internal int CreateNotification(String message)
        {
            Notification notif = new Notification()
            {
                Text = message
            };
            context.Notifications.Add(notif);
            context.SaveChanges();

            return notif.IdNotification;
        }
      
    }
}