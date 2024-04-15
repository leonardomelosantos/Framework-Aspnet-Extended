using System.Collections.Generic;

namespace FrameworkAspNetExtended.MVC.Controllers
{
    public interface IControllerErrors
    {
        void AddErrors(IList<string> messages);
    }
}
