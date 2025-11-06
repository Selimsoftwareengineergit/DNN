using Microsoft.AspNetCore.Mvc;

namespace DNN.Controllers
{
    public class StudentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
