using System;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using taesa_aprovador_api.Core;
using taesa_aprovador_api.Models;

namespace taesa_aprovador_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration Configuration;
        private readonly ITokenJWT TokenJWT;

        public AuthenticationController(AppDbContext context, IConfiguration configuration, ITokenJWT tokenJWT)
        {
            _context = context;
            Configuration = configuration;
            TokenJWT = tokenJWT;
        }

        [HttpPost]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public ActionResult Authentication([FromBody] User user)
        {
            if(user == null){
                return BadRequest();
            }

            user.Login = user.Email.Remove(user.Email.IndexOf("@"));

            if(!user.Email.Equals(user.Login + "@taesa.com.br")){
                return Unauthorized(new{message="Login e senha incorretos. Por fazer tente novamente."});
            }

            DirectoryEntry directoryEntry = new DirectoryEntry(Configuration.GetSection("Ldap").GetSection("Path").Value, user.Login, user.Password);
            directoryEntry.AuthenticationType = AuthenticationTypes.Secure;

            DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
            directorySearcher.Filter = string.Format("(&(objectClass=user)(objectCategory=person)(sAMAccountName={0}))", user.Login);
            
            try
            {
                SearchResult searchResult = directorySearcher.FindOne();
                ResultPropertyValueCollection rpvcResult = searchResult.Properties["memberOf"];
                
                bool statusGroup = false;
                foreach (Object PropertyValue in rpvcResult)
                {
                    if (PropertyValue.ToString().Contains(Configuration.GetSection("Ldap").GetSection("Group").Value))
                    {
                        statusGroup = true;
                        break;
                    }
                }

                if(!statusGroup){
                    return StatusCode(403, new{message="Você não possui acesso a este aplicativo. Por favor, em caso de dúvidas, contate a equipe de TI."});
                }
            } 
            catch(COMException ex)
            {
                switch(ex.ErrorCode)
                {
                    case -2147023570:
                        return Unauthorized(new{message="Login e senha incorretos. Por fazer tente novamente."});
                    default:
                        return StatusCode(500, new{message="Servidor indisponível."});
                }
            }

            var _user = _context.Users.Where(u => u.Email.Equals(user.Email)).FirstOrDefault();
            var notification = user.Notifications.First();

            if(_user == null){
                user.Notifications = null;
                _context.Users.Add(user);
                _context.SaveChanges();
                _user = user;
            }
            
            var _notification = _context.Notifications.Where(n => n.Uuid.Equals(notification.Uuid)).FirstOrDefault();
            notification.UserId = _user.Id;

            if(_notification == null){
                _context.Notifications.Add(notification);
            } else{
                _notification.UserId = notification.UserId;
                _notification.TokenFcm = notification.TokenFcm;
                _notification.UpdateAt = DateTime.Now;
                _context.Notifications.Update(_notification);
            }
            _context.SaveChanges();

            return Ok(new {token = TokenJWT.create(_user, "User"), id_user = _user.Id});
        }

        /// <summary>
        /// (Gerar Token JWT)
        /// </summary>
        /// <remarks>
        /// <h2>Autenticar:</h2>
        /// <div style="font-size: 15px">
        ///     Enviar uma requisição <b>POST</b> para <b>https://taesaaprovador.azurewebsites.net/api/authentication/token</b><br/><br/>
        ///     <b>OBS:</b> O token gerado será usado para acessar todos os endpoints da API
        /// </div>
        /// <h2>Passar no Header:</h2>
        /// <div style="font-size: 15px">
        ///     Content-Type: application/json
        /// </div>
        ///
        /// <h2>Propriedades: </h2>
        /// <div style="font-size: 15px">
        ///     <b>key:</b> (string) Passar a chave de acesso
        /// </div>
        /// </remarks>
        [HttpPost("token")]
        [AllowAnonymous]
        public ActionResult AuthenticationKey([FromBody] AuthenticationKey authenticationKey)
        {
            if(authenticationKey == null){
                return BadRequest();
            }

            if(!authenticationKey.Key.Equals(Configuration.GetSection("Taesa").GetSection("Key").Value)){
                return Unauthorized();
            }

            return Ok(TokenJWT.create(null, "System"));
        }
    }
}