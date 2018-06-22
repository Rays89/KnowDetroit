using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using KnowDetroit.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

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
            ViewBag.Reviews = Found.Reviews;
            //if (Found.Reviews.Count > 0)
            //{
            //    ViewBag.Rating = CalculateRating(SiteName);
            //}
            return View();
        }

        public ActionResult SearchLandmarkBySiteName(string SiteName)
        {
            DetroitEntities ORM = new DetroitEntities();
            ViewBag.Landmark = ORM.Landmarks.Where(c => c.SiteName.Contains(SiteName)).ToList();

            return View("ListOfLandmarks");
        }

        //public JsonResult SearchLandmarkBySiteName(string SiteName)
        //{
        //    //ORM
        //    DetroitEntities ORM = new DetroitEntities();

        //    //search by sitename
        //    List<Landmark> Result = ORM.Landmarks.Where(c => c.SiteName.Contains(SiteName)).ToList();

        //    //return data as Json
        //    return Json(Result);
        //}
        public ActionResult SortByRating()
        {
            DetroitEntities ORM = new DetroitEntities();
            //List<Landmark> Result = ORM.Landmarks.Where(c => c.SiteName.Contains(SiteName)).ToList().OrderBy(Landmarks.SiteName);
            // return ORM.Landmarks.Find(SiteName).orderByDescending(c => c.SiteName).Tolist()[0];
            List<Landmark> LandmarkList = ORM.Landmarks.OrderBy(c => c.SiteName).ToList();
            ViewBag.Landmark = ORM.Landmarks.OrderByDescending(x => (x.Rating / x.Reviews.Count)).ToList();
            return View("ListOfLandmark");
        }

        public ActionResult ReviewForm(string SiteName)
        {
            ViewBag.SiteName = SiteName;
            return View();
        }

        public ActionResult AddNewRating(Review userReview)
        {
            DetroitEntities ORM = new DetroitEntities();

            userReview.UserID = User.Identity.GetUserId();
            ORM.Reviews.Add(userReview);
            Landmark reviewed = ORM.Landmarks.Find(userReview.SiteName);
            reviewed.Rating += userReview.Rating;

            ORM.Entry(reviewed).State = EntityState.Modified;
            ORM.SaveChanges();
            //ViewBag.RatingList = ORM.Reviews.ToList();
            return RedirectToAction("ListOfLandmarks");

        }
        //public double CalculateRating(string SiteName)
        //{

        //    DetroitEntities ORM = new DetroitEntities();
        //    List<int> RatingList = ORM.Reviews.Where(c => c.SiteName == SiteName).Select(c => c.Rating).ToList();
        //    double finalRating = (double)RatingList.Sum() / RatingList.Count();
            
        //    return finalRating;
            
            

        //}
    }
}
