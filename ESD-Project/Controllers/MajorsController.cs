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
    public class MajorsController : Controller
    {
        private ModelESD db = new ModelESD();

        // GET: Majors
        [HasCredencial(RoleId = "MANAGE_MAJOR")]
        public ActionResult Index()
        {
            return View(db.Majors.ToList());
        }

        // GET: Majors/Details/5
        [HasCredencial(RoleId = "MANAGE_MAJOR")]
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Major major = db.Majors.Find(id);
            if (major == null)
            {
                return HttpNotFound();
            }
            return View(major);
        }

        // GET: Majors/Create
        [HasCredencial(RoleId = "MANAGE_MAJOR")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Majors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "MANAGE_MAJOR")]
        public ActionResult Create([Bind(Include = "MajorId,Name,MetaTitle,Description,CreateDate,CreateBy,ModifyDate,ModifyBy")] Major major, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];
                FileUploadService service = new FileUploadService();
                if (uploadFile != null)
                {
                    db.FileDetails.Add(new FileDetail
                    {
                        FileName = Path.GetFileName(uploadFile.FileName),
                        ContentType = uploadFile.ContentType,
                        Data = service.ConvertToBytes(uploadFile),
                        MajorId = major.MajorId
                    });
                }
                major.CreateBy = session.Username;
                major.CreateDate = DateTime.Now;
                db.Majors.Add(major);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(major);
        }

        // GET: Majors/Edit/5
        [HasCredencial(RoleId = "MANAGE_MAJOR")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Major major = db.Majors.Find(id);
            if (major == null)
            {
                return HttpNotFound();
            }
            return View(major);
        }

        // POST: Majors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "MANAGE_MAJOR")]
        public ActionResult Edit([Bind(Include = "MajorId,Name,MetaTitle,Description,CreateDate,CreateBy,ModifyDate,ModifyBy")] Major major, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {
                FileUploadService service = new FileUploadService();
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];
                if (uploadFile != null)
                {
                    FileDetail file = db.FileDetails.ToList().Find(x => x.MajorId == major.MajorId);
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
                            MajorId = major.MajorId
                        });
                    }
                }
                major.ModifyBy = session.Username;
                major.ModifyDate = DateTime.Now;
                db.Entry(major).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(major);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "MANAGE_ACCOUNT")]
        public ActionResult Delete(FormCollection formCollection)
        {
            if (formCollection["ID"] != null)
            {
                string[] ids = formCollection["ID"].Split(new char[] { ',' });
                foreach (string id in ids)
                {
                    var major = db.Majors.Find(long.Parse(id));
                    if (major.FileDetails != null)
                    {
                        foreach (var file in major.FileDetails.ToList())
                        {
                            db.FileDetails.Remove(file);
                        }
                    }

                    db.Majors.Remove(major);
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
