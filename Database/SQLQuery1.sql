select * from Stock
select * from Category
select * from Products

INSERT INTO Category (CategoryName)
VALUES
('Electronics'),
('Fashion'),
('Groceries'),
('Home Appliances'),
('Books'),
('Beauty & Personal Care'),
('Sports & Fitness');

INSERT INTO Products (CategoryId, Name, ImagePath, Description, Price)
VALUES
('22F82DDB-9081-4903-901A-E9DDC2406CEE',
 'iPhone 15',
 '/images/products/iphone15.jpg',
 'Latest Apple smartphone with A17 chip and improved battery performance',
 79999.00),

('22F82DDB-9081-4903-901A-E9DDC2406CEE',
 'Samsung 55 Inch 4K TV',
 '/images/products/samsung-tv.jpg',
 'Ultra HD Smart TV with HDR and Dolby Audio',
 55999.00),

('C97494A6-4139-4AE2-B659-525828C2DB5C',
 'Men Slim Fit T-Shirt',
 '/images/products/tshirt.jpg',
 'Cotton slim fit casual t-shirt',
 799.00),

('C97494A6-4139-4AE2-B659-525828C2DB5C',
 'Women Running Shoes',
 '/images/products/shoes.jpg',
 'Lightweight breathable sports shoes',
 2499.00),

('2C613B87-5F61-416D-A46B-73EF6A14ED1A',
 'Clean Code',
 '/images/products/cleancode.jpg',
 'A Handbook of Agile Software Craftsmanship',
 499.00),

('2C613B87-5F61-416D-A46B-73EF6A14ED1A',
 'Atomic Habits',
 '/images/products/atomichabits.jpg',
 'An Easy & Proven Way to Build Good Habits',
 399.00),

('CD690BB0-64AB-4276-B8FF-9777FC20BDE7',
 'Basmati Rice 5kg',
 '/images/products/rice.jpg',
 'Premium long grain basmati rice',
 699.00),

('CD690BB0-64AB-4276-B8FF-9777FC20BDE7',
 'Sunflower Oil 1L',
 '/images/products/oil.jpg',
 'Refined sunflower cooking oil',
 149.00),

('AEBFFF3F-976F-49A6-A152-3DEA5A451D66',
 'LG Double Door Refrigerator',
 '/images/products/fridge.jpg',
 'Frost free inverter refrigerator',
 38999.00),

('5DFA807A-AD29-4FF1-900A-100988C58B29',
 'Dumbbell Set 10kg',
 '/images/products/dumbbell.jpg',
 'Adjustable iron dumbbell set',
 1999.00),

('7F6212B2-8C0A-48FC-8ABF-9B8F0ACA557B',
 'Aloe Vera Face Wash',
 '/images/products/facewash.jpg',
 'Natural aloe vera face cleanser',
 249.00);


INSERT INTO Stock (ProductId, Quantity)
VALUES

((SELECT ProductId FROM Products WHERE Name = 'iPhone 15'), 120),
((SELECT ProductId FROM Products WHERE Name = 'Samsung 55 Inch 4K TV'), 40),

((SELECT ProductId FROM Products WHERE Name = 'Men Slim Fit T-Shirt'), 200),
((SELECT ProductId FROM Products WHERE Name = 'Women Running Shoes'), 150),

((SELECT ProductId FROM Products WHERE Name = 'Clean Code'), 90),
((SELECT ProductId FROM Products WHERE Name = 'Atomic Habits'), 110),

((SELECT ProductId FROM Products WHERE Name = 'Basmati Rice 5kg'), 300),
((SELECT ProductId FROM Products WHERE Name = 'Sunflower Oil 1L'), 500),

((SELECT ProductId FROM Products WHERE Name = 'LG Double Door Refrigerator'), 25),

((SELECT ProductId FROM Products WHERE Name = 'Dumbbell Set 10kg'), 75),

((SELECT ProductId FROM Products WHERE Name = 'Aloe Vera Face Wash'), 180);