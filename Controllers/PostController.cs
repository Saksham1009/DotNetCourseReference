using System.ComponentModel.DataAnnotations;
using DotnetAPI.Data;
using DotnetAPI.DTOs;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controller
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        public PostController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
        }

        [HttpGet("GetPosts")]
        public IEnumerable<Post> GetPosts()
        {
            string sql = @"SELECT [PostId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated] FROM TutorialAppSchema.Posts;";
            IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);
            return posts;
        }

        [HttpGet("GetPostsByUser/{userId}")]
        public IEnumerable<Post> GetPostsByUser(int userId)
        {
            string sql = @"SELECT [PostId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated] FROM TutorialAppSchema.Posts
                WHERE UserId = " + userId.ToString() + ";";
            IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);

            if (posts == null)
            {
                throw new Exception("No posts from this user yet");
            }

            return posts;
        }

        [HttpGet("GetPostById/{postId}")]
        public Post GetPostById(int postId)
        {
            string sql = @"SELECT [PostId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated] FROM TutorialAppSchema.Posts
                WHERE PostId = " + postId.ToString() + ";";
            Post post = _dapper.LoadDataSingle<Post>(sql);

            if (post == null)
            {
                throw new Exception("Post with the given ID does not exits");
            }

            return post;
        }

        [HttpGet("GetMyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"SELECT [PostId],
                [UserId],
                [PostTitle],
                [PostContent],  
                [PostCreated],
                [PostUpdated] FROM TutorialAppSchema.Posts
                WHERE UserId = " + this.User.FindFirst("userId")?.Value.ToString() + ";";

            IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);

            return posts;
        }

        [HttpPost("AddPost")]
        public IActionResult AddPost(PostToAddDTO postToAdd)
        {
            string sql = @"INSERT INTO TutorialAppSchema.Posts ([UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]) VALUES ( " + this.User.FindFirst("userId")?.Value.ToString() + 
                ", '" + postToAdd.PostTitle + "', '" + postToAdd.PostContent + "', GETDATE(), GETDATE() );";
            if (!_dapper.ExecuteSql(sql))
            {
                throw new Exception("Could not add a new Post");
            }

            return Ok();
        }

        [HttpPut("EditPost")]
        public IActionResult EditPost(PostToEditDTO postToEdit)
        {
            string sql = @"
            UPDATE TutorialAppSchema.Posts
                SET [PostTitle] = '" + postToEdit.PostTitle + "'," +
                " [PostContent] = '" + postToEdit.PostContent + "'," +
                " [PostUpdated] = GETDATE() WHERE PostId = " + postToEdit.PostId.ToString() +
                " AND USERID = " + this.User.FindFirst("userId")?.Value + ";";
            if (!_dapper.ExecuteSql(sql))
            {
                throw new Exception("Could not edit the Post");
            }

            return Ok();
        }

        [HttpDelete("DeletePost/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"DELETE FROM TutorialAppSchema.Posts 
                WHERE postId = " + postId.ToString() +
                "AND UserId = " + this.User.FindFirst("userId")?.Value + ";";
            
            if (!_dapper.ExecuteSql(sql))
            {
                throw new Exception("Could not delete the post");
            }

            return Ok();
        }

        [HttpGet("SearchTitles/{search}")]
        public IEnumerable<Post> SearchTitles(string search)
        {
            string sql = @"SELECT [PostId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated] FROM TutorialAppSchema.Posts
                WHERE PostTitle LIKE '%" + search + "%';";

            return _dapper.LoadData<Post>(sql);
        }
    }
}