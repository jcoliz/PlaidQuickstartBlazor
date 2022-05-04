namespace PlaidQuickstartBlazor.FrontEnd.Helpers;

public class ContextContainer
{
    public bool isItemAccess
    {
        get => _isItemAccess;
        set
        {
            _isItemAccess = value;
            NotifyStateChanged();
        }
    }
    private bool _isItemAccess;

    public bool linkSuccess
    {
        get => _linkSuccess;
        set
        {
            _linkSuccess = value;
            NotifyStateChanged();
        }
    }
    private bool _linkSuccess;

    public string? accessToken
    {
        get => _accessToken;
        set
        {
            _accessToken = value;
            NotifyStateChanged();
        }
    }
    private string? _accessToken;

    public string? itemId
    {
        get => _itemId;
        set
        {
            _itemId = value;
            NotifyStateChanged();
        }
    }
    private string? _itemId;

    public List<string> products { get; } = new List<string>();

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}