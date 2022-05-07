namespace PlaidQuickstartBlazor.Shared;

/// <summary>
/// Client application global state
/// </summary
/// <remarks>
/// Needed on server also, to support pre-rendering
/// 
/// TODO: No, not no more. This can be moved back into the client, and the "runningonserver" removed.
/// </remarks>
public class ContextContainer
{
    public bool IsItemAccess
    {
        get => _isItemAccess;
        set
        {
            _isItemAccess = value;
            NotifyStateChanged();
        }
    }
    private bool _isItemAccess;

    public bool LinkSuccess
    {
        get => _linkSuccess;
        set
        {
            _linkSuccess = value;
            NotifyStateChanged();
        }
    }
    private bool _linkSuccess;

    public PlaidCredentials? Credentials
    {
        get => _credentials;
        set
        {
            _credentials = value;
            NotifyStateChanged();
        }
    }
    private PlaidCredentials? _credentials;

    public bool RunningOnServer { get; set; }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}