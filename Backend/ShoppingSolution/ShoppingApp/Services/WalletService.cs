using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Wallet;

namespace ShoppingApp.Services
{
    public class WalletService : IWalletService
    {
        private readonly IRepository<Guid, Wallet> _walletRepository;

        public WalletService(IRepository<Guid, Wallet> walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<GetWalletAmountResponseDTO> GetWalletAmount(Guid userId)
        {
            var wallet = await _walletRepository
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                throw new AppException("Wallet not found for this user", 404);
            }

            return new GetWalletAmountResponseDTO
            {
                WalletBalance = wallet.WalletAmount
            };
        }
    }
}