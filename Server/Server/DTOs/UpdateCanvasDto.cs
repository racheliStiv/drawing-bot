namespace Server.DTOs
{
    //משמש עבור PUT לקנבס
    public class UpdateCanvasDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
