namespace API.Entities.Requests
{
    public record LoginRequest(
        string Username, 
        string Password);
}