using System.Text.Json.Serialization;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Models;

namespace rinha_backend_csharp.Configs;

[JsonSerializable(typeof(PaymentRequest))]
[JsonSerializable(typeof(SummaryQueryResult))]
[JsonSerializable(typeof(PaymentSummaryResponse))]
[JsonSerializable(typeof(PaymentSummary))]
[JsonSerializable(typeof(PaymentProcessorRequest))]
[JsonSerializable(typeof(PaymentPurgeResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;