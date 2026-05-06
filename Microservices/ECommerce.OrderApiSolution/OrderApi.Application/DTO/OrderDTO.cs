using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrderApi.Application.DTO
{
    public record OrderDTO(
        int Id,
        [Required,Range(1,int.MaxValue)] int ProductId,
        [Required,Range(1,int.MaxValue)] int ClientId,
        [Required, Range(1, int.MaxValue)] int PurchaseQuantity,
        DateTime OrderDate
        );
}
