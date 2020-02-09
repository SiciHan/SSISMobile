using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Team8SSISMobile.Models;

namespace Team8SSISMobile.DAO
{
    public class DepartmentDAO
    {
        private readonly SSISContext context;

        public DepartmentDAO()
        {
            context = new SSISContext();
        }
        //private readonly SSISContext context;

        //public DepartmentDAO()
        //{
        //    this.context = new SSISContext();
        //}
        //SH
        public Department FindDepartmentCollectionPoint(String codeDepartment)
        {
            return context.Departments.Where(d => d.CodeDepartment.Equals(codeDepartment)).FirstOrDefault();
        }
        public bool UpdateCollectionPt(string codeDepartment, int idCollectionPt)
        {
            Department model = null;
            using (SSISContext db = new SSISContext())
            {
                model = db.Departments.OfType<Department>()
                    .Where(x => x.CodeDepartment == codeDepartment)
                    .FirstOrDefault();
                if (model == null) return false;
                CollectionPoint collectionPt = db.CollectionPoints.OfType<CollectionPoint>()
                   .Where(x => x.IdCollectionPt == idCollectionPt)
                   .FirstOrDefault();
                if (collectionPt == null) return false;
                model.IdCollectionPt = idCollectionPt;
                db.SaveChanges();
            }
            return true;
        }

        internal string FindCodeDepartmentByIdEmployee(int v)
        {
            Department department = context.Employees.OfType<Employee>().Where(x => x.IdEmployee == v).Select(x => x.Department).FirstOrDefault();
            return department.CodeDepartment;
        }

        internal List<Department> FindDepartmentsByLocation(string location)
        {
            return context.Departments.OfType<Department>()
                .Where(x => x.CollectionPt.Location.Equals(location))
                .Include(x => x.Disbursements)
                .Include(x => x.Disbursements.Select(y => y.DisbursementItems))
                .Include(x=>x.Disbursements.Select(y=>y.DisbursementItems.Select(z=>z.Item)))
                .Include(x=>x.CollectionPt)
                .Include(x=>x.CollectionPt.CPClerks)
                .ToList();
        }
    }
}