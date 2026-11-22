namespace GotFired.Model
{
    /// <summary>
    /// A request the business rules reject (stale edit, invalid password, ...). Returned as HTTP 412 with
    /// { StatusMessage, StatusText }, which the SPA shows as a warning toast; not emailed as an error.
    /// </summary>
    public class BusinessRuleException(string message) : Exception(message);
}
