using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ESD_Project.Common;
using ESD_Project.Models;
using ESD_Project.Models.Code;

namespace ESD_Project.Controllers
{
    public class UsersController : BaseController
    {
        private ModelESD db = new ModelESD();

        // GET: Users
        [HasCredencial(RoleId = "MANAGE_ACCOUNT")]
        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.GroupMember).Include(u => u.Major);
            return View(users.ToList());
        }

        // GET: Users/Details/5
        [HasCredencial(RoleId = "MANAGE_ACCOUNT")]
        public ActionResult Details(long? id)
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

        // GET: Users/Create
        [HasCredencial(RoleId = "MANAGE_ACCOUNT")]
        public ActionResult Create()
        {
            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name");
            ViewBag.GroupId = new SelectList(db.GroupMembers, "GroupId", "Name");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "MANAGE_ACCOUNT")]
        public ActionResult Create([Bind(Include = "UserId,Name,Email,Username,Password,Phone,Address,DateOfBirth,CreateDate,CreateBy,ModifyDate,ModifyBy,AccountStatus,GroupId,MajorId")] User user,FileDetail fileDetail, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {   
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];
                var checkEmail = db.Users.FirstOrDefault(s => s.Email == user.Email);
                var checkUsername = db.Users.FirstOrDefault(s => s.Username == user.Username);
                ViewBag.GroupId = new SelectList(db.GroupMembers, "GroupId", "Name", user.GroupId);
                ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name", user.MajorId);
                if (checkEmail != null)
                {
                    ModelState.AddModelError("", "Email already exists.");
                    return View();
                }
                else if (checkUsername != null)
                {
                    ModelState.AddModelError("", "Username already exists.");
                    return View();
                }
                else
                {
                    FileUploadService service = new FileUploadService();
                    if(uploadFile != null)
                    {
                        db.FileDetails.Add(new FileDetail
                        {
                            FileName = Path.GetFileName(uploadFile.FileName),
                            ContentType = uploadFile.ContentType,
                            Data = service.ConvertToBytes(uploadFile),
                            UserId = user.UserId
                        });
                    }                   
                    if(user.GroupId == "Admin" || user.GroupId == "Manager")
                    {
                        user.MajorId = null;
                    }
                    user.CreateBy = session.Username;
                    user.CreateDate = DateTime.Now;
                    user.Password = Encryptor.GetMD5(user.Password);
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.Users.Add(user);
                    var result = db.SaveChanges();
                    if (result > 0)
                    {
                        ViewBag.Success = "Create Successfully!";
                        return View();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Create unsuccessfully! Please try again.");
                        return View();
                    }
                }
            }          
            return View(user);
        }

        // GET: Users/Edit/5
        [HasCredencial(RoleId = "MANAGE_ACCOUNT")]
        public ActionResult Edit(long? id)
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

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,Name,Email,Username,Password,Phone,Address,DateOfBirth,CreateDate,CreateBy,ModifyDate,ModifyBy,AccountStatus,GroupId,MajorId")] User user, HttpPostedFileBase uploadFile)
        {   
                FileUploadService service = new FileUploadService();
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];
                if (uploadFile != null) {
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
                return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "MANAGE_ACCOUNT")]
        public ActionResult Delete(FormCollection formCollection)
        {
            if(formCollection["ID"] != null)
            {
                string[] ids = formCollection["ID"].Split(new char[] { ',' });
                foreach (string id in ids)
                {
                    var user = db.Users.Find(long.Parse(id));
                    if (user.FileDetails != null) {
                        foreach (var file in user.FileDetails.ToList())
                        {
                            db.FileDetails.Remove(file);
                        }                      
                    }
                                   
                    db.Users.Remove(user);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Nothing to remove! Please try again.");
                return View("Index");
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
