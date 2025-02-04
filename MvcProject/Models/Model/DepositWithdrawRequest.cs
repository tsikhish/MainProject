﻿using MvcProject.Models.Repository.IRepository.Enum;
using System.ComponentModel.DataAnnotations;

namespace MvcProject.Models.Model
{
    public class DepositWithdrawRequest
    {
        public int Id { get; set; } 
        public string UserId { get; set; }
        [EnumDataType(typeof(TransactionType))]
        public TransactionType TransactionType { get; set; }
        public decimal Amount {  get; set; }
        [EnumDataType(typeof(Status))]
        public Status Status { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
