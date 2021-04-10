using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ESD_Project.Models;
using ESD_Project.Common;
using ESD_Project.Models.Code;

namespace ESD_Project.Controllers
{
    public class PostsController : BaseController
    {
        private ModelESD db = new ModelESD();

        // GET: Posts
        [HasCredencial(RoleId = "MANAGE_POST")]
        public ActionResult Index()
        {
            var posts = db.Posts.Include(p => p.Major).Include(p => p.Topic);
            return View(posts.ToList());
        }

        // GET: Posts/Details/5
        [HasCredencial(RoleId = "MANAGE_POST")]
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // GET: Posts/Create
        [HasCredencial(RoleId = "MANAGE_POST")]
        public ActionResult Create()
        {
            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name");
            ViewBag.TopicId = new SelectList(db.Topics, "TopicId", "Name");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "MANAGE_POST")]
        public ActionResult Create([Bind(Include = "PostId,Name,MetaTitle,Desciption,Status,CreateDate,CreateBy,ModifyDate,ModifyBy,TopicId,MajorId")] Post post)
        {
            if (ModelState.IsValid)
            {
                db.Posts.Add(post);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name", post.MajorId);
            ViewBag.TopicId = new SelectList(db.Topics, "TopicId", "Name", post.TopicId);
            return View(post);
        }

        // GET: Posts/Edit/5
        [HasCredencial(RoleId = "MANAGE_POST")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name", post.MajorId);
            ViewBag.TopicId = new SelectList(db.Topics, "TopicId", "Name", post.TopicId);
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "MANAGE_POST")]
        public ActionResult Edit([Bind(Include = "PostId,Name,MetaTitle,Desciption,Status,CreateDate,CreateBy,ModifyDate,ModifyBy,TopicId,MajorId")] Post post)
        {
            if (ModelState.IsValid)
            {
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MajorId = new SelectList(db.Majors, "MajorId", "Name", post.MajorId);
            ViewBag.TopicId = new SelectList(db.Topics, "TopicId", "Name", post.TopicId);
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "MANAGE_POST")]
        public ActionResult Delete(FormCollection formCollection)
        {
            if (formCollection["ID"] != null)
            {
                string[] ids = formCollection["ID"].Split(new char[] { ',' });
                foreach (string id in ids)
                {
                    var post = db.Posts.Find(int.Parse(id));
                    if (post.FileDetails != null)
                    {
                        foreach (var item in post.FileDetails.ToList())
                        {
                            db.FileDetails.Remove(item);
                        }
                    }
                    db.Posts.Remove(post);
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


        //// POST: Posts/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(long id)
        //{
        //    Post post = db.Posts.Find(id);
        //    var fileDetail = db.FileDetails.Where(x => x.PostId == id).ToList();
        //    foreach (var item in fileDetail) {
        //        db.FileDetails.Remove(item);
        //    }
        //    db.Posts.Remove(post);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}
        
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
