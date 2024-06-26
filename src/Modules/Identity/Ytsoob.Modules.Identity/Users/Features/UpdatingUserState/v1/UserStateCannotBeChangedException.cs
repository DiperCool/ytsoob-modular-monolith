using System.Net;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;
using Ytsoob.Modules.Identity.Shared.Models;

namespace Ytsoob.Modules.Identity.Users.Features.UpdatingUserState.v1;

internal class UserStateCannotBeChangedException : AppException
{
    public UserState State { get; }
    public Guid UserId { get; }

    public UserStateCannotBeChangedException(UserState state, Guid userId)
        : base(
            $"User state cannot be changed to: '{state.ToName()}' for user with ID: '{userId}'.",
            HttpStatusCode.InternalServerError
        )
    {
        State = state;
        UserId = userId;
    }
}
