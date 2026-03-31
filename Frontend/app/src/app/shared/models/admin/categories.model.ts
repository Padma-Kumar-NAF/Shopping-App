export class GetAllCategoryResponseDTO {
  categoryList: CategoryDTO[] = [];
}

export class CategoryDTO {
  categoryId: string = '';
  categoryName: string = '';
  productsCount: number = 0;
  createdAt: Date = new Date();
}

export class DeleteCategoryRequestDTO {
  categoryId: string = '';
}

export class DeleteCategoryResponseDTO {
  isSuccess: boolean = false;
}

export class AddCategoryRequestDTO {
  categoryName: string = '';
}

export class AddCategoryResponseDTO {
  categoryId: string = '';
}

export class EditCategoryRequestDTO {
  categoryId: string = '';
  categoryName: string = '';
}

export class EditCategoryResponseDTO{
  isSuccess : boolean = false;
}