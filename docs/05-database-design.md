# Database Design

## Overview

The database is SQL Server, managed via Entity Framework Core with code-first migrations. All primary keys are `GUID` (generated with `NEWID()`), and timestamps default to `GETUTCDATE()`. The schema supports a full e-commerce domain: users, products, orders, payments, reviews, wishlists, and more.

---

## Tables and Descriptions

| Table | Description |
|---|---|
| `Users` | Core user accounts with credentials and role |
| `UserDetails` | Extended profile info (phone, address details) |
| `Addresses` | Saved delivery addresses per user |
| `Carts` | One cart per user |
| `CartItems` | Individual product entries within a cart |
| `Categories` | Product categories |
| `Products` | Product catalog with soft-delete support |
| `Stock` | Inventory quantity per product (one-to-one) |
| `Orders` | Placed orders with totals, discount, and delivery info |
| `OrderDetails` | Line items for each order (product snapshot) |
| `Payments` | Payment records linked to orders |
| `Refunds` | Refund records linked to orders and payments |
| `PromoCodes` | Discount codes with date range and percentage |
| `Reviews` | Product reviews submitted by users |
| `WishList` | Named wishlists owned by users |
| `WishListItems` | Products within a wishlist |
| `Wallets` | Wallet balance per user |
| `Logs` | Server-side error logs |

---

## Key Relationships

```
User ──────────────── UserDetails       (one-to-one)
User ──────────────── Cart              (one-to-one)
User ──────────────── Wallet            (one-to-one)
User ──────────────── Addresses[]       (one-to-many)
User ──────────────── Orders[]          (one-to-many)
User ──────────────── Reviews[]         (one-to-many)
User ──────────────── WishLists[]       (one-to-many)
User ──────────────── Payments[]        (one-to-many)
User ──────────────── Refunds[]         (one-to-many)
User ──────────────── Logs[]            (one-to-many)

Category ──────────── Products[]        (one-to-many)

Product ───────────── Stock             (one-to-one)
Product ───────────── CartItems[]       (one-to-many)
Product ───────────── OrderDetails[]    (one-to-many)
Product ───────────── Reviews[]         (one-to-many)
Product ───────────── WishListItems[]   (one-to-many)

Cart ──────────────── CartItems[]       (one-to-many)

Order ─────────────── OrderDetails[]    (one-to-many)
Order ─────────────── Payment           (one-to-one)
Order ─────────────── Refund            (one-to-one)
Order ─────────────── Address           (many-to-one)
Order ─────────────── PromoCode         (many-to-one, nullable)

Payment ───────────── Refund            (one-to-one)

WishList ──────────── WishListItems[]   (one-to-many)
```

---

## Table Details

### Users

| Column | Type | Constraints |
|---|---|---|
| UserId | GUID | PK, default NEWID() |
| Name | string | Required |
| Email | string | Required |
| Password | string | Required (hashed) |
| SaltValue | string | Required |
| Role | string | Required (`"user"` or `"admin"`) |
| CreatedAt | DateTime | Required |
| Active | bool | Required (soft deactivation flag) |

---

### UserDetails

| Column | Type | Constraints |
|---|---|---|
| UserDetailsId | GUID | PK |
| UserId | GUID | FK → Users |
| Name | string | |
| Email | string | |
| PhoneNumber | string | |
| AddressLine1 | string | |
| AddressLine2 | string | |
| State | string | |
| City | string | |
| Pincode | string | |

---

### Addresses

| Column | Type | Constraints |
|---|---|---|
| AddressId | GUID | PK, default NEWID() |
| UserId | GUID | FK → Users (Restrict) |
| AddressLine1 | string | Required |
| AddressLine2 | string | Required |
| State | string | Required |
| City | string | Required |
| Pincode | string | Required |
| CreatedAt | DateTime | default GETUTCDATE() |

---

### Products

| Column | Type | Constraints |
|---|---|---|
| ProductId | GUID | PK |
| CategoryId | GUID | FK → Categories |
| Name | string | Required |
| ImagePath | string | Required |
| Description | string | Required |
| Price | decimal | Required |
| ActiveStatus | bool | Soft delete flag |
| CreatedAt | DateTime | Required |

---

### Stock

| Column | Type | Constraints |
|---|---|---|
| StockId | GUID | PK |
| ProductId | GUID | FK → Products (one-to-one) |
| Quantity | int | Required |
| CreatedAt | DateTime | Required |

---

### Orders

