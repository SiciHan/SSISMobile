using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Team8SSISMobile.Models;

namespace Team8SSISMobile.DAO
{

    //public class SSISInitializer<T> :DropCreateDatabaseIfModelChanges<SSISContext>
    public class SSISInitializer<T>: CreateDatabaseIfNotExists<SSISContext>
    {
        protected override void Seed(SSISContext context)
        {
            base.Seed(context);
        }
    }
}
