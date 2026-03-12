export interface ProductItem {
  id: string;
  name: string;
  category: string;
  price: number;
  image: string;
  description?: string;
  rating?: number;
  stock?: number;
  tags?: string[];
}

export interface SearchResult {
  query: string;
  exactMatch: ProductItem | null;
  relatedProducts: ProductItem[];
  totalResults: number;
}
