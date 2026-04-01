import { CategoryDTO } from "./categories.model";
import { OrderDetailsResponseDTO } from "./orders.model";
import { ProductDetails } from "./products.model";
import { UserDetailsDTO } from "./users.model";
import { PromoCodeItemDTO } from "./promocode.model";

export interface AppState {
  users: UserDetailsDTO[];
  categories: CategoryDTO[];
  products: ProductDetails[];
  orders: OrderDetailsResponseDTO[];
  promoCodes: PromoCodeItemDTO[];
}