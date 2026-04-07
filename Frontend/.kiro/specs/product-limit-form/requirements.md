# Requirements Document

## Introduction

The `product-limit-form` feature adds an admin UI component that lets administrators select a product from the existing product catalog and specify a numeric limit (e.g. stock cap or purchase limit) for that product. The form is built with Angular Reactive Forms and reads product data from the shared application state.

## Glossary

- **ProductLimitForm**: The Angular component (`app-product-limit`) that renders the form.
- **StoreService**: The shared state-management service that exposes `state$` as an `Observable<AppState>`.
- **AppState**: The application-wide state object; `AppState.products` holds the current list of `ProductDetails`.
- **ProductDetails**: Model representing a product, with fields `productId`, `productName`, `quantity`, etc.
- **Limit**: A non-negative integer entered by the administrator to cap a product's quantity or purchase allowance.

## Requirements

### Requirement 1: Load Product List from State

**User Story:** As an admin, I want the product dropdown to be populated from the current product list in state, so that I always see up-to-date products without an extra API call.

#### Acceptance Criteria

1. WHEN the ProductLimitForm component initialises, THE ProductLimitForm SHALL subscribe to `StoreService.state$` and derive the product list from `AppState.products`.
2. WHILE `AppState.products` is an empty array, THE ProductLimitForm SHALL render the dropdown with only the placeholder option visible.
3. WHEN `AppState.products` changes, THE ProductLimitForm SHALL reflect the updated list in the dropdown without requiring a page reload.

---

### Requirement 2: Render Reactive Form

**User Story:** As an admin, I want a form with a product dropdown and a numeric input, so that I can select a product and specify its limit in one place.

#### Acceptance Criteria

1. THE ProductLimitForm SHALL render a `<select>` element bound to a `productId` form control.
2. THE ProductLimitForm SHALL render each `ProductDetails` entry as an `<option>` whose value is `ProductDetails.productId` and whose label is `ProductDetails.productName`.
3. THE ProductLimitForm SHALL render a numeric `<input>` element bound to a `limit` form control that accepts only non-negative integers.
4. THE ProductLimitForm SHALL mark the `productId` control as required.
5. THE ProductLimitForm SHALL mark the `limit` control as required with a minimum value of 0.
6. WHILE the form is invalid, THE ProductLimitForm SHALL disable the submit button.

---

### Requirement 3: Handle Form Submission

**User Story:** As an admin, I want the form submission to output the selected product ID and the entered limit, so that downstream logic can consume those values.

#### Acceptance Criteria

1. WHEN the administrator submits the form with valid values, THE ProductLimitForm SHALL log the selected `productId` to the console.
2. WHEN the administrator submits the form with valid values, THE ProductLimitForm SHALL log the entered `limit` value to the console.
3. IF the form is invalid when submission is attempted, THEN THE ProductLimitForm SHALL not log any values and SHALL take no further action.
