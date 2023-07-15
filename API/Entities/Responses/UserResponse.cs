namespace API.Entities.Responses
{
    public record UserResponse(
        string Username, 
        string Token,
        string KnownAs,
        string? PhotoUrl,
        string Gender); 
}