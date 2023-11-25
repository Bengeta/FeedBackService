using Grpc.Net.Client;
using HessLibrary.Models;
using HessLibrary.Enums;
using HessLibrary.OrderServiceGrpc;

namespace ServicesGrpc.ServiceSent;
public class OrderService
{
    private string host = "bucket-api";
    private string port = "5230";
    private string adress;
    private ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        adress = "http://" + host + ":" + port;
        _logger = logger;
    }

    public async Task<ResponseModel<OrderResponseGrpc>> GetOrderById(long id)
    {
        try
        {
            var channel = GrpcChannel.ForAddress(adress);
            var client = new Order.OrderClient(channel);

            var request = new GetOrderByIdRequestGrpc
            {
                Id = id
            };
            var call = await client.GetOrderByIdAsync(request
                , deadline: DateTime.UtcNow.AddMinutes(1)
            );
            if (call != null)
                return new ResponseModel<OrderResponseGrpc>
                { ResultCode = ResultCode.Success, Data = call.Response };
            return new ResponseModel<OrderResponseGrpc> { ResultCode = ResultCode.Failed };
        }
        catch (Exception e)
        {
            _logger.LogError("Error in GetOrderById in OrderService \n" + e.Message);
            return new ResponseModel<OrderResponseGrpc>
            {
                ResultCode = ResultCode.Failed,
                Message = e.Message
            };
        }
    }
}