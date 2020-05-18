using Microsoft.AspNetCore.Mvc;
using Fundatest.Models;
using Fundatest.Services;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Fundatest.Controllers
{
    public class HomeController : Controller
    {              
        private readonly IApiService _apiService;
        readonly ILogger<HomeController> _log;

        public HomeController(IApiService apiService, ILogger<HomeController> log)
        {
            _log = log;
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            return View();
        }
               
        // Ik heb gekozen voor MVC met .net Core omdat ik hier ervaring mee heb en het een handig manier is om de gegevens te presenteren.
        public IActionResult MakelaarInfo()
        {
            MakelaarInfoViewModel model = null;
            try
            {
                model = new MakelaarInfoViewModel
                {
                    MakelaarsMetTuin = _apiService.GetMakelaarTop10("/amsterdam/tuin/"),
                    MakelaarsZonderTuin = _apiService.GetMakelaarTop10("/amsterdam/"),
                    Message = string.Empty
                };
            }
            catch (Exception ex)
            {
                model = new MakelaarInfoViewModel {                  
                    Message = "Er is helaas een fout opgetreden. See log for more details."
                };

                _log.LogError(ex, ex.Message);
            }

            return View(model); 
        }

    }
}
