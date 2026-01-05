using AutoMapper;
using Microsoft.Extensions.Logging;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Events;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;
using SocialNetwork.Infrastructure.Repos;
using static System.Net.Mime.MediaTypeNames;

namespace SocialNetwork.Application.Service
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PostService> _logger;
        private readonly IEventDispatcher _eventDispatcher;

        public PostService(IPostRepository postRepository,
            IUserRepository userRepository, 
            IMapper mapper,
            ILogger<PostService> logger,
            IEventDispatcher eventDispatcher)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _eventDispatcher = eventDispatcher;
        }
        public async Task BanPost(Guid id, CancellationToken cancellationToken = default)
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

        public async Task CreateAsync(CreatePostDto createPostDto, CancellationToken cancellationToken = default)
        {
            if (createPostDto == null)
                throw new ArgumentNullException("postDTO is null");

            var text = (createPostDto.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text)) 
                throw new ArgumentException("Post content cannot be empty.", nameof(createPostDto.Text));
            if (text.Length > 5000) 
                throw new ArgumentException("Post content is too long.", nameof(createPostDto.Text));

            var post = _mapper.Map<Post>(createPostDto);

            if (post == null)
                throw new InvalidOperationException("Mapping failed");

            var user = await _userRepository.GetByIdAsync(post.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            if(user.IsBanned)
                throw new InvalidOperationException("Banned users cannot create posts.");

            await _postRepository.CreateAsync(post);
            _logger.LogInformation("Post created successfully. AuthorId: {UserId}", post.UserId);

            var evt = new PostCreatedEvent
            (
                post.Id,
                post.UserId,
                post.CreatedAt
            );

            await _eventDispatcher.DispatchAsync(evt);
        }

        public async Task<IEnumerable<PostDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var posts = await _postRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<PostDto>>(posts);
        }

        public async Task<PostDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var post = await _postRepository.GetByIdAsync(id);

            return _mapper.Map<PostDto?>(post);
        }

        public Task<IEnumerable<PostDto>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)//TODO
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PostDto>> GetPostsByTagAsync(string tag, CancellationToken cancellationToken = default)//TODO
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<PostDto>> GetPostsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) 
        {
            var posts = await _postRepository.GetPostsByUserIdAsync(userId);

            return _mapper.Map<IEnumerable<PostDto>>(posts);
        }
    }
}
