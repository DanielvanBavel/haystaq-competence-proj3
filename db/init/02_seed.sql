-- Vaste gegevens waar de user stories en de tests naar verwijzen.

insert into restaurant (id, slug, name, cuisine, city, rating, estimated_delivery_minutes,
                        minimum_order, delivery_fee, free_delivery_from, is_open)
values ('a1000000-0000-0000-0000-000000000001', 'pizzeria-de-vuurplaat', 'Pizzeria De Vuurplaat',
        'Italiaans', 'Breda', 4.6, 35, 15.00, 2.50, 30.00, true),
       ('a1000000-0000-0000-0000-000000000002', 'sushi-noord', 'Sushi Noord',
        'Japans', 'Breda', 4.8, 45, 20.00, 3.50, 45.00, true),
       ('a1000000-0000-0000-0000-000000000003', 'burgerhuis-halve-maan', 'Burgerhuis Halve Maan',
        'Amerikaans', 'Breda', 4.2, 25, 12.50, 1.95, null, true),
       ('a1000000-0000-0000-0000-000000000004', 'thai-orchidee', 'Thai Orchidee',
        'Thais', 'Tilburg', 4.4, 40, 17.50, 2.95, 35.00, true),
       ('a1000000-0000-0000-0000-000000000005', 'shoarma-plein', 'Shoarma Plein',
        'Turks', 'Breda', 3.9, 20, 10.00, 1.50, null, true),
       ('a1000000-0000-0000-0000-000000000006', 'de-groene-pan', 'De Groene Pan',
        'Vegetarisch', 'Breda', 4.7, 30, 15.00, 2.50, 25.00, false);

insert into menu_item (id, restaurant_id, name, description, category, price, is_available, is_vegetarian,
                       spiciness_level)
values ('b1000000-0000-0000-0000-000000000101', 'a1000000-0000-0000-0000-000000000001', 'Margherita',
        'Tomatensaus, mozzarella, basilicum', 'Pizza', 9.50, true, true, 0),
       ('b1000000-0000-0000-0000-000000000102', 'a1000000-0000-0000-0000-000000000001', 'Salame Piccante',
        'Pittige salami en chilivlokken', 'Pizza', 12.50, true, false, 2),
       ('b1000000-0000-0000-0000-000000000103', 'a1000000-0000-0000-0000-000000000001', 'Quattro Formaggi',
        'Vier kazen', 'Pizza', 13.50, true, true, 0),
       ('b1000000-0000-0000-0000-000000000104', 'a1000000-0000-0000-0000-000000000001', 'Tiramisu',
        'Huisgemaakt', 'Dessert', 5.50, true, true, 0),
       ('b1000000-0000-0000-0000-000000000105', 'a1000000-0000-0000-0000-000000000001', 'Truffelpizza',
        'Alleen in het seizoen', 'Pizza', 18.50, false, true, 0),
       ('b1000000-0000-0000-0000-000000000106', 'a1000000-0000-0000-0000-000000000001', 'Cola',
        null, 'Drinken', 2.75, true, true, 0),

       ('b1000000-0000-0000-0000-000000000201', 'a1000000-0000-0000-0000-000000000002', 'Salmon set',
        '12 stuks', 'Sushi', 18.50, true, false, 0),
       ('b1000000-0000-0000-0000-000000000202', 'a1000000-0000-0000-0000-000000000002', 'Veggie set',
        '10 stuks', 'Sushi', 16.00, true, true, 0),
       ('b1000000-0000-0000-0000-000000000203', 'a1000000-0000-0000-0000-000000000002', 'Miso soep',
        null, 'Voorgerecht', 4.25, true, true, 0),

       ('b1000000-0000-0000-0000-000000000301', 'a1000000-0000-0000-0000-000000000003', 'Klassieke burger',
        'Rundvlees, cheddar, augurk', 'Burgers', 11.00, true, false, 0),
       ('b1000000-0000-0000-0000-000000000302', 'a1000000-0000-0000-0000-000000000003', 'Portobello burger',
        'Vegetarisch', 'Burgers', 10.50, true, true, 0),
       ('b1000000-0000-0000-0000-000000000303', 'a1000000-0000-0000-0000-000000000003', 'Friet',
        null, 'Bijgerecht', 3.25, true, true, 0),

       ('b1000000-0000-0000-0000-000000000401', 'a1000000-0000-0000-0000-000000000004', 'Pad Thai',
        'Met kip', 'Wok', 14.50, true, false, 1),
       ('b1000000-0000-0000-0000-000000000402', 'a1000000-0000-0000-0000-000000000004', 'Groene curry',
        'Pittig', 'Curry', 15.50, true, false, 3),

       ('b1000000-0000-0000-0000-000000000501', 'a1000000-0000-0000-0000-000000000005', 'Shoarma broodje',
        null, 'Broodjes', 7.50, true, false, 1),
       ('b1000000-0000-0000-0000-000000000502', 'a1000000-0000-0000-0000-000000000005', 'Falafel broodje',
        null, 'Broodjes', 7.00, true, true, 1),

       ('b1000000-0000-0000-0000-000000000601', 'a1000000-0000-0000-0000-000000000006', 'Bloemkoolsteak',
        null, 'Hoofdgerecht', 16.50, true, true, 0);

