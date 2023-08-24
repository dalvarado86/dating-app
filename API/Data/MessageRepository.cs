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

        public void AddMessage(Message message)
        {
            this.dbContext.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this.dbContext.Messages.Remove(message);
        }

        public async Task<Message> GetMessageAsync(int id)
        {
            return await this.dbContext.Messages.FindAsync(id);
        }

        public async Task<PaginationList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams)
        {
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

        public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername)
        {
            var messages = await this.dbContext.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(
                    m => m.RecipientUsername == currentUsername
                        && m.RecipientDeleted == false 
                        && m.SenderUsername == recipientUsername 
                        || m.RecipientUsername == recipientUsername 
                        && m.SenderUsername == currentUsername
                        && m.SenderDeleted == false
            )
            .OrderBy(m => m.MessageSent)
            .ToListAsync();

            var unreadMessages = messages
                .Where(m => m.DateRead == null
                    && m.RecipientUsername == currentUsername)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }

                await this.dbContext.SaveChangesAsync();
            }

            return this.mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
           return await this.dbContext.SaveChangesAsync() > 0;
        }
    }
}