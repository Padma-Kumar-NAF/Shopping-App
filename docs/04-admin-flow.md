# End-to-End Admin Flow

This document describes the complete journey of an admin user through the application.

---

## 1. Login

1. Admin navigates to `/auth` and logs in with admin credentials.
2. Backend returns a JWT with `role: "admin"`.
3. `publicGuard` detects the admin role and immediately redirects to `/admin/dashboard`.
4. The admin sidebar becomes visible with all management sections.

> Regular user routes (`/profile`, `/checkout`, `/payment`) are inaccessible to admins. Attempting to visit them redirects to `/admin`.

---

## 2. Dashboard Overview

- Route: `/admin/dashboard`
- Displays a summary of key metrics: total users, products, orders, and recent activity.
- Serves as the admin's home base for navigating to specific management areas.

---

## 3. User Management

- Route: `/admin/users`
- Admin loads all users via `POST /User/get-all-users` (paginated).
- Available actions:

| Action | Endpoint |
|---|---|
| View all users | `POST /User/get-all-users` |
| Deactivate a user | `POST /User/delete-user` |
| Change user role (user ↔ admin) | `POST /User/change-user-role` |

- Deactivating a user sets `Active = false` on the `User` record (soft deactivation).
- Changing a role updates the `Role` field; the user must log in again for the new role to take effect.

---

## 4. Product Management

- Route: `/admin/products`
- Admin loads all products via `POST /Products/get-products`.
- Available actions:

| Action | Endpoint |
|---|---|
| Add a new product | `POST /Products/add-product` |
| Edit product details | `POST /Products/update-product` |
| Soft delete a product | `POST /Products/delete-product` |

- Adding a product requires: name, description, price, category, image path, and initial stock quantity.
- Soft delete sets `ActiveStatus = false` — the product is hidden from users but not removed from the database.
- Stock is managed as part of the product record via the `Stock` table.

---

## 5. Category Management

- Route: `/admin/categories`
- Admin loads all categories via `POST /category/get-all-categories`.
- Available actions:

| Action | Endpoint |
|---|---|
| Add category | `POST /category/add-category` |
| Edit category name | `POST /category/edit-category` |
| Delete category | `DELETE /category/delete-category` |
| View products by category | `POST /category/products-by-category` |

---

## 6. Order Management

- Route: `/admin/orders`
- Admin loads all orders via `POST /orders/get-all-orders` (paginated, with filters).
- Available actions:

| Action | Endpoint |
|---|---|
| View all orders | `POST /orders/get-all-orders` |
| Update order status | `POST /orders/update-order-status` |
| Cancel an order | `POST /orders/cancel-order` |

- Order statuses typically flow: `Pending → Processing → Shipped → Delivered` (or `Cancelled`).
- Admin can manually update the status at any stage.

---

## 7. Promo Code Management

- Route: `/admin/promocodes`
- Admin loads all promo codes via `POST /PromoCode/get-all-promocode`.
- Available actions:

| Action | Endpoint |
|---|---|
| Create promo code | `POST /PromoCode/add-promocode` |
| Edit promo code | `POST /PromoCode/edit-promocode` |
| Delete promo code | `DELETE /PromoCode/delete-promocode` |
| Verify a promo code | `POST /PromoCode/verify-promocode` |

- Each promo code has: a name, discount percentage, valid date range (`FromDate` / `ToDate`), and a soft-delete flag.
- Expired or deleted promo codes are rejected during checkout.

---

## 8. Error Logs

- Route: `/admin/error-logs`
- Admin loads server-side error logs via `POST /logs/get-logs` (paginated).
- Each log entry contains:
  - Exception type and message
  - Stack trace
  - Controller and action where the error occurred
  - HTTP method and request path
  - Request body
  - User info (name, role, userId) at the time of the error
  - Timestamp and HTTP status code
- This is useful for debugging production issues without needing server access.

---

## 9. Logout

Same as the user flow — clicking "Logout" clears the JWT and redirects to `/auth`.
