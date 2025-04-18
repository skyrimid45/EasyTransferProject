using System;
using System.Linq;
using System.Web.Mvc;
using Project.Models;
using System.Collections.Generic;

namespace Project.Controllers
{
    public class ETransferController : Controller
    {
        private CustomerContext db = new CustomerContext();

        // GET: E-Transfer Home Page
        public ActionResult Index()
        {
            int? custId = Session["CustId"] as int?;
            if (custId != null)
            {
                // Fetch any pending requests sent to this user
                var pendingRequests = db.ETransfers
                    .Where(e => e.SenderId == custId && e.TransferType == "Request" && e.Status == "Pending")
                    .ToList();

                ViewBag.PendingRequests = pendingRequests;

                // Load account options (Chequing or Savings)
                var accountOptions = db.Accounts
                    .Where(a => a.CustId == custId)
                    .Select(a => new SelectListItem
                    {
                        Value = a.AccountID.ToString(),
                        Text = a.Type == AccountType.Chequings ? "Chequing" : "Savings"
                    }).ToList();

                ViewBag.AccountOptions = accountOptions;
            }

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

        // GET: /ETransfer/PendingRequests
        public ActionResult PendingRequests()
        {
            int? custId = Session["CustId"] as int?;
            if (custId == null)
                return RedirectToAction("Login", "Customer");

            var pendingRequests = db.ETransfers
                .Where(r => r.SenderId == custId && r.TransferType == "Request" && r.Status == "Pending")
                .ToList();

            var accountOptions = db.Accounts
                .Where(a => a.CustId == custId)
                .Select(a => new SelectListItem
                {
                    Value = a.AccountID.ToString(),
                    Text = a.Type == AccountType.Chequings ? "Chequing" : "Savings"
                }).ToList();

            ViewBag.AccountOptions = accountOptions;

            return View(pendingRequests);
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
            int? recipientId = Session["CustId"] as int?;
            if (recipientId == null)
                return RedirectToAction("Login", "Customer");

            var senders = db.Customers
                .Where(c => c.CustId != recipientId)
                .Select(c => new SelectListItem
                {
                    Value = c.CustId.ToString(),
                    Text = c.Name
                }).ToList();

            ViewBag.Senders = new SelectList(senders, "Value", "Text");
            return View();
        }

        // AJAX: Fetch sender email
        public JsonResult GetSenderEmail(int custId)
        {
            var sender = db.Customers.FirstOrDefault(c => c.CustId == custId);
            return Json(new { email = sender?.Email }, JsonRequestBehavior.AllowGet);
        }

        // POST: Handle Request Submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Request(string SenderEmail, int RecipientAccountType, decimal Amount)
        {
            int? recipientId = Session["CustId"] as int?;
            if (recipientId == null)
                return RedirectToAction("Login", "Customer");

            var sender = db.Customers.FirstOrDefault(c => c.Email == SenderEmail);
            var recipient = db.Customers.FirstOrDefault(c => c.CustId == recipientId);
            var recipientAccount = db.Accounts.FirstOrDefault(a => a.CustId == recipientId && (int)a.Type == RecipientAccountType);

            if (sender == null || recipientAccount == null || Amount <= 0)
            {
                ModelState.AddModelError("", "Invalid request details.");
                return Request();
            }

            var request = new ETransfer
            {
                SenderId = sender.CustId, // Sender is the one selected in the dropdown
                RecipientEmail = recipient.Email, // Recipient is the logged-in user
                Amount = Amount,
                TransferType = "Request",
                Status = "Pending",
                CreatedAt = DateTime.Now,
                TransferDate = DateTime.Now,
                SecurityQuestion = "",
                SecurityAnswer = "",
                RecipientAccountType = RecipientAccountType
            };

            db.ETransfers.Add(request);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Request submitted successfully.";
            return RedirectToAction("Request");
        }


        // POST: Respond to Pending Request (Accept or Deny)
        [HttpPost]
        public ActionResult RespondToRequest(int requestId, int SenderAccountId, string response)
        {
            var request = db.ETransfers.FirstOrDefault(r => r.Id == requestId);
            var senderAccount = db.Accounts.FirstOrDefault(a => a.AccountID == SenderAccountId);

            // ✅ Get recipient using their stored email in the request
            var recipient = db.Customers.FirstOrDefault(c => c.Email == request.RecipientEmail);

            if (request == null || senderAccount == null || recipient == null)
                return RedirectToAction("PendingRequests");

            if (response == "accept" && senderAccount.Balance >= request.Amount)
            {
                senderAccount.Balance -= request.Amount;

                // ✅ Deposit into the correct account type chosen by recipient
                var recipientAccount = db.Accounts
                    .FirstOrDefault(a => a.CustId == recipient.CustId && (int)a.Type == request.RecipientAccountType);

                if (recipientAccount != null)
                {
                    recipientAccount.Balance += request.Amount;
                }

                request.Status = "Sent";
                TempData["SuccessMessage"] = "Money request accepted and sent successfully!";
            }
            else if (response == "deny")
            {
                request.Status = "Denied";
                TempData["ErrorMessage"] = "Money request has been denied.";
            }

            db.SaveChanges();
            return RedirectToAction("PendingRequests");
        }


        // GET: GiftCard
        [HttpGet]
        public ActionResult GiftCard()
        {
            int? custId = Session["CustId"] as int?;
            if (custId == null)
                return RedirectToAction("Login", "Customer");

            // Account dropdown
            var accountOptions = db.Accounts
                .Where(a => a.CustId == custId && (a.Type == AccountType.Chequings || a.Type == AccountType.Savings))
                .Select(a => new SelectListItem
                {
                    Value = a.AccountID.ToString(),
                    Text = a.Type == AccountType.Chequings ? "Chequing" : "Savings"
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

                // ✅ Repopulate dropdowns
                var accountOptions = db.Accounts
                    .Where(a => a.CustId == senderCustId && (a.Type == AccountType.Chequings || a.Type == AccountType.Savings))
                    .Select(a => new SelectListItem
                    {
                        Value = a.AccountID.ToString(),
                        Text = a.Type == AccountType.Chequings ? "Chequing" : "Savings"
                    }).ToList();

                var recipients = db.Customers
                    .Where(c => c.CustId != senderCustId)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CustId.ToString(),
                        Text = c.Name
                    }).ToList();

                ViewBag.AccountOptions = new SelectList(accountOptions, "Value", "Text");
                ViewBag.Recipients = new SelectList(recipients, "Value", "Text");

                return View();
            }

            var recipientAccount = db.Accounts.FirstOrDefault(a => a.CustId == recipient.CustId);
            if (recipientAccount == null)
            {
                ModelState.AddModelError("", "Recipient does not have an account.");

                // ✅ Repopulate dropdowns again
                var accountOptions = db.Accounts
                    .Where(a => a.CustId == senderCustId && (a.Type == AccountType.Chequings || a.Type == AccountType.Savings))
                    .Select(a => new SelectListItem
                    {
                        Value = a.AccountID.ToString(),
                        Text = a.Type == AccountType.Chequings ? "Chequing" : "Savings"
                    }).ToList();

                var recipients = db.Customers
                    .Where(c => c.CustId != senderCustId)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CustId.ToString(),
                        Text = c.Name
                    }).ToList();

                ViewBag.AccountOptions = new SelectList(accountOptions, "Value", "Text");
                ViewBag.Recipients = new SelectList(recipients, "Value", "Text");

                return View();
            }

            // ✅ Perform the gift transfer
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

