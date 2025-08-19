using AutoMapper;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;
using SocialNetwork.Infrastructure.Repos;

namespace SocialNetwork.Application.Service
{
    public class PostService : IPostService
    {
        private readonly IGenerycRepository<Post> _postRepository;
        private readonly IMapper _mapper;
        public PostService(IGenerycRepository<Post> postRepository, IMapper mapper) 
        {
            _postRepository = postRepository;
            _mapper = mapper;
        }
        public async Task BanPost(Guid id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if(post == null)
            {
                throw new ArgumentException("Post not found");
            }
            post.IsBanned = true;
            await _postRepository.UpdateAsync(post);
        }

        public async Task CreateAsync(CreatePostDto postDto)
        {
            if (postDto == null)
                throw new ArgumentNullException("postDTO is null");

            var post = _mapper.Map<Post>(postDto);

            if (post == null)
                throw new InvalidOperationException("Mapping failed");

            await _postRepository.CreateAsync(post);
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _postRepository.GetAllAsync();
        }

        public Task<Post?> GetByIdAsync(Guid id)
        {
            return _postRepository.GetByIdAsync(id);
        }

        //TODO: Implement these methods

        public Task<IEnumerable<Post>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate)//TODO
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetPostsByTagAsync(string tag)//TODO
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId) //TODO
        {
            throw new NotImplementedException();
        }
    }
}
