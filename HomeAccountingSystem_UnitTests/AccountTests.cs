﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using HomeAccountingSystem_DAL.Model;
using HomeAccountingSystem_WebUI.Controllers;
using HomeAccountingSystem_WebUI.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Services;

namespace HomeAccountingSystem_UnitTests
{
    [TestClass]
    public class AccountTests
    {
        private readonly List<Account> _accounts = new List<Account>()
        {
            new Account() {AccountID = 1, UserId = "1"},
            new Account() {AccountID = 2, UserId = "2"},
            new Account() {AccountID = 3, UserId = "1"}
        };

        private readonly Mock<IAccountService> _mockAccountService;

        public AccountTests()
        {
            _mockAccountService = new Mock<IAccountService>();
        }

        [TestMethod]
        public async Task IndexReturnsPartialView()
        {
            _mockAccountService.Setup(m => m.GetListAsync()).ReturnsAsync(_accounts);
            AccountController target = new AccountController(_mockAccountService.Object);

            var result = await target.Index(new WebUser() { Id = "1" });
            var model = ((PartialViewResult)result).ViewData.Model as List<Account>;

            Assert.IsInstanceOfType(result, typeof(PartialViewResult));
            Assert.AreEqual(model.Count, 2);
            Assert.AreEqual(model[0].AccountID, 1);
            Assert.AreEqual(model[1].AccountID, 3);
        }


        [TestMethod]
        public async Task EditInputIdReturnsNewAccount()
        {
            _mockAccountService.Setup(m => m.GetItemAsync(It.Is<int>(v => v == 2))).ReturnsAsync(_accounts.Find(x => x.AccountID == 2));
            _mockAccountService.Setup(m => m.GetItemAsync(It.Is<int>(v => v < 1))).ReturnsAsync(null);
            _mockAccountService.Setup(m => m.GetItemAsync(It.Is<int>(v => v > 3))).ReturnsAsync(null);
            var target = new AccountController(_mockAccountService.Object);

            var result2 = ((PartialViewResult)await target.Edit(new WebUser(),2)).Model as Account;
            var result0 = ((PartialViewResult)await target.Edit(new WebUser(),0)).Model as Account;
            var result4 = ((PartialViewResult)await target.Edit(new WebUser(),4)).Model as Account;

            Assert.AreEqual(result0.AccountID, 0);
            Assert.AreEqual(result4.AccountID, 0);
            Assert.AreEqual(result2.AccountID,2);
        }

        [TestMethod]
        public async Task EditInputAccountNullReturnsRedirectToIndex()
        {
            var target = new AccountController(null);

            var resultNull = (RedirectToRouteResult)await target.Edit(null);
            var result0 = (RedirectToRouteResult)await target.Edit(new Account() {AccountID = 0});
            
            Assert.AreEqual(resultNull.RouteValues.ContainsValue("Index"),true);
            Assert.AreEqual(result0.RouteValues.ContainsValue("Index"), true);
        }

        [TestMethod]
        public async Task EditInputModelInvalidReturnsPartial()
        {
            var target = new AccountController(null);
            target.ModelState.AddModelError("","");

            var result = await target.Edit(new Account() {AccountID = 1});
            
            Assert.IsInstanceOfType(result, typeof(PartialViewResult));
            Assert.IsInstanceOfType(((PartialViewResult)result).Model, typeof(Account));
        }

        [TestMethod]
        public async Task EditInputModelValidReturnsRedirectToIndex()
        {
            
            var target = new AccountController(_mockAccountService.Object);

            var result = await target.Edit(new Account() {AccountID = 1});

            _mockAccountService.Verify(m => m.UpdateAsync(It.IsAny<Account>()),Times.Once);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
        }

