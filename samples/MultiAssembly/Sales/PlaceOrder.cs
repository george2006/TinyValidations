namespace Sales;

public sealed record PlaceOrder(string CustomerEmail, IReadOnlyCollection<OrderLine> Lines, decimal Total);

public sealed record OrderLine(string Sku, int Quantity);
