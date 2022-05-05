using Going.Plaid;

namespace PlaidQuickstartBlazor.Server.Helpers;

public class PlaidCredentials : PlaidOptions
{
	public string? LinkToken { get; set; }
	public string? AccessToken { get; set; }
	public string? ItemId { get; set; }
	public string? Products { get; set; }
}
