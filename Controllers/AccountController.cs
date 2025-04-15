using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;

namespace Project.Controllers
{
    public class AccountController : Controller
    {
        private CustomerContext db = new CustomerContext();

        // Check if the user is an Admin
        private bool IsAdmin()
        {
            return Session["Role"] != null && Session["Role"].ToString() == "Admin";
        }

        // GET: Account List
        public ActionResult Index()
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            var accounts = db.Accounts.Include("Customer").ToList(); // Load related customer data
            return View(accounts); // Display the account list
        }

        // GET: Account Details
        public ActionResult Details(int id)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            var account = db.Accounts.Include("Customer").FirstOrDefault(a => a.AccountID == id);
            if (account == null)
            {
                return HttpNotFound(); // Return 404 if account not found
            }
            return View(account); // Display account details
        }

        // GET: Create Account Form
        public ActionResult Create()
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            ViewBag.CustId = new SelectList(db.Customers, "CustId", "Name"); // Dropdown for Customers
            return View(); // Show the account creation form
        }

        // POST: Handle Account Creation
        [HttpPost]
        public ActionResult Create(Account account)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            if (ModelState.IsValid)
            {
                db.Accounts.Add(account); // Add new account to the database
                db.SaveChanges(); // Save changes to database
                return RedirectToAction("Index"); // Redirect to account list
            }

            ViewBag.CustId = new SelectList(db.Customers, "CustId", "Name", account.CustId);
            return View(account); // Return the view with errors if model is invalid
        }

        // GET: Edit Account Form
        public ActionResult Edit(int id)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            var account = db.Accounts.FirstOrDefault(a => a.AccountID == id);
            if (account == null)
            {
                return HttpNotFound(); // Return 404 if account not found
            }

            ViewBag.CustId = new SelectList(db.Customers, "CustId", "Name", account.CustId);
            return View(account); // Show the account editing form
        }

        // POST: Handle Account Edit
        [HttpPost]
        public ActionResult Edit(Account account)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            if (ModelState.IsValid)
            {
                var existingAccount = db.Accounts.FirstOrDefault(a => a.AccountID == account.AccountID);
                if (existingAccount != null)
                {
                    existingAccount.AccountNum = account.AccountNum;
                    existingAccount.CustId = account.CustId;
                    db.SaveChanges(); // Save changes to the account
                }
                return RedirectToAction("Index"); // Redirect to account list after editing
            }

            ViewBag.CustId = new SelectList(db.Customers, "CustId", "Name", account.CustId);
            return View(account); // Return the view with errors if model is invalid
        }

        // GET: Delete Confirmation
        public ActionResult Delete(int id)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            var account = db.Accounts.FirstOrDefault(a => a.AccountID == id);
            if (account == null)
            {
                return HttpNotFound(); // Return 404 if account not found
            }
            return View(account); // Show the account deletion confirmation page
        }

        // POST: Handle Account Deletion
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            var account = db.Accounts.FirstOrDefault(a => a.AccountID == id);
            if (account != null)
            {
                db.Accounts.Remove(account); // Remove the account from the database
                db.SaveChanges(); // Save changes to the database
            }
            return RedirectToAction("Index"); // Redirect to account list after deletion
        }

        // GET: Search Accounts (AJAX)
        public JsonResult Search(string query)
        {
            if (!IsAdmin()) // Role check
            {
                return Json(new { message = "Not Authorized" }, JsonRequestBehavior.AllowGet); // Return message if not Admin
            }

            if (string.IsNullOrEmpty(query))
            {
                return Json(new { message = "No query provided" }, JsonRequestBehavior.AllowGet); // Return a message if no query is provided
            }

            var accounts = db.Accounts
                .Where(a => a.AccountNum.ToString().Contains(query) || a.Customer.Name.Contains(query)) // Search by account number or customer name
                .Select(a => new
                {
                    a.AccountID,
                    a.AccountNum,
                    CustomerName = a.Customer.Name
                })
                .ToList();

            return Json(accounts, JsonRequestBehavior.AllowGet); // Return search results in JSON format
        }
    }
}
