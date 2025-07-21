using System.Threading.Channels;
using System.Threading.Tasks;
using rinha_backend_csharp.Dtos;

namespace rinha_backend_csharp.Queue;

public class PaymentQueue : IPaymentQueue
{
    private readonly Channel<PaymentRequest> _channel = Channel.CreateUnbounded<PaymentRequest>(
        new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });

    public ValueTask EnqueueAsync(PaymentRequest paymentRequest)
    {
        return _channel.Writer.WriteAsync(paymentRequest);
    }

    public ChannelReader<PaymentRequest> Reader => _channel.Reader;
}