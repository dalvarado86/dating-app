namespace API.Entities
{
    public class Connection
    {
        public Connection()
        {
        }

        public Connection(string connectionId, string username)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionId, nameof(connectionId));
            ArgumentException.ThrowIfNullOrEmpty(username, nameof(username));

            ConnectionId = connectionId;
            Username = username;
        }

        public string ConnectionId { get; set; }
        public string Username { get; set; }
    }
}