using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;
using System.Data.Entity;

namespace Project.Controllers
{
    public class TransactionController : Controller
    {
        private CustomerContext db = new CustomerContext(); // Assuming the same DbContext as CustomerController

        // Check if the user is an Admin
        private bool IsAdmin()
        {
            return Session["Role"] != null && Session["Role"].ToString() == "Admin";
        }

        // GET: Transaction List
        public ActionResult Index()
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            var transactions = db.Transactions.Include(t => t.Customer).ToList(); // Include customer details
            return View(transactions); // Display the list of transactions
        }

        // GET: Transaction Details
        public ActionResult Details(int id)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            var transaction = db.Transactions.FirstOrDefault(t => t.Id == id); // Get transaction by ID

            if (transaction == null)
            {
                return HttpNotFound(); // Return 404 if transaction is not found
            }

            return View(transaction); // Return transaction details to view
        }

        // GET: Create New Transaction
        public ActionResult Create()
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            ViewBag.CustId = new SelectList(db.Customers, "CustId", "Name");  // Populate Customers dropdown
            ViewBag.AccountID = new SelectList(db.Accounts, "AccountID", "AccountID"); // Populate Accounts dropdown
            return View(); // Show the create transaction form
        }

        // POST: Create New Transaction
        [HttpPost]
        public ActionResult Create(Transaction transaction)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            if (ModelState.IsValid)
            {
                db.Transactions.Add(transaction); // Add new transaction to the database
                db.SaveChanges(); // Save changes to database
                return RedirectToAction("Index"); // Redirect to transaction list after adding
            }
            return View(transaction); // Return the same view if model is not valid
        }

        // GET: Edit Transaction
        public ActionResult Edit(int id)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            var transaction = db.Transactions.FirstOrDefault(t => t.Id == id); // Get transaction by ID
            if (transaction == null)
            {
                return HttpNotFound(); // Return 404 if transaction is not found
            }

            // Populate dropdowns for Customer and Account
            ViewBag.CustId = new SelectList(db.Customers, "CustId", "Name", transaction.CustId);
            ViewBag.AccountID = new SelectList(db.Accounts, "AccountID", "AccountID", transaction.AccountID);

            return View(transaction); // Show the form for editing the transaction
        }

        // POST: Edit Transaction
        [HttpPost]
        public ActionResult Edit(Transaction transaction)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            if (ModelState.IsValid)
            {
                var existingTransaction = db.Transactions.FirstOrDefault(t => t.Id == transaction.Id); // Find the existing transaction
                if (existingTransaction != null)
                {
                    // Update the existing transaction
                    existingTransaction.CustId = transaction.CustId;
                    existingTransaction.AccountID = transaction.AccountID;
                    existingTransaction.Date = transaction.Date;
                    db.SaveChanges(); // Save changes to database
                }
                return RedirectToAction("Index"); // Redirect to the transaction list after editing
            }
            return View(transaction); // Return the form with errors if model is invalid
        }

        // GET: Delete Transaction (Confirm Page)
        public ActionResult Delete(int id)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            var transaction = db.Transactions.FirstOrDefault(t => t.Id == id); // Get transaction by ID
            if (transaction == null)
            {
                return HttpNotFound(); // Return 404 if transaction is not found
            }
            return View(transaction); // Show confirmation page to delete the transaction
        }

        // POST: Confirm Deletion
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin()) // Role check
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect to NotAuthorized if not Admin
            }

            var transaction = db.Transactions.FirstOrDefault(t => t.Id == id); // Get transaction by ID
            if (transaction != null)
            {
                db.Transactions.Remove(transaction); // Remove the transaction from the database
                db.SaveChanges(); // Save changes to the database
            }
            return RedirectToAction("Index"); // Redirect to transaction list after deletion
        }

        // Search query for transactions
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

            var transactions = db.Transactions
                .Where(t => t.Id.ToString().Contains(query) ||
                            t.AccountID.ToString().Contains(query) ||
                            (t.Customer != null && t.Customer.Name.Contains(query))) // Safe check for Customer
                .Select(t => new
                {
                    t.Id,
                    CustomerName = t.Customer != null ? t.Customer.Name : "N/A", // Default to "N/A" if no customer
                    t.AccountID,
                    Date = t.Date
                })
                .ToList()
                .Select(t => new
                {
                    t.Id,
                    t.CustomerName,
                    t.AccountID,
                    Date = t.Date.ToString("yyyy-MM-dd") // Format date for output
                });

            return Json(transactions, JsonRequestBehavior.AllowGet); // Return search results in JSON format
        }
    }
}
