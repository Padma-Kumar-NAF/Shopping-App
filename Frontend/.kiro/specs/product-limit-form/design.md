# Design Document: product-limit-form

## Overview

The `product-limit-form` feature is a standalone Angular component (`app-product-limit`) that renders a reactive form allowing administrators to select a product from the current catalog and enter a numeric limit for that product. The component reads product data reactively from `StoreService.state$` and outputs the selected `productId` and `limit` on valid submission.

The component is already scaffolded. This design covers the complete behaviour, validation rules, data flow, and testing strategy.

---

## Architecture

The component follows Angular's standard reactive-forms pattern within the existing admin feature module structure.

```
StoreService (BehaviorSubject<AppState>)
        │
        │ state$ | map(s => s.products)
        ▼
ProductLimit component
  ├── products$: Observable<ProductDetails[]>  ──► <select> via async pipe
  └── form: FormGroup
        ├── productId: FormControl (required)  ──► <select>
        └── limit: FormControl (required, min 0) ──► <input type="number">
                                                        │
                                              onSubmit() ──► console.log(productId, limit)
```

No new services or HTTP calls are introduced. The component is purely presentational with local form state.

---

## Components and Interfaces

### ProductLimit (`product-limit.ts`)

| Member | Type | Description |
|---|---|---|
| `form` | `FormGroup` | Reactive form with `productId` and `limit` controls |
| `products$` | `Observable<ProductDetails[]>` | Derived from `StoreService.state$` |
| `ngOnInit()` | lifecycle | Initialises the `FormGroup` with validators |
| `onSubmit()` | method | Guards on `form.invalid`, then logs `productId` and `limit` |

### Template (`product-limit.html`)

| Element | Binding | Notes |
|---|---|---|
| `<form>` | `[formGroup]="form"`, `(ngSubmit)="onSubmit()"` | Wraps all controls |
| `<select>` | `formControlName="productId"` | Populated via `*ngFor` / `@for` over `products$ \| async` |
| `<option>` | `[value]="product.productId"` | Label is `product.productName` |
| `<input type="number">` | `formControlName="limit"`, `min="0"` | Numeric input |
| `<button type="submit">` | `[disabled]="form.invalid"` | Disabled while form is invalid |

---

## Data Models

### ProductDetails (existing)

```typescript
class ProductDetails {
  productId: string;      // used as <option> value and submitted field
  productName: string;    // used as <option> display label
  quantity: number;       // available for display/reference
  // ... other fields not used by this component
}
```

### Form Value Shape

```typescript
interface ProductLimitFormValue {
  productId: string;   // non-empty string, required
  limit: number;       // integer >= 0, required
}
```

### Validation Rules

| Control | Validators | Invalid when |
|---|---|---|
| `productId` | `Validators.required` | empty string / null |
| `limit` | `Validators.required`, `Validators.min(0)` | null or value < 0 |

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Products observable reflects store state

*For any* list of products emitted by `StoreService.state$`, the component's `products$` observable should emit exactly that same list, both on initialisation and on every subsequent state change.

**Validates: Requirements 1.1, 1.3**

---

### Property 2: Each product appears as a correctly-valued option

*For any* non-empty list of `ProductDetails` provided via the store, every product in the list should appear in the rendered `<select>` as an `<option>` whose `value` attribute equals `product.productId` and whose text content equals `product.productName`.

**Validates: Requirements 2.2**

---

### Property 3: Negative or null limit values are invalid

*For any* value less than 0 or null assigned to the `limit` control, the control should be in an invalid state, and therefore the overall form should be invalid and the submit button should be disabled.

**Validates: Requirements 2.5, 2.6**

---

### Property 4: Submit button disabled state mirrors form validity

*For any* combination of `productId` and `limit` control values, the submit button's `disabled` attribute should be `true` if and only if `form.invalid` is `true`.

**Validates: Requirements 2.6**

---

### Property 5: Valid submission logs both values

*For any* valid `productId` (non-empty string) and valid `limit` (non-negative integer), calling `onSubmit()` should result in `console.log` being called with the `productId` and with the `limit` value.

**Validates: Requirements 3.1, 3.2**

---

### Property 6: Invalid submission produces no side effects

*For any* form state where `form.invalid` is `true`, calling `onSubmit()` should not invoke `console.log` and should leave the form state unchanged.

**Validates: Requirements 3.3**

---

## Error Handling

| Scenario | Behaviour |
|---|---|
| `AppState.products` is empty | Dropdown shows only the placeholder option; form remains invalid until a product is selected |
| `limit` is negative | `Validators.min(0)` marks control invalid; submit button stays disabled |
| `productId` not selected | `Validators.required` marks control invalid; submit button stays disabled |
| `onSubmit()` called with invalid form | Early return; no logging, no further action |

No network errors apply — the component reads from in-memory state only.

---

## Testing Strategy

### Dual Testing Approach

Both unit tests and property-based tests are used. Unit tests cover specific structural examples and edge cases; property-based tests verify universal correctness across generated inputs.

### Unit Tests (Vitest + Angular Testing Utilities)

Focus areas:
- DOM structure: `<select>`, `<input type="number">`, `<button type="submit">` are present with correct bindings (covers Requirements 2.1, 2.3)
- Empty state edge case: when `products` is `[]`, only the placeholder `<option>` is rendered (covers Requirement 1.2)
- `productId` required validation: control is invalid when value is `''` (covers Requirement 2.4)
- Component creation smoke test

### Property-Based Tests (fast-check)

Use `@fast-check/vitest` (or `fast-check` with Vitest). Each test runs a minimum of **100 iterations**.

Each test is tagged with a comment in the format:
`// Feature: product-limit-form, Property <N>: <property_text>`

| Property | Test description |
|---|---|
| Property 1 | Generate arbitrary `ProductDetails[]`, emit via mock store, assert `products$` emits same list |
| Property 2 | Generate arbitrary `ProductDetails[]`, render component, assert each product maps to a correct `<option>` |
| Property 3 | Generate arbitrary negative numbers and null, set as `limit` value, assert control and form are invalid |
| Property 4 | Generate arbitrary valid/invalid control value combinations, assert button disabled === form.invalid |
| Property 5 | Generate arbitrary valid `productId` strings and non-negative integers as `limit`, call `onSubmit()`, assert `console.log` called with both |
| Property 6 | Generate arbitrary invalid form states, call `onSubmit()`, assert `console.log` not called |

### Property-Based Testing Library

Use **fast-check** (`npm install --save-dev fast-check`), which integrates cleanly with Vitest and provides generators for primitives, arrays, and custom models.

```typescript
// Example tag format
// Feature: product-limit-form, Property 5: Valid submission logs both values
fc.assert(fc.property(
  fc.string({ minLength: 1 }),
  fc.integer({ min: 0 }),
  (productId, limit) => { /* ... */ }
), { numRuns: 100 });
```
