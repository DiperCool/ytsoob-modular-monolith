using BuildingBlocks.Core.Exception.Types;

namespace Ytsoob.Modules.Identity.Identity.Features.VerifyingEmail.v1.Exceptions;

public class VerificationTokenIsInvalidException : BadRequestException
{
    public VerificationTokenIsInvalidException(string userId)
        : base($"verification token is invalid for userId '{userId}'.") { }
}
