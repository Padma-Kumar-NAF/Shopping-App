
select * from Addresses

select * from Carts
select * from CartItems

select * from Category

select * from Logs
order by CreatedAt desc

update products
set ActiveStatus = 1

select * from Orders
select * from Payments
select * from Wallets
select * from Refunds

update Wallets
set WalletAmount = 6480

update orders 
set DeliveryDate = '2026-03-08 09:55:48.1612008'

delete from Refunds
delete from Payments
delete from OrderDetails
delete from Orders

select * from orders
select * from OrderDetails

select * from Payments

select * from Products
update Products
set ActiveStatus = 1

select * from PromoCodes
update PromoCodes set IsDeleted = 0

select * from Stock

select * from Refunds

select * from Reviews

--update Stock set Quantity = 100

select * from Users
select * from UserDetails

select * from UserMonthlyProductLimit
select * from UserPromoCodes
--delete from UserPromoCodes

select * from Wallets

select * from WishList
select * from WishListItems

insert into wallets (userId,WalletAmount)
values ('02E91748-23E7-467C-95EB-ED14A25E7ED9',3000);

update Users
set role = 'admin'
where UserId = '8D078F09-8A5C-43B6-8516-7C0E3A0AC2AD'

update orders
set Status = 'Delivered'

update Wallets
set WalletAmount = 1200

delete from stock
delete from Products

--delete from UserDetails
--delete from Addresses 
--delete from UserDetails
--delete from users

--delete from Addresses where UserId = 'F56106AD-FEE7-4BD2-B761-4389FC32DCB1'
--delete from UserDetails where userId = 'F56106AD-FEE7-4BD2-B761-4389FC32DCB1'
--delete from Users where UserId = 'F56106AD-FEE7-4BD2-B761-4389FC32DCB1'


--delete from Products where ProductId = '8D4F5791-5D11-4238-BDCA-F1AF347ADE6E'
--delete from Stock where ProductId = '8D4F5791-5D11-4238-BDCA-F1AF347ADE6E'

--delete from users where UserId = '7ECF76DC-2AF8-4A32-BBF4-097C7468FDE6'

--delete from PromoCodes
--delete from OrderDetails

--delete from payments
--delete from Refunds
--delete from Wallets

--delete from Refunds
--delete from Payments
--delete from OrderDetails
--delete from Orders

--delete from Payments

-- delete from UserDetails
-- delete from Addresses
--delete from users
--delete from Refunds
--delete from Payments
--delete from Addresses
--delete from orders
--delete from OrderDetails
--delete from Carts
--delete from CartItems
--delete from Reviews
--delete from Logs
--delete from Category
--delete from Products
--delete from Stock
--delete from WishListItems
--delete from WishList

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

INSERT INTO Products (
    CategoryId,
    Name,
    ImagePath,
    Description,
    Price,
    ActiveStatus
)
VALUES

-- Beauty (4)
('28897FBE-68D8-4172-BFE6-21AE6BB0CBAE','Essence Mascara Lash Princess','/images/products/beauty/mascara.jpg','Volumizing mascara',200,1),
('28897FBE-68D8-4172-BFE6-21AE6BB0CBAE','Eyeshadow Palette','/images/products/beauty/eyeshadow.jpg','Colorful eyeshadow palette',499,1),
('28897FBE-68D8-4172-BFE6-21AE6BB0CBAE','Lipstick Matte','/images/products/beauty/lipstick.jpg','Long lasting matte lipstick',300,1),
('28897FBE-68D8-4172-BFE6-21AE6BB0CBAE','Face Serum','/images/products/beauty/serum.jpg','Hydrating face serum',1000,1),

-- Fragrances (3)
('8B6C4647-9885-449D-A5F4-6004D6E9B948','Perfume Oil','/images/products/fragrances/perfume-oil.jpg','Long-lasting perfume oil',500,1),
('8B6C4647-9885-449D-A5F4-6004D6E9B948','Luxury Perfume','/images/products/fragrances/luxury-perfume.jpg','Premium fragrance',2000,1),
('8B6C4647-9885-449D-A5F4-6004D6E9B948','Body Spray','/images/products/fragrances/body-spray.jpg','Refreshing body spray',499,1),

-- Furniture (4)
('D15F212A-0D86-49C9-B437-966B01473123','Wooden Bed','/images/products/furniture/bed.jpg','Comfortable wooden bed',5999,1),
('D15F212A-0D86-49C9-B437-966B01473123','Office Chair','/images/products/furniture/chair.jpg','Ergonomic office chair',2000,1),
('D15F212A-0D86-49C9-B437-966B01473123','Dining Table','/images/products/furniture/table.jpg','Wooden dining table',5000,1),
('D15F212A-0D86-49C9-B437-966B01473123','Sofa Set','/images/products/furniture/sofa.jpg','Comfortable sofa set',5999,1),

-- Groceries (4)
('DB9A97D5-16DE-4E4A-8F08-A1EDC1E71B64','Apple','/images/products/groceries/apple.jpg','Fresh apples',20,1),
('DB9A97D5-16DE-4E4A-8F08-A1EDC1E71B64','Banana','/images/products/groceries/banana.jpg','Fresh bananas',30,1),
('DB9A97D5-16DE-4E4A-8F08-A1EDC1E71B64','Milk','/images/products/groceries/milk.jpg','Dairy milk',50,1),
('DB9A97D5-16DE-4E4A-8F08-A1EDC1E71B64','Bread','/images/products/groceries/bread.jpg','Fresh bread loaf',40,1);