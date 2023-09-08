using Microsoft.AspNetCore.Mvc;

namespace Controllers;

public class BaseController : ControllerBase
{
    protected string Token() => HttpContext.Items["Token"]?.ToString();
}