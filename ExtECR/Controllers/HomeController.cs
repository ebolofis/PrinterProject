using ExtECR.Filters;
using ExtECRMainLogic.Classes;
using ExtECRMainLogic.Models.ConfigurationModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace ExtECR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ExtECRInitializer extecrInitializer;
        //private readonly ExtECRDisplayer extecrDisplayer;
        public HomeController(ExtECRInitializer extecrInitializer, ILogger<HomeController> logger)
        {
            this.extecrInitializer = extecrInitializer;
            this.logger = logger;
        }

        [VersionFilter]
        public IActionResult Login()
        {
            ViewBag.Title = "H.I.T. - ExtECR Driver";
            ViewBag.SelectedNavigation = 0;
            HttpContext.Session.SetString("Pass", "");
            return View();
        }

        [VersionFilter]
        //[AuthorizationFilter]
        public IActionResult Index()
        {
            ViewBag.Title = "H.I.T. - ExtECR Driver";
            ViewBag.SelectedNavigation = 1;
            return View();
        }

        [VersionFilter]
        //[AuthorizationFilter]
        public async System.Threading.Tasks.Task<IActionResult> SettingsAsync()
        {
            ViewBag.Title = "H.I.T. - ExtECR Driver";

            InstallationDataMaster InstallationData = new InstallationDataMaster();
            
            InstallationData = extecrInitializer.GetInstallationMasterData();
            ViewBag.InstallationData = InstallationData;
                ViewBag.SelectedNavigation = 2;
            return View();
        }

        [VersionFilter]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            ViewBag.Title = "H.I.T. - ExtECR Driver";
            ViewBag.SelectedNavigation = -1;
            return View();
        }


    }
}
