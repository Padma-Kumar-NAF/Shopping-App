using ShoppingApp.Models.DTOs.Wallet;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IWalletService
    {
        public Task<GetWalletAmountResponseDTO> GetWalletAmount(Guid userId);
    }
}
