using Models;
using Models.DBTables;
using Requests;
using Responses;

namespace Interfaces;
public interface IMessageRepository
{
    public Task<ResponseModel<PaginatedListModel<MessageResponse>>> GetAllMessagesAsync(string token);
    public Task<ResponseModel<MessageResponse>> GetMessageAsync(string token, string id);
    public Task<ResponseModel<PaginatedListModel<MessageResponse>>> GetAllMessagesAsync();
    public Task<ResponseModel<MessageResponse>> GetMessageAsync(string id);
    public Task<ResponseModel<bool>> AddMessageAsync(string token, AddMessageRequest request);

}
