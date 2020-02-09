using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Team8SSISMobile.DAO;
using Team8SSISMobile.Models;

namespace Team8SSISMobile.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmployeeDAO _employeeDAO;
        private readonly CollectionPointDAO _collectionPointDAO;
        private readonly NotificationChannelDAO _notificationChannelDAO;
        private readonly RequisitionDAO _requisitionDAO;
        private readonly DelegationDAO _delegationDAO;
        private readonly DepartmentDAO _departmentDAO;
        private readonly DisbursementDAO _disbursementDAO;
        private readonly DisbursementItemDAO _disbursementItemDAO;
        public HomeController()
        {
            _employeeDAO = new EmployeeDAO();
            _collectionPointDAO = new CollectionPointDAO();
            _notificationChannelDAO = new NotificationChannelDAO();
            _requisitionDAO = new RequisitionDAO();
            _delegationDAO = new DelegationDAO();
            _departmentDAO = new DepartmentDAO();
            _disbursementDAO = new DisbursementDAO();
            _disbursementItemDAO = new DisbursementItemDAO();
        }

        // GET: Android
        //here is for mobile parts

        public JsonResult MobileLogin(string username, string password)
        {
            //Try to find the user from user name
            Employee user = _employeeDAO.FindEmployeeByUsername(username);
            //if the user exist
            if (user != null)
            {
                //get the hashed string of the input password
                SHA1 sh = new SHA1CryptoServiceProvider();
                sh.ComputeHash(Encoding.ASCII.GetBytes(password));
                byte[] re = sh.Hash;
                StringBuilder sb = new StringBuilder();
                foreach (byte b in re)
                {
                    sb.Append(b.ToString("x2"));//hexidecimal string of 2 chars
                }
                Console.WriteLine(sb.ToString());//5baa61e4c9b93f3f0682250b6cf8331b7ee68fd8
                //compare the input password to actual password, if matched
                if (user.HashedPassward.Equals(sb.ToString()))
                {
                    return Json(new { role = user.Role.Label, status = "success", id = user.IdEmployee }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { status = "fail" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FindEmployees(int id)
        {

            List<string> list = _employeeDAO.FindEmployeeNamesByHeadId(id);

            var namelist = list.Select(x => new { name = x });

            return Json(new { namelist = namelist }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FindCurrentRepAndCP(int id)
        {

            string CP = _collectionPointDAO.FindByHeadId(id);
            string Rep = _employeeDAO.FindDepartmentRepByHeadId(id);
            return Json(new { CP = CP, Rep = Rep }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ChangeRepAndCP(string newRep, string newCP,int id)
        {
            Employee newRepE = _employeeDAO.FindEmployeeByName(newRep);
            string codeDepartment = newRepE.CodeDepartment;
            string oldCollectionPoint = _collectionPointDAO.FindByDepartment(codeDepartment);
            Employee oldRep = _employeeDAO.FindDepartmentRep(codeDepartment);

            //change old rep to employee
            _employeeDAO.PutOldRepBack(oldRep.Name);

            _employeeDAO.ChangeNewRepCP(newRepE.Name, newCP);


            if (newRepE.Name != oldRep.Name)
            {
                //@Shutong: send notification here
                int IdEmployee = oldRep.IdEmployee;

                string message = "Hi " + oldRep.Name
                    + ", you are not Department Rep anymore.";
                _notificationChannelDAO.CreateNotificationsToIndividual(IdEmployee,id, message);

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.Credentials = new System.Net.NetworkCredential(@"huangyuzhe2019@gmail.com", "woshishenaqq6!");
                client.EnableSsl = true;
                //client.Timeout = 5000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                MailMessage mm = new MailMessage("huangyuzhe2019@gmail.com", oldRep.Email);
                mm.Subject = "SSIS System Email";
                mm.Body = message;
                client.Send(mm);

                IdEmployee = newRepE.IdEmployee;
                
                message = "Hi " + newRepE.Name
                   + ", you are appointed as Department Rep.";
                _notificationChannelDAO.CreateNotificationsToIndividual(IdEmployee, id, message);

                mm = new MailMessage("huangyuzhe2019@gmail.com", newRepE.Email);
                mm.Subject = "SSIS System Email";
                mm.Body = message;
                client.Send(mm);
                //end of notification sending
            }
            //if rep didnot change but only cp changes
            else
            {
                if (oldCollectionPoint != newCP)
                {
                    int IdEmployee = oldRep.IdEmployee;
                    
                    
                    string message = "Hi " + oldRep.Name
                        + ", your collection point has been changed by your head.";
                    _notificationChannelDAO.CreateNotificationsToIndividual(IdEmployee, id, message);

                    SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                    client.Credentials = new System.Net.NetworkCredential(@"huangyuzhe2019@gmail.com", "woshishenaqq6!");
                    client.EnableSsl = true;
                    //client.Timeout = 5000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    MailMessage mm = new MailMessage("huangyuzhe2019@gmail.com", oldRep.Email);
                    mm.Subject = "SSIS System Email";
                    mm.Body = message;
                    client.Send(mm);
                    

                }

            }
            return Json(new { status = "Ok" });

        }


        public JsonResult FindNotifications(int id)
        {

            List<NotificationChannel> notificationChannels = _notificationChannelDAO.FindAllNotificationsByIdReceiver(id);

            var notis = notificationChannels.Select(x => new { message= x.Notification.Text, date=x.Date.ToString()});

            return Json(new { notifications =notis  }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult FindRequisitions(int id)
        {
            //will return pending requisitions
            List<Requisition> requisitions = _requisitionDAO.FindAllPendingRequisitionByHeadId(id);

            var reqs = requisitions.Select(x => new { remark=x.HeadRemark, requisitionId=x.IdRequisition, name = x.Employee.Name, date = x.RaiseDate.ToString(),items=x.RequisitionItems.Select(y=>new { description = y.Item.Description, unit = y.Unit }) });

            return Json(new { requisitions = reqs}, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ApproveRequisition(int requisitionId,string remark)
        {
            _requisitionDAO.UpdateApproveStatusAndRemarks(requisitionId,remark);

            //@Shutong: send notification here
            Requisition req = _requisitionDAO.FindRequisitionByRequisionId(requisitionId);
            int IdEmployee = req.IdEmployee;
     
            string message = "Hi," + _employeeDAO.FindEmployeeById(IdEmployee).Name
                + " your requisition: " + req.IdRequisition + " raised on " + req.RaiseDate + " has been approved. Remarks: " + remark;

            _notificationChannelDAO.CreateNotificationsToIndividual(IdEmployee, (int)Session["IdEmployee"], message);

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new System.Net.NetworkCredential(@"huangyuzhe2019@gmail.com", "woshishenaqq6!");
            client.EnableSsl = true;
            //client.Timeout = 5000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            MailMessage mm = new MailMessage("huangyuzhe2019@gmail.com", _employeeDAO.FindEmployeeById(IdEmployee).Email);
            mm.Subject = "SSIS System Email";
            mm.Body = message;
            client.Send(mm);
            //end of notification sending 
            return Json(new { status="Ok" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RejectRequisition(int requisitionId,string remark)
        {
            _requisitionDAO.UpdateRejectStatusAndRemarks(requisitionId,remark);

            //@Shutong: send notification here
            Requisition req = _requisitionDAO.FindRequisitionByRequisionId(requisitionId);
            int IdEmployee = req.IdEmployee;

            string message = "Hi," + _employeeDAO.FindEmployeeById(IdEmployee).Name
                   + " your requisition: " + req.IdRequisition + " raised on " + req.RaiseDate + " has been rejected. Remarks: " + remark;

            _notificationChannelDAO.CreateNotificationsToIndividual(IdEmployee, (int)Session["IdEmployee"], message);

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new System.Net.NetworkCredential(@"huangyuzhe2019@gmail.com", "woshishenaqq6!");
            client.EnableSsl = true;
            //client.Timeout = 5000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            MailMessage mm = new MailMessage("huangyuzhe2019@gmail.com", _employeeDAO.FindEmployeeById(IdEmployee).Email);
            mm.Subject = "SSIS System Email";
            mm.Body = message;
            client.Send(mm);
            //end of notification sending 
            return Json(new { status = "Ok" }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CreateDelegation(int id,string employeeName,string startDate,string endDate,string message)
        {
            //Date format: Feb 2,2020

            DateTime StartDate = DateTime.ParseExact(startDate,"MMM d, yyyy",new CultureInfo("en-US"));
            DateTime EndDate = DateTime.ParseExact(endDate, "MMM d, yyyy", new CultureInfo("en-US")).AddDays(1);

            _delegationDAO.CreateDelegation(employeeName, StartDate, EndDate);

            //@Shutong: send notification here
           
            
            Employee e = _employeeDAO.FindEmployeeByName(employeeName);
            int IdEmployee = e.IdEmployee;

            string msg = "Hi," + employeeName
                     + " You are delegated to Acting Department Head from " + StartDate.ToString() + " to " + EndDate.ToString()
                     + " to assist approve stationery requisition. \r\nRemarks: " + message;

            _notificationChannelDAO.CreateNotificationsToIndividual(IdEmployee, id, msg);

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new System.Net.NetworkCredential(@"huangyuzhe2019@gmail.com", "woshishenaqq6!");
            client.EnableSsl = true;
            //client.Timeout = 5000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            MailMessage mm = new MailMessage("huangyuzhe2019@gmail.com", _employeeDAO.FindEmployeeById(IdEmployee).Email);
            mm.Subject = "SSIS System Email";
            mm.Body = msg;
            client.Send(mm);
            
            msg = "Hi Department Head," 
                     + " you have delegated "+employeeName+" as Acting Department Head from " + StartDate.ToString() + " to " + EndDate.ToString()
                     + " to assist approve stationery requisition. \r\nRemarks: " + message;

            _notificationChannelDAO.CreateNotificationsToIndividual(id, id, msg);

            mm = new MailMessage("huangyuzhe2019@gmail.com", _employeeDAO.FindEmployeeById(id).Email);
            mm.Subject = "SSIS System Email";
            mm.Body = msg;
            client.Send(mm);

            //end of notification sending 

            //add one more day here since the enddate is inclusive.
            return Json(new { status="Ok"}, JsonRequestBehavior.AllowGet);
        }


        public JsonResult FindCollectionPoint(int id,string location)
        {
            //scheduled and received
            
            List<int> idCPs=_collectionPointDAO.FindByClerkId(id);
            var departments = from d in _departmentDAO.FindDepartmentsByLocation(location)
                              select new
                              {
                                  deptName = d.Name,
                                  deptRep =(_employeeDAO.FindDepartmentRep(d.CodeDepartment)==null)? "": _employeeDAO.FindDepartmentRep(d.CodeDepartment).Name,
                                  contact = _employeeDAO.FindDepartmentRep(d.CodeDepartment)==null? "": _employeeDAO.FindDepartmentRep(d.CodeDepartment).Tel,
                                  disId = (d.Disbursements.Where(x => x.IdStatus == 10 || x.IdStatus == 11).FirstOrDefault()==null)? null: (d.Disbursements.Where(x => x.IdStatus == 10 || x.IdStatus == 11).FirstOrDefault()
                                  .IdDisbursement.ToString()),
                                  items = (d.Disbursements.Where(x => x.IdStatus == 10 || x.IdStatus == 11).FirstOrDefault() == null) ? null: (from i in d.Disbursements.Where(x => (x.IdStatus == 10 || x.IdStatus == 11)&& idCPs.Contains(x.IdCollectionPt.GetValueOrDefault(0))).FirstOrDefault().DisbursementItems
                                          select new { description = i.Item.Description, unit = i.UnitIssued })
                              };
            return Json(new { location, departments }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateQuantity(DisbursementModel model)
        {

            int id = model.id;
            string disbursementId = model.disbursementId;
            List<ItemModel> items = model.items;
            //int id, string disbursementId, JArray items

            if (String.IsNullOrEmpty(disbursementId))
            {
                return Json(new { status = "Bad" }, JsonRequestBehavior.AllowGet);
            }

            Disbursement d=_disbursementDAO.FindById(Int32.Parse(disbursementId));


            
            if (d == null)
            {
                return Json(new { status = "Bad" }, JsonRequestBehavior.AllowGet);

            }else if(d.Status.Label.Equals("Received")){
                return Json(new { status = "Bad" }, JsonRequestBehavior.AllowGet);
            }

            bool isAuth = false;
            foreach(CPClerk cpclerk in d.CollectionPoint.CPClerks)
            {
                if (cpclerk.IdStoreClerk == id)
                {
                    isAuth = true;
                }
            }
            if (isAuth == false)
            {
                return Json(new { status = "Bad" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                _disbursementItemDAO.UpdateQuantityIssued(Int32.Parse(disbursementId),items);
            }

            return Json(new { status = "Ok" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AcknowledgeDisbursement(int id, string disbursementId, string remark)
        {
            if (String.IsNullOrEmpty(disbursementId))
            {
                return Json(new { status = "Bad" }, JsonRequestBehavior.AllowGet);
            }
            //check if the disbursement is signed by the rep,
            //if unsigned, return bad
            //if signed,update status and disbursed by
            Disbursement d = _disbursementDAO.FindById(Int32.Parse(disbursementId));
            if (d == null)
            {
                return Json(new { status = "Bad" }, JsonRequestBehavior.AllowGet);

            }
            else if (!d.Status.Label.Equals("Received"))
            {
                return Json(new { status = "Bad" }, JsonRequestBehavior.AllowGet);
            }
            bool isAuth = false;
            foreach (CPClerk cpclerk in d.CollectionPoint.CPClerks)
            {
                if (cpclerk.IdStoreClerk == id)
                {
                    isAuth = true;
                }
            }
            if (isAuth == false)
            {
                return Json(new { status = "Bad" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                _disbursementDAO.UpdateDisbursementToDisbursed(id, Int32.Parse(disbursementId));
                //send notification here

            }
            return Json(new { status = "Ok" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FindCPClerks(int id)
        {
            var location = from name in _collectionPointDAO.FindNameByClerkId(id) select new { cp = name };
            return Json(new { locations=location }, JsonRequestBehavior.AllowGet);
        }

    
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}