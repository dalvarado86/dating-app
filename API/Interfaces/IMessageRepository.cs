using API.Entities;
using API.Entities.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessageAsync(int id);
        Task<PaginationList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUserName, string recipientUserName);
        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection> GetConnectionAsync(string connectionId);
        Task<Group> GetMessageGroupAsync(string groupName);
        Task<Group> GetGroupForConnectionAsync(string connectionId);
    }
}