using GongChaWebApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GongChaWebApplication.Controllers
{
    public class RedirectRoleAttribute : ActionFilterAttribute
    {
        private GongChaDbContext db = new GongChaDbContext();
        public string IdParameter;
        public string IdType;
        public string RedirectUrl;
        public object RouteValues;
        //public RedirectRoleAttribute(string role, string redirectUrl)
        //{
        //    Roles = role;
        //    this.RedirectUrl = redirectUrl;
        //}
        public RedirectMethods RedirectRole;
        private bool _isSelectedRole;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var parameters = filterContext.ActionParameters;
            if (String.IsNullOrEmpty(IdParameter))
            {
                IdParameter = "Id";
            }
            if (parameters.ContainsKey(IdParameter))
            {
                var parameterValue = parameters[IdParameter];
                
            }
        }

        //public RedirectRoleAttribute()
        //{
        //    //var url = new UrlHelper(filterContext.RequestContext);
        //    //filterContext.Result = new RedirectResult(url.Action("Index", RouteValues));
        //}
        //public override void OnAuthorization(AuthorizationContext filterContext)
        //{
        //    base.OnAuthorization(filterContext);
        //    // Basic Authorization is done.
        //    if (_isSelectedRole)
        //    {
        //        //filterContext.Controller.ViewBag.StatusMessage += "You are not authorized";
        //        var member = filterContext.HttpContext.User.Identity.Name;
        //        //switch (RedirectRole)
        //        //{
        //        //    case RedirectRoles.SELF_ONLY:
        //        //        Member CurrentMember = (Member)filterContext.HttpContext.Session["CurrentMember"];
        //        //        if ((int)filterContext.RouteData.Values["Id"] != CurrentMember.Id)
        //        //        {
        //        //            var url = new UrlHelper(filterContext.RequestContext);
        //        //            filterContext.Result = new RedirectResult(url.Action(filterContext.ActionDescriptor.ActionName, new { Id = CurrentMember.Id }));
        //        //        }
        //        //        break;
        //        //    case RedirectRoles.SAME_STORE_ONLY:
        //        //        break;
        //        //    default:
        //        //        break;
        //        //}
        //    }
        //}
        //protected override bool AuthorizeCore(HttpContextBase httpContext)
        //{
        //    _isSelectedRole = base.AuthorizeCore(httpContext);
        //    return _isSelectedRole;
        //}
        //protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        //{
        //    // Do nothing. It is intended to have make it not authorized.
        //    return;
        //    //base.HandleUnauthorizedRequest(filterContext);
        //}

    }
    public enum AccessibleTypes
    {
        NONE,
        SELF_ONLY,
        OWN_STORE,
        ALL,
    }

}