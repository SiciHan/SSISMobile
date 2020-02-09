﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Team8SSISMobile.Models;

namespace Team8SSISMobile.DAO
{
    public class StockRecordDAO
    {
        private readonly SSISContext context;
        public StockRecordDAO()
        {
            this.context = new SSISContext();
        }

        public StockRecord FindById(int voucherId)
        {
            StockRecord voucher = context.StockRecords
                .Where(x => x.IdStockRecord == voucherId)
                .Include(x=>x.StoreClerk)
                .Include(x=>x.Item)
                .Include(x=>x.Operation)
                .Include(x=>x.Supplier)
                
                .FirstOrDefault();

            return voucher;
        }

        public void UpdateStockRecord(int IdStoreClerk, List<int> IdDisbursementItem)
        {
            foreach (int x in IdDisbursementItem)
            {
                DisbursementItem disbursementItem = context.DisbursementItems
                                                    .Where(di => di.IdDisbursementItem == x)
                                                    .FirstOrDefault();

                Operation operation = context.Operations.Where(o => o.IdOperation == 1).FirstOrDefault();
                Item item = context.Items.Where(i => i.IdItem == disbursementItem.IdItem).FirstOrDefault();
                Employee storeclerk = context.Employees.Where(e => e.IdEmployee == IdStoreClerk).FirstOrDefault();
                Disbursement disbursement = context.Disbursements
                                                    .Where(d => d.IdDisbursement == disbursementItem.IdDisbursement).FirstOrDefault();
                Department department = context.Departments
                                                .Where(dpt => dpt.CodeDepartment.Equals(disbursement.CodeDepartment)).FirstOrDefault();
                Supplier supplier = context.Suppliers.Where(s => s.CodeSupplier == item.CodeSupplier1).FirstOrDefault();

                StockRecord stockrecord = new StockRecord();
                stockrecord.Date = DateTime.Now;
                stockrecord.IdDepartment = disbursement.CodeDepartment;
                stockrecord.Department = department;
                stockrecord.IdOperation = 1;
                stockrecord.Operation = operation;
                stockrecord.IdItem = disbursementItem.IdItem;
                stockrecord.Item = item;
                stockrecord.IdSupplier = item.CodeSupplier1;
                stockrecord.Supplier = supplier;
                stockrecord.IdStoreClerk = IdStoreClerk;
                stockrecord.StoreClerk = storeclerk;
                stockrecord.Unit = disbursementItem.UnitIssued;
                context.StockRecords.Add(stockrecord);
                context.SaveChanges();

            }
        }

        public List<StockRecord> FindVoucher()
        {
            List<StockRecord> vouchers = context.StockRecords
                .Include("Operation")
                .Include("Department")
                .Include("Supplier")
                .Include("StoreClerk")
                .Include("Item")
                .Where(
                    x => x.IdOperation == 3 || 
                    x.IdOperation == 4 || 
                    x.IdOperation == 5 || 
                    x.IdOperation == 6 &&
                    x.Supplier.SupplierItems.Where(y => y.IdItem == x.IdItem).Select(y => y.Price).FirstOrDefault() >= 250
                )
                .ToList();

            return vouchers;
        }

        public List<StockRecord> FindJudgedVoucher()
        {
            List<StockRecord> vouchers = context.StockRecords
                .Include("Operation")
                .Include("Department")
                .Include("Supplier")
                .Include("StoreClerk")
                .Include("Item")
                .Where(
                    x => x.IdOperation >= 7 &&
                    x.IdOperation <= 14 && 
                    x.Supplier.SupplierItems.Where(y => y.IdItem == x.IdItem).Select(y => y.Price).FirstOrDefault() >= 250
                )
                .ToList();

            return vouchers;
        }

        public List<StockRecord> FindVoucherForSupervisor()
        {
            List<StockRecord> vouchers = context.StockRecords
                .Include("Operation")
                .Include("Department")
                .Include("Supplier")
                .Include("StoreClerk")
                .Include("Item")
                .Where(
                    x => x.IdOperation == 3 ||
                    x.IdOperation == 4 ||
                    x.IdOperation == 5 ||
                    x.IdOperation == 6
                )
                .ToList();

            return vouchers;
        }

