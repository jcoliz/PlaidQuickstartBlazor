using PlaidQuickstartBlazor.Shared;

namespace PlaidQuickstartBlazor.FrontEnd.Helpers;

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

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}