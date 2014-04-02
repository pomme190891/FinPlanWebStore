using System.Web.Mvc;
using System.Web.Routing;
using FinPlanWeb.DTOs;

namespace FinPlanWeb.Controllers
{
    public class BaseController : Controller
    {
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            ViewBag.User = Session["User"] as UserLoginDTO;
        }
    }
}