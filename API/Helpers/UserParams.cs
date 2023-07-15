namespace API.Helpers
{
    public class UserParams
    {
        public int PageNumber { get; set; } = 1;
        
        public int PageSize
        {
            get => DefaultPageSize;
            set => DefaultPageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public string? CurrentUsername { get; set; }
        public string? Gender { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 100;
        public string OrderBy { get; set; } = "lastActive";
        private const int MaxPageSize = 50;
        private int DefaultPageSize = 10;
    }
}