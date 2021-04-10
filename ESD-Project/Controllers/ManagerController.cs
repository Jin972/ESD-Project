using ESD_Project.Common;
using ESD_Project.Models;
using ESD_Project.Models.Code;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ESD_Project.Controllers
{
    public class ManagerController : BaseController
    {
        private ModelESD db = new ModelESD();
        // GET: Manager
        [HasCredencial(RoleId = "FUNCTION_OF_MANAGER")]
        public ActionResult Index()
        {
            var major = db.Majors;
            return View(major.ToList());
        }

        [HasCredencial(RoleId = "FUNCTION_OF_MANAGER")]
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

        [HasCredencial(RoleId = "FUNCTION_OF_MANAGER")]
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
                return RedirectToAction("Profile", new { id = user.UserId });
            }
            return View(user);
        }

        [HasCredencial(RoleId = "FUNCTION_OF_MANAGER")]
        public ActionResult ListTopic(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var topic = db.Topics.Where(x => x.MajorId == id);
            return View(topic.ToList());
        }

        [HasCredencial(RoleId = "FUNCTION_OF_MANAGER")]
        public ActionResult ViewPost(long? id) {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var PostList = db.Posts.Where(m =>m.TopicId == id && m.Status == true);
            return View(PostList.ToList());
        }

        public ActionResult DownFileZip(FormCollection formCollection)
        {
                string[] ids = formCollection["ID"].Split(new char[] { ',' });

                using (var memoryStream = new MemoryStream())
                {

                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (string id in ids)
                        {
                            var post = db.Posts.Find(int.Parse(id));
                            if (post.FileDetails != null)
                            {
                                foreach (var item in post.FileDetails)
                                {
                                    var file = archive.CreateEntry(item.FileName.ToString(), CompressionLevel.Optimal);
                                    using (var stream = file.Open())
                                    {
                                        stream.Write(item.Data, 0, item.Data.Length);
                                    }
                                }
                            }
                        }
                    }
                    var time = DateTime.Now;
                    string fileName = "Posts - " + time + ".zip";
                    return File(memoryStream.ToArray(), "application/zip", fileName);
            }
        }
    }
}