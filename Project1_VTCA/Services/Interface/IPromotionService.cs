﻿using Project1_VTCA.Data;
using System.Threading.Tasks;

namespace Project1_VTCA.Services.Interface
{
    public interface IPromotionService
    {
       
        Task<(decimal? DiscountedPrice, string PromotionCode)> CalculateDiscountedPriceAsync(Product product);
    }
}