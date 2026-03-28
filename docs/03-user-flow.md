# End-to-End User Flow

This document describes the complete journey of a regular (non-admin) user through the application.

---

## 1. Registration

1. User navigates to `/auth` and selects the "Sign Up" tab.
2. Fills in: name, email, password, phone number, and a default address (line 1, line 2, city, state, pincode).
3. On submit, the frontend calls `POST /api/auth/register`.
4. Backend creates a `User` record (with hashed password + salt), a `UserDetails` record, a `Cart`, a `Wallet`, and saves the provided address.
5. On success, the user is redirected to the login view.

---

## 2. Login

1. User enters email and password on the `/auth` page.
2. Frontend calls `POST /api/auth/login`.
3. Backend verifies credentials, generates a JWT (24-hour expiry) containing userId, name, email, and role.
4. Frontend stores the token in `localStorage` as `JWT-Token`.
5. `AuthStateService` decodes the token, sets the user signal, and schedules an auto-logout timer.
6. If the user had tried to access a protected route before logging in, they are redirected to that route. Otherwise, they land on the home page.

---

## 3. Browsing Products

1. User visits `/products` (or the home page).
2. Frontend calls `POST /Products/get-products` with pagination parameters.
3. User can:
   - Search by name using the search bar → `POST /Products/search-product`
   - Filter by price range and/or category → `POST /Products/get-products-with-filter`
   - Use autocomplete suggestions → `GET /Products/suggestions?query=`
4. Clicking a product navigates to `/product-detail/:productId`.
5. Product detail page loads via `POST /Products/get-product-by-id`, showing description, price, stock, category, and reviews.

---

## 4. Managing the Wishlist

1. On the product detail page, user clicks "Add to Wishlist".
2. If no wishlist exists, user creates one via `POST /api/wishlist/create`.
3. Product is added via `POST /api/wishlist/add-product`.
4. User can view all wishlists at `/profile/wishlist`.
5. Products can be removed via `DELETE /api/wishlist/remove-product`.
6. Entire wishlists can be deleted via `DELETE /api/wishlist/delete`.

---

## 5. Managing the Cart

1. User clicks "Add to Cart" on a product page.
2. Frontend calls `POST /Cart/add-to-cart` with `productId` and `quantity`.
3. User navigates to `/profile/cart` to review items.
4. Cart is loaded via `POST /Cart/get-user-cart`.
5. User can:
   - Increase/decrease quantity → `POST /Cart/update-user-cart`
   - Remove a single item → `DELETE /Cart/remove-from-cart`
   - Clear the entire cart → `POST /Cart/remove-all-from-cart`

---

## 6. Managing Addresses

1. User navigates to `/profile/address`.
2. Existing addresses are loaded via `POST /Address/get-address`.
3. User can:
   - Add a new address → `PUT /Address/create-address`
   - Edit an address → `POST /Address/edit-address`
   - Delete an address → `DELETE /Address/delete-address`

---

## 7. Checkout

1. User navigates to `/checkout` (requires login).
2. Checkout page shows:
   - Cart items summary
   - Address selector (loads from `POST /Address/get-address`)
   - Promo code input (verified via `POST /PromoCode/verify-promocode`)
   - Wallet balance toggle (loaded via `GET /api/Wallet/balance`)
   - Order total with discount and wallet deduction applied
3. User selects an address, optionally applies a promo code, and optionally uses wallet balance.
4. User clicks "Proceed to Payment".

---

## 8. Payment

1. User lands on `/payment`.
2. Payment options:
   - **Stripe**: User enters card details; Stripe returns a `paymentIntentId`.
   - **Wallet only**: If wallet balance covers the full amount.
3. Frontend calls `POST /Cart/order-all-from-cart` with:
   - `addressId`, `paymentType`, `promoCode`, `useWallet`, `stripePaymentId`
4. Backend:
   - Deducts stock for each product.
   - Creates an `Order` and `OrderDetails` records.
   - Creates a `Payment` record.
   - Applies promo code discount if valid.
   - Deducts wallet amount if `useWallet` is true.
   - Clears the cart.
5. Response includes `orderId`, `finalAmount`, and `paymentStatus`.

---

## 9. Viewing Orders

1. User navigates to `/profile/orders`.
2. Orders are loaded via `POST /orders/get-user-orders`.
3. Each order shows: status, items, total amount, delivery date, and payment info.
4. User can:
   - **Cancel an order** → `POST /orders/cancel-order` (if eligible)
   - **Request a refund** → `POST /orders/refund-order`

---

## 10. Writing a Review

1. On the product detail page, after purchasing, user submits a review.
2. Frontend calls `POST /Review/add-review` with `productId`, `summary`, and `reviewPoints` (1–5).
3. Review appears on the product detail page for all users.
4. User can delete their own review via `POST /Review/delete-review`.

---

## 11. Profile Management

1. User navigates to `/profile`.
2. Profile data is loaded via `GET /User/get-user-by-id`.
3. User can:
   - Update name, phone, address details → `POST /User/update-user-details`
   - Change email → `POST /User/edit-user-email`

---

## 12. Logout

1. User clicks "Logout" in the navbar.
2. `AuthStateService.logout()` clears the JWT from `localStorage`, resets the user signal, and redirects to `/auth`.
3. The auto-logout timer is also cancelled.
