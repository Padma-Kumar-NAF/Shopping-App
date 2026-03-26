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

        public async Task<ApiResponse<GetWalletAmountResponseDTO>> GetWalletAmount(Guid userId)
        {
            try
            {
                //Console.WriteLine("----------");
                //Console.WriteLine(userId);
                var wallet = await _walletRepository
                    .GetQueryable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    throw new AppException("You don’t have a wallet yet.", 404);
                }

                return new ApiResponse<GetWalletAmountResponseDTO>
                {
                    Data = new GetWalletAmountResponseDTO
                    {
                        WalletBalance = wallet.WalletAmount
                    },
                    StatusCode = 200,
                    Message = "Wallet fetched",
                    Action = "ShowWallet"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while canceling order", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while canceling order", ex, 500);
            }
        }
    }
}