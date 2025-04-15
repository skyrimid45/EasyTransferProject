using System;
using System.Linq;
using System.Web.Mvc;
using Project.Models;

namespace Project.Controllers
{
    public class ETransferController : Controller
    {
        private CustomerContext db = new CustomerContext();

        // GET: E-Transfer Home Page
        public ActionResult Index()
        {
            return View();
        }

        // GET: Send Money Page
        public ActionResult SendMoney()
        {
            int? customerId = Session["CustId"] as int?; // ✅ Match the key used in POST

            if (customerId == null)
                return RedirectToAction("Login", "Customer");

            var accountOptions = db.Accounts
                .Where(a => a.CustId == customerId)
                .Select(a => new SelectListItem
                {
                    Value = a.AccountID.ToString(),
                    Text = a.Type == 0 ? "Chequing" : "Savings"
                }).ToList();

            ViewBag.AccountOptions = new SelectList(accountOptions, "Value", "Text");

            var recipients = db.Customers
                .Where(c => c.CustId != customerId)
                .Select(c => new SelectListItem
                {
                    Value = c.CustId.ToString(),
                    Text = c.Name
                }).ToList();

            ViewBag.Recipients = new SelectList(recipients, "Value", "Text");

            return View();
        }


        // AJAX: Get email from selected recipient ID
        public JsonResult GetRecipientEmail(int custId)
        {
            var recipient = db.Customers.FirstOrDefault(c => c.CustId == custId);
            return Json(new { email = recipient?.Email }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMoney(ETransfer model)
        {
            int custId = Convert.ToInt32(Session["CustId"]);

            // Repopulate ViewBag dropdowns before returning the view
            var accountOptions = db.Accounts
                .Where(a => a.CustId == custId)
                .Select(a => new SelectListItem
                {
                    Value = a.AccountID.ToString(),
                    Text = a.Type == 0 ? "Chequing" : "Savings"
                }).ToList();

            ViewBag.AccountOptions = new SelectList(accountOptions, "Value", "Text");

            var recipients = db.Customers
                .Where(c => c.CustId != custId)
                .Select(c => new SelectListItem
                {
                    Value = c.CustId.ToString(),
                    Text = c.Name
                }).ToList();

            ViewBag.Recipients = new SelectList(recipients, "Value", "Text");

            // Validate logic
            var senderAccount = db.Accounts.FirstOrDefault(a => a.AccountID == model.SenderId && a.CustId == custId);
            var recipient = db.Customers.FirstOrDefault(c => c.Email == model.RecipientEmail);

            if (senderAccount == null || recipient == null)
            {
                ModelState.AddModelError("", "Invalid sender or recipient.");
                return View(model);
            }

            if (model.Amount <= 0 || model.Amount > senderAccount.Balance)
            {
                ModelState.AddModelError("", "Invalid amount.");
                return View(model);
            }

            // Perform transfer
            senderAccount.Balance -= model.Amount;

            var recipientAccount = db.Accounts
                .FirstOrDefault(a => a.CustId == recipient.CustId && a.Type == 0);

            if (recipientAccount != null)
            {
                recipientAccount.Balance += model.Amount;
            }

            model.SenderId = senderAccount.CustId ?? 0;
            model.TransferType = "Send"; // 🛠️ Fixes the NULL error
            model.CreatedAt = DateTime.Now;
            model.TransferDate = DateTime.Now;
            model.Status = "Sent";

            db.ETransfers.Add(model);
            db.SaveChanges();


            TempData["SuccessMessage"] = "Money sent successfully!";
            return RedirectToAction("SendMoney");
        }


        // GET: Request Money Page
        public ActionResult Request()
        {
            return View();
        }

        // GET: GiftCard
        [HttpGet]
        public ActionResult GiftCard()
        {
            int? custId = Session["CustId"] as int?;
            if (custId == null)
                return RedirectToAction("Login", "Customer");

            var accountOptions = db.Accounts
                .Where(a => a.CustId == custId && (a.Type == AccountType.Chequings || a.Type == AccountType.Savings))
                .Select(a => new SelectListItem
                {
                    Value = a.AccountID.ToString(),
                    Text = a.Type == AccountType.Chequings ? "Chequing" : "Savings"
                }).ToList();

            ViewBag.AccountOptions = new SelectList(accountOptions, "Value", "Text");

            return View();
        }

        // POST: /ETransfer/GiftCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GiftCard(int SenderId, string RecipientEmail, decimal GiftAmount)
        {
            int? senderCustId = Session["CustId"] as int?;
            if (senderCustId == null)
                return RedirectToAction("Login", "Customer");

            var senderAccount = db.Accounts.FirstOrDefault(a => a.AccountID == SenderId && a.CustId == senderCustId);
            var recipient = db.Customers.FirstOrDefault(c => c.Email == RecipientEmail);

            if (senderAccount == null || recipient == null || GiftAmount <= 0 || GiftAmount > senderAccount.Balance)
            {
                ModelState.AddModelError("", "Invalid input or insufficient funds.");

                // Repopulate dropdown
                var accountOptions = db.Accounts
                    .Where(a => a.CustId == senderCustId && (a.Type == AccountType.Chequings || a.Type == AccountType.Savings))
                    .Select(a => new SelectListItem
                    {
                        Value = a.AccountID.ToString(),
                        Text = a.Type == AccountType.Chequings ? "Chequing" : "Savings"
                    }).ToList();

                ViewBag.AccountOptions = new SelectList(accountOptions, "Value", "Text");
                return View();
            }

            var recipientAccount = db.Accounts.FirstOrDefault(a => a.CustId == recipient.CustId);
            if (recipientAccount == null)
            {
                ModelState.AddModelError("", "Recipient does not have an account.");
                return View();
            }

            // Transfer the gift
            senderAccount.Balance -= GiftAmount;
            recipientAccount.GiftBalance += GiftAmount;

            db.SaveChanges();

            TempData["SuccessMessage"] = "Gift card sent successfully!";
            return RedirectToAction("GiftCard");
        }



        // GET: Success Page
        public ActionResult Success()
        {
            return View();
        }

    }
}
