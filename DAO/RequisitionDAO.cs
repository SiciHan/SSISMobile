﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Team8SSISMobile.Models;

namespace Team8SSISMobile.DAO
{
    public class RequisitionDAO
    {
        private readonly SSISContext context;

        public RequisitionDAO()
        {
            this.context = new SSISContext();
        }
        //SH
        public Requisition FindRequisitionByRequisionId(int idRequisition)
        {
            
            //return context.Requisitions.Include("Employee").Where(r => r.IdRequisition == idRequisition ).FirstOrDefault();
           
            return context.Requisitions.OfType<Requisition>().Include("Employee").Where(r => r.IdRequisition == idRequisition ).FirstOrDefault();
        }
        //SH
        public void UpdateApproveStatus(int idRequisition)
        {
            Requisition r = FindRequisitionByRequisionId(idRequisition);
            r.ApprovedDate = DateTime.Now;
            r.IdStatusCurrent = 3;
            DateTime saveNow = DateTime.Now;
            r.ApprovedDate = saveNow;
            context.SaveChanges();
        }
        //SH
        public void UpdateRejectStatus(int idRequisition)
        {
            Requisition r = FindRequisitionByRequisionId(idRequisition);
            r.IdStatusCurrent = 4;
            context.SaveChanges();
        }

        public Requisition CreateRequisition(int IdEmployee)
        {
            Requisition requisition = new Requisition();
            requisition.IdEmployee = IdEmployee;
            requisition.StatusCurrent = context.Status.OfType<Status>().Where(x => x.Label.Equals("Incomplete")).FirstOrDefault();
            requisition.RaiseDate = DateTime.Now;
            requisition.ApprovedDate = DateTime.Parse("01/01/1900");
            requisition.WithdrawlDate = DateTime.Parse("01/01/1900");
            context.Requisitions.Add(requisition);
            context.SaveChanges();
            return requisition;

        }

        public List<Retrieval> RetrieveRequisition(List<string> DClerk, DateTime StartDate, DateTime EndDate)
        {
            // Get Employee That is working in Department from DClerk
            List<int> IdEmployee = new List<int>();
            foreach (string CodeDpt in DClerk)
            {
                var IdEmp = context.Employees
                                    .Where(x => x.CodeDepartment.Equals(CodeDpt))
                                    .Select(x => x.IdEmployee);
                if (IdEmp != null)
                {
                    foreach (var ie in IdEmp)
                        IdEmployee.Add(ie);
                }
            }

            // Get approved requisition between startdate and enddate
            // Ignore employee for the moment
            List<int> IdRequestedItem = new List<int>();
            foreach (int ie in IdEmployee) 
            {
                var IdReqItem = context.Requisitions
                                    .Where(x => x.IdStatusCurrent == 3)
                                    .Where(x => x.ApprovedDate <= EndDate)
                                    .Where(x => x.ApprovedDate >= StartDate)
                                    .Where(x => x.IdEmployee == ie)
                                    .Select(x => x.IdRequisition);
                if (IdReqItem != null)
                {
                    foreach (var iri in IdReqItem)
                        IdRequestedItem.Add(iri);
                }
                
            }

            // Get Retrieval Form 
            List<Retrieval> RetrievalItem = new List<Retrieval>();
            foreach (var sr in IdRequestedItem)
            {
                var retrieval = context.Items
                                        .Join(context.RequisitionItems,
                                        i => i.IdItem, ri => ri.IdItem,
                                        (i, ri) => new { i, ri })
                                        .Join(context.Requisitions,
                                        iri => iri.ri.IdRequisiton, r => r.IdRequisition,
                                        (iri, r) => new { iri, r })
                                        .Join(context.Employees,
                                        riri => riri.r.IdEmployee, e => e.IdEmployee,
                                        (riri, e) => new Retrieval
                                        {
                                            Description = riri.iri.i.Description,
                                            IdItem = riri.iri.i.IdItem,
                                            StockUnit = riri.iri.i.StockUnit,
                                            Unit = riri.iri.ri.Unit,
                                            CodeDepartment = e.CodeDepartment,
                                            IdRequisition = riri.r.IdRequisition,
                                            ApprovedDate = riri.r.ApprovedDate,
                                            Location = riri.iri.i.Location
                                        }).Where(x => x.IdRequisition == sr);
                if (retrieval != null)
                {
                    foreach (var r in retrieval)
                        RetrievalItem.Add(r);
                }
                
            }

            return RetrievalItem;

        }

        internal void DeleteRequisition(int? selectedId)
        {
            int id = selectedId.GetValueOrDefault(0);
            Requisition requisition = context.Requisitions.OfType<Requisition>().Where(x => x.IdRequisition == id).FirstOrDefault();
            context.Requisitions.Remove(requisition);
            context.SaveChanges();
        }

        internal string GetStatusLabel(int reqID)
        {
            Requisition requisition=context.Requisitions.OfType<Requisition>().Where(x => x.IdRequisition == reqID).Include(x=>x.StatusCurrent).FirstOrDefault();
            return requisition.StatusCurrent.Label;
        }

        internal List<Requisition> RetrieveRequisitionByEmpId(int idEmployee)
        {
            return context.Requisitions.OfType<Requisition>().Where(x => x.IdEmployee == idEmployee).ToList();
        }

        internal Requisition RetrieveRequisitionByReqId(int ReqId)
        {
            return context.Requisitions.OfType<Requisition>().Where(x => x.IdRequisition == ReqId).FirstOrDefault();

        }

        public List<Requisition> FindAllRequisition()
        {
            return context.Requisitions.OfType<Requisition>()
                                        .Include(r => r.RequisitionItems)
                                        .Include(r => r.RequisitionItems.Select(ri => ri.Item))
                                        .Include(r => r.Employee)
                                        .ToList();
        }

        public List<Requisition> FindAllPendingRequisitionByHeadId(int id)
        {
            Employee head = context.Employees.Where(x => x.IdEmployee == id).Include(x => x.Department).FirstOrDefault();
            string codedepartment = head.CodeDepartment;
            return context.Requisitions.OfType<Requisition>()
                                        .Include(r => r.RequisitionItems)
                                        .Include(r => r.RequisitionItems.Select(ri => ri.Item))
                                        .Include(r => r.Employee)
                                        .Where(x=>x.Employee.CodeDepartment==codedepartment)
                                        .Where(x=>x.StatusCurrent.Label=="Pending")
                                        .ToList();
        }


        public void UpdateApproveStatusAndRemarks(int idRequisition, string remarks)
        {
            Requisition r = FindRequisitionByRequisionId(idRequisition);
            r.ApprovedDate = DateTime.Now;
            r.IdStatusCurrent = 3;
            r.HeadRemark = remarks;
            DateTime saveNow = DateTime.Now;
            r.ApprovedDate = saveNow;
            context.SaveChanges();
        }
        //SH
        public void UpdateRejectStatusAndRemarks(int idRequisition, string remarks)
        {
            Requisition r = FindRequisitionByRequisionId(idRequisition);
            r.IdStatusCurrent = 4;
            r.HeadRemark = remarks;
            context.SaveChanges();
        }
    }
}