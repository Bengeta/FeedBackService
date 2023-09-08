using Grpc.Net.Client;
using Models;
using OrderService;
using HessBackend.Enums;

namespace ServicesGrpc.ServiceSent;
public class OrderService
{
    private string host = "bucket-api";
    private string port = "5230";
    private string adress;

    public OrderService()
    {
        adress = "http://" + host + ":" + port;
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
            Console.WriteLine(e.Message);
            return new ResponseModel<OrderResponseGrpc>
            {
                ResultCode = ResultCode.Failed,
                Message = e.Message
            };
        }
    }
}