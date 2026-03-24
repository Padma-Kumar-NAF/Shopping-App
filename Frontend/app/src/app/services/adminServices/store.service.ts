import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { AppState } from '../../models/admin/appState.model';
import { UserDetailsDTO } from '../../models/admin/users.model';
import { OrderDetailsResponseDTO } from '../../models/admin/orders.model';
import { CategoryDTO } from '../../models/admin/categories.model';
import { ProductDetails } from '../../models/admin/products.model';

export interface PageCache {
  users: Set<number>;
  orders: Set<number>;
  products: Set<number>;
  categories: Set<number>;
}

@Injectable({
  providedIn: 'root',
})
export class StoreService {
  private initialState: AppState = {
    users: [],
    categories: [],
    products: [],
    orders: [],
  };

  private store = new BehaviorSubject<AppState>(this.initialState);

  state$: Observable<AppState> = this.store.asObservable();

  readonly pageCache: PageCache = {
    users: new Set<number>(),
    orders: new Set<number>(),
    products: new Set<number>(),
    categories: new Set<number>(),
  };

  get value(): AppState {
    return this.store.getValue();
  }

  private setState(newState: AppState) {
    this.store.next(newState);
  }

  setUsers(users: UserDetailsDTO[]) {
    this.setState({ ...this.value, users });
  }

  appendUsers(users: UserDetailsDTO[]) {
    const existing = this.value.users;
    const existingIds = new Set(existing.map(u => u.userId));
    const newOnes = users.filter(u => !existingIds.has(u.userId));
    this.setState({ ...this.value, users: [...existing, ...newOnes] });
  }

  addUser(user: UserDetailsDTO) {
    this.setState({ ...this.value, users: [...this.value.users, user] });
  }

  updateUser(updated: UserDetailsDTO) {
    const users = this.value.users.map(u => u.userId === updated.userId ? updated : u);
    this.setState({ ...this.value, users });
  }

  setCategories(categories: CategoryDTO[]) {
    this.setState({ ...this.value, categories });
  }

  appendCategories(categories: CategoryDTO[]) {
    const existing = this.value.categories;
    const existingIds = new Set(existing.map(c => c.categoryId));
    const newOnes = categories.filter(c => !existingIds.has(c.categoryId));
    this.setState({ ...this.value, categories: [...existing, ...newOnes] });
  }

  addCategory(category: CategoryDTO) {
    this.setState({ ...this.value, categories: [...this.value.categories, category] });
  }

  updateCategory(updated: CategoryDTO) {
    const categories = this.value.categories.map(c =>
      c.categoryId === updated.categoryId ? updated : c
    );
    this.setState({ ...this.value, categories });
  }

  setProducts(products: ProductDetails[]) {
    this.setState({ ...this.value, products });
  }

  appendProducts(products: ProductDetails[]) {
    const existing = this.value.products;
    const existingIds = new Set(existing.map(p => p.productId));
    const newOnes = products.filter(p => !existingIds.has(p.productId));
    this.setState({ ...this.value, products: [...existing, ...newOnes] });
  }

  addProduct(product: ProductDetails) {
    this.setState({ ...this.value, products: [...this.value.products, product] });
  }

  updateProduct(updated: ProductDetails) {
    const products = this.value.products.map(p =>
      p.productId === updated.productId ? updated : p
    );
    this.setState({ ...this.value, products });
  }

  setOrders(orders: OrderDetailsResponseDTO[]) {
    this.setState({ ...this.value, orders });
  }

  appendOrders(orders: OrderDetailsResponseDTO[]) {
    const existing = this.value.orders;
    const existingIds = new Set(existing.map(o => o.orderId));
    const newOnes = orders.filter(o => !existingIds.has(o.orderId));
    this.setState({ ...this.value, orders: [...existing, ...newOnes] });
  }

  addOrder(order: OrderDetailsResponseDTO) {
    this.setState({ ...this.value, orders: [...this.value.orders, order] });
  }
}