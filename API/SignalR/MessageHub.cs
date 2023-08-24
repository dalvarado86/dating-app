using API.Entities;
using API.Entities.DTOs;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IHubContext<PresenceHub> presenceHub;

        public MessageHub(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHubContext<PresenceHub> presenceHub)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.presenceHub = presenceHub ?? throw new ArgumentNullException(nameof(presenceHub));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await this.unitOfWork.MessageRepository
                .GetMessageThreadAsync(Context.User.GetUsername(), otherUser);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var group = await RemoveFromMessageGroup();

            await Clients
                .Group(group.Name)
                .SendAsync("UpdatedGroup");

            await base.OnDisconnectedAsync(ex);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower())
            {
                throw new HubException("You cannot send messages to yourself");
            }

            var sender = await this.unitOfWork.UserRepository
                .GetUserByUsernameAsync(username);

            var recipient = await this.unitOfWork.UserRepository
                .GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null)
            {
                throw new HubException("Not found user");
            }

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await this.unitOfWork.MessageRepository
                .GetMessageGroupAsync(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker
                    .GetConnectionsForUser(recipient.UserName);

                if (connections != null)
                {
                    await this.presenceHub.Clients
                        .Clients(connections)
                        .SendAsync(
                            "NewMessageReceived",
                            new
                            {
                                username = sender.UserName,
                                knownAs = sender.KnownAs
                            });
                }
            }

            this.unitOfWork.MessageRepository.AddMessage(message);

            if (await this.unitOfWork.Complete())
            {
                await Clients
                    .Group(groupName)
                    .SendAsync("NewMessage", this.mapper.Map<MessageDto>(message));
            }
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;

            return stringCompare
                ? $"{caller}-{other}"
                : $"{other}-{caller}";
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await this.unitOfWork.MessageRepository.GetMessageGroupAsync(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                this.unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await this.unitOfWork.Complete())
            {
                return group;
            }

            throw new HubException("Failed to add to group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await this.unitOfWork.MessageRepository.GetGroupForConnectionAsync(Context.ConnectionId);
            var connection = group.Connections
                .FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            this.unitOfWork.MessageRepository.RemoveConnection(connection);

            if (await this.unitOfWork.Complete())
            {
                return group;
            }

            throw new HubException("Failed to remove from group");
        }
    }
}