using HessBackend.Enums;
using Interfaces;
using Models;
using Models.DBTables;
using Utils;
using MongoDB.Driver;
using MongoDB.Bson;
using ServicesGrpc.ServiceSent;
using Requests;

namespace Repository;
public class MessageRepository : IMessageRepository
{
    private readonly IMongoCollection<MessageModel> _MessagesCollection;
    private readonly ServicesGrpc.ServiceSent.OrderService _orderService;
    private readonly UserService _userService;

    public MessageRepository(IMongoDatabase database, ServicesGrpc.ServiceSent.OrderService orderService, UserService userService)
    {
        _orderService = orderService;
        _userService = userService;
        _MessagesCollection = database.GetCollection<MessageModel>("Messages");
    }

    public async Task<ResponseModel<List<MessageModel>>> GetAllMessagesAsync(string token)
    {
        try
        {
            var userResponse = await _userService.GetGetUserByToken(token);
            if (userResponse.ResultCode != ResultCode.Success)
                return new ResponseModel<List<MessageModel>> { ResultCode = ResultCode.UserNotFound };
            var filter = Builders<MessageModel>.Filter.Eq(x => x.UserId, userResponse.Data.Id);
            var document = await _MessagesCollection.Find(filter).ToListAsync();
            if (document == null)
                return new ResponseModel<List<MessageModel>> { ResultCode = ResultCode.Failed };
            return new ResponseModel<List<MessageModel>> { ResultCode = ResultCode.Success, Data = document };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<List<MessageModel>> { ResultCode = ResultCode.Failed };
        }
    }

    public async Task<ResponseModel<MessageModel>> GetMessageAsync(string token, string id)
    {
        try
        {
            var userResponse = await _userService.GetGetUserByToken(token);
            if (userResponse.ResultCode != ResultCode.Success)
                return new ResponseModel<MessageModel> { ResultCode = ResultCode.UserNotFound };
            var filter = Builders<MessageModel>.Filter.And(
                Builders<MessageModel>.Filter.Eq(x => x.UserId, userResponse.Data.Id),
                Builders<MessageModel>.Filter.Eq(x => x.Id, id)
                );

            var document = await _MessagesCollection.Find(filter).FirstOrDefaultAsync();
            if (document == null)
            {
                document = new MessageModel { UserId = userResponse.Data.Id };
                await _MessagesCollection.InsertOneAsync(document);
            }
            return new ResponseModel<MessageModel> { ResultCode = ResultCode.Success, Data = document };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<MessageModel> { ResultCode = ResultCode.Failed };
        }
    }

    public async Task<ResponseModel<bool>> AddMessageAsync(string token, AddMessageRequest request)
    {
        try
        {
            var userResponse = await _userService.GetGetUserByToken(token);
            if (userResponse.ResultCode != ResultCode.Success)
                return new ResponseModel<bool> { ResultCode = ResultCode.UserNotFound };
            var MessageModel = new MessageModel { UserId = userResponse.Data.Id, Message = request.Message };
            await _MessagesCollection.InsertOneAsync(MessageModel);
            return new ResponseModel<bool> { ResultCode = ResultCode.Success, Data = true };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<bool> { ResultCode = ResultCode.Failed };
        }
    }
}