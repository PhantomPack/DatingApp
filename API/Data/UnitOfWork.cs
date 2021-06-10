using System.Threading.Tasks;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using API.Entities;
using Microsoft.AspNetCore.Http;
namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPhotoRepository _photoRepository;
             private readonly IVisitsRepository _visitsRepository;
        public UnitOfWork(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, IPhotoRepository photoRepository,
        IVisitsRepository visitsRepository)
        {
            _visitsRepository = visitsRepository;
            _photoRepository = photoRepository;
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

    public IUserRepository UserRepository => new UserRepository(_context, _mapper, _httpContextAccessor, _photoRepository);

    public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);

    public ILikesRepository LikesRepository => new LikesRepository(_context);
public IVisitsRepository VisitsRepository => new VisitsRepository(_context);
    public async Task<bool> Complete()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
        return _context.ChangeTracker.HasChanges();
    }
}
}

