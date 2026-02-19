-- Users table
--user_id , user_name , user_phone_number , is_active , is_created_account , address_line_1 , address_line_2 ,
-- user_state , user_pincode , user_city , created_at
    
create table Users(
user_id uniqueidentifier default newid() primary key,
user_name varchar(50) not null,
user_phone_number text not null,
is_active int,
address_line_1 varchar(50),
address_line_2 varchar(50),
user_state varchar(20),
user_pincode varchar(6),
user_city varchar(20),
created_at DATETIME NOT NULL DEFAULT GETDATE()
)

-- Category
-- category_id , category_name , created_at

create table Category(
category_id uniqueidentifier default newid() primary key,
category_name varchar(50) not null,
created_at DATETIME NOT NULL DEFAULT GETDATE()
)

select * from Category

-- Products Table
-- product_id , product_name , product_category , product_price , product_image , created_at

CREATE TABLE Products (
    product_id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    product_name VARCHAR(50) NOT NULL,
    product_category UNIQUEIDENTIFIER,
    product_price int,
    product_image VARCHAR(255),
    created_at DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Product_Category
        FOREIGN KEY (product_category) REFERENCES Category(category_id)
);

-- Cart Table
-- cart_id , user_id , product_id , category_id , created_at ,

create table Cart (
cart_id UNIQUEIDENTIFIER default newid() primary key,
user_id UNIQUEIDENTIFIER,
product_id UNIQUEIDENTIFIER,

constraint FK_User_Id 
foreign key(user_id) references Users(user_id),

constraint FK_Product_id
FOREIGN KEY (product_id) REFERENCES Products(product_id)
)

alter table Cart
add created_at DATETIME NOT NULL DEFAULT GETDATE();


-- Log Table
-- log_id , entry_type(enum -> fail , success) , error_message , error_number , created_at 

CREATE TABLE Logs (
    log_id INT IDENTITY(1,1) PRIMARY KEY,
    entry_type VARCHAR(10) NOT NULL CHECK (entry_type IN ('success', 'fail')),
    error_message VARCHAR(500),
    error_number INT,
    created_at DATETIME NOT NULL DEFAULT GETDATE()
);


-- Stocks
-- stock_id ,, product_id , product_quantity , created_at

create table Stocks(
 stock_id INT IDENTITY(1,1) PRIMARY KEY,
 product_id uniqueidentifier,
 product_quantity int,
 created_at datetime not null default getdate()

 constraint FK_Category_Id foreign key(product_id) references Products(product_id),
)

-- Orders Table
-- order_id , orderd_placed_on (date) , order_category(category_id from category) , ordered_product_id(product_id from Products) , is_delivered ,
-- order_quantity , order_total_price , created_at

create table Orders(
order_id uniqueidentifier default newid() primary key,
category_id uniqueidentifier,
product_id uniqueidentifier,
is_delivered VARCHAR(10) NOT NULL CHECK (is_delivered IN ('delivered', 'pending')),
order_quantity int not null,
order_total_price int not null,
order_placed_on datetime,
created_at datetime not null default getdate()

constraint FK_Order_Category_Id foreign key(category_id) references Category(category_id),
constraint FK_Order_Product_Id foreign key(product_id) references Products(product_id),
)

alter table Orders
add user_id uniqueidentifier constraint FK_Order_User_Id foreign key (user_id) references Users(user_id);

select * from Orders