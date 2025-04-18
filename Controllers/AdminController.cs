using System.Linq;
using System.Web.Mvc;
using Project.Models; 
using System.Security.Cryptography;
using System.Text;
using System;
using System.Collections.Generic;
using System.Data.Entity;


namespace Project.Controllers
{
    public class AdminController : Controller
    {
        private CustomerContext db = new CustomerContext(); // Ensure this matches your DbContext
        private bool IsAdmin()
        {
            return Session["Role"] != null && Session["Role"].ToString() == "Admin";
        }
        // Show Login Page
        public ActionResult Login()
        {
            return View();
        }

        // Handle Login Submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Admin admin)
        {
            if (ModelState.IsValid)
            {
                // Debugging Log - Check if form submits
                System.Diagnostics.Debug.WriteLine("Login Form Submitted: " + admin.Username);

                // Hash the password (for security purposes)
                string hashedPassword = HashPassword(admin.Password);

                // Check database for matching credentials
                var existingAdmin = db.Admins.FirstOrDefault(a =>
                    a.Username.Trim().ToLower() == admin.Username.Trim().ToLower() &&
                    a.Password == hashedPassword); // Compare hashed passwords

                if (existingAdmin != null)
                {
                    // Store session data if admin is authenticated
                    Session["AdminId"] = existingAdmin.AdminId;
                    Session["Username"] = existingAdmin.Username;
                    Session["IsAdmin"] = true; // Set the session role flag
                    Session["Role"] = "Admin";
                    return RedirectToAction("PostLoginRedirect", "Home", new { role = "Admin" });
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }
            return View(admin);
        }

        // Admin Dashboard
        public ActionResult Dashboard()
        {
            // Check if session has admin role
            if (Session["IsAdmin"] == null || !(bool)Session["IsAdmin"])
            {
                return RedirectToAction("NotAuthorized", "Home"); // Redirect if not an admin
            }

            // Return the dashboard view if the user is authenticated as an admin
            return View();
        }

        // Logout
        public ActionResult Logout()
        {
            // Clear session data on logout
            Session.Clear();
            return RedirectToAction("Login", "Customer");
        }

        // Show Registration Form
        public ActionResult Register()
        {
            return View();
        }

        // Handle Form Submission for Registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Admin admin)
        {
            if (ModelState.IsValid)
            {
                // Hash the password before storing it
                admin.Password = HashPassword(admin.Password);

                // Add admin to the database
                db.Admins.Add(admin);
                db.SaveChanges();

                return RedirectToAction("Login"); // Redirect to login after registration
            }
            return View(admin);
        }
        // GET: Admin/ManageUsers
        public ActionResult ManageUsers()
        {
            if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

            var customers = db.Customers.ToList(); // Assuming you have a Customers table
            return View(customers); // This will use Views/Admin/ManageUsers.cshtml
        }

        // POST: Admin/ManageUsers
        [HttpPost]
        public ActionResult ManageUsers(int customerId, string role)
        {
            if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

            var customer = db.Customers.Find(customerId);
            if (customer != null)
            {
                customer.Role = role; // Update the customer's role

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            return RedirectToAction("ManageUsers");
        }


        // Password hashing (for security)
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes); // Store and compare hashed passwords
            }
        }

        [HttpPost]
        public ActionResult DeleteUser(int customerId)
        {
            if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

            var customer = db.Customers.Find(customerId);
            if (customer != null)
            {
                // Prevent the currently logged-in admin from deleting themselves
                if (customer.CustId == (int?)Session["AdminId"])
                {
                    TempData["ErrorMessage"] = "You cannot delete your own account.";
                    return RedirectToAction("ManageUsers");
                }

                db.Customers.Remove(customer);
                db.SaveChanges();

                TempData["SuccessMessage"] = "User deleted successfully.";
            }

            return RedirectToAction("ManageUsers"); // Ensures a return in all code paths
        }

        // GET: Admin/AllTransfers
        public ActionResult AllTransfers()
        {
            var eTransfers = db.ETransfers.OrderByDescending(t => t.TransferDate).ToList();

            // Fetching Cheque Uploads with related Customer data
            var cheques = db.ChequeUploads
                            .Include(c => c.Customer)  // Include the related Customer entity
                            .OrderByDescending(c => c.UploadedAt)  // Order by the uploaded date
                            .ToList();

            // Passing data to the view using ViewBag
            ViewBag.ETransfers = eTransfers ?? new List<ETransfer>();
            ViewBag.Cheques = cheques ?? new List<ChequeUpload>();

            return View();
        }
        public ActionResult ViewQuestions()
        {
            if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

            var messages = db.ContactMessages.OrderByDescending(m => m.DateSubmitted).ToList();
            return View(messages);
        }





    }

}
