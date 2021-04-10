using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ESD_Project.Common
{
    public class HasCredencialAttribute : AuthorizeAttribute
    {
        public string RoleId { get; set; }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var session = (UserLogin)HttpContext.Current.Session[CommonConstants.USER_SESSION];
            if(session == null)
            {
                return true;
            }
            List<string> privilegeLevels = this.GetCredentialByLoggedInUser(session.Username);

            if (privilegeLevels.Contains(this.RoleId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error_401.cshtml"
            };
        }

        private List<string> GetCredentialByLoggedInUser(string userName)
        {
            var credentials = (List<string>)HttpContext.Current.Session[CommonConstants.SESSION_CREADENTIALS];
            return credentials;
        }
    }
}