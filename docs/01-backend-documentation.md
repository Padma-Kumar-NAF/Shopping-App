# Backend Documentation

## Overview

The backend is a RESTful API built with ASP.NET Core. It follows a layered architecture separating Controllers, Services, Repositories, and Models. All responses use a unified `ApiResponse<T>` wrapper. The application supports two roles — `admin` and `user` — enforced via JWT claims.

---

## Technologies Used

| Technology | Purpose |
|---|---|
| ASP.NET Core 8 | Web API framework |
| Entity Framework Core | ORM for database access |
| SQL Server (LocalDB) | Relational database |
| JWT Bearer | Authentication & authorization |
| AspNetCoreRateLimit | IP-based rate limiting |
| Stripe / Razorpay | Payment processing |
| BCrypt (salt-based) | Password hashing |

---

## Project Structure

```
ShoppingApp/
├── Controllers/          # HTTP endpoints
├── Services/             # Business logic
├── Repositories/         # Data access layer (generic IRepository<T,TKey>)
├── Interfaces/
│   ├── ControllerInterface/   # Controller contracts
│   ├── ServicesInterface/     # Service contracts
│   └── RepositoriesInterface/ # Repository contracts
├── Models/               # Domain entities
│   └── DTOs/             # Request/Response data transfer objects
├── Contexts/             # EF Core DbContext (ShoppingContext)
├── Middleware/           # Global exception handler
├── Filters/              # ValidateRequestAttribute
├── Exceptions/           # AppException (custom exception)
├── Migrations/           # EF Core migration history
└── Program.cs            # App bootstrap, DI registration
```

---

## Key Modules and Services

| Service | Responsibility |
|---|---|
| `IUserService` | Register, login, update profile, manage roles, deactivate users |
| `ITokenService` | Generate JWT tokens with userId, name, email, role claims |
| `IPasswordService` | Hash and verify passwords using salt |
| `IProductService` | CRUD for products, search, filter, suggestions, soft delete |
| `ICategoryService` | CRUD for categories, get products by category |
| `ICartService` | Manage cart items, update quantities, place order from cart |
| `IOrderService` | Place orders, cancel, update status, refund, get order history |
| `IAddressService` | CRUD for user delivery addresses |
| `IWishListService` | Create/delete wishlists, add/remove products |
| `IReviewService` | Add and delete product reviews |
| `IPromoCodeService` | Create, verify, edit, delete promo codes |
| `IWalletService` | Retrieve wallet balance (used during checkout) |
| `ILogService` | Retrieve stored error logs (admin only) |
| `IUnitOfWork` | Coordinates transactions across repositories |

---

## Middleware Pipeline (Program.cs)

```
Request
  → CORS (AllowAngular policy)
  → Rate Limiting (50 req/min per IP)
  → ExceptionMiddleware (global error handler + DB logging)
  → Authentication (JWT Bearer)
  → Authorization (Role-based)
  → Controllers
```

---

## API Endpoints

Base URL: `https://localhost:7023`

All responses follow this shape:
```json
{
  "statusCode": 200,
  "message": "Success",
  "data": { ... },
  "action": "ActionName"
}
```

---

### Authentication — `/api/auth`

#### POST `/api/auth/register`
Registers a new user.

Request:
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "Secret123",
  "phoneNumber": "9999999999",
  "addressLine1": "123 Main St",
  "addressLine2": "Apt 4",
  "state": "Tamil Nadu",
  "city": "Chennai",
  "pincode": "600001"
}
```

Response `200`:
```json
{
  "statusCode": 200,
  "message": "User created",
  "data": {
    "userId": "guid",
    "name": "John Doe",
    "email": "john@example.com",
    "userDetails": { ... }
  }
}
```

---

#### POST `/api/auth/login`
Authenticates a user and returns a JWT token.

Request:
```json
{
  "email": "john@example.com",
  "password": "Secret123"
}
```

Response `200`:
```json
{
  "statusCode": 200,
  "message": "Login successful",
  "data": {
    "token": "<jwt>"
  }
}
```

---

### Products — `/Products`

| Method | Route | Description | Auth |
|---|---|---|---|
| POST | `/Products/get-products` | Paginated product list | Public |
| POST | `/Products/search-product` | Search by name | Public |
| POST | `/Products/get-product-by-id` | Get single product | Public |
| POST | `/Products/get-products-with-filter` | Filter by price range + category | Public |
| GET | `/Products/suggestions?query=` | Autocomplete suggestions | Public |
| POST | `/Products/add-product` | Add new product | Admin |
| POST | `/Products/update-product` | Update product details | Admin |
| POST | `/Products/delete-product` | Soft delete product | Admin |

Example — `POST /Products/get-products`:
```json
// Request
{ "pagination": { "pageSize": 20, "pageNumber": 1 } }

