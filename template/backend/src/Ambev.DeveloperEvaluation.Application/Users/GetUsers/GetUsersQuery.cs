using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUsers;

public record GetUsersQuery(
    int Page = 1,
    int Size = 10,
    string? Order = null,
    string? Username = null,
    string? Email = null) : IRequest<GetUsersResult>;
