using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Application.Events
{
    public record PostCreatedEvent(
        Guid PostId, 
        Guid AuthorId,  
        DateTime CreatedAt) :INotification;
}
