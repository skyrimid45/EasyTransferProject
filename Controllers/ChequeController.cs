using Project.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.Controllers
{
    public class ChequeController : Controller
    {
        private CustomerContext db = new CustomerContext();

        // GET: Cheque/Upload
        public ActionResult Upload()
        {
            return View();
        }

        // POST: Cheque/Upload
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase chequeImage, decimal amount, string accountType)
        {
            int? custId = Session["CustId"] as int?;
            if (custId == null || chequeImage == null || amount <= 0 || string.IsNullOrEmpty(accountType))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            string fileName = Path.GetFileName(chequeImage.FileName);
            string filePath = Path.Combine(Server.MapPath("~/Content/Uploads"), fileName);
            chequeImage.SaveAs(filePath);

            var upload = new ChequeUpload
            {
                CustId = custId.Value,
                FilePath = "/Content/Uploads/" + fileName,
                Amount = amount,
                AccountType = accountType,
                Status = "Pending",
                UploadedAt = DateTime.Now
            };

            db.ChequeUploads.Add(upload);
            db.SaveChanges();

            ViewBag.Message = "Cheque uploaded successfully!";
            return View();
        }

        // GET: Admin View - Review Cheques
        public ActionResult Review()
        {
            if (Session["Role"]?.ToString() != "Admin")
                return RedirectToAction("NotAuthorized", "Home");

            var pending = db.ChequeUploads.Where(c => c.Status == "Pending").ToList();
            return View(pending);
        }

        // POST: Admin Approve
        [HttpPost]
        public ActionResult Approve(int id)
        {
            var cheque = db.ChequeUploads.Find(id);
            if (cheque != null && cheque.Status == "Pending")
            {
                cheque.Status = "Approved";

                var account = db.Accounts.FirstOrDefault(a => a.CustId == cheque.CustId && a.Type.ToString() == cheque.AccountType);
                if (account != null)
                {
                    account.Balance += cheque.Amount;
                }

                db.SaveChanges();
            }

            return RedirectToAction("Review");
        }

        // POST: Admin Deny
        [HttpPost]
        public ActionResult Deny(int id)
        {
            var cheque = db.ChequeUploads.Find(id);
            if (cheque != null && cheque.Status == "Pending")
            {
                cheque.Status = "Denied";
                db.SaveChanges();
            }

            return RedirectToAction("Review");
        }
    }
}
