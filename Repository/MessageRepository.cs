using HessLibrary.Enums;
using Interfaces;
using HessLibrary.Models;
using Models.DBTables;
using Utils;
using MongoDB.Driver;
using ServicesGrpc.ServiceSent;
using Requests;
using Responses;
using AutoMapper;
using HessLibrary.FeedbackServiceGrpc;
using HessLibrary.Interfaces;

namespace Repository;
public class MessageRepository : IMessageRepository
{
    private readonly IMongoCollection<MessageModel> _MessagesCollection;
    private readonly ServicesGrpc.ServiceSent.OrderService _orderService;
    private readonly UserService _userService;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly IMapper _mapper;
    private readonly ILogger<MessageRepository> _logger;

    public MessageRepository(IMongoDatabase database,IRabbitMqService rabbitMqService, ILogger<MessageRepository> logger, IMapper mapper, ServicesGrpc.ServiceSent.OrderService orderService, UserService userService)
    {
        _logger = logger;
        _orderService = orderService;
        _userService = userService;
        _mapper = mapper;
        _rabbitMqService = rabbitMqService;
        _MessagesCollection = database.GetCollection<MessageModel>("messages");
    }

    public async Task<ResponseModel<PaginatedListModel<MessageResponse>>> GetAllMessagesAsync(string token, int page = 0, int pageSize = 10)
    {
        try
        {
            var userResponse = await _userService.GetGetUserByToken(token);
            if (userResponse.ResultCode != ResultCode.Success)
            {
                _logger.LogError("Error in GetAllMessagesAsync in MessageRepository - User not found");
                return new ResponseModel<PaginatedListModel<MessageResponse>> { ResultCode = ResultCode.UserNotFound };
            }
            var filter = Builders<MessageModel>.Filter.Eq(x => x.UserId, userResponse.Data.Id);
            return await GetMessages(filter, page, pageSize);
        }
        catch (Exception e)
        {
            _logger.LogError("Error in GetAllMessagesAsync in MessageRepository \n" + e.Message);
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
            _logger.LogError("Error in GetAllMessagesAsync in MessageRepository \n" + e.Message);
            return new ResponseModel<PaginatedListModel<MessageResponse>> { ResultCode = ResultCode.Failed };
        }
    }

    private async Task<ResponseModel<PaginatedListModel<MessageResponse>>> GetMessages(FilterDefinition<MessageModel>? filter, int page = 0, int pageSize = 10)
    {
        try
        {
            var documents = await _MessagesCollection.Find(filter).ToListAsync();
            if (documents == null)
            {
                _logger.LogError("Error in GetMessages in MessageRepository - No messages found");
                return new ResponseModel<PaginatedListModel<MessageResponse>> { ResultCode = ResultCode.Failed };
            }

            var documentsMapped = _mapper.Map<List<MessageResponse>>(documents);
            var documentsPagged = PagedList<MessageResponse>.ToPagedList(documentsMapped, page, pageSize);
            var ans = _mapper.Map<PaginatedListModel<MessageResponse>>(documentsPagged);
            return new ResponseModel<PaginatedListModel<MessageResponse>> { ResultCode = ResultCode.Success, Data = ans };
        }
        catch (Exception e)
        {
            _logger.LogError("Error in GetMessages in MessageRepository \n" + e.Message);
            return new ResponseModel<PaginatedListModel<MessageResponse>> { ResultCode = ResultCode.Failed };
        }
    }

