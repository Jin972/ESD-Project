using ESD_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using ESD_Project.Common;
using System.Data.Entity;
using ESD_Project.Models.Code;

namespace ESD_Project.Controllers
{
    public class StudentController : BaseController
    {
        private ModelESD db = new ModelESD();
        // GET: Student
        [HasCredencial(RoleId = "FUNCTION_OF_STUDENT")]
        public ActionResult Index()
        {
            var session = (UserLogin)Session[CommonConstants.USER_SESSION];
            var MyPost = db.Posts.Where(m => m.CreateBy.Equals(session.Username));
            if (MyPost.Count() > 0)
            {
                return View(MyPost.ToList());
            }
            else 
            {
                ViewBag.NotFoundYourPosts = "Currently, You do not have any Posts!";
                return View(MyPost.ToList());
            }
        }

        [HasCredencial(RoleId = "FUNCTION_OF_STUDENT")]
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

        [HasCredencial(RoleId = "FUNCTION_OF_STUDENT")]
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
        [HasCredencial(RoleId = "FUNCTION_OF_STUDENT")]
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

        //--------------------------------------Search Topic function-------------------------------------------------
        [HttpGet]
        [HasCredencial(RoleId = "FUNCTION_OF_STUDENT")]
        public ActionResult Search() {
            return View();
        }

        [HttpPost]
        [HasCredencial(RoleId = "FUNCTION_OF_STUDENT")]
        public ActionResult Search(string SearchString)
        {
            var SearchResult = db.Topics.Where(s => s.Name.Contains(SearchString) || s.CreateBy.Contains(SearchString)).ToList();
            if (SearchResult.Count() > 0)
            {
                return View(SearchResult);
            }
            else 
            {
                ViewBag.NotFoundResult = "Not Found Your Result! \n This result may be unexisted or not based on your faculty.";
                return View(SearchResult);
            }
        }


        //-----------------------------------------Show Post Details--------------------------------------------------
        [HttpGet]
        [HasCredencial(RoleId = "FUNCTION_OF_STUDENT")]
        public ActionResult SubmitPosts(long? id, long? myPostsId) 
        {
            if (ModelState.IsValid) {
                if (myPostsId != null)
                {
                    Post post = db.Posts.Find(myPostsId);
                    ViewBag.DeadLine = post.Topic.ClosureDate;
                    if (post == null)
                    {
                        ViewBag.PostId = "Null";
                    }
                    else 
                    {
                        ViewBag.FileDetail = post.FileDetails;
                    }
                    return View(post);
                }
                else
                {
                    var session = (UserLogin)Session[CommonConstants.USER_SESSION];
                    Topic topic = db.Topics.Find(id);
                    ViewBag.DeadLine = topic.ClosureDate;
                    ViewBag.TopicName = topic.Name;
                    Post post = db.Posts.SingleOrDefault(m => m.TopicId == id && m.CreateBy == session.Username);
                    
                    if (post == null)
                    {
                        ViewBag.PostId = "Null";
                    }
                    else
                    {
                        ViewBag.FileDetail = post.FileDetails;
                    }
                    return View(post);
                }
            }
            return View();
        }

        //-----------------------------------------------------Submit file-------------------------------------------------------
        [HttpPost]
        [HasCredencial(RoleId = "FUNCTION_OF_STUDENT")]
        public ActionResult SubmitPosts(long? id,Post post, List<HttpPostedFileBase> postedFile)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (postedFile != null)
            {
                FileUploadService service = new FileUploadService();
                Service sendEmail = new Service();
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];

                //Add file to Database
                List<FileDetail> fileDetails = new List<FileDetail>();
                foreach (var file in postedFile)
                {
                    FileDetail fileDetail = new FileDetail()
                    {
                        FileName = Path.GetFileName(file.FileName),
                        ContentType = file.ContentType,
                        Data = service.ConvertToBytes(file)
                    };
                    fileDetails.Add(fileDetail);
                }

                // Add Post                
                Topic topic = db.Topics.Find(id);
                post.TopicId = id;
                post.MajorId = topic.MajorId;
                post.FileDetails = fileDetails;
                post.CreateBy = session.Username;
                post.CreateDate = DateTime.Now;
                db.Posts.Add(post);
                db.SaveChanges();

                var user = db.Users.FirstOrDefault(x => x.Username == topic.CreateBy);
                var receiver = user.Email;
                var subject = "Your student has submitted their post submission for your Topic";
                var body = "Hi "+user.Name+ "\n Your student has submitted their post submission for your Topic\nTopic: "+topic.Name+"\nStudent's name: "+session.Name+"\nSubmit Date: "+ DateTime.Now;
                sendEmail.SendAutometicalEmail(receiver, subject, body);
                return RedirectToAction("SubmitPosts");
            }
            ViewBag.upLoadError = "You have not file yet!";
            return View();
        }

        //---------------------------------------------------Edit Post function----------------------------------------------------------
        [HttpPost]
        [HasCredencial(RoleId = "FUNCTION_OF_STUDENT")]
        public ActionResult EditPost(Post post, List<HttpPostedFileBase> postedFile)
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
                Service sendEmail = new Service();
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];

                post.ModifyBy = session.Username;
                post.ModifyDate = DateTime.Now;
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();

                Topic topic = db.Topics.Find(post.TopicId);
                var user = db.Users.ToList().Find(x => x.Username == topic.CreateBy);
                if (user != null) 
                {
                    var receiver = user.Email;
                    var subject = "Your student has submitted their post submission for your Topic";
                    var body = "Hi " + user.Name + "\n Your student has submitted their post submission for your Topic\nTopic: " + topic.Name + "\nStudent's name: " + session.Name + "\nSubmit Date: " + DateTime.Now;
                    sendEmail.SendAutometicalEmail(receiver, subject, body);
                }
                

                ViewBag.EditPost = "This post has been edited successfully!";
                return RedirectToAction("SubmitPosts", new { myPostsId = post.PostId });
            }
            return View(post);
        }

        //----------------------------------------------Download file------------------------------------------------------
        [HttpPost]
        public FileResult Download(int? fileId)
        {
            FileDetail file = db.FileDetails.ToList().Find(x => x.FileDetailsId == fileId);
            return File(file.Data, file.ContentType, file.FileName);
        }

        //---------------------------------------------------Delete Post----------------------------------------------------
        [HttpPost]
        [HasCredencial(RoleId = "FUNCTION_OF_STUDENT")]
        public ActionResult DeletePost()
        {
                var Id = Request["postId"];
                long postId = long.Parse(Id);
                Post post = db.Posts.Find(postId);
                if (post == null)
                {
                    return HttpNotFound();
                }
                var fileDetail = db.FileDetails.Where(x => x.PostId == postId).ToList();
                foreach (var item in fileDetail.ToList()) 
                {
                    db.FileDetails.Remove(item);
                    
                }
                db.SaveChanges();
                return RedirectToAction("SubmitPosts", new { myPostsId = postId });
        }

        //--------------------------------------------------Delete file-----------------------------------------------------------
        [HttpPost]
        [HasCredencial(RoleId = "DELETE_FILE")]
        public JsonResult DeleteFile(int? id)
        {
            if (id == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                FileDetail fileDetail = db.FileDetails.Find(id);
                if (fileDetail == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //Remove from database
                db.FileDetails.Remove(fileDetail);
                db.SaveChanges();
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        [HasCredencial(RoleId = "FUNCTION_OF_STUDENT")]
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
            return RedirectToAction("SubmitPosts", new { myPostsId = long.Parse(PostId) });
        }
    }
}