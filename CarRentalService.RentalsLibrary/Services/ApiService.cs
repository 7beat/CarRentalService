using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace CarRentalService.RentalsLibrary.Services;
public class ApiService
{
    private static AsyncCircuitBreakerPolicy circuitBreakerPolicy;
    private static AsyncRetryPolicy retryPolicy;

    public ApiService()
    {
        retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(2, sd => TimeSpan.FromSeconds(20));

        circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
            exceptionsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
