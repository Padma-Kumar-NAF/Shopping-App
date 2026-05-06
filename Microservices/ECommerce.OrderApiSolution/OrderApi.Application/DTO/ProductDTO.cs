using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrderApi.Application.DTO
{
    public record ProductDTO(
        int Id,
        [Required] string Name,
        [Required, Range(1, int.MaxValue)] int Quanatity,
        [Required, DataType(DataType.Currency)] decimal Price
        );
}
