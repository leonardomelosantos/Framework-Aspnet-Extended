using System;
using System.Web.Management;

namespace FrameworkAspNetExtended.Entities.Exceptions
{
    public class WebCustomErrorEvent : WebErrorEvent
    {
        public WebCustomErrorEvent(string message, object eventSource, Exception exception) :
            base(message, eventSource, 100002, exception)
        {
        }
    }
}
