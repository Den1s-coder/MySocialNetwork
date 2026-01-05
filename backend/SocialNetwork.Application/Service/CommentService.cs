using AutoMapper;
using Microsoft.Extensions.Logging;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Events;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Interfaces;
using SocialNetwork.Infrastructure.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Service
{
    public class CommentService : ICommentService
    {

        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentService> _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IUserRepository _userRepository;

        public CommentService(ICommentRepository commentRepository, 
            IMapper mapper,
            ILogger<CommentService> logger,
            IEventDispatcher eventDispatcher,
            IUserRepository userRepository)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
            _logger = logger;
            _eventDispatcher = eventDispatcher;
            _userRepository = userRepository;
        }

        public async Task BanComment(Guid id, CancellationToken cancellationToken = default)
        {
            var post = await _commentRepository.GetByIdAsync(id);
            if (post == null)
            {
                throw new ArgumentException("Comment not found");
            }
            post.IsBanned = true;

            await _commentRepository.UpdateAsync(post, cancellationToken);
        }

        public async Task CreateAsync(CreateCommentDto createCommentDto, CancellationToken cancellationToken = default)
        {
            if (createCommentDto == null)
                throw new ArgumentNullException("commentDTO is null");

            var text = (createCommentDto.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Comment text cannot be empty", nameof(createCommentDto));
            if (text.Length > 1000)
                throw new ArgumentException("Comment text is too long", nameof(createCommentDto));

            var comment = _mapper.Map<Comment>(createCommentDto);

            if (comment == null)
                throw new InvalidOperationException("Mapping failed");

            var user = await _userRepository.GetByIdAsync(comment.AuthorId, cancellationToken);
            if (user == null)
                throw new ArgumentException("User not found");

            if (user.IsBanned)
                throw new InvalidOperationException("Banned users cannot create comments.");

            await _commentRepository.CreateAsync(comment, cancellationToken);

            var evt = new CommentCreatedEvent
            (
                comment.Id,
                comment.AuthorId,
                comment.PostId,
                comment.CreatedAt
            );

            await _eventDispatcher.DispatchAsync(evt, cancellationToken);
        }

        public async Task<IEnumerable<CommentDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var comments = await _commentRepository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task<CommentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);

            return _mapper.Map<CommentDto?>(comment);
        }

        public async Task<IEnumerable<CommentDto>> GetPostCommentsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid post ID");

            var comments = await _commentRepository.GetPostCommentsAsync(id, cancellationToken);

            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }
    }
}
