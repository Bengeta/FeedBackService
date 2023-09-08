using Models;
using Models.DBTables;
using Requests;

namespace Interfaces;
public interface IMessageRepository
{
    public Task<ResponseModel<List<MessageModel>>> GetAllMessagesAsync(string token);
    public Task<ResponseModel<MessageModel>> GetMessageAsync(string token, string id);
    public Task<ResponseModel<bool>> AddMessageAsync(string token, AddMessageRequest request);

}
