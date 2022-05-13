using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlaidQuickstartBlazor.Tests.Unit;

[TestClass]
public class EnumParsingTest
{
    [TestMethod]
    public void SerializeBasic()
    {
        var account = new Account() { Name = "Name", Subtype = AccountSubtype.Credit_card };
        var json = System.Text.Json.JsonSerializer.Serialize(account); 
        Console.WriteLine(json);
    }
    [TestMethod]
    public void SerializeWithECF()
    {
        var account = new Account() { Name = "Name", Subtype = AccountSubtype.Credit_card };
        var json = System.Text.Json.JsonSerializer.Serialize(account, options:new System.Text.Json.JsonSerializerOptions()
        {
            Converters = { new PlaidApi.Converters.EnumConverterFactory() }
        });
        Console.WriteLine(json);
    }

    [TestMethod]
    public void DeSerializeWithECF()
    {
        var account = new Account() { Name = "Name", Subtype = AccountSubtype.Credit_card };
        var json = System.Text.Json.JsonSerializer.Serialize(account, options: new System.Text.Json.JsonSerializerOptions()
        {
            Converters = { new PlaidApi.Converters.EnumConverterFactory() }
        });

        var actual = JsonSerializer.Deserialize<Account>(json, options: new System.Text.Json.JsonSerializerOptions()
        {
            Converters = { new PlaidApi.Converters.EnumConverterFactory() }
        });

        Assert.AreEqual(account.Subtype, actual!.Subtype);
    }
}

public class AccountSubtypeConverter : JsonConverter<AccountSubtype>
{
    public override AccountSubtype Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var nope = reader.GetString();
        return AccountSubtype.Cd;
    }
    //    =>
    //            DateTimeOffset.ParseExact(reader.GetString()!,
    //                "MM/dd/yyyy", CultureInfo.InvariantCulture);

    public override void Write(
        Utf8JsonWriter writer,
        AccountSubtype value,
        JsonSerializerOptions options) =>
            writer.WriteStringValue("CD!");
}


public class Account
{
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Name { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("subtype")]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    //[System.Text.Json.Serialization.JsonConverter(typeof(PlaidApi.Converters.EnumConverterFactory))]
    
    public AccountSubtype? Subtype { get; set; }
}

public enum AccountSubtype
{

