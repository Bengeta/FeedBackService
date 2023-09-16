using AutoMapper;
using HessLibrary.Models;
using Models.DBTables;
using Responses;
using Requests;
using Feedback;


namespace Feedback.Utils
{
    public class AutoMappingProfiles : Profile
    {
        public AutoMappingProfiles()
        {
            CreateMap<AddMessageRequest, MessageModel>();
            CreateMap<MessageModel, MessageResponse>();
            CreateMap<MessageResponse, MessageGrpc>();

            CreateMap<PaginatedListModel<MessageResponse>, PaginatedListMessageGrpc>();
            
            CreateMap(typeof(PagedList<>), typeof(PaginatedListModel<>))
                .ConvertUsing(typeof(PagedListTypeConverter<>));
        }
    }
}