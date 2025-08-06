namespace Server.DTOs
{
    //משמש עבור GET לציור בקנבס

    public class DrawingInCanvasDto
    {
        public int Id { get; set; }

        public int CanvasId { get; set; }

        public string DrawingAttributes { get; set; } = string.Empty;
    }
}
