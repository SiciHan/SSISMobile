﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Team8SSISMobile.Models;

namespace Team8SSISMobile.DAO
{
    public class SupplierItemDAO
    {
        private readonly SSISContext context;

        public SupplierItemDAO()
        {
            this.context = new SSISContext();
        }

        public List<SupplierItem> GetSuppliersById(int itemId)
        {
            List<SupplierItem> suppliers = new List<SupplierItem>();
            using (SSISContext db = new SSISContext())
            {
                suppliers = db.SupplierItems
                    .Include("Supplier")
                    .Include("Item")
                    .Where(s => s.IdItem == itemId)
                    .ToList();
            }
            return suppliers;
        }

        public SupplierItem FindByItem(Item item)
        {
            return context.SupplierItems
                .Include("Item")
                .Where(x => x.IdItem == item.IdItem && x.IdSupplier.Equals(item.CodeSupplier1))
                .FirstOrDefault();
        }

        public SupplierItem FindSupplierItemByIditemAndCodesupplier(int iditem, string codesupplier)
        {
            return context.SupplierItems.OfType<SupplierItem>()
                .Where(x => x.IdItem ==iditem && x.IdSupplier.Equals(codesupplier))
                .Include(y=>y.Item)
                .Include(y => y.Supplier).FirstOrDefault();
        }
    }
}