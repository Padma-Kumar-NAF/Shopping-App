import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { AppState } from '../../models/admin/appState.model';
import { UserDetailsDTO } from '../../models/admin/users.model';
import { OrderDetailsResponseDTO } from '../../models/admin/orders.model';
import { CategoryDTO } from '../../models/admin/categories.model';
import { ProductDetails } from '../../models/admin/products.model';

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

  // Snapshot getter
  get value(): AppState {
    return this.store.getValue();
  }

  private setState(newState: AppState) {
    this.store.next(newState);
  }

  setUsers(users: UserDetailsDTO[]) {
    this.setState({ ...this.value, users });
  }

  addUser(user: UserDetailsDTO) {
    this.setState({
      ...this.value,
      users: [...this.value.users, user],
    });
  }

  setCategories(categories: CategoryDTO[]) {
    this.setState({ ...this.value, categories });
  }

  addCategory(category: CategoryDTO) {
    this.setState({
      ...this.value,
      categories: [...this.value.categories, category],
    });
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

  addProduct(product: ProductDetails) {
    this.setState({
      ...this.value,
      products: [...this.value.products, product],
    });
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

  addOrder(order: OrderDetailsResponseDTO) {
    this.setState({
      ...this.value,
      orders: [...this.value.orders, order],
    });
  }
}