using AutoMapper;
using SocialNetwork.Application.DTO;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Domain.Enums;
using SocialNetwork.Domain.Interfaces;

namespace SocialNetwork.Application.Service
{
    public class FriendService : IFriendService
    {
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public FriendService(IUserRepository userRepository, IFriendshipRepository friendshipRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _friendshipRepository = friendshipRepository;
            _mapper = mapper;
        }

        public Task AcceptFriendRequest(Guid requestId, CancellationToken cancellationToken = default)
        {
            var friendship =  _friendshipRepository.GetByIdAsync(requestId);
            if (friendship == null)
                throw new Exception("Friend request not found");

            if (friendship.Result.Status != FriendshipStatus.Pending)
                throw new Exception("Friend request is not pending");

            friendship.Result.Status = FriendshipStatus.Accepted;
            return _friendshipRepository.UpdateAsync(friendship.Result);
        }

        public async Task DeclineFriendRequest(Guid requestId, CancellationToken cancellationToken = default)
        {
            var friendship = await _friendshipRepository.GetByIdAsync(requestId);
            if (friendship == null)
                throw new Exception("Friend request not found");

            if (friendship.Status != FriendshipStatus.Pending)
                throw new Exception("Friend request is not pending");

            await _friendshipRepository.DeleteAsync(requestId);
        }

        public Task<IEnumerable<Friendship>> GetAllFriends(CancellationToken cancellationToken = default)
        {
            return  _friendshipRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Friendship>> GetFriendsOfUser(Guid userId, CancellationToken cancellationToken = default)
        {
            var friendships =  await _friendshipRepository.GetUserFriendshipsAsync(userId);
            return friendships
                .Where(f => f.Status == FriendshipStatus.Accepted)
                .ToList();

        }

        public async Task<IEnumerable<FriendRequestDto>> GetPendingFriendRequests(Guid userId, CancellationToken cancellationToken = default)
        {
            var friendships = await _friendshipRepository.GetUserFriendshipsAsync(userId);
            var pendingRequests = friendships
                .Where(f => f.Status == FriendshipStatus.Pending && f.AddresseeId == userId)
                .ToList();

            return _mapper.Map<IEnumerable<FriendRequestDto>>(pendingRequests);
        }

        public async Task RemoveFriend(Guid UserId, Guid friendId)
        {
            var friend = await _userRepository.GetByIdAsync(friendId);
            if (friend == null)
                throw new Exception("Friend not found");

            var AreFriends = await _friendshipRepository.AreFriendsAsync(UserId, friendId);
            if (AreFriends)
                throw new Exception("Users are not friends");

            var friendships = await _friendshipRepository.GetUserFriendshipsAsync(UserId);
            var friendship = friendships.FirstOrDefault(f =>
                (f.RequesterId == UserId && f.AddresseeId == friendId) ||
                (f.RequesterId == friendId && f.AddresseeId == UserId));

            if (friendship == null)
                throw new Exception("Friendship not found");

            await _friendshipRepository.DeleteAsync(friendship.Id);
 
        }

        public async Task SendFriendRequest(FriendRequestDto request, CancellationToken cancellationToken = default)
        {
            var receiver = await _userRepository.GetByIdAsync(request.ReceiverId);
            if (receiver == null)
                throw new Exception("Reciever not found");

            var adresssee = await _userRepository.GetByIdAsync(request.RequesterId);
            if (adresssee == null)
                throw new Exception("Adresssee not found");

            var AreFriends = await _friendshipRepository.AreFriendsAsync(request.RequesterId, request.ReceiverId);
            if (AreFriends)
                throw new Exception("Users are already friends");

            var friendship = _mapper.Map<Friendship>(request);

            friendship.Status = FriendshipStatus.Pending;

            await _friendshipRepository.CreateAsync(friendship);
        }
    }
}
