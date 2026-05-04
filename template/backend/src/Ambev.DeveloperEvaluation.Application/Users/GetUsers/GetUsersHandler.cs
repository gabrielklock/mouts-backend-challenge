using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUsers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, GetUsersResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<GetUsersResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var paged = await _userRepository.GetPagedAsync(
            request.Page, request.Size,
            request.Order,
            request.Username, request.Email,
            cancellationToken);

        return new GetUsersResult
        {
            Data = _mapper.Map<IEnumerable<GetUserResult>>(paged.Data),
            TotalItems = paged.Total,
            CurrentPage = request.Page,
            TotalPages = (int)Math.Ceiling(paged.Total / (double)request.Size)
        };
    }
}
