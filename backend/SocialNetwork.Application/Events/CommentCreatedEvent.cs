using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Events
{
    public record CommentCreatedEvent(
        Guid PostId,
        Guid CommentId,
        Guid AuthorId,
        DateTime CreatedAt) :INotification;
}
