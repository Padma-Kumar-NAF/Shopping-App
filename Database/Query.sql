-- Users table
--user_id , user_name , user_phone_number , is_active , is_created_account , address_line_1 , address_line_2 ,
-- user_state , user_pincode , user_city , created_at

-- Products Table
-- product_id , product_name , product_category , product_price , product_is_available , product_image , created_at

-- Orders Table
-- order_id , orderd_placed_on (date) , order_category(category_id from category) user_id (user_id from Users), ordered_product_id(product_id from Products) , is_delivered ,
-- order_quantity , order_total_price , created_at

-- Category
-- category_id , category_name , category_total_products , created_at

-- Stocks
-- stock_id , category_id , product_id , product_quantity , created_at

-- Cart Table
-- cart_id , user_id , product_id , category_id , created_at ,

-- Log Table
-- log_id , entry_type(enum -> fail , success) , error_message , error_number , created_at 

use ShoppingApp

 

