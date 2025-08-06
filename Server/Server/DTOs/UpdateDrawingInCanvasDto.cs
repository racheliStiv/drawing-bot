namespace Server.DTOs
{

    //משמש עבור PUT לציור בקנבס
    public class UpdateDrawingInCanvasDto
    {
        public int Id { get; set; }
        public int CanvasId { get; set; }
        public string DrawingAttributes { get; set; } = string.Empty;
    }
}