        public List<StockRecord> FindJudgedVoucherForSupervisor()
        {
            List<StockRecord> vouchers = context.StockRecords
                .Include("Operation")
                .Include("Department")
                .Include("Supplier")
                .Include("StoreClerk")
                .Include("Item")
                .Where(
                    x => x.IdOperation >= 7 &&
                    x.IdOperation <= 14
                    //x.Supplier.SupplierItems.Where(y => y.IdItem == x.IdItem).Select(y => y.Price).FirstOrDefault() >= 250
                )
                .ToList();

            return vouchers;
        }

        public void UpdateVoucherToApproved(List<StockRecord> vouchers)
        {
            foreach(StockRecord voucher in vouchers)
            {
                StockRecord temp = context.StockRecords
                    .Where(x => x.IdStockRecord == voucher.IdStockRecord)
                    .FirstOrDefault();
                if(temp.IdOperation == 3)
                {
                    temp.IdOperation = 7;
                }
                else if(temp.IdOperation == 4)
                {
                    temp.IdOperation = 9;
                }
                else if (temp.IdOperation == 5)
                {
                    temp.IdOperation = 12;
                }
                else if (temp.IdOperation == 6)
                {
                    temp.IdOperation = 14;
                }
                context.SaveChanges();
            }
        }

        public void UpdateVoucherToRejected(List<StockRecord> vouchers)
        {
            foreach (StockRecord voucher in vouchers)
            {
                StockRecord temp = context.StockRecords
                    .Where(x => x.IdStockRecord == voucher.IdStockRecord)
                    .FirstOrDefault();
                if (temp.IdOperation == 3)
                {
                    temp.IdOperation = 8;
                }
                else if (temp.IdOperation == 4)
                {
                    temp.IdOperation = 10;
                }
                else if (temp.IdOperation == 5)
                {
                    temp.IdOperation = 11;
                }
                else if (temp.IdOperation == 6)
                {
                    temp.IdOperation = 13;
                }
                context.SaveChanges();
            }
        }
        //James
        internal void StockAdjustmentDuringDisbursement(int qtyDisbursed, DisbursementItem di, int idStoreClerk)
        {
            // 1. Raise SA for damaged good for approval
            StockRecord raiseSA = new StockRecord
            {
                Date = DateTime.Now,
                IdOperation = 5,
                IdDepartment = di.Disbursement.CodeDepartment,
                IdStoreClerk = idStoreClerk,
                IdItem = di.IdItem,
                Unit = (di.UnitIssued - qtyDisbursed) * -1
            };
            context.StockRecords.Add(raiseSA);

            // 2. Create reversal StockRecord transaction for unit issued to the department
            StockRecord reverseSr = new StockRecord
            {
                Date = DateTime.Now,
                IdOperation = 1,
                IdDepartment = di.Disbursement.CodeDepartment,
                IdStoreClerk = idStoreClerk,
                IdItem = di.IdItem,
                Unit = di.UnitIssued - qtyDisbursed
            };
            context.StockRecords.Add(reverseSr);

            context.SaveChanges();
        }

        internal void RaiseSA(DateTime date, int idOperation, String idDepartment, String idSupplier, int idStoreClerk, int idItem, int unit)
        {
            StockRecord sr = new StockRecord
            {
                Date = date,
                IdOperation = idOperation,
                IdDepartment = idDepartment,
                IdSupplier = idSupplier,
                IdStoreClerk = idStoreClerk,
                IdItem = idItem,
                Unit = unit
            };

            context.StockRecords.Add(sr);
            context.SaveChanges();
        }

        //James
        internal List<StockRecord> FindByMonthAndYear(DateTime month)
        {
            return context.StockRecords
                .Where(x => x.Date.Year == month.Year && x.Date.Month == month.Month && x.IdOperation > 2)
                .Include(x => x.Operation)
                .Include(x => x.Department)
                .Include(x => x.Supplier)
                .Include(x => x.StoreClerk)
                .Include(x => x.Item)
                .ToList();
        }
    }
}