-- Opties: maat (verplicht kiezen) en extra's (optioneel).
insert into menu_item_option (id, menu_item_id, name, kind, price_delta, is_default)
values ('c1000000-0000-0000-0000-000000000001', 'b1000000-0000-0000-0000-000000000101', 'Klein (26 cm)', 'Size', 0.00, true),
       ('c1000000-0000-0000-0000-000000000002', 'b1000000-0000-0000-0000-000000000101', 'Groot (32 cm)', 'Size', 3.00, false),
       ('c1000000-0000-0000-0000-000000000003', 'b1000000-0000-0000-0000-000000000101', 'Extra kaas', 'Extra', 1.50, false),
       ('c1000000-0000-0000-0000-000000000004', 'b1000000-0000-0000-0000-000000000101', 'Verse basilicum', 'Extra', 0.75, false),

       ('c1000000-0000-0000-0000-000000000005', 'b1000000-0000-0000-0000-000000000102', 'Klein (26 cm)', 'Size', 0.00, true),
       ('c1000000-0000-0000-0000-000000000006', 'b1000000-0000-0000-0000-000000000102', 'Groot (32 cm)', 'Size', 3.00, false),
       ('c1000000-0000-0000-0000-000000000007', 'b1000000-0000-0000-0000-000000000102', 'Extra pittig', 'Extra', 0.00, false),

       ('c1000000-0000-0000-0000-000000000008', 'b1000000-0000-0000-0000-000000000301', 'Extra patty', 'Extra', 3.50, false),
       ('c1000000-0000-0000-0000-000000000009', 'b1000000-0000-0000-0000-000000000301', 'Bacon', 'Extra', 1.75, false),

       ('c1000000-0000-0000-0000-000000000010', 'b1000000-0000-0000-0000-000000000303', 'Klein', 'Size', 0.00, true),
       ('c1000000-0000-0000-0000-000000000011', 'b1000000-0000-0000-0000-000000000303', 'Groot', 'Size', 1.25, false),
       ('c1000000-0000-0000-0000-000000000012', 'b1000000-0000-0000-0000-000000000303', 'Mayonaise', 'Extra', 0.60, false);

insert into promo_code (id, code, kind, percentage, fixed_amount, minimum_subtotal, valid_until,
                        max_redemptions, times_redeemed, once_per_customer, restaurant_id)
values ('d1000000-0000-0000-0000-000000000001', 'WELKOM10', 'Percentage', 10, 0.00, 20.00,
        current_date + 90, 1000, 0, true, null),
       ('d1000000-0000-0000-0000-000000000002', 'GRATISBEZORGD', 'FreeDelivery', 0, 0.00, 15.00,
        current_date + 30, 1000, 0, false, null),
       ('d1000000-0000-0000-0000-000000000003', 'VIJFEURO', 'FixedAmount', 0, 5.00, 25.00,
        current_date + 14, 1000, 0, false, null),
       ('d1000000-0000-0000-0000-000000000004', 'PIZZA20', 'Percentage', 20, 0.00, 30.00,
        current_date + 60, 1000, 0, false, 'a1000000-0000-0000-0000-000000000001'),
       ('d1000000-0000-0000-0000-000000000005', 'ZOMER2024', 'Percentage', 15, 0.00, 0.00,
        current_date - 5, 1000, 0, false, null),
       ('d1000000-0000-0000-0000-000000000006', 'OPOP', 'FixedAmount', 0, 7.50, 20.00,
        current_date + 30, 1, 1, false, null);
