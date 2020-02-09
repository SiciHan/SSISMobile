using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Team8SSISMobile.Models;

namespace Team8SSISMobile.DAO
{
    public class SupplierDAO
    {
        public Supplier GetSupplierById(string codeSupplier)
        {
            Supplier supplier = new Supplier();
            using(SSISContext db = new SSISContext())
            {
                supplier = db.Suppliers
                    .Where(s => s.CodeSupplier == codeSupplier)
                    .FirstOrDefault();
            }
            return supplier;
        }
    }
}