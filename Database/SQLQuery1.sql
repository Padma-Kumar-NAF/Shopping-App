select * from Stock
select * from Category
select * from Products

select * from Users
select * from UserDetails

select * from Carts
select * from CartItems

select * from Addresses

select * from Orders
select * from OrderDetails

select * from Reviews

delete from Orders
delete from OrderDetails

delete from Users
where UserId = '8F5FBFF8-ABD1-449E-89E4-F967B6B66FA3'

UPDATE Orders
SET TotalAmount = 1098,
    TotalProductsCount = 2
WHERE OrderId = 'F654FDA4-4A95-4251-9F8E-D5ED9F38552B';

delete from Orders
where TotalProductsCount = 1

Select * from Carts 
where UserId = '7D5C9FBB-96C5-4922-9EB2-0EE165B3A44A'

select * from Products
where ProductId = '9B86691D-998C-4290-B203-1E66BAE9F1D3'

INSERT INTO Carts (UserId)
VALUES ('7D5C9FBB-96C5-4922-9EB2-0EE165B3A44A');

DECLARE @CartId UNIQUEIDENTIFIER;

SELECT @CartId = CartId
FROM Carts
WHERE UserId = '7D5C9FBB-96C5-4922-9EB2-0EE165B3A44A';

insert into OrderDetails
(OrderId , ProductId , ProductName , Quantity , ProductPrice)
values(
'F654FDA4-4A95-4251-9F8E-D5ED9F38552B',
'40591F10-D29C-4649-94AE-38B41B0BA0B7',
'Basmati Rice 5kg',
1,
699
)

Insert Into Orders
(UserId , Status , TotalProductsCount , TotalAmount , AddressId)
Values('1B6B8DBB-3D30-423E-BF3F-9035772CA730',
'Not Delivered',
1,
399,
'ED00BDCF-BCAF-433D-8E9E-1BFCB24B7EEC'
);

INSERT INTO Addresses
(UserId, AddressLine1, AddressLine2, State, City, Pincode)
VALUES

-- Padma
('DAF096B8-0C48-4574-9E6F-02C19503874A',
 'Flat No 12, MG Residency',
 'Near Metro Station, Andheri East',
 'Maharashtra',
 'Mumbai',
 '400069'
),

-- Padma Kumar
('7D5C9FBB-96C5-4922-9EB2-0EE165B3A44A',
 'No 45, Anna Salai',
 'Opposite Express Avenue Mall',
 'Tamil Nadu',
 'Chennai',
 '600002'
),

-- Kumar
('1B6B8DBB-3D30-423E-BF3F-9035772CA730',
 '78 Residency Road',
 'Near Brigade Road Signal',
 'Karnataka',
 'Bengaluru',
 '560025'
);

INSERT INTO UserDetails
( UserId, Name, Email, PhoneNumber, AddressLine1, AddressLine2, State, City, Pincode)
VALUES
( 'DAF096B8-0C48-4574-9E6F-02C19503874A',
 'Padma',
 'dcd@gmail.com',
 '9876543210',
 '12 MG Road',
 'Near Metro Station',
 'Maharashtra',
 'Mumbai',
 '400001'
),

('7D5C9FBB-96C5-4922-9EB2-0EE165B3A44A',
 'Padma Kumar',
 'abc@gmail.com',
 '9123456780',
 '45 Anna Salai',
 'Opposite City Mall',
 'Tamil Nadu',
 'Chennai',
 '600002'
),

('1B6B8DBB-3D30-423E-BF3F-9035772CA730',
 'Kumar',
 'efg@gmail.com',
 '9988776655',
 '78 Residency Road',
 'Near Park',
 'Karnataka',
 'Bengaluru',
 '560025');

INSERT INTO CartItems (CartId, ProductId, Quantity)
VALUES
(@CartId, '9B86691D-998C-4290-B203-1E66BAE9F1D3', 1),
(@CartId, 'E0244232-AAE2-43ED-9523-477F47B9E453', 1),
(@CartId, '2AE00D55-29DA-44EB-90F8-6FAB1AF56348', 2);

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