    public async Task<ResponseModel<MessageResponse>> GetMessageAsync(string token, string id)
    {
        try
        {
            var userResponse = await _userService.GetGetUserByToken(token);
            if (userResponse.ResultCode != ResultCode.Success)
            {
                _logger.LogError("Error in GetAllMessagesAsync in MessageRepository - User not found");
                return new ResponseModel<MessageResponse> { ResultCode = ResultCode.UserNotFound };
            }
            var filter = Builders<MessageModel>.Filter.And(
                Builders<MessageModel>.Filter.Eq(x => x.UserId, userResponse.Data.Id),
                Builders<MessageModel>.Filter.Eq(x => x.Id, id)
                );

            return await GetMessage(filter);
        }
        catch (Exception e)
        {
            _logger.LogError("Error in GetMessageAsync in MessageRepository \n" + e.Message);
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
            _logger.LogError("Error in GetMessageAsync in MessageRepository \n" + e.Message);
            return new ResponseModel<MessageResponse> { ResultCode = ResultCode.Failed };
        }
    }

    private async Task<ResponseModel<MessageResponse>> GetMessage(FilterDefinition<MessageModel>? filter)
    {
        try
        {
            var document = await _MessagesCollection.Find(filter).FirstOrDefaultAsync();
            if (document == null)
            {
                _logger.LogError("Error in GetMessages in MessageRepository - No messages found");
                return new ResponseModel<MessageResponse> { ResultCode = ResultCode.Failed };
            }

            var response = _mapper.Map<MessageResponse>(document);
            return new ResponseModel<MessageResponse> { ResultCode = ResultCode.Success, Data = response };
        }
        catch (Exception e)
        {
            _logger.LogError("Error in GetMessage in MessageRepository \n" + e.Message);
            return new ResponseModel<MessageResponse> { ResultCode = ResultCode.Failed };
        }
    }


    public async Task<ResponseModel<bool>> AddMessageAsync(string token, AddMessageRequest request)
    {
        try
        {
            var userResponse = await _userService.GetGetUserByToken(token);
            if (userResponse.ResultCode != ResultCode.Success)
            {
                _logger.LogError("Error in GetAllMessagesAsync in MessageRepository - User not found");
                return new ResponseModel<bool> { ResultCode = ResultCode.UserNotFound };
            }
            var MessageModel = new MessageModel { UserId = userResponse.Data.Id, Message = request.Message, Title = request.Title };
            await _MessagesCollection.InsertOneAsync(MessageModel);
            return new ResponseModel<bool> { ResultCode = ResultCode.Success, Data = true };
        }
        catch (Exception e)
        {
            _logger.LogError("Error in AddMessageAsync in MessageRepository \n" + e.Message);
            return new ResponseModel<bool> { ResultCode = ResultCode.Failed };
        }
    }

    public async Task<ResponseModel<bool>> UpdateMessageAsync(AddAnswerRequestGrpc request)
    {
        try
        {
            var messageResponse = await GetMessageAsync(request.Id);
            if (messageResponse.ResultCode != ResultCode.Success)
                return new ResponseModel<bool> { ResultCode = ResultCode.Failed };

            var filter = Builders<MessageModel>.Filter.Eq(x => x.Id, request.Id);
            var update = Builders<MessageModel>.Update.Set(x => x.Answer, request.Answer);

            _logger.LogInformation("Id - " + request.Id);
            var document = await _MessagesCollection.Find(filter).FirstOrDefaultAsync();
            if (document == null)
            {
                _logger.LogError("Error in UpdateMessageAsync in MessageRepository - No messages found");
                return new ResponseModel<bool> { ResultCode = ResultCode.Failed };
            }

            var result = await _MessagesCollection.UpdateOneAsync(filter, update);

            if (result.IsAcknowledged && result.ModifiedCount > 0)
            {
                await _rabbitMqService.SendMessage(obj: document, queueList: new List<string> { "FeedbackQueue" });
                return new ResponseModel<bool> { ResultCode = ResultCode.Success, Data = true };
            }
            else
            {
                return new ResponseModel<bool> { ResultCode = ResultCode.Failed };
            }

        }
        catch (Exception e)
        {
            _logger.LogError("Error in UpdateMessageAsync in MessageRepository \n" + e.Message);
            return new ResponseModel<bool> { ResultCode = ResultCode.Failed };
        }
    }
}