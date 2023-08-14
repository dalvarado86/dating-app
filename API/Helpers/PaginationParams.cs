namespace API.Helpers
{
    public class PaginationParams
    {
        public int PageNumber { get; set; } = 1;
        
        public int PageSize
        {
            get => DefaultPageSize;
            set => DefaultPageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        private const int MaxPageSize = 50;
        private int DefaultPageSize = 10;
    }
}