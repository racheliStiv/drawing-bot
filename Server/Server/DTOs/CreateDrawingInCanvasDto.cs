namespace Server.DTOs
{

    //משמש עבור POST לציור בקנבס
    public class CreateDrawingInCanvasDto
    {
        public int CanvasId { get; set; }
        public string DrawingAttributes { get; set; } = string.Empty;
    }
}
