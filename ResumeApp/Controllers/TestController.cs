using Microsoft.AspNetCore.Mvc;
using ResumeApp.Data;
using System.Linq;

namespace ResumeApp.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

    
    }
}
