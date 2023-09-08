using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DBTables;
using Requests;

namespace Controllers.v1;
[ApiController]
[Route("api/")]
public class MessageController : BaseController
{
    private IMessageRepository _messageRepository;

    public MessageController(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    [HttpGet]
    [Route("messages")]
    public async Task<ResponseModel<List<MessageModel>>> GetMessages()
    {
        return await _messageRepository.GetAllMessagesAsync(Token());
    }
    [HttpGet]
    [Route("messages/{id}")]
    public async Task<ResponseModel<MessageModel>> GetMessage(string id)
    {
        return await _messageRepository.GetMessageAsync(Token(), id);
    }
    [HttpPost]
    [Route("messages")]
    public async Task<ResponseModel<bool>> AddMessage(AddMessageRequest request)
    {
        return await _messageRepository.AddMessageAsync(Token(), request);
    }
}
