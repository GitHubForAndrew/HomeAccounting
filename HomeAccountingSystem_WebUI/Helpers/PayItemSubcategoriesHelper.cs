﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAccountingSystem_DAL.Abstract;
using HomeAccountingSystem_DAL.Model;
using HomeAccountingSystem_DAL.Repositories;
using HomeAccountingSystem_WebUI.Abstract;
using HomeAccountingSystem_WebUI.Models;

namespace HomeAccountingSystem_WebUI.Helpers
{
    public class PayItemSubcategoriesHelper:IPayItemSubcategoriesHelper
    {
        private readonly IRepository<PayingItem> _payingItemRepository;

        public PayItemSubcategoriesHelper(IRepository<PayingItem> repo)
        {
            _payingItemRepository = repo;
        }

        public async Task<List<PayItemSubcategories>> GetPayItemsWithSubcategoriesInDatesWeb(DateTime dateFrom, DateTime dateTo,
            IWorkingUser user, int typeOfFlowId)
        {
            var pItems = _payingItemRepository.GetList() //Отфильтрованный по дате,юзеру,типу список транзакций
                .Where(x => x.UserId == user.Id &&
                            ((x.Date >= dateFrom.Date) && (x.Date <= dateTo.Date))
                            && x.Category.TypeOfFlowID == typeOfFlowId)
                .ToList();
            var payItemSubcategoriesList = new List<PayItemSubcategories>();

            var ids = pItems.GroupBy(x => x.CategoryID) //Список ИД категорий за выбранный период
                .ToList();
            ids.ForEach(id => payItemSubcategoriesList.Add(new PayItemSubcategories() //Наполняем у ViewModel свойства CategoryId
            {
                CategoryId = id.Key,
                CategorySumm = new OverAllItem(),
                ProductPrices = new List<ProductPrice>()
            }));

            var catNameGrouping = pItems //Наполняем список View Model и заполняем у каждого итема свойство CategorySumm
                .Where(x => x.UserId == user.Id &&
                            ((x.Date >= dateFrom.Date) && (x.Date <= dateTo.Date))
                            && x.Category.TypeOfFlowID == typeOfFlowId)
                .GroupBy(x => x.Category.Name)
                .ToList();

            var i = 0;
            foreach (var item in payItemSubcategoriesList)
            {
                item.CategorySumm.Category = catNameGrouping[i].Key;
                item.CategorySumm.Summ = catNameGrouping[i].Sum(x => x.Summ);
                i++;
            };


            foreach (var item in payItemSubcategoriesList)
            {
                item.ProductPrices = await FillProductPrices(item.CategoryId,dateFrom,dateTo).ConfigureAwait(false);
            }
            return payItemSubcategoriesList;
        }

        private async Task<List<ProductPrice>> FillProductPrices(int catId,DateTime dateFrom,DateTime dateTo)
        {
            var context = new AccountingContext();
            var tmp = await context.Database.SqlQuery<ProductPrice>(
                $"select prod.ProductName ProductName,SUM(pip.Summ) Price from PayingItem as pi " +
                $"join PaiyngItemProduct as pip on pip.PayingItemID = pi.ItemID " +
                $"join Product as prod on prod.ProductID = pip.ProductID " +
                $"where pi.CategoryID = {catId} and pi.Date>='{dateFrom.Date}' and pi.Date<='{dateTo.Date}' " +
                $"group by prod.ProductName")
                .ToListAsync();
            return tmp;
        }

    }
}