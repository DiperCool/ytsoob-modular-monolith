using System.Net;
using BuildingBlocks.Core.Exception.Types;

namespace Ytsoob.Modules.Identity.Identity.Exceptions;

public class IdentityUserNotFoundException : AppException
{
    public IdentityUserNotFoundException(string emailOrUserName)
        : base($"User with email or username: '{emailOrUserName}' not found.", HttpStatusCode.NotFound) { }

    public IdentityUserNotFoundException(Guid id)
        : base($"User with id: '{id}' not found.", HttpStatusCode.NotFound) { }
}
