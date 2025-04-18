using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string Question { get; set; }
        public string Email { get; set; }
        public DateTime DateSubmitted { get; set; }
    }
}