namespace Application.Abstractions.Clock;

public interface IDateTime
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
