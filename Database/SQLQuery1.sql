select * from Stock
select * from Users
select * from Addresses

update Users 
set Active = 1
where UserId = 'DBFEC735-AEFF-484B-8A3C-EFC92247E2E0'

select * from PromoCodes

select * from Logs
delete from PromoCodes 

delete from Users
where UserId = '15C095AD-D196-49BF-B2A2-8995DC6B3E81'
select count(*) from Users

select count(*) from Products
select * from Products

select * from Payments
select * from Orders



select * from Refunds
delete from Refunds where OrderId = 'A22DF22F-2606-4262-A1DA-4A5DF0746EE2'

select * from Logs
order by CreatedAt desc

select * from Refunds



select * from Category
select * from Users
select * from UserDetails	

select * from WishList
select * from WishListItems

select * from Carts
select * from CartItems

select * from Orders
select * from OrderDetails

select * from Reviews

update orders
set Status = 'Delivered'


-- delete from UserDetails
-- delete from Addresses
delete from users
delete from Refunds
delete from Payments
delete from Addresses
delete from orders
delete from OrderDetails
delete from Carts
delete from CartItems
delete from Reviews
delete from Logs
delete from Category
delete from Products
delete from Stock
delete from WishListItems

-- delete from logs

-- User - Payment -> One to many ==
-- User - Refund -> one to many ==
-- User - WishList -> One to many ==
-- WishList - WishListItems -> one to many ==
-- WishListItems - Products -> many to many =
-- Payment - Order -> one to one =
-- Payment - Refund -> one to one

--update Stock
--set Quantity = 100

--update Orders
--set Status = 'Not Delivered'

--delete from products
--delete from Stock
--delete from CartItems

--delete from Carts

--delete from OrderDetails
--delete from Orders
--delete from Payments

--delete from Refunds
--delete from UserDetails

--delete from UserDetails
--delete from Users

--delete from Addresses
--delete from reviews
--delete from Orders

--delete from Orders
--delete from OrderDetails


--INSERT INTO Products (CategoryId, Name, ImagePath, Description, Price)
--VALUES
--('B9FA9336-6EC3-4578-9BFD-DEE49AFB4884','Essence Mascara Lash Princess','https://cdn.dummyjson.com/product-images/beauty/essence-mascara-lash-princess/1.webp','Volumizing and lengthening mascara',9.99),

--('B9FA9336-6EC3-4578-9BFD-DEE49AFB4884','Eyeshadow Palette with Mirror','https://cdn.dummyjson.com/product-images/beauty/eyeshadow-palette-with-mirror/1.webp','Eyeshadow palette with mirror',19.99),

--('B9FA9336-6EC3-4578-9BFD-DEE49AFB4884','Powder Canister','https://cdn.dummyjson.com/product-images/beauty/powder-canister/1.webp','Setting powder with matte finish',14.99),

--('B9FA9336-6EC3-4578-9BFD-DEE49AFB4884','Red Lipstick','https://cdn.dummyjson.com/product-images/beauty/red-lipstick/1.webp','Creamy red lipstick',12.99),

--('B9FA9336-6EC3-4578-9BFD-DEE49AFB4884','Red Nail Polish','https://cdn.dummyjson.com/product-images/beauty/red-nail-polish/1.webp','Glossy red nail polish',8.99),

--('B9FA9336-6EC3-4578-9BFD-DEE49AFB4884','Calvin Klein CK One','https://cdn.dummyjson.com/product-images/fragrances/calvin-klein-ck-one/1.webp','Fresh unisex fragrance',49.99),

--('B9FA9336-6EC3-4578-9BFD-DEE49AFB4884','Chanel Coco Noir Eau De','https://cdn.dummyjson.com/product-images/fragrances/chanel-coco-noir-eau-de/1.webp','Elegant evening fragrance',129.99),

--('B9FA9336-6EC3-4578-9BFD-DEE49AFB4884','Dior J adore','https://cdn.dummyjson.com/product-images/fragrances/dior-j adore/1.webp','Floral luxury fragrance',89.99),

--('B9FA9336-6EC3-4578-9BFD-DEE49AFB4884','Dolce Shine Eau De','https://cdn.dummyjson.com/product-images/fragrances/dolce-shine-eau-de/1.webp','Fruity vibrant fragrance',69.99),

--('B9FA9336-6EC3-4578-9BFD-DEE49AFB4884','Gucci Bloom Eau De','https://cdn.dummyjson.com/product-images/fragrances/gucci-bloom-eau-de/1.webp','Romantic floral fragrance',79.99),

--('6CDA086A-E4FF-437C-8594-BFA848F0D74A','Annibale Colombo Bed','https://cdn.dummyjson.com/product-images/furniture/annibale-colombo-bed/1.webp','Luxury bed frame',1899.99),

--('6CDA086A-E4FF-437C-8594-BFA848F0D74A','Annibale Colombo Sofa','https://cdn.dummyjson.com/product-images/furniture/annibale-colombo-sofa/1.webp','Premium sofa',2499.99),

--('6CDA086A-E4FF-437C-8594-BFA848F0D74A','Bedside Table African Cherry','https://cdn.dummyjson.com/product-images/furniture/bedside-table-african-cherry/1.webp','Stylish bedside table',299.99),

