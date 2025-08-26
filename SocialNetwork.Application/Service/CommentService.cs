using AutoMapper;
using Microsoft.Extensions.Logging;
using SocialNetwork.Application.DTO;
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

        public CommentService(ICommentRepository commentRepository, 
            IMapper mapper, 
            ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task BanComment(Guid id)
        {
            var post = await _commentRepository.GetByIdAsync(id);
            if (post == null)
            {
                throw new ArgumentException("Comment not found");
            }
            post.IsBanned = true;
            await _commentRepository.UpdateAsync(post);
        }

        public async Task CreateAsync(CreateCommentDto commentDto)
        {
            if (commentDto == null)
                throw new ArgumentNullException("commentDTO is null");

            var comment = _mapper.Map<Comment>(commentDto);

            if (comment == null)
                throw new InvalidOperationException("Mapping failed");

            await _commentRepository.CreateAsync(comment);

            _logger.LogInformation("Comment created successfully. AuthorId: {UserId}", comment.AuthorId);
        }

        public Task<IEnumerable<Comment>> GetAllAsync()
        {
            return _commentRepository.GetAllAsync();
        }

        public Task<Comment?> GetByIdAsync(Guid id)
        {
            return _commentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Comment>> GetPostCommentsAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid post ID");

            return await _commentRepository.GetPostCommentsAsync(id);
        }
    }
}
