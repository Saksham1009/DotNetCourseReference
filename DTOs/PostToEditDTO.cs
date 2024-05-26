namespace DotnetAPI.DTOs
{
    public class PostToEditDTO
    {
        public int PostId {get; set;}
        public string PostTitle {get; set;}
        public string PostContent {get; set;}
        public PostToEditDTO()
        {
            if (PostTitle == null)
            {
                PostTitle = "";
            }
            if (PostContent == null)
            {
                PostContent = "";
            }
        }
    }
}