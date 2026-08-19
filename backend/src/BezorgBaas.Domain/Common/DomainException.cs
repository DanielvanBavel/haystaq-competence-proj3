namespace BezorgBaas.Domain.Common;

/// <summary>
/// De enige fout die het domein gooit. De code is bedoeld voor de API-laag en
/// wordt ook aan de client getoond: deze applicatie is juist wel duidelijk over
/// wat er misgaat, zodat je er tests op kunt schrijven.
/// </summary>
public sealed class DomainException : Exception
{
    public string Code { get; }
    public DomainErrorKind Kind { get; }

    private DomainException(DomainErrorKind kind, string code, string message) : base(message)
    {
        Kind = kind;
        Code = code;
    }

    public static DomainException Invalid(string code, string message) =>
        new(DomainErrorKind.Invalid, code, message);

    public static DomainException Conflict(string code, string message) =>
        new(DomainErrorKind.Conflict, code, message);

    public static DomainException NotFound(string code, string message) =>
        new(DomainErrorKind.NotFound, code, message);

    public static void Require(bool condition, string code, string message)
    {
        if (!condition)
        {
            throw Invalid(code, message);
        }
    }

    public static void RequireState(bool condition, string code, string message)
    {
        if (!condition)
        {
            throw Conflict(code, message);
        }
    }
}

public enum DomainErrorKind
{
    Invalid,
    Conflict,
    NotFound
}
