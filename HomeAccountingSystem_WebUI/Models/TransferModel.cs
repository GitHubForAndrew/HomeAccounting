﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HomeAccountingSystem_DAL.Model;
using HomeAccountingSystem_WebUI.Infrastructure;

namespace HomeAccountingSystem_WebUI.Models
{
    public class TransferModel
    {
        public List<Account> FromAccounts { get; set; }
        public List<Account> ToAccounts { get; set; }

        [TransferModel(ErrorMessage = "Необходимо завести хотя бы один счет")]
        public int FromId { get; set; }

        public int ToId { get; set; }

        [Required(ErrorMessage = "Введите сумму для перевода")]
        [RegularExpression(@"\d+(,)?\d+",ErrorMessage = "Некорректно введена сумма")]
        public string Summ { get; set; }
    }
}