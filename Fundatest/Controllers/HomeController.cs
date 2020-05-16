using Microsoft.AspNetCore.Mvc;
using Fundatest.Models;
using Fundatest.Services;
using System;

namespace Fundatest.Controllers
{
    public class HomeController : Controller
    {
              
        private readonly IApiService _apiService;

        public HomeController(IApiService apiService)
        {
          
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            HomeViewModel model = null;
            try
            {
                model = new HomeViewModel
                {
                    MakelaarsMetTuin = _apiService.GetTop10("/amsterdam/tuin/"),
                    MakelaarsZonderTuin = _apiService.GetTop10("/amsterdam/"),
                    Message = string.Empty
                };
            }
            catch (Exception ex)
            {
                model = new HomeViewModel {
                    MakelaarsMetTuin = new System.Collections.Generic.List<Model.MakelaarCount>(),
                    MakelaarsZonderTuin = new System.Collections.Generic.List<Model.MakelaarCount>(),
                    Message = "Er is helaas een fout opgetreden : " + ex.Message 
                };
            }

            return View(model); 
        }

    }
}
