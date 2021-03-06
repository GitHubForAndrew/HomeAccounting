﻿using System.Collections.Generic;
using System.Web.Mvc;
using HomeAccountingSystem_DAL.Abstract;
using HomeAccountingSystem_DAL.Model;
using HomeAccountingSystem_WebUI.Controllers;
using HomeAccountingSystem_WebUI.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Services;

namespace WebUI.Tests
{
    [TestClass]
    public class NavRightControllertTests
    {
        private readonly Mock<IPayingItemService> _service;
        private readonly Mock<IDbHelper> _dbHelper;


        public NavRightControllertTests()
        {
            _service = new Mock<IPayingItemService>();
            _dbHelper = new Mock<IDbHelper>();
        }


        [TestMethod]
        public void GetIncomingBudget()
        {
            _service.Setup(m => m.GetListByTypeOfFlow(It.IsAny<IWorkingUser>(),It.IsAny<int>())).Returns(new List<PayingItem>());
            _dbHelper.Setup(m => m.GetSummForDay(It.IsAny<List<PayingItem>>())).Returns(500);
            _dbHelper.Setup(m => m.GetSummForMonth(It.IsAny<List<PayingItem>>())).Returns(300);
            _dbHelper.Setup(m => m.GetSummForWeek(It.IsAny<List<PayingItem>>())).Returns(1000);
            var target = new NavRightController(_service.Object, _dbHelper.Object);

            var result = ((PartialViewResult)target.MenuIncoming(new WebUser())).Model as BudgetModel;

            Assert.AreEqual(500, result.Day);
            Assert.AreEqual(300, result.Month);
            Assert.AreEqual(1000, result.Week);
        }

        [TestMethod]
        public void GetOutgoBudget()
        {
            _service.Setup(m => m.GetListByTypeOfFlow(It.IsAny<IWorkingUser>(),It.IsAny<int>())).Returns(new List<PayingItem>());
            var target = new NavRightController(_service.Object, _dbHelper.Object);
            _dbHelper.Setup(m => m.GetSummForDay(It.IsAny<List<PayingItem>>())).Returns(1000);
            _dbHelper.Setup(m => m.GetSummForMonth(It.IsAny<List<PayingItem>>())).Returns(1500);
            _dbHelper.Setup(m => m.GetSummForWeek(It.IsAny<List<PayingItem>>())).Returns(1300);

            var result = ((PartialViewResult)target.MenuOutgo(new WebUser())).Model as BudgetModel; 

            Assert.AreEqual(1000, result.Day);
            Assert.AreEqual(1300, result.Week);
            Assert.AreEqual(1500, result.Month);
        }
    }
}
