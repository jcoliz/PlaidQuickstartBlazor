# Porting Guide

## Plaid API Changes

Most porting updates are caused by changes to the underlying Plaid API, which is surfaced
through the QuickStart by the Going.Plaid library.

For reference, have the latest Going.Plaid libraries on the local machine. 
BE SURE to include the latest Plaid API by recursing submodules when pulling:

```
PS> git pull --recurse-submodules
```

## Frontend changes: products.tsx

https://github.com/plaid/quickstart/blob/master/frontend/src/Components/ProductTypes/Products.tsx

The primary reference file for the frontend is Products.tsx, e.g.

``` xml
<Endpoint
    endpoint="assets"
    name="Assets"
    categories={assetsCategories}
    schema="/assets_report/get/"
    description="Create and retrieve assets information an asset report"
    transformData={transformAssetsData}
/>
```

...becomes this in FrontEnd/Components/Products.razor:

``` xml
<Endpoint
    Api="assets"
    Name="Assets"
    Schema="/assets_report/get/"
    Description="Create and retrieve assets information an asset report"
/>
```

Notice that categories and transformData are not needed in the frontend, as that is handled in the backend for my version.

## Backend calling patterns: node/index.js

https://github.com/plaid/quickstart/blob/master/node/index.js

The node backend is the primary reference for the server side in the .NET version.

Every API in the node/index.js should be an API endpoint in FetchController.cs. Note the "endpoint" attributed in the "Endpoint" component describes which 
endpoint in FetchController will be called

Consider the "accounts" backend endpoint, which is implemented in node as:

``` TS
app.get('/api/accounts', function (request, response, next) {
  Promise.resolve()
    .then(async function () {
      const accountsResponse = await client.accountsGet({
        access_token: ACCESS_TOKEN,
      });
      prettyPrintResponse(accountsResponse);
      response.json(accountsResponse.data);
    })
    .catch(next);
});
```

Becomes this in C#:

``` c#
[HttpGet]
[ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Accounts()
{
    var request = new Going.Plaid.Accounts.AccountsGetRequest();

    var response = await _client.AccountsGetAsync(request);

    if (response.Error is not null)
        return Error(response.Error);

    DataTable result = new ServerDataTable("Name", "Balance/r", "Subtype", "Mask")
    {
        Rows = response.Accounts
            .Select(x =>
                new Row(
                    x.Name,
                    x.Balances?.Current?.ToString("C2") ?? string.Empty,
                    x.Subtype?.ToString() ?? string.Empty,
                    x.Mask ?? string.Empty
                )
            )
            .ToArray()
    };

    return Ok(result);
}
```

## Data transformation for viewing: dataUtilities.ts

Note that there is a difference between the reference Quickstart and the .NET Quickstart in how the data is returned to the client.
In the reference Quickstart, the entire response (or a large subset) is returned unaltered. 
The needed data is then extracted and formatted on the Frontend for how it will be displayed to the user.

In the .NET Quickstart, I do this extraction and formatting on the server, then return the display-ready information for the user. 
The client doesn't need to do anything further--just display it.

The extracting and formatting is handled in this file: https://github.com/plaid/quickstart/blob/master/frontend/src/dataUtilities.ts

Thus, any changes to dataUtilities.ts are typically ported to FetchController.cs, e.g.

This reference TS:

``` ts
export const transformBalanceData = (data: AccountsGetResponse) => {
  const balanceData = data.accounts;
  return balanceData.map((account) => {
    const balance: number | null | undefined =
      account.balances.available || account.balances.current;
    const obj: DataItem = {
      name: account.name,
      balance: formatCurrency(balance, account.balances.iso_currency_code),
      subtype: account.subtype,
      mask: account.mask!,
    };
    return obj;
  });
};
```

...is reflected in the C# `Accounts()` endpoint as:

```c#
DataTable result = new ServerDataTable("Name", "Balance/r", "Subtype", "Mask")
{
    Rows = response.Accounts
        .Select(x =>
            new Row(
                x.Name,
                x.Balances?.Current?.ToString("C2") ?? string.Empty,
                x.Subtype?.ToString() ?? string.Empty,
                x.Mask ?? string.Empty
            )
        )
        .ToArray()
};
```
