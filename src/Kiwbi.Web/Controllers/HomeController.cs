using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kiwbi.Web.Models;

namespace Kiwbi.Web.Controllers;

public class HomeController(IConfiguration configuration) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Contact()
    {
        ViewBag.ContactEmail = configuration["Contact:Email"];
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
