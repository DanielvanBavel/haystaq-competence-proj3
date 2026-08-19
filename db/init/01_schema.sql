-- BezorgBaas - schema
-- Beheerd met SQL-scripts; EF Core mapt hierop en maakt zelf niets aan.

create table restaurant (
    id                         uuid primary key,
    slug                       varchar(120) not null unique,
    name                       varchar(160) not null,
    cuisine                    varchar(60)  not null,
    city                       varchar(80)  not null,
    rating                     numeric(2, 1) not null check (rating between 0 and 5),
    estimated_delivery_minutes integer      not null check (estimated_delivery_minutes > 0),
    minimum_order              numeric(8, 2) not null check (minimum_order >= 0),
    delivery_fee               numeric(8, 2) not null check (delivery_fee >= 0),
    free_delivery_from         numeric(8, 2) check (free_delivery_from is null or free_delivery_from >= 0),
    is_open                    boolean      not null default true
);

create table menu_item (
    id              uuid primary key,
    restaurant_id   uuid          not null references restaurant (id) on delete cascade,
    name            varchar(120)  not null,
    description     varchar(400),
    category        varchar(60)   not null,
    price           numeric(8, 2) not null check (price >= 0),
    is_available    boolean       not null default true,
    is_vegetarian   boolean       not null default false,
    spiciness_level integer       not null default 0 check (spiciness_level between 0 and 3)
);

create table menu_item_option (
    id           uuid primary key,
    menu_item_id uuid          not null references menu_item (id) on delete cascade,
    name         varchar(80)   not null,
    kind         varchar(10)   not null check (kind in ('Size', 'Extra')),
    price_delta  numeric(8, 2) not null check (price_delta >= 0),
    is_default   boolean       not null default false
);

create table promo_code (
    id                uuid primary key,
    code              varchar(30)   not null unique,
    kind              varchar(20)   not null check (kind in ('Percentage', 'FixedAmount', 'FreeDelivery')),
    percentage        integer       not null default 0 check (percentage between 0 and 100),
    fixed_amount      numeric(8, 2) not null default 0,
    minimum_subtotal  numeric(8, 2) not null default 0,
    valid_until       date          not null,
    max_redemptions   integer       not null default 1000,
    times_redeemed    integer       not null default 0,
    once_per_customer boolean       not null default false,
    restaurant_id     uuid references restaurant (id) on delete cascade
);

create table customer_order (
    id                  uuid primary key,
    order_number        varchar(20)   not null unique,
    restaurant_id       uuid          not null references restaurant (id),
    customer_name       varchar(120)  not null,
    customer_email      varchar(160)  not null,
    address_street      varchar(160)  not null,
    address_house_number varchar(10)  not null,
    address_postal_code varchar(10)   not null,
    address_city        varchar(80)   not null,
    address_note        varchar(200),
    delivery_date       date          not null,
    delivery_slot_start time          not null,
    delivery_slot_end   time          not null,
    payment_method      varchar(10)   not null check (payment_method in ('Ideal', 'Card', 'Cash')),
    payment_reference   varchar(40),
    status              varchar(12)   not null check (status in
        ('Placed', 'Accepted', 'Preparing', 'OnTheWay', 'Delivered', 'Cancelled', 'Rejected')),
    subtotal            numeric(8, 2) not null,
    delivery_fee        numeric(8, 2) not null,
    discount            numeric(8, 2) not null default 0,
    total               numeric(8, 2) not null,
    promo_code          varchar(30),
    placed_at           timestamptz   not null
);

create table order_line (
    id             uuid primary key,
    order_id       uuid          not null references customer_order (id) on delete cascade,
    menu_item_id   uuid          not null,
    item_name      varchar(120)  not null,
    option_summary varchar(300),
    quantity       integer       not null check (quantity between 1 and 20),
    unit_price     numeric(8, 2) not null,
    line_total     numeric(8, 2) not null
);

create table order_status_change (
    id         uuid primary key,
    order_id   uuid        not null references customer_order (id) on delete cascade,
    status     varchar(12) not null,
    note       varchar(300),
    changed_at timestamptz not null
);

create index menu_item_restaurant_idx on menu_item (restaurant_id);
create index order_restaurant_idx on customer_order (restaurant_id);
create index order_customer_idx on customer_order (customer_email);
