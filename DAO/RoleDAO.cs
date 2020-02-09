using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Team8SSISMobile.Models;

namespace Team8SSISMobile.DAO
{
    public class RoleDAO
    {
        private readonly SSISContext context;
        public RoleDAO()
        {
            this.context = new SSISContext();
        }

        public string FindRoleLabelById(int id)
        {
            Role role=context.Roles.OfType<Role>().Where(r => r.IdRole == id).FirstOrDefault();
            return role.Label;
        }   

    }
}