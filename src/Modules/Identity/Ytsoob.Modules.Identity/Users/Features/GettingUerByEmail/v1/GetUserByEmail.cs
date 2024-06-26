using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Core.Exception;
using Ytsoob.Modules.Identity.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Ytsoob.Modules.Identity.Identity.Exceptions;
using Ytsoob.Modules.Identity.Users.Dtos.v1;
using Ytsoob.Modules.Identity.Shared.Models;

namespace Ytsoob.Modules.Identity.Users.Features.GettingUerByEmail.v1;

public record GetUserByEmail(string Email) : IQuery<GetUserByEmailResponse>;

internal class GetUserByIdValidator : AbstractValidator<GetUserByEmail>
{
    public GetUserByIdValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email address is not valid");
    }
}

internal class GetUserByEmailHandler : IQueryHandler<GetUserByEmail, GetUserByEmailResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public GetUserByEmailHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = Guard.Against.Null(userManager, nameof(userManager));
        _mapper = Guard.Against.Null(mapper, nameof(mapper));
    }

    public async Task<GetUserByEmailResponse> Handle(GetUserByEmail query, CancellationToken cancellationToken)
    {
        Guard.Against.Null(query, nameof(query));

        var identityUser = await _userManager.FindUserWithRoleByEmailAsync(query.Email);
        Guard.Against.NotFound(identityUser, new IdentityUserNotFoundException(query.Email));

        var userDto = _mapper.Map<IdentityUserDto>(identityUser);

        return new GetUserByEmailResponse(userDto);
    }
}
