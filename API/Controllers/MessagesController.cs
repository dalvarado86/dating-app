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
        private readonly IUserRepository userRepository;
        private readonly IMessageRepository messageRepository;
        private readonly IMapper mapper;

        public MessagesController(
            IUserRepository userRepository,
            IMessageRepository messageRepository, 
            IMapper mapper)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
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

            var sender = await this.userRepository.GetUserByUsernameAsync(username);
            var recipient = await this.userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) 
            {
                return NotFound();
            }

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.Username,
                RecipientUsername = recipient.Username,
                Content = createMessageDto.Content
            };

            this.messageRepository.AddMessage(message);

            if (await this.messageRepository.SaveAllAsync()) 
            {
                return Ok(this.mapper.Map<MessageDto>(message));
            }

            return BadRequest("Failed to send message.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await this.messageRepository.GetMessagesForUserAsync(messageParams);

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

            return Ok(await this.messageRepository.GetMessageThreadAsync(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();

            var message = await this.messageRepository.GetMessageAsync(id);

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
                this.messageRepository.DeleteMessage(message);
            }

            if (await this.messageRepository.SaveAllAsync()) 
            {
                return Ok();
            }

            return BadRequest("Problem deleting the message.");
        }
    }
}