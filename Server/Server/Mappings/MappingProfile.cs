using AutoMapper;
using Server.DTOs;
using Server.Models;

namespace Server.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Canvas
            CreateMap<Canvas, CanvasDto>().ReverseMap();
            CreateMap<CreateCanvasDto, Canvas>();
            CreateMap<UpdateCanvasDto, Canvas>();

            // DrawingInCanvas
            CreateMap<DrawingInCanvas, DrawingInCanvasDto>().ReverseMap();
            CreateMap<CreateDrawingInCanvasDto, DrawingInCanvas>();
            CreateMap<UpdateDrawingInCanvasDto, DrawingInCanvas>();

            //All canvases 
            CreateMap<Canvas, CanvasSummaryDto>();

        }
    }
}
