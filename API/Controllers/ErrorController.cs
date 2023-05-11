using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // For testing purpose
    public class ErrorController : BaseApiController
    {
        private readonly DataContext context;

        public ErrorController(DataContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }

         [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            var user = this.context.Users.Find(-1);

            if (user is null) 
            {
                return NotFound();
            }

            return user;
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            var user = this.context.Users.Find(-1);

            var result = user.ToString();

            return result;
        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("This was not a good request");
        }
    }
}