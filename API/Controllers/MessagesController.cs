using API.Entities;
using API.Entities.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public MessagesController(
            IUnitOfWork unitOfWork, 
            IMapper mapper)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) 
            {
                return BadRequest("You cannot send messages to yourself.");
            }

            var sender = await this.unitOfWork.UserRepository
                .GetUserByUsernameAsync(username);
            
            var recipient = await this.unitOfWork.UserRepository
                .GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) 
            {
                return NotFound();
            }

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            this.unitOfWork.MessageRepository.AddMessage(message);

            if (await this.unitOfWork.Complete()) 
            {
                return Ok(this.mapper.Map<MessageDto>(message));
            }

            return BadRequest("Failed to send message.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await this.unitOfWork.MessageRepository
                .GetMessagesForUserAsync(messageParams);

            Response.AddPaginationHeader(
                new PaginationHeader(
                    messages.CurrentPage,
                    messages.PageSize, 
                    messages.TotalCount,
                    messages.TotalPages));

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();

            return Ok(await this.unitOfWork.MessageRepository
                .GetMessageThreadAsync(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();

            var message = await this.unitOfWork.MessageRepository
                .GetMessageAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            if (message.SenderUsername != username && message.RecipientUsername != username) 
            {
                return Unauthorized();
            }

            if (message.SenderUsername == username) 
            {
                message.SenderDeleted = true;
            }

            if (message.RecipientUsername == username) 
            {
                message.RecipientDeleted = true;
            }

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                this.unitOfWork.MessageRepository.DeleteMessage(message);
            }

            if (await this.unitOfWork.Complete()) 
            {
                return Ok();
            }

            return BadRequest("Problem deleting the message.");
        }
    }
}