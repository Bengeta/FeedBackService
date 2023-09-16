using Feedback;
using HessLibrary.Models;
using Models.DBTables;
using Requests;
using Responses;

namespace Interfaces;
public interface IMessageRepository
{
    public Task<ResponseModel<PaginatedListModel<MessageResponse>>> GetAllMessagesAsync(string token, int page = 0, int pageSize = 10);
    public Task<ResponseModel<MessageResponse>> GetMessageAsync(string token, string id);
    public Task<ResponseModel<PaginatedListModel<MessageResponse>>> GetAllMessagesAsync(int page = 0, int pageSize = 10);
    public Task<ResponseModel<MessageResponse>> GetMessageAsync(string id);
    public Task<ResponseModel<bool>> AddMessageAsync(string token, AddMessageRequest request);
    public Task<ResponseModel<bool>> UpdateMessageAsync(AddAnswerRequestGrpc request);
}
