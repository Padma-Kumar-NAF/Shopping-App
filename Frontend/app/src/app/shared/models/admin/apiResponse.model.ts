export class ApiResponse<T>{
    message: string = '';
    data?: T;
    action: string = '';
    statusCode: number = 0;
}