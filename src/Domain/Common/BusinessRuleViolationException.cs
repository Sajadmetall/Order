namespace Commerce.Domain.Common;

public sealed class BusinessRuleViolationException(string message) : Exception(message)
{
}
