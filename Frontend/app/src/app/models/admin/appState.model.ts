import { CategoryDTO } from "./categories.model";
import { OrderDetailsResponseDTO } from "./orders.model";
import { ProductDetails } from "./products.model";
import { UserDetailsDTO } from "./users.model";

export interface AppState {
  users: UserDetailsDTO[];
  categories: CategoryDTO[];
  products: ProductDetails[];
  orders: OrderDetailsResponseDTO[];
}