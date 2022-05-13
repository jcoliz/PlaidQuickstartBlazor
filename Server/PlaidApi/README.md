# Generated Plaid API

This is a test of what it would look like to generate the Plaid API using
NSwag instead of using Going.Plaid.

## Findings

This works great with Newtonsoft.Json. Perhaps I should stop right there! There's no problem with
the server taking a dependency on newtonsoft.

What would it take to get it working with System.Text.Json.

As far as I can tell, the issue comes down to nullable enums and enums using EnumMember Values.

Both of these are solved with Going.Plaid enum converter. I can inject the EnumCinverterFactory in numerous ways

e.g.

```json
      "jsonConverters": [
        "PlaidApi.Converters.EnumConverterFactory"
      ],
```

The problem is that the default string converter is always put onto the enums. If I just comment out the converter line below
it works fine for this one. *sigh*

```c#
        [System.Text.Json.Serialization.JsonPropertyName("subtype")]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public AccountSubtype? Subtype { get; set; }
```