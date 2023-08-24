using API.Entities;
using API.Entities.DTOs;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext dbContext;
        private readonly IMapper mapper;

        public MessageRepository(DataContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public void AddGroup(Group group)
        {
            ArgumentNullException.ThrowIfNull(group, nameof(group));
            this.dbContext.Groups.Add(group);
        }


        public void AddMessage(Message message)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));
            this.dbContext.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));
            this.dbContext.Messages.Remove(message);
        }

        public async Task<Connection> GetConnectionAsync(string connectionId)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionId, nameof(connectionId));
            return await this.dbContext.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnectionAsync(string connectionId)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionId, nameof(connectionId));

            return await this.dbContext.Groups
                .Include(x => x.Connections)
                .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }


        public async Task<Message> GetMessageAsync(int id)
        {
            return await this.dbContext.Messages.FindAsync(id);
        }

        public async Task<Group> GetMessageGroupAsync(string groupName)
        {
            ArgumentException.ThrowIfNullOrEmpty(groupName, nameof(groupName));

            return await this.dbContext.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }


        public async Task<PaginationList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams)
        {
            ArgumentNullException.ThrowIfNull(messageParams, nameof(messageParams));

            var query = this.dbContext.Messages
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username
                        && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username
                        && u.SenderDeleted == false),
                _ => query.Where(u => u.Recipient.UserName == messageParams.Username
                        && u.RecipientDeleted == false && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(this.mapper.ConfigurationProvider);
            return await PaginationList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUserName, string recipientUserName)
        {
            ArgumentException.ThrowIfNullOrEmpty(currentUserName, nameof(currentUserName));
            ArgumentException.ThrowIfNullOrEmpty(recipientUserName, nameof(recipientUserName));

            var query = this.dbContext.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(
                    m => m.RecipientUsername == currentUserName && m.RecipientDeleted == false &&
                    m.SenderUsername == recipientUserName ||
                    m.RecipientUsername == recipientUserName && m.SenderDeleted == false &&
                    m.SenderUsername == currentUserName
            )
            .OrderBy(m => m.MessageSent)
            .AsQueryable();

            var unreadMessages = query
                .Where(m => m.DateRead == null
                         && m.RecipientUsername == currentUserName)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            return await query
                .ProjectTo<MessageDto>(this.mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            ArgumentNullException.ThrowIfNull(connection, nameof(connection));
            this.dbContext.Connections.Remove(connection);
        }
    }
}