// Response data
{
  "productList": [
    {
      "productId": "guid",
      "productName": "Laptop",
      "categoryName": "Electronics",
      "price": 49999.00,
      "quantity": 10,
      "imagePath": "/images/laptop.jpg",
      "review": [{ "summary": "Great!", "reviewPoints": 5 }]
    }
  ]
}
```

---

### Categories — `/category`

| Method | Route | Description | Auth |
|---|---|---|---|
| POST | `/category/add-category` | Create category | Admin |
| DELETE | `/category/delete-category` | Delete category | Admin |
| POST | `/category/edit-category` | Edit category name | Admin |
| POST | `/category/get-all-categories` | Paginated category list | Public |
| POST | `/category/products-by-category` | Products filtered by category | Public |

---

### Cart — `/Cart` (Requires `user` role)

| Method | Route | Description |
|---|---|---|
| POST | `/Cart/add-to-cart` | Add product to cart |
| POST | `/Cart/get-user-cart` | Get cart with pagination |
| POST | `/Cart/update-user-cart` | Update item quantity |
| POST | `/Cart/order-all-from-cart` | Place order for all cart items |
| POST | `/Cart/remove-all-from-cart` | Clear entire cart |
| DELETE | `/Cart/remove-from-cart` | Remove single item |

Example — `POST /Cart/order-all-from-cart`:
```json
// Request
{
  "addressId": "guid",
  "paymentType": "Stripe",
  "promoCode": "SAVE10",
  "useWallet": true,
  "stripePaymentId": "pi_xxx"
}

// Response data
{
  "isSuccess": true,
  "orderId": "guid",
  "subtotal": 5000.00,
  "discountPercentage": 10,
  "discountAmount": 500.00,
  "walletUsed": 200.00,
  "finalAmount": 4300.00,
  "paymentStatus": "Paid"
}
```

---

### Orders — `/orders`

| Method | Route | Description | Auth |
|---|---|---|---|
| POST | `/orders/place-order` | Place a single order | User/Admin |
| POST | `/orders/get-user-orders` | Get current user's orders | User/Admin |
| POST | `/orders/get-all-orders` | Get all orders (admin) | Admin |
| POST | `/orders/cancel-order` | Cancel an order | User/Admin |
| POST | `/orders/update-order-status` | Update order status | Admin |
| POST | `/orders/refund-order` | Request a refund | User |

---

### Address — `/Address` (Requires `user` role)

| Method | Route | Description |
|---|---|---|
| PUT | `/Address/create-address` | Add new address |
| POST | `/Address/get-address` | Get user addresses |
| POST | `/Address/edit-address` | Edit existing address |
| DELETE | `/Address/delete-address` | Delete address |

---

### Wishlist — `/api/wishlist` (Requires `user` role)

| Method | Route | Description |
|---|---|---|
| POST | `/api/wishlist/create` | Create a wishlist |
| DELETE | `/api/wishlist/delete` | Delete a wishlist |
| POST | `/api/wishlist/add-product` | Add product to wishlist |
| DELETE | `/api/wishlist/remove-product` | Remove product from wishlist |
| POST | `/api/wishlist/user-wishlist` | Get user's wishlists |

---

### Reviews — `/Review`

| Method | Route | Description | Auth |
|---|---|---|---|
| POST | `/Review/add-review` | Add a product review | User |
| POST | `/Review/delete-review` | Delete a review | User |

---

### Promo Codes — `/PromoCode`

| Method | Route | Description |
|---|---|---|
| POST | `/PromoCode/add-promocode` | Create promo code |
| POST | `/PromoCode/verify-promocode` | Validate a promo code |
| POST | `/PromoCode/get-all-promocode` | List all promo codes |
| POST | `/PromoCode/edit-promocode` | Edit promo code |
| DELETE | `/PromoCode/delete-promocode` | Delete promo code |

---

### User Management — `/User`

| Method | Route | Description |
|---|---|---|
| POST | `/User/get-all-users` | List all users (admin) |
| GET | `/User/get-user-by-id` | Get current user profile |
| POST | `/User/edit-user-email` | Update email |
| POST | `/User/update-user-details` | Update profile details |
| POST | `/User/delete-user` | Deactivate a user |
| POST | `/User/change-user-role` | Change user role (admin) |

---

### Wallet — `/api/Wallet`

| Method | Route | Description |
|---|---|---|
| GET | `/api/Wallet/balance` | Get wallet balance |

---

### Logs — `/logs` (Admin)

| Method | Route | Description |
|---|---|---|
| POST | `/logs/get-logs` | Retrieve paginated error logs |

---

## Authentication & Authorization Flow

1. User calls `POST /api/auth/login` with email + password.
2. Backend verifies password using stored salt hash.
3. On success, `ITokenService.GenerateToken()` creates a JWT containing:
   - `ClaimTypes.NameIdentifier` → UserId (Guid)
   - `ClaimTypes.Name` → Name
   - `ClaimTypes.Email` → Email
   - `ClaimTypes.Role` → `"admin"` or `"user"`
4. Token is valid for **1440 minutes (24 hours)**, with `ClockSkew = Zero`.
5. Client stores the token in `localStorage` as `JWT-Token`.
6. All subsequent requests include the token in the `Authorization: Bearer <token>` header.
7. `BaseController.GetUserIdOrThrow()` extracts the UserId from claims on every protected request.
8. Role-based authorization is applied at the controller or action level via `[Authorize(Roles = "...")]`.

---

## Error Handling and Status Codes

All unhandled exceptions are caught by `ExceptionMiddleware`, which:
- Logs the full exception (message, stack trace, request body, user info) to the `Logs` database table.
- Returns a structured JSON error response.

Custom `AppException` allows services to throw domain errors with a specific HTTP status code:
```csharp
throw new AppException("Product not found", 404);
throw new AppException("User not authenticated", 401);
```

| Status Code | Meaning |
|---|---|
| 200 | Success |
| 400 | Bad request / validation failure |
| 401 | Unauthenticated (missing or invalid token) |
| 403 | Forbidden (insufficient role) |
| 404 | Resource not found |
| 429 | Too many requests (rate limit exceeded) |
| 500 | Unexpected server error |

The `ValidateRequestAttribute` filter automatically returns `400` for invalid model state before the action executes.
