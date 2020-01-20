using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using taesa_aprovador_api.Models;
using taesa_aprovador_api.Models.PushNotification;
using taesa_aprovador_api.Core;
using Microsoft.AspNetCore.Authorization;
using taesa_aprovador_api.Authorization;

namespace taesa_aprovador_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IPushNotification _pushNotification;

        public ItemController(AppDbContext context, IHttpClientFactory clientFactory, IPushNotification pushNotification)
        {
            _context = context;
            _clientFactory = clientFactory;
            _pushNotification = pushNotification;
        }

        [Permissions("User")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public ActionResult<IEnumerable<Item>> GetItems()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type.Equals("UserID"));

            if(claim == null){
                return BadRequest();
            }

            var _user = _context.Users.Find(int.Parse(claim.Value));
            
            var items = _context.Items.Where(i => i.UserId == _user.Id && i.Status == true).OrderBy(i => i.DateLimit).ToList();

            return Ok(items);
        }

        [Permissions("User")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPut("status/{id}")]
        public ActionResult UpdateStatus(int id, [FromBody] ItemStatus itemStatus)
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type.Equals("UserID"));

            if(claim == null){
                return BadRequest();
            }

            var _user = _context.Users.Find(int.Parse(claim.Value));

            if(itemStatus == null){
                return BadRequest();
            }

            var item = _context.Items.Where(i => i.UserId == _user.Id && i.Id == id && i.Status == true).FirstOrDefault();

            if(item == null){
                return NotFound();
            }

            try{
                /*
                var client = _clientFactory.CreateClient("Taesa");
                var json = JsonConvert.SerializeObject(itemStatus);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await client.PutAsync("item/status/" + item.TaesaId, stringContent);

                if(!result.IsSuccessStatusCode){
                    return BadRequest(await result.Content.ReadAsStringAsync());
                }
                */

                item.Comments = itemStatus.Comments;
                item.ApprovalStatus = itemStatus.Status;
                item.Status = false;
                item.UpdateAt = DateTime.Now;

                _context.Items.Update(item);
                _context.SaveChanges();

                return NoContent();

            } catch(Exception ex){
                return BadRequest(new {message = ex.Message});
            }
        }

        [Permissions("System")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("{id}")]
        public ActionResult<Item> GetItem(int id)
        {   
            var item = _context.Items.Find(id);

            if(item == null){
                return BadRequest();
            }

            return item;
        }

        /// <summary>
        /// (Cadastrar Item)
        /// </summary>
        /// <remarks>
        /// <h2>Cadastrar Item:</h2>
        /// <div style="font-size: 15px">
        ///     Enviar uma requisição <b>POST</b> para <b>https://taesaaprovador.azurewebsites.net/api/item</b>
        /// </div>
        /// <h2>Passar no Header:</h2>
        /// <div style="font-size: 15px">
        ///     Content-Type: application/json<br/>
        ///     Authorization: bearer &lt;token&gt;<br/><br/>
        ///     <b>OBS:</b> Altere o &lt;token&gt; pelo Token gerado na autenticação
        /// </div>
        ///
        /// <h2>Propriedades: </h2>
        /// <div style="font-size: 15px">
        ///     <b>taesaId:</b> (inteiro) ID do item gerado pela Taesa<hr/>
        ///     <b>email:</b> (string) E-mail do usuário responsável por aprovar ou reprovar o item<hr/>
        ///     <b>title:</b> (string) Título do item<hr/>
        ///     <b>content:</b> (string) Conteúdo no formato HTML. Tags aceitas: &lt;br&gt;, &lt;a&gt;, &lt;b&gt;, &lt;p&gt;. 
        ///     <b>OBS: </b> Não usar <b>target</b> nos links<hr/>
        ///     <b>sourceSystem:</b> (string) Sistema de origem<hr/>
        ///     <b>dateLimit:</b> (string) Data limite da aprovação (YYYY-MM-DD)<hr/>
        ///     <b>approvers:</b> (string) Todos os aprovadores<hr/>
        ///     <b>approvalType:</b> (string) Tipo de aprovação
        /// </div>
        /// </remarks>
        [Permissions("System")]
        [HttpPost]
        public async Task<ActionResult<Item>> AddItem([FromBody] Item item)
        {
            if(item == null){
                return BadRequest();
            }

            var _item = _context.Items.Where(i => i.TaesaId == item.TaesaId).FirstOrDefault();

            if(_item != null){
                return BadRequest(new {message = "Item já cadastrado."});
            }

            item.FilterAttribute();

            var user = _context.Users.Include("Notifications").Where(u => u.Email.Equals(item.Email)).FirstOrDefault();

            if(user == null){
                user = new User {Email = item.Email};
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            item.UserId = user.Id;
            _context.Add(item);
            _context.SaveChanges();

            // Enviar Notificação
            if(user.Notifications != null && user.Notifications.Count > 0){
                var NotificationScheme = new NotificationScheme
                {
                    Title = "Aprovador Taesa",
                    Body = "Novo item cadastrado: " + item.Title,
                    Data = item.GetItem(),
                    Notifications = user.Notifications
                };

                await _pushNotification.Send(NotificationScheme);
            }

            return CreatedAtAction("GetItem", new {id = item.Id}, item);
        }

        /// <summary>
        /// (Editar Item)
        /// </summary>
        /// <remarks>
        /// <h2>Editar Item:</h2>
        /// <div style="font-size: 15px">
        ///     Enviar uma requisição <b>PUT</b> para <b>https://taesaaprovador.azurewebsites.net/api/item/{taesaId}</b><br/><br/>
        ///     <b>OBS:</b> Altere o {taesaId} pelo id do item que deseja alterar
        /// </div>
        /// <h2>Passar no Header:</h2>
        /// <div style="font-size: 15px">
        ///     Content-Type: application/json<br/>
        ///     Authorization: bearer &lt;token&gt;<br/><br/>
        ///     <b>OBS:</b> Altere o &lt;token&gt; pelo Token gerado na autenticação
        /// </div>
        ///
        /// <h2>Propriedades: </h2>
        /// <div style="font-size: 15px">
        ///     <b>taesaId:</b> (inteiro) ID do item gerado pela Taesa<hr/>
        ///     <b>email:</b> (string) E-mail do usuário responsável por aprovar ou reprovar o item<hr/>
        ///     <b>title:</b> (string) Título do item<hr/>
        ///     <b>content:</b> (string) Conteúdo no formato HTML. Tags aceitas: &lt;br&gt;, &lt;a&gt;, &lt;b&gt;, &lt;p&gt;.
        ///     <b>OBS: </b> Não usar <b>target</b> nos links<hr/>    
        ///     <b>sourceSystem:</b> (string) Sistema de origem<hr/>
        ///     <b>dateLimit:</b> (string) Data limite da aprovação (YYYY-MM-DD)<hr/>
        ///     <b>approvers:</b> (string) Todos os aprovadores<hr/>
        ///     <b>approvalType:</b> (string) Tipo de aprovação
        /// </div>
        /// </remarks>
        [Permissions("System")]
        [HttpPut("{taesaId}")]
        public ActionResult<Item> UpdateItem(int taesaId, [FromBody] Item item)
        {
            if(item == null || item.TaesaId != taesaId){
                return BadRequest();
            }

            var _item = _context.Items.Include("User")
                            .Where(i => i.TaesaId == taesaId && i.Status == true).FirstOrDefault();

            if(_item == null){
                return NotFound();
            }

            if(!_item.User.Email.Equals(item.Email)){
                var user = _context.Users.Where(u => u.Email.Equals(item.Email)).FirstOrDefault();
                
                if(user == null){
                    user = new User {Email = item.Email};
                    _context.Users.Add(user);
                    _context.SaveChanges();
                }
                _item.UserId = user.Id;
            }

            _item.Email = item.Email;
            _item.Title = item.Title;
            _item.Content = item.Content;
            _item.Approvers = item.Approvers;
            _item.ApprovalType = item.ApprovalType;
            _item.SourceSystem = item.SourceSystem;
            _item.DateLimit = item.DateLimit;
            _item.UpdateAt = DateTime.Now; 

            _context.Items.Update(_item);
            _context.SaveChanges();

            return Ok(_item);
        }

        /// <summary>
        /// (Deletar Item)
        /// </summary>
        /// <remarks>
        /// <h2>Deletar Item:</h2>
        /// <div style="font-size: 15px">
        ///     Enviar uma requisição <b>DELETE</b> para <b>https://taesaaprovador.azurewebsites.net/api/item/{taesaId}</b><br/><br/>
        ///     <b>OBS:</b> Altere o {taesaId} pelo id do item que deseja deletar
        /// </div>
        /// <h2>Passar no Header:</h2>
        /// <div style="font-size: 15px">
        ///     Content-Type: application/json<br/>
        ///     Authorization: bearer &lt;token&gt;<br/><br/>
        ///     <b>OBS:</b> Altere o &lt;token&gt; pelo Token gerado na autenticação
        /// </div>
        /// </remarks>
        [Permissions("System")]
        [HttpDelete("{taesaId}")]
        public ActionResult DeleteItem(int taesaId)
        {   
            var item = _context.Items.Where(i => i.TaesaId == taesaId && i.Status == true).FirstOrDefault();

            if(item == null){
                return NotFound();
            }

            item.Status = false;
            _context.Items.Update(item);
            _context.SaveChanges();

            return NoContent();
        }
    }
}