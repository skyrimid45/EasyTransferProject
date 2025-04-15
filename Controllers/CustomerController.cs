using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Project.Models;

namespace Project.Controllers
{
    public class CustomerController : Controller
    {
        private CustomerContext db = new CustomerContext();
        // GET: Employee

        public ActionResult Index()  
        {
            {
                if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

                var customer = db.Customers.ToList();

                return View(customer);
            }

        }

        // Details Action: Fetch Customer by ID

        public ActionResult Details(int id)
        {
            if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

            var customer = db.Customers.FirstOrDefault(c => c.CustId == id);

            if (customer == null)
            {
                return HttpNotFound(); // Returns a 404 if the customer is not found
            }

            return View(customer);
        }

        // Create Action: Show the form

        public ActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

            return View();
        }

        // Create: Handle form submission

        [HttpPost]
        public ActionResult Create(Customer customer)
        {
            if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index"); // Redirect to list after adding
            }
            return View(customer);
        }

        // Edit: Show form with existing customer data

        public ActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

            var customer = db.Customers.FirstOrDefault(c => c.CustId == id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // Edit: Handle edit request
        [HttpPost]
        public ActionResult Edit(Customer customer)
        {
            if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

            if (ModelState.IsValid)
            {
                var existingCustomer = db.Customers.FirstOrDefault(c => c.CustId == customer.CustId);
                if (existingCustomer != null)
                {
                    existingCustomer.Name = customer.Name;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.Telephone = customer.Telephone;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        // Delete: Show confirmation page
        public ActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

            var customer = db.Customers.FirstOrDefault(c => c.CustId == id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // Delete: Handle deletion request
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("NotAuthorized", "Home");

            var customer = db.Customers.FirstOrDefault(c => c.CustId == id);
            if (customer != null)
            {
                db.Customers.Remove(customer);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        //AUTHENTICATION

        public ActionResult Login()
        {
            return View();
        }

        // Handle Login Submission
        [HttpPost]
        public ActionResult Login(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Email) || string.IsNullOrEmpty(customer.Password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View(customer);
            }

            string hashedPassword = Hash256.HashPassword(customer.Password);

            var existingCustomer = db.Customers.FirstOrDefault(c =>
                c.Email.Trim().ToLower() == customer.Email.Trim().ToLower() &&
                c.Password == hashedPassword);

            if (existingCustomer != null)
            {

                Session["CustId"] = existingCustomer.CustId;
                Session["Name"] = existingCustomer.Name;
                Session["Role"] = existingCustomer.Role;

                return RedirectToAction("PostLoginRedirect", "Home", new { role = existingCustomer.Role });
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(customer);
        }




        // Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // Customer Dashboard
        public ActionResult Dashboard()
        {
            if (Session["CustId"] == null)
                return RedirectToAction("Login");

            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        // Handle Registration
        [HttpPost]
        public ActionResult Register(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Check if Email already exists
                var existingCustomer = db.Customers.FirstOrDefault(c => c.Email == customer.Email);
                if (existingCustomer != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(customer);
                }
                // Hash the password before saving
                customer.Password = Hash256.HashPassword(customer.Password);

                // Save new customer
                db.Customers.Add(customer);
                db.SaveChanges();

                // Create Chequings account
                var chequings = new Account
                {
                    AccountNum = GenerateAccountNumber(),
                    Type = AccountType.Chequings,
                    Balance = 1000.00M,
                    CustId = customer.CustId
                };

                // Create Savings account
                var savings = new Account
                {
                    AccountNum = GenerateAccountNumber(),
                    Type = AccountType.Savings,
                    Balance = 5000.00M,
                    CustId = customer.CustId
                };

                // Add accounts to DB
                db.Accounts.Add(chequings);
                db.Accounts.Add(savings);
                db.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(customer);
        }


        // GET: Search Customers (AJAX)
        public JsonResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            var customers = db.Customers
                .Where(c => c.CustId.ToString().Contains(query) || c.Name.Contains(query))
                .Select(c => new
                {
                    c.CustId,
                    c.Name,
                    c.Email,
                    c.Telephone
                })
                .ToList();

            return Json(customers, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ViewProfile()
        {
            int? custId = Session["CustId"] as int?;

            if (custId == null)
            {
                return RedirectToAction("Login");  // Redirect to login if session does not exist
            }

            var customer = db.Customers.FirstOrDefault(c => c.CustId == custId);
            if (customer == null)
            {
                return HttpNotFound();  // Return an error if customer is not found
            }

            var chequing = db.Accounts.FirstOrDefault(a => a.CustId == custId && a.Type == AccountType.Chequings);
            var savings = db.Accounts.FirstOrDefault(a => a.CustId == custId && a.Type == AccountType.Savings);

            // Gift balance can come from either account (if both have a GiftBalance column)
            var giftAccount = db.Accounts.FirstOrDefault(a => a.CustId == custId);

            ViewBag.ChequingBalance = chequing?.Balance ?? 0;
            ViewBag.SavingsBalance = savings?.Balance ?? 0;
            ViewBag.GiftBalance = giftAccount?.GiftBalance ?? 0;

            return View(customer);
        }



        private int GenerateAccountNumber()
        {
            Random rand = new Random();
            return rand.Next(10000000, 99999999); // 8-digit account number on register
        }
        private bool IsAdmin()
        {
            return Session["Role"] != null && Session["Role"].ToString() == "Admin";
        }

        private bool IsCustomer()
        {
            return Session["Role"] != null && Session["Role"].ToString() == "Customer";
        }

    }

}