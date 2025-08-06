using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class DrawingInCanvas
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Canvas")]
        public int CanvasId { get; set; }
        //תכונת עבור השכבה הלוגית - מציין את הקשר בין קנבס לציור ומשמש בעיקר עבור קנבס שיוכל להציג את הציורים שבתוכו
        public Canvas Canvas { get; set; }

        [Required]
        public string DrawingAttributes { get; set; } = string.Empty;
    }
}
