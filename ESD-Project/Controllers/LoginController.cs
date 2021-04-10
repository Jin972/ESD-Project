using BotDetect.Web.Mvc;
using ESD_Project.Common;
using ESD_Project.Models;
using ESD_Project.Models.Code;
using reCAPTCHA.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ESD_Project.Controllers
{
    public class LoginController : Controller
    {
        private ModelESD db = new ModelESD();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [CaptchaValidator(
        PrivateKey = "6LcMf50aAAAAAHRPs0ao0XAVlfE1Gus9OVv72Aj2",
        ErrorMessage = "Invalid input captcha.",
        RequiredMessage = "The captcha field is required.")]
        public ActionResult Login(LoginModel model, bool captchaValid)
        {
            if (ModelState.IsValid)
            {
                var checkLogin = new AccountModel();
                var result = checkLogin.Login(model.Username, Encryptor.GetMD5(model.Password));
                if (result == 1)
                {
                    var user = checkLogin.GetByID(model.Username);
                    var userSession = new UserLogin();
                    userSession.ID = user.UserId;
                    userSession.Name = user.Name;
                    userSession.Username = user.Username;
                    userSession.MajorID = user.MajorId;
                    userSession.GroupId = user.GroupId;
                    if (user.FileDetails != null)
                    {
                        foreach (var item in user.FileDetails)
                        {
                            userSession.AvatarId = item.FileDetailsId;
                        }
                    }
                    var listCredentials = checkLogin.GetListCreadentials(model.Username);
                    Session.Add(CommonConstants.SESSION_CREADENTIALS, listCredentials);
                    Session.Add(CommonConstants.USER_SESSION, userSession);

                    // Authorize the "Admin" right
                    if (user.GroupId == "Admin")
                    {
                        // direct to admin page if authorize successfully
                        return RedirectToAction("Index", "Admin");

                    }

                    // Authorize the "Manager" right
                    if (user.GroupId == "Manager")
                    {
                        // direct to Marketing Manager page if authorize successfully
                        return RedirectToAction("Index", "Manager");
                    }

                    // Authorize the "Coordinator" right
                    if (user.GroupId == "Coordinator")
                    {
                        // direct to Marketing Coordinator page if authorize successfully
                        return RedirectToAction("Index", "Coordinator");
                    }

                    // Authorize the "Student" right
                    if (user.GroupId == "Student")
                    {
                        // direct to Student page if authorize successfully
                        return RedirectToAction("Index", "Student");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Your account have not the right to login. Please try againt with another account");
                    }

                }
                else if (result == 0)
                {
                    ModelState.AddModelError("", "This account does not exist!");
                }
                else if (result == -1)
                {
                    ModelState.AddModelError("", "The password is locked!");
                }
                else if (result == -2)
                {
                    ModelState.AddModelError("", "The password is incorrect!");
                }
                else
                {
                    ModelState.AddModelError("", "Login is incorrect!");
                }
            }
            return View();

        }

        //Logout
        [AllowAnonymous]
        public ActionResult Logout()
        {
            //Session[CommonConstants.USER_SESSION] = null; 
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}