    [System.Runtime.Serialization.EnumMember(Value = @"401a")]
    _401a = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"401k")]
    _401k = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"403B")]
    _403B = 2,

    [System.Runtime.Serialization.EnumMember(Value = @"457b")]
    _457b = 3,

    [System.Runtime.Serialization.EnumMember(Value = @"529")]
    _529 = 4,

    [System.Runtime.Serialization.EnumMember(Value = @"brokerage")]
    Brokerage = 5,

    [System.Runtime.Serialization.EnumMember(Value = @"cash isa")]
    Cash_isa = 6,

    [System.Runtime.Serialization.EnumMember(Value = @"education savings account")]
    Education_savings_account = 7,

    [System.Runtime.Serialization.EnumMember(Value = @"ebt")]
    Ebt = 8,

    [System.Runtime.Serialization.EnumMember(Value = @"fixed annuity")]
    Fixed_annuity = 9,

    [System.Runtime.Serialization.EnumMember(Value = @"gic")]
    Gic = 10,

    [System.Runtime.Serialization.EnumMember(Value = @"health reimbursement arrangement")]
    Health_reimbursement_arrangement = 11,

    [System.Runtime.Serialization.EnumMember(Value = @"hsa")]
    Hsa = 12,

    [System.Runtime.Serialization.EnumMember(Value = @"isa")]
    Isa = 13,

    [System.Runtime.Serialization.EnumMember(Value = @"ira")]
    Ira = 14,

    [System.Runtime.Serialization.EnumMember(Value = @"lif")]
    Lif = 15,

    [System.Runtime.Serialization.EnumMember(Value = @"life insurance")]
    Life_insurance = 16,

    [System.Runtime.Serialization.EnumMember(Value = @"lira")]
    Lira = 17,

    [System.Runtime.Serialization.EnumMember(Value = @"lrif")]
    Lrif = 18,

    [System.Runtime.Serialization.EnumMember(Value = @"lrsp")]
    Lrsp = 19,

    [System.Runtime.Serialization.EnumMember(Value = @"non-taxable brokerage account")]
    NonTaxable_brokerage_account = 20,

    [System.Runtime.Serialization.EnumMember(Value = @"other")]
    Other = 21,

    [System.Runtime.Serialization.EnumMember(Value = @"other insurance")]
    Other_insurance = 22,

    [System.Runtime.Serialization.EnumMember(Value = @"other annuity")]
    Other_annuity = 23,

    [System.Runtime.Serialization.EnumMember(Value = @"prif")]
    Prif = 24,

    [System.Runtime.Serialization.EnumMember(Value = @"rdsp")]
    Rdsp = 25,

    [System.Runtime.Serialization.EnumMember(Value = @"resp")]
    Resp = 26,

    [System.Runtime.Serialization.EnumMember(Value = @"rlif")]
    Rlif = 27,

    [System.Runtime.Serialization.EnumMember(Value = @"rrif")]
    Rrif = 28,

    [System.Runtime.Serialization.EnumMember(Value = @"pension")]
    Pension = 29,

    [System.Runtime.Serialization.EnumMember(Value = @"profit sharing plan")]
    Profit_sharing_plan = 30,

    [System.Runtime.Serialization.EnumMember(Value = @"retirement")]
    Retirement = 31,

    [System.Runtime.Serialization.EnumMember(Value = @"roth")]
    Roth = 32,

    [System.Runtime.Serialization.EnumMember(Value = @"roth 401k")]
    Roth_401k = 33,

    [System.Runtime.Serialization.EnumMember(Value = @"rrsp")]
    Rrsp = 34,

    [System.Runtime.Serialization.EnumMember(Value = @"sep ira")]
    Sep_ira = 35,

    [System.Runtime.Serialization.EnumMember(Value = @"simple ira")]
    Simple_ira = 36,

    [System.Runtime.Serialization.EnumMember(Value = @"sipp")]
    Sipp = 37,

    [System.Runtime.Serialization.EnumMember(Value = @"stock plan")]
    Stock_plan = 38,

    [System.Runtime.Serialization.EnumMember(Value = @"thrift savings plan")]
    Thrift_savings_plan = 39,

    [System.Runtime.Serialization.EnumMember(Value = @"tfsa")]
    Tfsa = 40,

    [System.Runtime.Serialization.EnumMember(Value = @"trust")]
    Trust = 41,

    [System.Runtime.Serialization.EnumMember(Value = @"ugma")]
    Ugma = 42,

    [System.Runtime.Serialization.EnumMember(Value = @"utma")]
    Utma = 43,

    [System.Runtime.Serialization.EnumMember(Value = @"variable annuity")]
    Variable_annuity = 44,

    [System.Runtime.Serialization.EnumMember(Value = @"credit card")]
    Credit_card = 45,

    [System.Runtime.Serialization.EnumMember(Value = @"paypal")]
    Paypal = 46,

    [System.Runtime.Serialization.EnumMember(Value = @"cd")]
    Cd = 47,

    [System.Runtime.Serialization.EnumMember(Value = @"checking")]
    Checking = 48,

    [System.Runtime.Serialization.EnumMember(Value = @"savings")]
    Savings = 49,

    [System.Runtime.Serialization.EnumMember(Value = @"money market")]
    Money_market = 50,

    [System.Runtime.Serialization.EnumMember(Value = @"prepaid")]
    Prepaid = 51,

    [System.Runtime.Serialization.EnumMember(Value = @"auto")]
    Auto = 52,

    [System.Runtime.Serialization.EnumMember(Value = @"business")]
    Business = 53,

    [System.Runtime.Serialization.EnumMember(Value = @"commercial")]
    Commercial = 54,

    [System.Runtime.Serialization.EnumMember(Value = @"construction")]
    Construction = 55,

    [System.Runtime.Serialization.EnumMember(Value = @"consumer")]
    Consumer = 56,

    [System.Runtime.Serialization.EnumMember(Value = @"home equity")]
    Home_equity = 57,

    [System.Runtime.Serialization.EnumMember(Value = @"loan")]
    Loan = 58,

    [System.Runtime.Serialization.EnumMember(Value = @"mortgage")]
    Mortgage = 59,

    [System.Runtime.Serialization.EnumMember(Value = @"overdraft")]
    Overdraft = 60,

    [System.Runtime.Serialization.EnumMember(Value = @"line of credit")]
    Line_of_credit = 61,

    [System.Runtime.Serialization.EnumMember(Value = @"student")]
    Student = 62,

    [System.Runtime.Serialization.EnumMember(Value = @"cash management")]
    Cash_management = 63,

    [System.Runtime.Serialization.EnumMember(Value = @"keogh")]
    Keogh = 64,

    [System.Runtime.Serialization.EnumMember(Value = @"mutual fund")]
    Mutual_fund = 65,

    [System.Runtime.Serialization.EnumMember(Value = @"recurring")]
    Recurring = 66,

    [System.Runtime.Serialization.EnumMember(Value = @"rewards")]
    Rewards = 67,

    [System.Runtime.Serialization.EnumMember(Value = @"safe deposit")]
    Safe_deposit = 68,

    [System.Runtime.Serialization.EnumMember(Value = @"sarsep")]
    Sarsep = 69,

    [System.Runtime.Serialization.EnumMember(Value = @"payroll")]
    Payroll = 70,

}

