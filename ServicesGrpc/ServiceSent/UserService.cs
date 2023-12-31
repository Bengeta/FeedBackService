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
    private ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        adress = "http://" + host + ":" + port;
        _logger = logger;
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
            _logger.LogError("Error in GetGetUserByToken in UserServiceGrpc \n" + e.Message);
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
            _logger.LogError("Error in GetUserById in UserServiceGrpc \n" + e.Message);
            return new ResponseModel<UserResponseGrpc>
            {
                ResultCode = ResultCode.Failed,
                Message = e.Message
            };
        }
    }
}