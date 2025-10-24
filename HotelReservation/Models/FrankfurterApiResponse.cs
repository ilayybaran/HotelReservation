// Models/FrankfurterApiResponse.cs
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class FrankfurterApiResponse
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; } // Genellikle 1 olur

    [JsonPropertyName("base")]
    public string Base { get; set; } // Sorgulanan ana para birimi (örn: "TRY")

    [JsonPropertyName("date")]
    public DateOnly Date { get; set; } // Kurların tarihi

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal> Rates { get; set; } // Diğer para birimlerine göre oranlar
}