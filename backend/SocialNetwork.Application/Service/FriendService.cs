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

        public Task AcceptFriendRequest(Guid requestId)
        {
            throw new NotImplementedException();
        }

        public Task DeclineFriendRequest(Guid requestId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Friendship>> GetAllFriends()
        {
            return  _friendshipRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Friendship>> GetFriendsOfUser(Guid userId)
        {
            var friendships =  await _friendshipRepository.GetUserFriendshipsAsync(userId);
            return friendships
                .Where(f => f.Status == FriendshipStatus.Accepted)
                .ToList();

        }

        public async Task<IEnumerable<FriendRequestDto>> GetPendingFriendRequests(Guid userId)
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

            // TODO: Logic to remove friendship goes here
        }

        public async Task SendFriendRequest(FriendRequestDto request)
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
