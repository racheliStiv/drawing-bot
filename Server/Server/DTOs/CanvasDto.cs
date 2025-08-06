namespace Server.DTOs
{
    //משמש עבור GET לקנבס
    public class CanvasDto
    {
        public int Id { get; set; }
        //public string Name { get; set; } = string.Empty;
        public List<DrawingInCanvasDto> Drawings { get; set; } = new();
    }
}
