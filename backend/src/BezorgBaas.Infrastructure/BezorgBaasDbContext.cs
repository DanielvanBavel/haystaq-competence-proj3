using BezorgBaas.Domain.Catalog;
using BezorgBaas.Domain.Common;
using BezorgBaas.Domain.Ordering;
using BezorgBaas.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace BezorgBaas.Infrastructure;

/// <summary>
/// EF Core mapt op een schema dat door SQL-scripts wordt beheerd (zie db/init).
/// Er zijn dus geen migraties nodig om de applicatie te kunnen draaien.
/// </summary>
public class BezorgBaasDbContext : DbContext
{
    public BezorgBaasDbContext(DbContextOptions<BezorgBaasDbContext> options) : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Restaurant>(entity =>
        {
            entity.ToTable("restaurant");
            entity.HasKey(restaurant => restaurant.Id);
            entity.Property(restaurant => restaurant.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(restaurant => restaurant.Slug).HasColumnName("slug");
            entity.Property(restaurant => restaurant.Name).HasColumnName("name");
            entity.Property(restaurant => restaurant.Cuisine).HasColumnName("cuisine");
            entity.Property(restaurant => restaurant.City).HasColumnName("city");
            entity.Property(restaurant => restaurant.Rating).HasColumnName("rating");
            entity.Property(restaurant => restaurant.EstimatedDeliveryMinutes)
                .HasColumnName("estimated_delivery_minutes");
            entity.Property(restaurant => restaurant.MinimumOrder).HasColumnName("minimum_order")
                .HasConversion(MoneyConverter);
            entity.Property(restaurant => restaurant.DeliveryFee).HasColumnName("delivery_fee")
                .HasConversion(MoneyConverter);
            entity.Property(restaurant => restaurant.FreeDeliveryFrom).HasColumnName("free_delivery_from")
                .HasConversion(NullableMoneyConverter);
            entity.Property(restaurant => restaurant.IsOpen).HasColumnName("is_open");
            entity.HasMany(restaurant => restaurant.Menu)
                .WithOne()
                .HasForeignKey(item => item.RestaurantId);
            entity.Navigation(restaurant => restaurant.Menu).AutoInclude();
        });

        builder.Entity<MenuItem>(entity =>
        {
            entity.ToTable("menu_item");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(item => item.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(item => item.Name).HasColumnName("name");
            entity.Property(item => item.Description).HasColumnName("description");
            entity.Property(item => item.Category).HasColumnName("category");
            entity.Property(item => item.Price).HasColumnName("price").HasConversion(MoneyConverter);
            entity.Property(item => item.IsAvailable).HasColumnName("is_available");
            entity.Property(item => item.IsVegetarian).HasColumnName("is_vegetarian");
            entity.Property(item => item.SpicinessLevel).HasColumnName("spiciness_level");
            entity.HasMany(item => item.Options)
                .WithOne()
                .HasForeignKey(option => option.MenuItemId);
            entity.Navigation(item => item.Options).AutoInclude();
        });

        builder.Entity<MenuItemOption>(entity =>
        {
            entity.ToTable("menu_item_option");
            entity.HasKey(option => option.Id);
            entity.Property(option => option.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(option => option.MenuItemId).HasColumnName("menu_item_id");
            entity.Property(option => option.Name).HasColumnName("name");
            entity.Property(option => option.Kind).HasColumnName("kind").HasConversion<string>();
            entity.Property(option => option.PriceDelta).HasColumnName("price_delta").HasConversion(MoneyConverter);
            entity.Property(option => option.IsDefault).HasColumnName("is_default");
        });

        builder.Entity<Order>(entity =>
        {
            entity.ToTable("customer_order");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(order => order.OrderNumber).HasColumnName("order_number");
            entity.Property(order => order.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(order => order.CustomerName).HasColumnName("customer_name");
            entity.Property(order => order.CustomerEmail).HasColumnName("customer_email");
            entity.Property(order => order.DeliveryDate).HasColumnName("delivery_date");
            entity.Property(order => order.DeliverySlotStart).HasColumnName("delivery_slot_start");
            entity.Property(order => order.DeliverySlotEnd).HasColumnName("delivery_slot_end");
            entity.Property(order => order.PaymentMethod).HasColumnName("payment_method").HasConversion<string>();
            entity.Property(order => order.PaymentReference).HasColumnName("payment_reference");
            entity.Property(order => order.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(order => order.Subtotal).HasColumnName("subtotal").HasConversion(MoneyConverter);
            entity.Property(order => order.DeliveryFee).HasColumnName("delivery_fee").HasConversion(MoneyConverter);
            entity.Property(order => order.Discount).HasColumnName("discount").HasConversion(MoneyConverter);
            entity.Property(order => order.Total).HasColumnName("total").HasConversion(MoneyConverter);
            entity.Property(order => order.PromoCode).HasColumnName("promo_code");
            entity.Property(order => order.PlacedAt).HasColumnName("placed_at");

            entity.OwnsOne(order => order.Address, address =>
            {
                address.Property(value => value.Street).HasColumnName("address_street");
                address.Property(value => value.HouseNumber).HasColumnName("address_house_number");
                address.Property(value => value.PostalCode).HasColumnName("address_postal_code");
                address.Property(value => value.City).HasColumnName("address_city");
                address.Property(value => value.Note).HasColumnName("address_note");
            });
            entity.Navigation(order => order.Address).IsRequired();

            entity.HasMany(order => order.Lines).WithOne().HasForeignKey(line => line.OrderId);
            entity.HasMany(order => order.History).WithOne().HasForeignKey(change => change.OrderId);
            entity.Navigation(order => order.Lines).AutoInclude();
            entity.Navigation(order => order.History).AutoInclude();
        });

        builder.Entity<OrderLine>(entity =>
        {
            entity.ToTable("order_line");
            entity.HasKey(line => line.Id);
            entity.Property(line => line.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(line => line.OrderId).HasColumnName("order_id");
            entity.Property(line => line.MenuItemId).HasColumnName("menu_item_id");
            entity.Property(line => line.ItemName).HasColumnName("item_name");
            entity.Property(line => line.OptionSummary).HasColumnName("option_summary");
            entity.Property(line => line.Quantity).HasColumnName("quantity");
            entity.Property(line => line.UnitPrice).HasColumnName("unit_price").HasConversion(MoneyConverter);
            entity.Property(line => line.LineTotal).HasColumnName("line_total").HasConversion(MoneyConverter);
        });

        builder.Entity<OrderStatusChange>(entity =>
        {
            entity.ToTable("order_status_change");
            entity.HasKey(change => change.Id);
            entity.Property(change => change.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(change => change.OrderId).HasColumnName("order_id");
            entity.Property(change => change.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(change => change.Note).HasColumnName("note");
            entity.Property(change => change.ChangedAt).HasColumnName("changed_at");
        });

        builder.Entity<PromoCode>(entity =>
        {
            entity.ToTable("promo_code");
            entity.HasKey(promo => promo.Id);
            entity.Property(promo => promo.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(promo => promo.Code).HasColumnName("code");
            entity.Property(promo => promo.Kind).HasColumnName("kind").HasConversion<string>();
            entity.Property(promo => promo.Percentage).HasColumnName("percentage");
            entity.Property(promo => promo.FixedAmount).HasColumnName("fixed_amount").HasConversion(MoneyConverter);
            entity.Property(promo => promo.MinimumSubtotal).HasColumnName("minimum_subtotal")
                .HasConversion(MoneyConverter);
            entity.Property(promo => promo.ValidUntil).HasColumnName("valid_until");
            entity.Property(promo => promo.MaxRedemptions).HasColumnName("max_redemptions");
            entity.Property(promo => promo.TimesRedeemed).HasColumnName("times_redeemed");
            entity.Property(promo => promo.OncePerCustomer).HasColumnName("once_per_customer");
            entity.Property(promo => promo.RestaurantId).HasColumnName("restaurant_id");
        });
    }

    private static readonly System.Linq.Expressions.Expression<Func<Money, decimal>> ToDecimal =
        money => money.Amount;

    private static readonly System.Linq.Expressions.Expression<Func<decimal, Money>> ToMoney =
        amount => new Money(amount);

    private static Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Money, decimal>
        MoneyConverter => new(ToDecimal, ToMoney);

    private static Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Money?, decimal?>
        NullableMoneyConverter => new(
            money => money == null ? null : money.Value.Amount,
            amount => amount == null ? null : new Money(amount.Value));
}
