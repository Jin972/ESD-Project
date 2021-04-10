using ESD_Project.Common;
using ESD_Project.Models;
using ESD_Project.Models.Code;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ESD_Project.Controllers
{
    public class CoordinatorController : BaseController
    {
        private ModelESD db = new ModelESD();
        // GET: Coordinator
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult Index()
        {
            return View();
        }

        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
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

        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
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
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
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

        //Manage Topic- Show topic list
        [HttpGet]
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult ManageTopic() 
        {
            var session = (UserLogin)Session[CommonConstants.USER_SESSION];
            var topic = db.Topics.Where(x => x.MajorId == session.MajorID).ToList();
            return View(topic);
        }



        // GET: Topics/Details/5
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Topic topic = db.Topics.Find(id);
            if (topic == null)
            {
                return HttpNotFound();
            }
            return View(topic);
        }

        // GET: Topics/Create
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult Create()
        {
            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name");
            return View();
        }

        // POST: Topics/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult Create([Bind(Include = "TopicId,Name,MetaTitle,Description,DisplayOrder,ClosureDate,CreateDate,CreateBy,ModifyDate,ModifyBy,Status,MajorId")] Topic topic, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];
                FileUploadService service = new FileUploadService();
                if(uploadFile != null)
                {
                    db.FileDetails.Add(new FileDetail
                    {
                        FileName = Path.GetFileName(uploadFile.FileName),
                        ContentType = uploadFile.ContentType,
                        Data = service.ConvertToBytes(uploadFile),
                        TopicId = topic.TopicId
                    });
                }
                topic.MajorId = session.MajorID;
                topic.CreateBy = session.Username;
                topic.CreateDate = DateTime.Now;
                db.Topics.Add(topic);
                db.SaveChanges();
                return RedirectToAction("ManageTopic");
            }

            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name", topic.MajorId);
            return View(topic);
        }

        // GET: Topics/Edit/5
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Topic topic = db.Topics.Find(id);
            if (topic == null)
            {
                return HttpNotFound();
            }
            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name", topic.MajorId);
            return View(topic);
        }

        // POST: Topics/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult Edit([Bind(Include = "TopicId,Name,MetaTitle,Description,DisplayOrder,ClosureDate,CreateDate,CreateBy,ModifyDate,ModifyBy,Status,MajorId")] Topic topic, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {
                FileUploadService service = new FileUploadService();
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];
                if (uploadFile != null)
                {
                    FileDetail file = db.FileDetails.ToList().Find(x => x.TopicId == topic.TopicId);
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
                            TopicId = topic.TopicId
                        });
                    }
                }
                topic.ModifyBy = session.Username;
                topic.ModifyDate = DateTime.Now;
                db.Entry(topic).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ManageTopic");
            }
            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name", topic.MajorId);
            return View(topic);
        }

        //------------------------------------Delete Topic----------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult DeleteTopic(FormCollection formCollection)
        {
            if (formCollection["ID"] != null)
            {
                string[] ids = formCollection["ID"].Split(new char[] { ',' });
                foreach (string id in ids)
                {
                    var topic = db.Topics.Find(int.Parse(id));
                    if (topic.FileDetails != null)
                    {
                        foreach (var item in topic.FileDetails.ToList())
                        {
                            db.FileDetails.Remove(item);
                        }
                    }
                    db.Topics.Remove(topic);
                    db.SaveChanges();
                }
                return RedirectToAction("ManageTopic");
            }
            else
            {
                ModelState.AddModelError("", "Nothing to remove! Please try again.");
                return View("Index");
            }

        }


        //----------------------------------Show Posts List------------------------------------------------
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult ViewListPost(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var post = db.Posts.Where(x => x.TopicId == id).ToList();
            return View(post);
        }

        //----------------------------------View Post Details-------------------------------------
        [HttpGet]
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult ViewPostDetails(long? id) 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Include(m => m.Comment).SingleOrDefault(m => m.PostId == id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        //----------------------------------View Post Details - process file-------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult ViewPostDetails(Post post, List<HttpPostedFileBase> postedFile)
        {
            if (ModelState.IsValid)
            {

                //New Files

                foreach (var file in postedFile)
                {
                    FileUploadService service = new FileUploadService();
                    if (file != null && file.ContentLength > 0)
                    {
                        FileDetail fileDetail = new FileDetail()
                        {
                            FileName = Path.GetFileName(file.FileName),
                            ContentType = file.ContentType,
                            Data = service.ConvertToBytes(file),
                            PostId = post.PostId
                        };
                        db.Entry(fileDetail).State = EntityState.Added;
                    }
                }
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];                       
                post.ModifyBy = session.Username;
                post.ModifyDate = DateTime.Now;
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.EditPost = "This post has been edited successfully!";
                return RedirectToAction("ViewListPost", new {id = post.TopicId });
            }
            return View(post);
        }

        //--------------------------------Add Comment--------------------------------------
        [HttpPost]
        [HasCredencial(RoleId = "FUNCTION_OF_COORDINATOR")]
        public ActionResult AddComment()
        {
            var PostId = Request["PostId"];
            var UserId = Request["UserId"];
            string cmtContent = Request["CmtContent"];
            db.Comments.Add(new Comment
            {
                Content = cmtContent,
                CmtDate = DateTime.Now,
                PostId = long.Parse(PostId),
                UserId = long.Parse(UserId)
            });
            db.SaveChanges();
            return RedirectToAction("ViewPostDetails", new { id = long.Parse(PostId) });
        }

        //--------------------------------delete Comment----------------------------------------
        [HttpPost]
        [HasCredencial(RoleId = "DELETE_COMMENT")]
        public JsonResult DeleteComment(int? id)
        {
            if (id == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                Comment comment = db.Comments.Find(id);
                if (comment == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //Remove from database
                db.Comments.Remove(comment);
                db.SaveChanges();
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}