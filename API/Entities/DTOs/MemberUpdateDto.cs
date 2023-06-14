namespace API.Entities.DTOs
{
    public record MemberUpdateDto
    {
        public string Introduction { get; init; }
        public string LookingFor { get; init; }
        public string Interests { get; init; }
        public string City { get; init; }
        public string Country { get; init; }
    }
}