        [TestMethod]
        public void AddReturnsPartialView()
        {
            AccountController target = new AccountController(null);

            var result = target.Add(new WebUser());
            var model = ((PartialViewResult)result).ViewData.Model as Account;

            Assert.IsInstanceOfType(result, typeof(PartialViewResult));
            Assert.AreEqual(model.AccountID, 0);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task AddModelStateValidReturnsRedirectToIndex()
        {
            Account account = new Account() { AccountID = 1, AccountName = "Acc1" };
            AccountController target = new AccountController(_mockAccountService.Object);

            var result = await target.Add(new WebUser() { Id = "1" }, account);

            _mockAccountService.Verify(m => m.Create(account),Times.Once);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
        }

        [TestMethod]
        public async Task Cannot_Add_Invalid_Account()
        {
            Account account = new Account() { AccountID = 1, AccountName = "Acc1" };
            AccountController target = new AccountController(null);
            target.ModelState.AddModelError("error", "error");

            var result = await target.Add(new WebUser() { Id = "1" }, account);

            Assert.IsInstanceOfType(result, typeof(PartialViewResult));
        }

        [TestMethod]
        public async Task DeleteInput6ReturnsRedirectToIndex()
        {
            _mockAccountService.Setup(m => m.HasAnyDependencies(It.Is<int>(v => v >= 6 && v <= 10))).Returns(false);
            var target = new AccountController(_mockAccountService.Object);

            var id6 = await target.Delete(new WebUser() {Id = "1"}, 6);

            Assert.IsInstanceOfType(id6,typeof(RedirectToRouteResult));
        }

        [TestMethod]
        public async Task DeleteInput3ReturnsRedirectAfterDelete()
        {
            _mockAccountService.Setup(m => m.HasAnyDependencies(It.Is<int>(v => v >= 1 && v <= 5))).Returns(true);
            var target = new AccountController(_mockAccountService.Object);

            var id3 = await target.Delete(new WebUser() { Id = "1" }, 3);

            _mockAccountService.Verify(m => m.HasAnyDependencies(3),Times.Once);
            Assert.IsInstanceOfType(id3, typeof(RedirectToRouteResult));
        }

        [TestMethod]
        public async Task TransferMoneyReturnPartialView()
        {
            _mockAccountService.Setup(m => m.GetListAsync()).ReturnsAsync(_accounts);
            AccountController target = new AccountController(_mockAccountService.Object);

            var result = ((PartialViewResult)await  target.TransferMoney(new WebUser() { Id = "1" })).ViewData.Model as TransferModel;

            Assert.AreEqual(result.FromAccounts.Count, 2);
        }

        [TestMethod]
        public async Task TransferMoneyIfAccountHasNotEnoughMoney()
        {
            _mockAccountService.Setup(m => m.GetListAsync()).ReturnsAsync(_accounts);
            _mockAccountService.Setup((IAccountService m) => m.GetItemAsync(It.IsAny<int>())).ReturnsAsync(new Account() { AccountID = 1, UserId = "1", Cash = 5000 });
            TransferModel tmodel = new TransferModel() { FromId = 1, ToId = 1, Summ = 1000.ToString() };
            WebUser user = new WebUser() { Id = "1" };
            AccountController target = new AccountController(_mockAccountService.Object);

            ActionResult result = await target.TransferMoney(user, tmodel);

            Assert.IsInstanceOfType(result, typeof(PartialViewResult));
            _mockAccountService.Verify(m => m.UpdateAsync(It.IsAny<Account>()),Times.Never);
        }

        [TestMethod]
        public async Task TransferMoneyModelStateInvalid()
        {
            _mockAccountService.Setup(m => m.GetListAsync()).ReturnsAsync(_accounts);
            _mockAccountService.Setup(m => m.GetItemAsync(It.IsAny<int>()))
                .ReturnsAsync(new Account() { AccountID = 1, UserId = "1", Cash = 5000 });
            TransferModel tModel = new TransferModel()
            {
                FromAccounts = ((List<Account>)await _mockAccountService.Object.GetListAsync()).ToList(),
                Summ = "1000"
            };
            AccountController target = new AccountController(_mockAccountService.Object);

            target.ModelState.AddModelError("error", "error");
            var result = await target.TransferMoney(new WebUser() { Id = "1" }, tModel);

            Assert.IsInstanceOfType(result, typeof(PartialViewResult));
        }

        [TestMethod]
        public async Task TransferMoneyIfAccountHasEnoughMoney()
        {
            _mockAccountService.Setup(m => m.HasEnoughMoney(It.IsAny<Account>(), It.IsAny<decimal>())).Returns(true);
            _mockAccountService.Setup(m => m.GetListAsync()).ReturnsAsync(_accounts);
            _mockAccountService.Setup((IAccountService m) => m.GetItemAsync(It.IsAny<int>())).ReturnsAsync(new Account() { AccountID = 1, UserId = "1", Cash = 5000 });
            TransferModel tmodel = new TransferModel() { FromId = 1, ToId = 1, Summ = 1000.ToString() };
            WebUser user = new WebUser() { Id = "1" };
            AccountController target = new AccountController(_mockAccountService.Object);

            ActionResult result = await target.TransferMoney(user, tmodel);

            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            _mockAccountService.Verify(m => m.UpdateAsync(It.IsAny<Account>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task GetItemsReturnsListofAccounts()
        {
            _mockAccountService.Setup(m => m.GetListAsync()).ReturnsAsync(_accounts);
            AccountController target = new AccountController(_mockAccountService.Object);
            var id = 3;

            var result = (await target.GetItems(id, new WebUser() { Id = "1" })).ViewData.Model as List<Account>;

            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(result[0].AccountID, 1);
        }
    }
}
