using System.Threading.Channels;
using System.Threading.Tasks;
using rinha_backend_csharp.Dtos;

namespace rinha_backend_csharp.Services.Queue;

public class PaymentQueue : IPaymentQueue
{
    private readonly Channel<PaymentRequest> _channel = Channel.CreateUnbounded<PaymentRequest>(
        new UnboundedChannelOptions
        {
            SingleReader = true,  // Only one PaymentWorker reads
            SingleWriter = false  // Multiple HTTP endpoints can write concurrently
        });

    public ValueTask EnqueueAsync(PaymentRequest paymentRequest)
    {
        return _channel.Writer.WriteAsync(paymentRequest);
    }

    public ChannelReader<PaymentRequest> Reader => _channel.Reader;
}