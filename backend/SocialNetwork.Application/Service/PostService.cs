using AutoMapper;
using Microsoft.Extensions.Logging;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;
using SocialNetwork.Infrastructure.Repos;

namespace SocialNetwork.Application.Service
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PostService> _logger;

        public PostService(IPostRepository postRepository,
            IUserRepository userRepository, 
            IMapper mapper,
            ILogger<PostService> logger) 
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task BanPost(Guid id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if(post == null)
            {
                _logger.LogWarning("Attempted to ban a post that does not exist: {PostId}", id);
                throw new ArgumentException("Post not found");
            }
            post.IsBanned = true;
            await _postRepository.UpdateAsync(post);
        }

        public async Task CreateAsync(CreatePostDto createPostDto)
        {
            if (createPostDto == null)
                throw new ArgumentNullException("postDTO is null");

            var post = _mapper.Map<Post>(createPostDto);

            if (post == null)
                throw new InvalidOperationException("Mapping failed");

            if(await _userRepository.GetByIdAsync(post.UserId) == null)
                throw new ArgumentException("User not found");

            await _postRepository.CreateAsync(post);
            _logger.LogInformation("Post created successfully. AuthorId: {UserId}", post.UserId);
        }

        public async Task<IEnumerable<PostDto>> GetAllAsync()
        {
            var posts = await _postRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<PostDto>>(posts);
        }

        public async Task<PostDto?> GetByIdAsync(Guid id)
        {
            var post = await _postRepository.GetByIdAsync(id);

            return _mapper.Map<PostDto?>(post);
        }

        public Task<IEnumerable<PostDto>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate)//TODO
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PostDto>> GetPostsByTagAsync(string tag)//TODO
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<PostDto>> GetPostsByUserIdAsync(Guid userId) 
        {
            var posts = await _postRepository.GetPostsByUserIdAsync(userId);

            return _mapper.Map<IEnumerable<PostDto>>(posts);
        }
    }
}
