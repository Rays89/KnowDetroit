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
using System.Configuration;
using Newtonsoft.Json.Linq;

namespace KnowDetroit.Controllers
{[Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = " Description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Our contact page.";

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

        public ActionResult AddNewRating(Review userReview, HttpPostedFileBase upload)
        {
            DetroitEntities ORM = new DetroitEntities();

            userReview.UserID = User.Identity.GetUserId();
            userReview.imageURL = UploadImage(upload);
            ORM.Reviews.Add(userReview);
            Landmark reviewed = ORM.Landmarks.Find(userReview.SiteName);
            reviewed.Rating += userReview.Rating;


            ORM.Entry(reviewed).State = EntityState.Modified;
            ORM.SaveChanges();
            //ViewBag.RatingList = ORM.Reviews.ToList();
            return RedirectToAction("ListOfLandmarks");

        }
        public string UploadImage(HttpPostedFileBase upload)
        {
            Account account = new Account(
                ConfigurationManager.AppSettings.Get("Cloudinary-Name"),
       ConfigurationManager.AppSettings.Get("Cloudinary-Key"),
       ConfigurationManager.AppSettings.Get("Cloudinary-Secret"));

            Cloudinary cloudinary = new Cloudinary(account);
            if (upload != null)
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(upload.FileName, upload.InputStream)
                };


                var uploadResult = cloudinary.Upload(uploadParams);

                JObject JsonData = (JObject)uploadResult.JsonObj;
                ViewBag.uploadResult = JsonData;
                return JsonData["url"].ToString();
            }
            else
                return null;

        }
        public ActionResult ShowUserReviews()
        {
            //1 ORM
            DetroitEntities ORM = new DetroitEntities();
            string currentUser = User.Identity.GetUserId();
            //2 Locate order to delete or edit
            List<Review> UserReviews = ORM.AspNetUsers.Find(currentUser).Reviews.ToList();
            //3 Show item
            ViewBag.UserReviews = UserReviews;
            return View();

        }
        public ActionResult DeleteReview(int ReviewNumber)
        {
            //1 ORM
            DetroitEntities ORM = new DetroitEntities();

            DbContextTransaction DeleteTransaction = ORM.Database.BeginTransaction();

            //2 Locate review to delete
            Review Found = ORM.Reviews.Find(ReviewNumber);
            //3 Remove review
            if (Found != null)
            {
                try
                {
                    ORM.Reviews.Remove(Found);
                    //4 Save to database
                    ORM.SaveChanges();
                    DeleteTransaction.Commit();
                    return RedirectToAction("ListReviews");
                }
                catch (Exception ex)
                {
                    DeleteTransaction.Rollback();
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Customer Not Found";
                return View("Error");
            }
        }
        public ActionResult ListReviews()
        {
            DetroitEntities ORM = new DetroitEntities();
            ViewBag.Review = ORM.Reviews.ToList();
            return View();
        }
        public ActionResult WalkingTour()
        {
            return View();
        }

        public ActionResult HighlyRatedLandmark()
        {
            return View();
        }
        public ActionResult ShowUserPhotos(string SiteName)
        {
            DetroitEntities ORM = new DetroitEntities();

            return View();
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
