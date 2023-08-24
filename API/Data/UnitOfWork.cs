using API.Interfaces;
using AutoMapper;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IMapper mapper;
        private readonly DataContext dbContext;

        public UnitOfWork(DataContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public IUserRepository UserRepository => new UserRepository(this.dbContext, this.mapper);

        public IMessageRepository MessageRepository => new MessageRepository(this.dbContext, this.mapper);

        public ILikesRepository LikesRepository => new LikesRepository(this.dbContext);

        public async Task<bool> Complete()
        {
            return await this.dbContext.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return this.dbContext.ChangeTracker.HasChanges();
        }
    }
}