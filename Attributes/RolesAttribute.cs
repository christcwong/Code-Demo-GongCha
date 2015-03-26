using GongChaWebApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GongChaWebApplication.Controllers
{
    public class RolesAttribute : AuthorizeAttribute
    {
        public RedirectMethods SuccessfulRedirectMethod;
        public RedirectMethods FailureRedirectMethod;
        private bool _isAuthorized;
        public RolesAttribute(params string[] roles)
        {
            if (roles.Any(r => r.ToUpper().Contains("ANY")))
            {
                var type = typeof(MemberAccessLevels);
                var allRoles = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Where(f => f.FieldType == typeof(string)).Select(f => (string)f.GetValue(null));
                Roles = String.Join(",", allRoles.ToList());
            }
            else
            {
                Roles = String.Join(",", roles);
            }
        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (_isAuthorized)
            {
                var url = new UrlHelper(filterContext.RequestContext);
                switch (SuccessfulRedirectMethod)
                {
                    case RedirectMethods.INDEX:
                        filterContext.Result = new RedirectResult(url.Action("Index"));
                        break;
                    case RedirectMethods.SAME_REQUEST:
                        filterContext.Result = new RedirectResult(url.Action("Logout", "Member", new { returnUrl = filterContext.HttpContext.Request.Url.PathAndQuery }));
                        break;
                    default:
                        break;
                };
            }
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var url = new UrlHelper(filterContext.RequestContext);
            switch (FailureRedirectMethod)
            {
                case RedirectMethods.INDEX:
                    filterContext.Controller.ViewBag.StatusMessage += "You are not authorized.";
                    filterContext.Result = new RedirectResult(url.Action("Index"));
                    break;
                case RedirectMethods.SAME_REQUEST:
                    filterContext.Controller.ViewBag.StatusMessage += "You are not authorized.";
                    filterContext.Result = new RedirectResult(url.Action("Login", "Member", new { returnUrl = filterContext.HttpContext.Request.Url.PathAndQuery }));
                    break;
                default:
                    break;
            };

            //base.HandleUnauthorizedRequest(filterContext);
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if ((Member)httpContext.Session["CurrentMember"] != null)
            {
                _isAuthorized = base.AuthorizeCore(httpContext);
                return _isAuthorized;
            }
            _isAuthorized = false;
            return _isAuthorized;
        }
    }
    public enum RedirectMethods
    {
        NO_REDIRECT,
        INDEX,
        SAME_REQUEST,
    }
}