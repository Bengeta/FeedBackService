using AutoMapper;
using Grpc.Core;
using Enums;
using Interfaces;
using User;
using Feedback;

namespace GrpcService.ServiceGet;
public class FeedbackService : FeedBack.FeedBackBase
{
    private readonly ILogger<FeedbackService> _logger;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public FeedbackService(ILogger<FeedbackService> logger, IMessageRepository messageRepository, IMapper mapper)
    {
        _logger = logger;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    public override async Task<GetMessageResponseGrpc> GetMessage(GetMessageRequestGrpc request,
    ServerCallContext context)
    {
        try
        {
            var response = await _messageRepository.GetMessageAsync(request.Id);
            if (response.ResultCode == ResultCode.Success)
                return new GetMessageResponseGrpc() { Success = true, Response = _mapper.Map<MessageGrpc>(response.Data) };

            return new GetMessageResponseGrpc() { Success = false };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new GetMessageResponseGrpc() { Success = false };
        }
    }

    public override async Task<GetMessagesResponseGrpc> GetMessages(GetMessagesRequestGrpc request,
    ServerCallContext context)
    {
        try
        {
            var response = await _messageRepository.GetAllMessagesAsync();
            if (response.ResultCode == ResultCode.Success)
                return new GetMessagesResponseGrpc() { Success = true, Response = _mapper.Map<PaginatedListMessageGrpc>(response.Data) };

            return new GetMessagesResponseGrpc() { Success = false };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new GetMessagesResponseGrpc() { Success = false };
        }
    }

    public override async Task<AddAnswerResponseGrpc> AddAnswer(AddAnswerRequestGrpc request,
    ServerCallContext context)
    {
        try
        {
            var response = await _messageRepository.GetAllMessagesAsync();
            if (response.ResultCode == ResultCode.Success)
                return new AddAnswerResponseGrpc() { Success = true};

            return new AddAnswerResponseGrpc() { Success = false };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new AddAnswerResponseGrpc() { Success = false };
        }
    }
}