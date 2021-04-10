using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ESD_Project.Common;
using ESD_Project.Models;

namespace ESD_Project.Controllers
{
    public class AcademicYearsController : Controller
    {
        private ModelESD db = new ModelESD();

        // GET: AcademicYears
        [HasCredencial(RoleId = "MANAGE_ACADEMIC_YEAR")]
        public ActionResult Index()
        {
            return View(db.AcademicYears.ToList());
        }

        // GET: AcademicYears/Details/5
        [HasCredencial(RoleId = "MANAGE_ACADEMIC_YEAR")]
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AcademicYear academicYear = db.AcademicYears.Find(id);
            if (academicYear == null)
            {
                return HttpNotFound();
            }
            return View(academicYear);
        }

        // GET: AcademicYears/Create
        [HasCredencial(RoleId = "MANAGE_ACADEMIC_YEAR")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: AcademicYears/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "MANAGE_ACADEMIC_YEAR")]
        public ActionResult Create([Bind(Include = "AcademicYearId,StartYear,EndYear,CreateDate,CreateBy,ModifyDate,ModifyBy")] AcademicYear academicYear)
        {
            if (ModelState.IsValid)
            {
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];
                if (session != null)
                {
                    academicYear.CreateBy = session.Username;
                    academicYear.CreateDate = DateTime.Now;
                }               
                db.AcademicYears.Add(academicYear);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(academicYear);
        }

        // GET: AcademicYears/Edit/5
        [HasCredencial(RoleId = "MANAGE_ACADEMIC_YEAR")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AcademicYear academicYear = db.AcademicYears.Find(id);
            if (academicYear == null)
            {
                return HttpNotFound();
            }
            return View(academicYear);
        }

        // POST: AcademicYears/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredencial(RoleId = "MANAGE_ACADEMIC_YEAR")]
        public ActionResult Edit([Bind(Include = "AcademicYearId,StartYear,EndYear,CreateDate,CreateBy,ModifyDate,ModifyBy")] AcademicYear academicYear)
        {
            if (ModelState.IsValid)
            {
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];
                academicYear.ModifyBy = session.Username;
                academicYear.ModifyDate = DateTime.Now;
                db.Entry(academicYear).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(academicYear);
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
                    var years = db.AcademicYears.Find(long.Parse(id));
                    db.AcademicYears.Remove(years);
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
