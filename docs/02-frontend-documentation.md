# Frontend Documentation

## Overview

The frontend is a Single Page Application (SPA) built with Angular 21. It communicates with the backend REST API using Angular's `HttpClient`. The app supports two distinct experiences — a customer-facing storefront and an admin management dashboard — separated by role-based routing guards.

---

## Technologies Used

| Technology | Purpose |
|---|---|
| Angular 21 | SPA framework |
| Angular Material 21 | UI component library |
| Tailwind CSS 4 | Utility-first styling |
| RxJS 7 | Reactive state and async data streams |
| Angular Signals | Fine-grained reactive state (auth state) |
| Angular SSR | Server-side rendering support |
| jwt-decode | Decode JWT tokens client-side |
| jsPDF / XLSX | Invoice and report generation |
| ngx-sonner | Toast notifications |
| Vitest | Unit testing |

---

## Folder Structure

```
src/app/
├── components/
│   ├── adminComponents/        # Admin-only views
│   │   ├── admin-layout/       # Admin shell with sidebar
│   │   ├── dashboard-overview/ # Stats and summary
│   │   ├── users-management/   # User list and actions
│   │   ├── orders-management/  # All orders view
│   │   ├── product-management/ # Add/edit/delete products
│   │   ├── category-management/# Category CRUD
│   │   ├── promocode-management/# Promo code CRUD
│   │   └── error-logs/         # View server error logs
│   ├── homeComponents/
│   │   ├── home/               # Landing page
│   │   └── footer/             # Footer
│   ├── profileComponents/      # Authenticated user views
│   │   ├── profile/            # Profile shell
│   │   ├── cart/               # Shopping cart
│   │   ├── wishlist/           # Wishlists
│   │   ├── orders/             # Order history
│   │   └── address/            # Saved addresses
│   ├── auth/                   # Login / Register page
│   ├── product-listing/        # Browse all products
│   ├── product-detail/         # Single product view
│   ├── checkout/               # Checkout flow
│   ├── payment/                # Payment processing
│   ├── shared/
│   │   ├── navbar/             # Top navigation bar
│   │   └── pagination/         # Reusable pagination
│   ├── spinner/                # Loading indicator
│   ├── unauthorized/           # 403 page
│   └── page-not-found/         # 404 page
├── services/
│   ├── auth.service.ts         # Auth API calls + token decode
│   ├── auth-state.service.ts   # Global auth state (Signals)
│   ├── auth.guard.ts           # Role-based route guard
│   ├── public-auth.guard.ts    # Public + authRequired guards
│   ├── product.service.ts      # Product API calls
│   ├── cart.service.ts         # Cart API calls
│   ├── wishlist.service.ts     # Wishlist API calls
│   ├── review.service.ts       # Review API calls
│   ├── profile.service.ts      # User profile API calls
│   ├── invoice.service.ts      # PDF/XLSX invoice generation
│   ├── loading.service.ts      # Global loading spinner state
│   ├── product-state.service.ts# Product browsing state
│   ├── product-suggestion.service.ts # Search autocomplete
│   ├── redirect.service.ts     # Store intended route before login
│   ├── address-selection.service.ts  # Address picker state
│   ├── adminServices/
│   │   ├── store.service.ts    # Admin global state store
│   │   ├── user.service.ts     # Admin user management API
│   │   ├── products.service.ts # Admin product API
│   │   ├── category.service.ts # Admin category API
│   │   ├── orders.service.ts   # Admin orders API
│   │   ├── promocode.service.ts# Admin promo code API
│   │   └── error-logs.service.ts # Admin logs API
│   └── userServices/
│       ├── address.service.ts  # User address API
│       ├── order.service.ts    # User order API
│       └── wallet.service.ts   # Wallet balance API
├── models/
│   ├── users/                  # DTOs for user-facing features
│   └── admin/                  # DTOs for admin features
├── interceptors/
│   └── interceptor.ts          # Attaches JWT token to all requests
├── constants/
│   ├── order-status.constants.ts
│   └── pagination.constants.ts
└── game/                       # Easter egg game component
```

---

## Components

### User-Facing Components