| Column | Type | Constraints |
|---|---|---|
| OrderId | GUID | PK |
| UserId | GUID | FK → Users |
| AddressId | GUID | FK → Addresses (Restrict) |
| PromoCodeId | GUID? | FK → PromoCodes (nullable) |
| Status | string | Required (e.g. Pending, Shipped) |
| TotalProductsCount | int | Required |
| TotalAmount | decimal | Required (before discount) |
| DiscountPercentage | int | Default 0 |
| DiscountAmount | decimal | Default 0 |
| OrderTotalAmount | decimal | Final amount after discount |
| DeliveryDate | DateTime | Required |
| CreatedAt | DateTime | Required |

---

### OrderDetails

| Column | Type | Constraints |
|---|---|---|
| OrderDetailsId | GUID | PK |
| OrderId | GUID | FK → Orders |
| ProductId | GUID | FK → Products |
| ProductName | string | Snapshot at time of order |
| ProductPrice | decimal | Snapshot at time of order |
| Quantity | int | Required |
| CreatedAt | DateTime | Required |

---

### Payments

| Column | Type | Constraints |
|---|---|---|
| PaymentId | GUID | PK |
| UserId | GUID | FK → Users |
| OrderId | GUID | FK → Orders (one-to-one) |
| TotalAmount | decimal | |
| PaymentType | string | e.g. `"Stripe"`, `"Wallet"` |
| StripePaymentId | string | Stripe transaction reference |
| CreatedAt | DateTime | |

---

### Refunds

| Column | Type | Constraints |
|---|---|---|
| RefundId | GUID | PK |
| UserId | GUID | FK → Users |
| OrderId | GUID | FK → Orders |
| PaymentId | GUID | FK → Payments |
| RefundAmount | decimal | |
| CreatedAt | DateTime | |

---

### PromoCodes

| Column | Type | Constraints |
|---|---|---|
| PromoCodeId | GUID | PK |
| PromoCodeName | string | |
| DiscountPercentage | int | |
| FromDate | DateTime | Validity start |
| ToDate | DateTime | Validity end |
| IsDeleted | bool | Soft delete flag |
| CreatedAt | DateTime | |

---

### Reviews

| Column | Type | Constraints |
|---|---|---|
| ReviewId | GUID | PK |
| UserId | GUID | FK → Users |
| ProductId | GUID | FK → Products |
| Summary | string | Required |
| ReviewPoints | int | Required (1–5) |
| CreatedAt | DateTime | Required |

---

### WishList

| Column | Type | Constraints |
|---|---|---|
| WishListId | GUID | PK |
| UserId | GUID | FK → Users |
| WhishListName | string | |
| CreatedAt | DateTime | |

---

### WishListItems

| Column | Type | Constraints |
|---|---|---|
| (composite or own PK) | | |
| WishListId | GUID | FK → WishList |
| ProductId | GUID | FK → Products |

---

### Wallets

| Column | Type | Constraints |
|---|---|---|
| WalletId | GUID | PK |
| UserId | GUID | FK → Users (one-to-one) |
| WalletAmount | decimal | |
| CreatedAt | DateTime | |

---

### Logs

| Column | Type | Description |
|---|---|---|
| LogId | GUID | PK |
| Message | string | Exception message |
| StackTrace | string | Full stack trace |
| InnerException | string | Inner exception message |
| ExceptionType | string | Exception class name |
| UserName | string | User at time of error |
| Role | string | User role |
| UserId | string | User ID |
| Controller | string | Controller name |
| Action | string | Action name |
| HttpMethod | string | GET, POST, etc. |
| RequestPath | string | URL path |
| QueryString | string | Query parameters |
| RequestBody | string | Raw request body |
| StatusCode | int | HTTP status code |
| CreatedAt | DateTime | Timestamp |

---

## ER Diagram (Text Representation)

```
[Users] 1──1 [UserDetails]
[Users] 1──1 [Carts] 1──* [CartItems] *──1 [Products]
[Users] 1──1 [Wallets]
[Users] 1──* [Addresses] 1──* [Orders]
[Users] 1──* [Orders] 1──1 [Payments] 1──1 [Refunds]
[Users] 1──* [Reviews] *──1 [Products]
[Users] 1──* [WishLists] 1──* [WishListItems] *──1 [Products]
[Categories] 1──* [Products] 1──1 [Stock]
[Products] 1──* [OrderDetails] *──1 [Orders]
[PromoCodes] 1──* [Orders]
```

---

## Important Design Notes

- All PKs are GUIDs to avoid sequential ID enumeration attacks.
- `Product.ActiveStatus = false` is used for soft delete — products are never physically removed.
- `User.Active = false` is used for soft deactivation of user accounts.
- `PromoCode.IsDeleted = true` is used for soft delete of promo codes.
- `OrderDetails` stores a snapshot of `ProductName` and `ProductPrice` at the time of ordering, so historical orders remain accurate even if the product is later updated or deleted.
- Delete behaviors use `Restrict` (not cascade) to prevent accidental data loss across related entities.
