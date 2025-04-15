using System.Net.Mail;
using System.Net;
using System.Web.Mvc;

public class HomeController : Controller
{
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

    public ActionResult Startup()
    {
        return View();
    }

    public ActionResult RoleBasedHome()
    {
        if (Session["Role"] != null)
        {
            string role = Session["Role"].ToString();

            if (role == "Admin")
            {
                return RedirectToAction("AdminHome");
            }
            else if (role == "Customer")
            {
                return RedirectToAction("CustomerHome");
            }
        }

        // Default fallback (e.g., login page or landing page)
        return RedirectToAction("Index");
    }

    public ActionResult PostLoginRedirect(string role)
    {
        if (role == "Admin")
        {
            return RedirectToAction("AdminHome", "Home");
        }
        else if (role == "Customer")
        {
            return RedirectToAction("CustomerHome", "Home");
        }

        return RedirectToAction("Login", "Customer");
    }
    public ActionResult AdminHome()
    {
        if (Session["Role"]?.ToString() != "Admin")
            return RedirectToAction("NotAuthorized", "Home");
        return View();
    }
    public ActionResult CustomerHome()
    {
        if (Session["Role"]?.ToString() != "Customer")
            return RedirectToAction("NotAuthorized", "Home");
        return View();
    }
    public ActionResult NotAuthorized()
    {
        return View();
    }
    public ActionResult PrivacySecurity()
    {
        return View();
    }

    // POST: Home/AskQuestion
    [HttpPost]
    public ActionResult AskQuestion(string question, string topic, string email)
    {
        if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = "Please enter a question, select a topic, and provide your email address.";
            return RedirectToAction("Contact");
        }

        string recipientEmail = GetRecipientEmail(topic); 
        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("browngiovanni890@gmail.com", "bylayjmubkhcxyhf"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(recipientEmail),
                Subject = $"New Question - {topic}",
                Body = $"From: {email}\nTopic: {topic}\n\nQuestion:\n{question}",
                IsBodyHtml = false
            };

            mailMessage.To.Add(recipientEmail);
            mailMessage.ReplyToList.Add(new MailAddress(email));

            smtpClient.Send(mailMessage);

            TempData["Message"] = "Thank you for your question! We’ve received it and will respond shortly.";
        }
        catch
        {
            TempData["Error"] = "There was an error sending your question. Please try again.";
        }

        return RedirectToAction("Contact");
    }

    // ✅ Make sure this method is inside the controller
    private string GetRecipientEmail(string topic)
    {
        switch (topic)
        {
            case "Account Support":
                return "browngiovanni890@gmail.com";
            case "Technical Help":
                return "browngiovanni445@gmail.com";
            case "Loan Inquiries":
                return "browngiovanni584@gmail.com";
            default:
                return "general@example.com";
        }
    }
}