--('6CDA086A-E4FF-437C-8594-BFA848F0D74A','Knoll Saarinen Executive Conference Chair','https://cdn.dummyjson.com/product-images/furniture/knoll-saarinen-executive-conference-chair/1.webp','Ergonomic office chair',499.99),

--('6CDA086A-E4FF-437C-8594-BFA848F0D74A','Wooden Bathroom Sink With Mirror','https://cdn.dummyjson.com/product-images/furniture/wooden-bathroom-sink-with-mirror/1.webp','Bathroom sink with mirror',799.99),

--('653BCA28-BE62-4C4E-AA9E-CAEF3BE028C4','Apple','https://cdn.dummyjson.com/product-images/groceries/apple/1.webp','Fresh apples',1.99),

--('653BCA28-BE62-4C4E-AA9E-CAEF3BE028C4','Beef Steak','https://cdn.dummyjson.com/product-images/groceries/beef-steak/1.webp','High quality beef steak',12.99),

--('653BCA28-BE62-4C4E-AA9E-CAEF3BE028C4','Cat Food','https://cdn.dummyjson.com/product-images/groceries/cat-food/1.webp','Nutritious cat food',8.99),

--('653BCA28-BE62-4C4E-AA9E-CAEF3BE028C4','Chicken Meat','https://cdn.dummyjson.com/product-images/groceries/chicken-meat/1.webp','Fresh chicken meat',9.99),

--('653BCA28-BE62-4C4E-AA9E-CAEF3BE028C4','Cooking Oil','https://cdn.dummyjson.com/product-images/groceries/cooking-oil/1.webp','Multipurpose cooking oil',4.99),

--('653BCA28-BE62-4C4E-AA9E-CAEF3BE028C4','Cucumber','https://cdn.dummyjson.com/product-images/groceries/cucumber/1.webp','Fresh cucumber',1.49),

--('653BCA28-BE62-4C4E-AA9E-CAEF3BE028C4','Dog Food','https://cdn.dummyjson.com/product-images/groceries/dog-food/1.webp','Dog nutrition food',10.99),

--('653BCA28-BE62-4C4E-AA9E-CAEF3BE028C4','Eggs','https://cdn.dummyjson.com/product-images/groceries/eggs/1.webp','Fresh eggs',2.99),

--('653BCA28-BE62-4C4E-AA9E-CAEF3BE028C4','Fish Steak','https://cdn.dummyjson.com/product-images/groceries/fish-steak/1.webp','Fish steak',14.99),

--('653BCA28-BE62-4C4E-AA9E-CAEF3BE028C4','Green Bell Pepper','https://cdn.dummyjson.com/product-images/groceries/green-bell-pepper/1.webp','Green bell pepper',1.29),

--('653BCA28-BE62-4C4E-AA9E-CAEF3BE028C4','Green Chili Pepper','https://cdn.dummyjson.com/product-images/groceries/green-chili-pepper/1.webp','Spicy chili pepper',0.99);



--INSERT INTO Stock (ProductId, Quantity) VALUES ('0D78DCD3-628C-4F64-878F-01E2C3EC3D54', 120);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('51DE8804-4D89-4046-9918-08B8A5AB459A', 15);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('BC7931A8-7641-4A69-B6FE-09691D3B4FD0', 8);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('43D214B0-394F-483A-BC85-0C695167B1F7', 60);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('FEFECA38-59BC-48B1-9E24-0CA776A3FC21', 50);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('B0293BB1-2E51-4316-AA50-0E6D83A6F351', 40);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('09E92916-803C-4EEE-916B-1D0A0DC5314F', 90);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('264D6E10-370E-4A51-9451-39CAFDCA01E0', 6);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('BEEA56F5-0B6C-4790-9952-6480DFBFF29E', 200);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('75CFBD51-99CF-4D82-82FF-70FC18CF9939', 150);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('7D5FE242-73B7-42BC-A5C0-718D68EA1E5D', 35);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('F81144D0-E3D8-4DBA-97F9-7A6804F5751D', 30);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('7EC3DDE1-E20F-4C53-980B-7A69B5487BE8', 45);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('BB7094BB-0546-4150-B749-81DAE66BE1A0', 100);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('1BD14CC4-D261-4C73-9037-89F3F4ABA63A', 70);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('9B161467-0312-4358-B4AC-9B2345F1C203', 120);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('78734743-D66F-4489-854B-9EFCEA6D3108', 25);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('CDC238B6-B084-4A54-9E68-9FC27EEB5707', 5);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('22370DE9-2E86-4953-8F51-A15D8926579D', 140);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('F7ECF731-8C44-4E04-B8CB-ACEEEEB3A26C', 50);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('FCACB67F-8B1A-4F23-95D5-B60D3A582E2A', 85);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('BFF73A72-443D-4D1C-816E-B8E1FB1A723F', 35);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('20C08473-8BC4-4265-8D8E-BA6498D25BF2', 12);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('D0EB6C8E-CF92-4645-BACF-C3BDC6E4746E', 110);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('FB71B9B2-31E8-4429-87C0-F28D04FE71AC', 90);
--INSERT INTO Stock (ProductId, Quantity) VALUES ('DA259070-1F1A-42A6-A015-FE1500AD9219', 40);