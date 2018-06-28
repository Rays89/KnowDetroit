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
using System.Net;
using System.IO;

namespace KnowDetroit.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //Build an HTTP Request

            HttpWebRequest request = WebRequest.CreateHttp("https://forecast.weather.gov/MapClick.php?lat=42.3316&lon=-83.0486&FcstType=json");

            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
            //For secret keys
            //request.Headers.Add("secret-key", "hdvshdvlahvlkey");
            //Get the HTTP Response
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //Parse the data from the HTTP Response
            if (response.StatusCode == HttpStatusCode.OK)//200 Status Code
            {
                StreamReader data = new StreamReader(response.GetResponseStream());
                //string RawData = data.ReadToEnd();
                //Json
                JObject WeatherData = JObject.Parse(data.ReadToEnd());
                ViewBag.Today = WeatherData["data"]["weather"][0];
                ViewBag.Icon = WeatherData["data"]["iconLink"][0];
                ViewBag.Temp = WeatherData["data"]["temperature"][0];
                ViewBag.WeatherData = WeatherData["time"]["startPeriodName"][0];
                //ViewBag.IconWeek = WeatherData["data"]["iconLink"];

                DetroitEntities ORM = new DetroitEntities();
                Random rnd = new Random();
                ViewBag.Random = ORM.Landmarks.ToList()[rnd.Next(ORM.Landmarks.ToList().Count)];

                return View();
            }
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact Page";

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
           
            return View();
        }

        public ActionResult SearchLandmarkBySiteName(string SiteName)
        {
            DetroitEntities ORM = new DetroitEntities();
           // List<Landmark> Landmarks = ORM.Landmarks.Where(c => c.SiteName.Contains(SiteName)).ToList();
            ViewBag.Landmark = ORM.Landmarks.Where(c => c.SiteName.Contains(SiteName)).ToList();
            return View("ListOfLandmarks");
        } 

       
        public ActionResult SortOptions(string sortOption)
        {
            DetroitEntities ORM = new DetroitEntities();

            if (sortOption == "0")
            {
                ViewBag.Landmark = ORM.Landmarks.ToList();
            }
            else if (sortOption == "1")
            {
                List<Landmark> landmarkList = ORM.Landmarks.Where(x => x.Reviews.Count > 0).OrderByDescending(x => ((double)x.Rating / x.Reviews.Count)).ToList();
                List<Landmark> unreviewed = ORM.Landmarks.Where(x => x.Reviews.Count == 0).ToList();
                foreach (Landmark landmark in unreviewed)
                {
                    landmarkList.Add(landmark);
                }

                ViewBag.Landmark = landmarkList;
            }
            else
            {
                ViewBag.Landmark = ORM.Landmarks.OrderByDescending(x => x.Reviews.Where(y => y.Recommended == "Yes").Count()).ToList();
            }
            return View("ListOfLandmarks");
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
                    //4 Save to database
                    ORM.Landmarks.Find(Found.SiteName).Rating -= Found.Rating;
                    ORM.Entry(ORM.Landmarks.Find(Found.SiteName)).State = EntityState.Modified;
                    ORM.Reviews.Remove(Found);
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
            DetroitEntities ORM = new DetroitEntities();
            ViewBag.Landmark = ORM.Landmarks.Where(x => x.Reviews.Count > 0).OrderByDescending(x => ((double)x.Rating / x.Reviews.Count)).Take(3).ToList();

            return View("ListOfLandmarks");
        }
        public ActionResult ShowUserPhotos(string SiteName)
        {
            DetroitEntities ORM = new DetroitEntities();
            ViewBag.UserPhotos = ORM.Reviews.Where(c => c.SiteName.Contains(SiteName)).ToList();
            return View();
        }
        public ActionResult ShowReviews(string SiteName)
        {
            DetroitEntities ORM = new DetroitEntities();
            ViewBag.Review = ORM.Reviews.Where(c => c.SiteName.Contains(SiteName)).OrderByDescending(c => c.ReviewNumber).ToList();
            ViewBag.SiteName = SiteName;
            return View();
        }
        public ActionResult EditReviewPage(int ReviewNumber)
        {
            DetroitEntities ORM = new DetroitEntities();

            Review Found = ORM.Reviews.Find(ReviewNumber);

          
            return View(Found);
        }
        public ActionResult UpdateReview(Review updatedReview)
        {
            DetroitEntities ORM = new DetroitEntities();
            Review OldReview = ORM.Reviews.Find(updatedReview.ReviewNumber);
            if (OldReview != null)
            {
                //3. Update the existing customer

                OldReview.Recommended = updatedReview.Recommended;
                OldReview.Review1 = updatedReview.Review1;
                if (updatedReview.Rating != OldReview.Rating)
                {
                    int difference = updatedReview.Rating - OldReview.Rating;
                    ORM.Landmarks.Find(updatedReview.SiteName).Rating += difference;
                    OldReview.Rating = updatedReview.Rating;
                }

                ORM.Entry(OldReview).State = EntityState.Modified;
                ORM.Entry(ORM.Landmarks.Find(OldReview.SiteName)).State = EntityState.Modified;
                //4. save back to the DB 
                ORM.SaveChanges();

                return RedirectToAction("About");

            }
            else
            {
                ViewBag.ErrorMessage = "Oops! Something went wrong!";
                return View("Error");
            }
        }
    }


}
