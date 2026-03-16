import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Category } from '../../../models/admin.model';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-category-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category-management.html',
  styleUrl: './category-management.css',
})
export class CategoryManagement {
  categories = signal<Category[]>([
    {
      id: '1',
      name: 'Electronics',
      description: 'Electronic devices and gadgets',
      productCount: 145,
      createdAt: new Date('2024-01-01'),
    },
    {
      id: '2',
      name: 'Fashion',
      description: 'Clothing and accessories',
      productCount: 289,
      createdAt: new Date('2024-01-05'),
    },
    {
      id: '3',
      name: 'Home & Kitchen',
      description: 'Home appliances and kitchen items',
      productCount: 98,
      createdAt: new Date('2024-01-10'),
    },
    {
      id: '4',
      name: 'Sports',
      description: 'Sports equipment and fitness gear',
      productCount: 67,
      createdAt: new Date('2024-01-15'),
    },
  ]);

  newCategory = signal<{ name: string; description: string }>({
    name: '',
    description: '',
  });

  showAddForm = signal<boolean>(false);

  toggleAddForm(): void {
    this.showAddForm.update((v) => !v);
    if (!this.showAddForm()) {
      this.resetForm();
    }
  }

  updateCategoryField(field: 'name' | 'description', value: string): void {
    this.newCategory.update((c) => ({ ...c, [field]: value }));
  }

  addCategory(): void {
    const cat = this.newCategory();

    if (!cat.name.trim()) {
      toast.error('Category name is required');
      return;
    }

    // Check for duplicate
    const exists = this.categories().some((c) => c.name.toLowerCase() === cat.name.toLowerCase());

    if (exists) {
      toast.error('Category already exists');
      return;
    }

    const newCat: Category = {
      id: Date.now().toString(),
      name: cat.name.trim(),
      description: cat.description.trim(),
      productCount: 0,
      createdAt: new Date(),
    };

    this.categories.update((cats) => [...cats, newCat]);
    toast.success('Category added successfully!');
    this.resetForm();
    this.showAddForm.set(false);
  }

  deleteCategory(categoryId: string): void {
    const category = this.categories().find((c) => c.id === categoryId);
    if (category && category.productCount && category.productCount > 0) {
      toast.error(`Cannot delete category with ${category.productCount} products`);
      return;
    }

    this.categories.update((cats) => cats.filter((c) => c.id !== categoryId));
    toast.success('Category deleted successfully');
  }

  resetForm(): void {
    this.newCategory.set({ name: '', description: '' });
  }
}
