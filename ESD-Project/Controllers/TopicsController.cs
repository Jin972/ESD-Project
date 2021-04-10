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
    public class TopicsController : BaseController
    {
        private ModelESD db = new ModelESD();

        // GET: Topics
        [HasCredencial(RoleId = "MANAGE_TOPIC")]
        public ActionResult Index()
        {
            var topics = db.Topics.Include(t => t.Major);
            return View(topics.ToList());
        }

        // GET: Topics/Details/5
        [HasCredencial(RoleId = "MANAGE_TOPIC")]
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
        [HasCredencial(RoleId = "MANAGE_TOPIC")]
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
        [HasCredencial(RoleId = "MANAGE_TOPIC")]
        public ActionResult Create([Bind(Include = "TopicId,Name,MetaTitle,Description,DisplayOrder,ClosureDate,CreateDate,CreateBy,ModifyDate,ModifyBy,Status,MajorId")] Topic topic, HttpPostedFileBase uploadFile)
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
                        TopicId = topic.TopicId
                    });
                }              
                topic.CreateBy = session.Username;
                topic.CreateDate = DateTime.Now;
                db.Topics.Add(topic);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name", topic.MajorId);
            return View(topic);
        }

        // GET: Topics/Edit/5
        [HasCredencial(RoleId = "MANAGE_TOPIC")]
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
        [HasCredencial(RoleId = "MANAGE_TOPIC")]
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
                return RedirectToAction("Index");
            }
            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name", topic.MajorId);
            return View(topic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "MANAGE_TOPIC")]
        public ActionResult Delete(FormCollection formCollection)
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
