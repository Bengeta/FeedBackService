using Grpc.Core;
using Grpc.Net.Client;
using HessLibrary.Models;
using HessLibrary.Enums;
using HessLibrary.UserServiceGrpc;

namespace ServicesGrpc.ServiceSent;

public class UserService
{
    private string host = "api";
    private string port = "5002";
    private string adress;

    public UserService()
    {
        adress = "http://" + host + ":" + port;
    }

    public async Task<ResponseModel<UserResponseGrpc>> GetGetUserByToken(string token)
    {
        try
        {
            var channel = GrpcChannel.ForAddress(adress);
            var client = new User.UserClient(channel);

            var call = await client.GetUserAsync(new GetUserByTokenGrpc { Token = token }
                , deadline: DateTime.UtcNow.AddMinutes(1)
            );
            if (call != null)
                return new ResponseModel<UserResponseGrpc>
                { ResultCode = ResultCode.Success, Data = call };
            return new ResponseModel<UserResponseGrpc> { ResultCode = ResultCode.Failed };
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new ResponseModel<UserResponseGrpc>
            {
                ResultCode = ResultCode.Failed,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel<UserResponseGrpc>> GetUserById(int id)
    {
        try
        {
            var channel = GrpcChannel.ForAddress(adress);
            var client = new User.UserClient(channel);

            var call = await client.GetUserByIdAsync(new GetUserByIdGrpc { Id = id }
                , deadline: DateTime.UtcNow.AddMinutes(1)
            );
            if (call != null)
                return new ResponseModel<UserResponseGrpc>
                { ResultCode = ResultCode.Success, Data = call };
            return new ResponseModel<UserResponseGrpc> { ResultCode = ResultCode.Failed };
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new ResponseModel<UserResponseGrpc>
            {
                ResultCode = ResultCode.Failed,
                Message = e.Message
            };
        }
    }
}