import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductLimit } from './product-limit';

describe('ProductLimit', () => {
  let component: ProductLimit;
  let fixture: ComponentFixture<ProductLimit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductLimit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductLimit);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
