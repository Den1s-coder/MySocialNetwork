using SocialNetwork.mobile.Models;
using SocialNetwork.mobile.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.mobile.DTO;

namespace SocialNetwork.mobile.Services
{
    public class ApiDataStore : IDataStore<Item>
    {
        private readonly PostService _postService;

        public ApiDataStore()
        {
            _postService = new PostService();
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            var posts = await _postService.GetPostsAsync();
            return posts.Select(p => new Item { Id = p.Id, Text = p.Text, Description = p.UserName });
        }

        public async Task<Item> GetItemAsync(string id)
        {
            var p = await _postService.GetPostAsync(id);
            if (p == null) return null;
            return new Item { Id = p.Id.ToString(), Text = p.Text, Description = p.UserName };
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            return await _postService.CreatePostAsync(item.Text);
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            return await _postService.DeletePostAsync(id);
        }

        public async Task<IEnumerable<Post>> GetPostsAsync(bool forceRefresh = false)
        {
            return await _postService.GetPostsAsync(forceRefresh);
        }

        public async Task<Post> GetPostAsync(string id)
        {
            return await _postService.GetPostAsync(id);
        }

        public async Task<bool> CreateCommentAsync(string postId, string text)
        {
            return await _postService.CreateCommentAsync(postId, text);
        }

        public async Task<IEnumerable<Post>> GetMyPostsAsync()
        {
            return await _postService.GetMyPostsAsync();
        }
    }
}
