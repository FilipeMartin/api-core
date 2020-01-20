using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using taesa_aprovador_api.Authorization;
using taesa_aprovador_api.Models;

namespace taesa_aprovador_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        [Permissions("User")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        public ActionResult AddNotification([FromBody] Notification notification)
        {
            if(notification == null){
                return BadRequest();
            }

            var _user = _context.Users.Find(notification.UserId);

            if(_user == null){
                return NotFound(new {message = "Usuário não encontrado!"});
            }

            var _notification = _context.Notifications.Where(n => n.Uuid.Equals(notification.Uuid)).FirstOrDefault();

            if(_notification == null){
                _context.Notifications.Add(notification);
            } else{
                _notification.UserId = notification.UserId;
                _notification.TokenFcm = notification.TokenFcm;
                _notification.UpdateAt = DateTime.Now;
                _context.Notifications.Update(_notification);
            }
            _context.SaveChanges();

            return Ok(new {message = "Registro cadastrado com sucesso!"});
        }
    }
}