# Tasks

## Task List

- [ ] 1. Implement ProductLimit component logic
  - [ ] 1.1 Inject StoreService and derive products$ via map(s => s.products)
  - [ ] 1.2 Initialise FormGroup with productId (required) and limit (required, min 0) controls in ngOnInit
  - [ ] 1.3 Implement onSubmit() to guard on form.invalid and log productId and limit

- [ ] 2. Implement ProductLimit template
  - [ ] 2.1 Bind <form> to formGroup and ngSubmit
  - [ ] 2.2 Render <select> with formControlName="productId" and @for loop over products$ | async
  - [ ] 2.3 Render <input type="number"> with formControlName="limit" and min="0"
  - [ ] 2.4 Bind submit button [disabled]="form.invalid"

- [ ] 3. Write unit tests
  - [ ] 3.1 Test DOM structure: select, number input, and submit button are present
  - [ ] 3.2 Test empty state edge case: only placeholder option rendered when products is []
  - [ ] 3.3 Test productId required validation: control invalid when value is empty string

- [ ] 4. Write property-based tests using fast-check
  - [ ] 4.1 Property 1 — products$ reflects store state: for any ProductDetails[], assert products$ emits same list
  - [ ] 4.2 Property 2 — options match products: for any ProductDetails[], assert each maps to correct <option> value and label
  - [ ] 4.3 Property 3 — negative/null limit is invalid: for any value < 0 or null, assert limit control and form are invalid
  - [ ] 4.4 Property 4 — button disabled mirrors form validity: for any control values, assert button.disabled === form.invalid
  - [ ] 4.5 Property 5 — valid submission logs both values: for any valid productId and non-negative limit, assert console.log called with both
  - [ ] 4.6 Property 6 — invalid submission has no side effects: for any invalid form state, assert console.log not called
