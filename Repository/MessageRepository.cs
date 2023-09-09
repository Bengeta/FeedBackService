using Enums;
using Interfaces;
using Models;
using Models.DBTables;
using Utils;
using MongoDB.Driver;
using MongoDB.Bson;
using ServicesGrpc.ServiceSent;
using Requests;
using Responses;
using AutoMapper;
using Azure;

namespace Repository;
public class MessageRepository : IMessageRepository
{
    private readonly IMongoCollection<MessageModel> _MessagesCollection;
    private readonly ServicesGrpc.ServiceSent.OrderService _orderService;
    private readonly UserService _userService;
    private readonly IMapper _mapper;

    public MessageRepository(IMongoDatabase database, IMapper mapper, ServicesGrpc.ServiceSent.OrderService orderService, UserService userService)
    {
        _orderService = orderService;
        _userService = userService;
        _mapper = mapper;
        _MessagesCollection = database.GetCollection<MessageModel>("Messages");
    }

    public async Task<ResponseModel<PaginatedListModel<MessageResponse>>> GetAllMessagesAsync(string token, int page = 0, int pageSize = 10)
    {
        try
        {
            var userResponse = await _userService.GetGetUserByToken(token);
            if (userResponse.ResultCode != ResultCode.Success)
                return new ResponseModel<PaginatedListModel<MessageResponse>> { ResultCode = ResultCode.UserNotFound };
            var filter = Builders<MessageModel>.Filter.Eq(x => x.UserId, userResponse.Data.Id);
            return await GetMessages(filter, page, pageSize);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<PaginatedListModel<MessageResponse>> { ResultCode = ResultCode.Failed };
        }
    }
    public async Task<ResponseModel<PaginatedListModel<MessageResponse>>> GetAllMessagesAsync(int page = 0, int pageSize = 10)
    {
        try
        {
            var filter = Builders<MessageModel>.Filter.Empty;
            return await GetMessages(filter, page, pageSize);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<PaginatedListModel<MessageResponse>> { ResultCode = ResultCode.Failed };
        }
    }

    private async Task<ResponseModel<PaginatedListModel<MessageResponse>>> GetMessages(FilterDefinition<MessageModel>? filter, int page = 0, int pageSize = 10)
    {
        try
        {
            var documents = await _MessagesCollection.Find(filter).ToListAsync();
            if (documents == null)
                return new ResponseModel<PaginatedListModel<MessageResponse>> { ResultCode = ResultCode.Failed };

            var documentsMapped = _mapper.Map<List<MessageResponse>>(documents);
            var documentsPagged = PagedList<MessageResponse>.ToPagedList(documentsMapped, page, pageSize);
            var ans = _mapper.Map<PaginatedListModel<MessageResponse>>(documentsPagged);
            return new ResponseModel<PaginatedListModel<MessageResponse>> { ResultCode = ResultCode.Success, Data = ans };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<PaginatedListModel<MessageResponse>> { ResultCode = ResultCode.Failed };
        }
    }

    public async Task<ResponseModel<MessageResponse>> GetMessageAsync(string token, string id)
    {
        try
        {
            var userResponse = await _userService.GetGetUserByToken(token);
            if (userResponse.ResultCode != ResultCode.Success)
                return new ResponseModel<MessageResponse> { ResultCode = ResultCode.UserNotFound };
            var filter = Builders<MessageModel>.Filter.And(
                Builders<MessageModel>.Filter.Eq(x => x.UserId, userResponse.Data.Id),
                Builders<MessageModel>.Filter.Eq(x => x.Id, id)
                );

            return await GetMessage(filter);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<MessageResponse> { ResultCode = ResultCode.Failed };
        }
    }
    public async Task<ResponseModel<MessageResponse>> GetMessageAsync(string id)
    {
        try
        {
            var filter = Builders<MessageModel>.Filter.And(
                Builders<MessageModel>.Filter.Eq(x => x.Id, id)
                );
            return await GetMessage(filter);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<MessageResponse> { ResultCode = ResultCode.Failed };
        }
    }

    private async Task<ResponseModel<MessageResponse>> GetMessage(FilterDefinition<MessageModel>? filter)
    {
        try
        {
            var document = await _MessagesCollection.Find(filter).FirstOrDefaultAsync();
            if (document == null)
                return new ResponseModel<MessageResponse> { ResultCode = ResultCode.Failed };

            var response = _mapper.Map<MessageResponse>(document);
            return new ResponseModel<MessageResponse> { ResultCode = ResultCode.Success, Data = response };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<MessageResponse> { ResultCode = ResultCode.Failed };
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