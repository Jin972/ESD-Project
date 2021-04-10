using ESD_Project.Common;
using ESD_Project.Models;
using ESD_Project.Models.Code;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;

namespace ESD_Project.Controllers
{
    public class AdminController : BaseController
    {
        ModelESD db = new ModelESD();
        // GET: Admin
        [HasCredencial(RoleId = "FUNCTION_OF_ADMIN")]
        public ActionResult Index()
        {
            var userCount = db.Users.Count();
            ViewBag.userCount = userCount;

            var postCount = db.Posts.Count();
            ViewBag.postCount = postCount;

            var topicCount = db.Topics.Count();
            ViewBag.topicCount = topicCount;

            var fileCount = db.FileDetails.Count();
            ViewBag.fileCount = fileCount;
            return View();
        }

        [HasCredencial(RoleId = "FUNCTION_OF_ADMIN")]
        public new ActionResult Profile(long? id) 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HasCredencial(RoleId = "FUNCTION_OF_ADMIN")]
        public ActionResult editProfile(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.GroupId = new SelectList(db.GroupMembers, "GroupId", "Name", user.GroupId);
            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name", user.MajorId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult editProfile([Bind(Include = "UserId,Name,Email,Username,Password,Phone,Address,DateOfBirth,CreateDate,CreateBy,ModifyDate,ModifyBy,AccountStatus,GroupId,MajorId")] User user, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {
                FileUploadService service = new FileUploadService();
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];
                if (uploadFile != null)
                {
                    FileDetail file = db.FileDetails.ToList().Find(x => x.UserId == user.UserId);
                    if (file != null)
                    {
                        file.FileName = Path.GetFileName(uploadFile.FileName);
                        file.ContentType = uploadFile.ContentType;
                        file.Data = service.ConvertToBytes(uploadFile);
                        db.Entry(file).State = EntityState.Modified;
                    }
                    else
                    {
                        db.FileDetails.Add(new FileDetail
                        {
                            FileName = Path.GetFileName(uploadFile.FileName),
                            ContentType = uploadFile.ContentType,
                            Data = service.ConvertToBytes(uploadFile),
                            UserId = user.UserId
                        });
                    }
                }
                user.ModifyBy = session.Username;
                user.ModifyDate = DateTime.Now;
                var Password = Request["Password"];
                ViewBag.GroupId = new SelectList(db.GroupMembers, "GroupId", "Name", user.GroupId);
                ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name", user.MajorId);


                var email = Request["Email"];
                var checkEmailNull = db.Users.Where(x => x.UserId.Equals(user.UserId) && x.Email.Equals(email)).Count();
                if (checkEmailNull <= 0)
                {
                    var isEmailExist = db.Users.Where(s => s.Email == email).Count();
                    if (isEmailExist > 0)
                    {
                        ModelState.AddModelError("", "Email already exists.");
                        return View(user);
                    }
                }

                var checkPass = db.Users.Where(x => x.UserId == user.UserId && x.Password == Password).Count();
                if (checkPass <= 0)
                {
                    user.Password = Encryptor.GetMD5(Password);
                }

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Profile", new { id = user.UserId});
            }
            return View(user);
        }

        [HasCredencial(RoleId = "SHOW_FILE")]
        public ActionResult Show(int fileId)
        {
            if (fileId < 1)
            {
                return null;
            }
            FileDetail file = db.FileDetails.ToList().Find(x => x.FileDetailsId == fileId);
            if (file == null)
            {
                return null;   
            }
            return File(file.Data, file.FileName);
        }

        public ActionResult Chart()
        {
            //Create Bar Chart
            List<string> Faculty = db.Majors.Select(x => x.Name).Distinct().ToList();
            List<int> post = new List<int>();
            foreach (string item in Faculty)
            {
                post.Add(db.Posts.Count(x => x.Major.Name == item));
            }
            ViewBag.Faculty = Faculty;
            ViewBag.PostCount = post.ToList();

            // Create Pie Chart
            var Totalpost = db.Posts.Count();

            List<float> postPie = new List<float>();
            foreach (var item in Faculty)
            {

                var c = (Totalpost > 0) ? db.Posts.Count(x => x.Major.Name == item) / Totalpost * 100 : 0;
                postPie.Add(c);
            }
            ViewBag.PostPercent = postPie.ToList();
          

            //var Post = db.Posts.Select(x => x.Major.Name).Distinct().ToList();
            List<int> contributor = new List<int>();
            foreach (var item in Faculty)
            {
                var p = db.Posts.Where(x => x.Major.Name == item).ToList();
                var c = p.Select(x => x.CreateBy).Distinct();
                contributor.Add(c.Count());
            }
            ViewBag.Contributor = contributor.ToList();
            return View();
        }
    }
}