namespace MvcProject.Models.Model
{
    public class Token
    {
        public string? UserId {  get; set; }
        public Guid PublicToken { get; set; }
        public bool PublicIsValid { get; set; }

        public Guid PrivateToken {  get; set; }
        public bool PrivateIsValid {  get; set; }
    }
}
