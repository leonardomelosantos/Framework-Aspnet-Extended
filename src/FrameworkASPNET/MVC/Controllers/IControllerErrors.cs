using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkAspNetExtended.MVC.Controllers
{
	public interface IControllerErrors
	{
		void AddErrors(IList<string> messages);
	}
}
