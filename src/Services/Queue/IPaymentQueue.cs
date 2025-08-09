using System.Threading.Channels;
using System.Threading.Tasks;
using rinha_backend_csharp.Dtos;

namespace rinha_backend_csharp.Services.Queue;

public interface IPaymentQueue
{
    ValueTask EnqueueAsync(PaymentRequest paymentRequest);
    ChannelReader<PaymentRequest> Reader { get; }
}