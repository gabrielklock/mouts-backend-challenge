using Ambev.DeveloperEvaluation.Common.Validation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new NullReferenceException());

    protected string GetCurrentUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? throw new NullReferenceException();

    protected IActionResult Ok<T>(T data, string message = "") =>
            base.Ok(new ApiResponseWithData<T> { Data = data, Success = true, Message = message });

    protected IActionResult Ok(string message) =>
            base.Ok(new ApiResponse { Success = true, Message = message });

    protected IActionResult Created<T>(T data, string message = "") =>
        base.Created(string.Empty, new ApiResponseWithData<T> { Data = data, Success = true, Message = message });

    protected IActionResult Created<T>(string routeName, object routeValues, T data, string message = "") =>
        base.CreatedAtRoute(routeName, routeValues, new ApiResponseWithData<T> { Data = data, Success = true, Message = message });

    protected IActionResult BadRequest(string message, IEnumerable<ValidationFailure> errors) =>
        base.BadRequest(new ApiResponse { Message = message, Success = false, Errors = errors.Select(e => (ValidationErrorDetail)e) });

    protected IActionResult BadRequest(IEnumerable<ValidationFailure> errors) =>
        BadRequest("Validation failed", errors);

    protected IActionResult NotFound(string message = "Resource not found") =>
        base.NotFound(new ApiResponse { Message = message, Success = false });
}
