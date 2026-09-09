namespace StayHub.Business.Results;

public enum UserError
{
    None = 0,
    UserNotFound,
    UsernameAlreadyExists,
    EmailAlreadyExists,
    UnsupportedRole,
    LastActiveAdmin
}
