using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using GongChaWebApplication.Models;

namespace GongChaWebApplication.Controllers
{
    public class LoggingAttribute : ActionFilterAttribute
    {
        protected readonly ILog log = LogManager.GetLogger('\t'.ToString());

        public bool needLogging { get; set; }

        public LoggingAttribute()
        {
            needLogging = true;
        }

        public LoggingAttribute(bool needLogging)
        {
            this.needLogging = needLogging;
        }

        public override void OnActionExecuting(ActionExecutingContext
    filterContext)
        {
            var loggingAttributes = filterContext.ActionDescriptor.GetCustomAttributes(typeof(LoggingAttribute), true);
            foreach (var item in loggingAttributes)
            {
                if (!((LoggingAttribute)item).needLogging)
                {
                    return;
                }
            }

            var sb = new StringBuilder();
            sb.Append(" : ");
            sb.Append(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName);
            sb.Append(" - ");
            sb.Append(filterContext.ActionDescriptor.ActionName);
            sb.Append(" - Member : { ");
            Member CurrentMember = (Member)filterContext.HttpContext.Session["CurrentMember"];
            sb.Append(CurrentMember == null ? "null" : CurrentMember.Email);
            sb.Append(" } - paramters :{ ");
            var parameters = filterContext.ActionParameters;
            foreach (var parameter in parameters)
            {
                sb.Append(" { ");
                sb.Append(parameter.Key);
                sb.Append(" : ");
                sb.Append(parameter.Value);
                sb.Append(" } ");
            }
            sb.Append(" } ");
            this.log.Info(sb.ToString());
        }
    }
}