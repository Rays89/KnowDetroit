using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using KnowDetroit.Models;

namespace KnowDetroit.Controllers
{
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
        public ActionResult ListOfLandmarks()
        {
            DetroitEntities ORM = new DetroitEntities();
            ViewBag.Landmark = ORM.Landmarks.ToList();
            return View();
        }
        public ActionResult LandmarkView(string SiteName)
        {
            DetroitEntities ORM = new DetroitEntities();
            Landmark Found = ORM.Landmarks.Find(SiteName);
            ViewBag.Found = Found;
            return View();
        }

        public ActionResult GetMap(string latitude, string longitude)
        {
            string image = $"https://image.maps.cit.api.here.com/mia/1.6/mapview?c={latitude}%2C{longitude}&z=14&app_id=8OqfRkeQ31Pfn1gfN1CJ&app_code=iaoGxpIIRFQ8aD-Lud4ZOQ";
            ViewBag.Map = image;
            return RedirectToAction("LandmarkView");
        }

        public JsonResult SearchLandmarkBySiteName(string SiteName)
        {
            //ORM
            DetroitEntities ORM = new DetroitEntities();

            //search by sitename
            List<Landmark> Result = ORM.Landmarks.Where(c => c.SiteName.Contains(SiteName)).ToList();

            //return data as Json
            return Json(Result);
        }
    }
}
