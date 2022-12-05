# Porting Guide

## Going.Plaid

Most porting updates are caused by changes to the underlying Plaid API, which is surfaced
to the QuickStart by the Going.Plaid library.

For reference, have the latest Going.Plaid libraries on the local machine. 
BE SURE to include the latest Plaid API by recursing submodules when pulling:

```
PS> git pull --recurse-submodules
```

## dataUtilities.ts

https://github.com/plaid/quickstart/blob/master/frontend/src/dataUtilities.ts

This source file contains the data transformations from the raw data returned by the server to the tabular display
shown to the user.

In my version of the quickstart, this transformation happens on the backend, with only the displayed data
returned to the client.

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

Becomes this C#:

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

## node/index.js

https://github.com/plaid/quickstart/blob/master/node/index.js

The node backend is the primary reference for the server side in the .NET version.

Every API in the node/index.js should be an API endpoint in FetchController.cs

e.g. the "Accounts" API shown above is implemented in the example as:

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

## products.tsx

https://github.com/plaid/quickstart/blob/master/frontend/src/Components/ProductTypes/Products.tsx

The primary reference file for the frontend is Products.tsx, e.g.

``` TS
{products.includes("assets") && (
<Endpoint
    endpoint="assets"
    name="Assets"
    categories={assetsCategories}
    schema="/assets_report/get/"
    description="Create and retrieve assets information an asset report"
    transformData={transformAssetsData}
/>
)}
```

becomes this in FrontEnd/Components/Products.razor:

``` xml
<Endpoint
    Api="assets"
    Name="Assets"
    Schema="/assets_report/get/"
    Description="Create and retrieve assets information an asset report"
/>
```

Notice that categories and transformData are not needed in the frontend, as that is handled in the backend for my version.