| Component | Route | Description |
|---|---|---|
| `HomeComponent` | `/` | Landing page with featured products |
| `ProductListing` | `/products` | Browse and filter all products |
| `ProductDetail` | `/product-detail/:productId` | Product info, reviews, add to cart/wishlist |
| `Auth` | `/auth` | Login and registration forms |
| `Profile` | `/profile` | Shell for authenticated user sections |
| `Cart` | `/profile/cart` | View and manage cart items |
| `WishlistComponent` | `/profile/wishlist` | View and manage wishlists |
| `OrdersComponent` | `/profile/orders` | Order history and details |
| `Address` | `/profile/address` | Manage delivery addresses |
| `CheckoutComponent` | `/checkout` | Review order before payment |
| `PaymentComponent` | `/payment` | Complete payment (Stripe/Wallet) |

### Admin Components

| Component | Route | Description |
|---|---|---|
| `AdminLayout` | `/admin` | Admin shell with sidebar navigation |
| `DashboardOverview` | `/admin/dashboard` | Summary stats |
| `UsersManagement` | `/admin/users` | View, deactivate, change roles |
| `OrdersManagement` | `/admin/orders` | View and update all orders |
| `ProductManagement` | `/admin/products` | Add, edit, soft-delete products |
| `CategoryManagement` | `/admin/categories` | Category CRUD |
| `PromocodeManagement` | `/admin/promocodes` | Promo code CRUD |
| `ErrorLogs` | `/admin/error-logs` | View server-side error logs |

---

## Services and State Management

### Auth State (Angular Signals)

`AuthStateService` is the central auth store using Angular Signals:

```typescript
readonly user = signal<UserDetails | null>(null);
readonly isAuthenticated = computed(() => this.user() !== null);
readonly role = computed(() => this.user()?.userRole ?? '');
```

- On app load, `loadUserFromStorage()` reads the JWT from `localStorage`, validates expiry, decodes it, and sets the user signal.
- `setUser()` is called after login to persist the token and update state.
- `scheduleAutoLogout()` sets a timer to auto-logout when the token expires.
- `logout()` clears state, removes the token, and redirects to `/auth`.

### HTTP Interceptor

`interceptor.ts` automatically attaches the JWT token to every outgoing request:
```
Authorization: Bearer <token>
```

### Loading State

`LoadingService` exposes a signal that the `SpinnerComponent` subscribes to, showing a global loading overlay during API calls.

### Admin Store

`store.service.ts` acts as a centralized state store for the admin dashboard, holding lists of users, products, orders, categories, and promo codes to avoid redundant API calls.

---

## Routing and Guards

### Route Guards

| Guard | Behavior |
|---|---|
| `publicGuard` | Allows all users; redirects admins to `/admin/dashboard` |
| `authRequiredGuard` | Requires login; stores intended URL and redirects to `/auth` if unauthenticated |
| `roleGuard` | Requires a specific role; redirects to appropriate fallback if role doesn't match |

### Route Structure

```
/                         → HomeComponent          [publicGuard]
/products                 → ProductListing          [publicGuard]
/product-detail/:id       → ProductDetail           [publicGuard]
/auth                     → Auth
/checkout                 → CheckoutComponent       [authRequiredGuard]
/payment                  → PaymentComponent        [authRequiredGuard]
/profile                  → Profile                 [authRequiredGuard]
  /profile/cart           → Cart
  /profile/wishlist       → WishlistComponent
  /profile/orders         → OrdersComponent
  /profile/address        → Address
/admin                    → AdminLayout             [roleGuard: admin]
  /admin/dashboard        → DashboardOverview
  /admin/users            → UsersManagement
  /admin/orders           → OrdersManagement
  /admin/products         → ProductManagement
  /admin/categories       → CategoryManagement
  /admin/promocodes       → PromocodeManagement
  /admin/error-logs       → ErrorLogs
/unauthorized             → UnauthorizedComponent
/page-not-found           → PageNotFound
/**                       → redirect to /page-not-found
```

---

## UI/UX Flow

### Guest User
1. Lands on the home page — sees featured products and categories.
2. Can browse `/products`, filter by price/category, and search by name.
3. Can view product detail pages including reviews.
4. Clicking "Add to Cart" or "Checkout" redirects to `/auth` with the intended URL stored.

### Authenticated User
1. After login, the JWT is decoded and the user signal is set.
2. The navbar updates to show profile links and cart icon.
3. User can add products to cart, manage wishlists, and proceed to checkout.
4. Checkout page shows address selection, promo code input, and wallet toggle.
5. Payment page handles Stripe payment or wallet-only payment.
6. After order placement, user is redirected to order confirmation.

### Admin User
1. After login, `publicGuard` redirects admin directly to `/admin/dashboard`.
2. Admin sees a sidebar with all management sections.
3. Admin cannot access user-facing routes like `/profile` or `/checkout`.
