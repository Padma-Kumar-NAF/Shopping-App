using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Wallet;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IWalletService
    {
        public Task<ApiResponse<GetWalletAmountResponseDTO>> GetWalletAmount(Guid userId);
    }
}
