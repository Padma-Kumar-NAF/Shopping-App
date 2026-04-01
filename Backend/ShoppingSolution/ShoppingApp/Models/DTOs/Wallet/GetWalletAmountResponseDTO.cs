namespace ShoppingApp.Models.DTOs.Wallet
{
    public record GetWalletAmountResponseDTO
    {
        public decimal WalletBalance { get; set; }
    }
}
