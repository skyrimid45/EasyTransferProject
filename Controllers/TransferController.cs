using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;

namespace Project.Controllers
{
    public class TransferController : Controller
    {
        private CustomerContext db = new CustomerContext();

        // GET: Transfer Form
        public ActionResult Index()
        {
            // Safely convert the session value to an integer
            int? custId = Session["CustId"] as int?;

            // Check if custId is null
            if (custId == null)
            {
                return RedirectToAction("Login", "Customer");  // Redirect to login page if session is not set
            }

            // Use the converted custId value in your query
            var accounts = db.Accounts.Where(a => a.CustId == custId.Value).ToList();

            var model = new TransferView
            {
                Accounts = accounts
            };

            return View("Index", model);  // Ensure this points to the Index.cshtml view
        }


        // POST: Transfer Funds
        [HttpPost]
        public ActionResult Index(TransferView model)
        {
            int? custId = Session["CustId"] as int?;

            if (custId == null)
            {
                return RedirectToAction("Login", "Customer");
            }

            model.Accounts = db.Accounts.Where(a => a.CustId == custId.Value).ToList();

            if (ModelState.IsValid)
            {
                var from = db.Accounts.FirstOrDefault(a => a.AccountID == model.FromAccountId && a.CustId == custId);
                var to = db.Accounts.FirstOrDefault(a => a.AccountID == model.ToAccountId && a.CustId == custId);

                if (from == null || to == null)
                {
                    ModelState.AddModelError("", "Invalid account selection.");
                }
                else if (from.AccountID == to.AccountID)
                {
                    ModelState.AddModelError("", "Cannot transfer to the same account.");
                }
                else if (from.Balance < model.Amount)
                {
                    ModelState.AddModelError("", "Insufficient funds.");
                }
                else
                {
                    from.Balance -= model.Amount;
                    to.Balance += model.Amount;
                    db.SaveChanges();

                    return RedirectToAction("Success");
                }
            }

            return View("Index", model);
        }


        // GET: Success Page
        public ActionResult Success()
        {
            return View("TransferSuccess");
        }
    }
}
