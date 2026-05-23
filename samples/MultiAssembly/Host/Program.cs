using Identity;
using Microsoft.Extensions.DependencyInjection;
using Sales;
using TinyValidations;

var services = new ServiceCollection();

services.AddIdentitySample();
services.AddSalesSample();
services.UseTinyValidations();

var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<ITinyValidator>();

await ValidateAsync(validator, new RegisterUser("taken@example.com", "A", 15));
await ValidateAsync(validator, new PlaceOrder("blocked@example.com", Array.Empty<OrderLine>(), 0));

static async Task ValidateAsync<T>(ITinyValidator validator, T command)
{
    var result = await validator.ValidateAsync(command);

    Console.WriteLine(typeof(T).Name);

    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  {error.Member}: {error.Message}");
    }

    Console.WriteLine();
}
