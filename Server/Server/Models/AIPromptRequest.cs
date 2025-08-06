namespace Server.Models
{
    public class AIPromptRequest
    {
        public string Prompt { get; set; }  

        // ג׳ייסון שמכיל את הציורים הקיימים על הקנבס – במידה וקיים
        public string? ExistingDrawingsJson { get; set; }
    }
}
