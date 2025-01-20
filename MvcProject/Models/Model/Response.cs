﻿using MvcProject.Models.IRepository.Enum;

namespace MvcProject.Models.Model
{
    public class Response
    {
        public int DepositWithdrawRequestId {  get; set; }
        public Status? Status { get; set; }
        public string? Hash { get; set; }
    }
}
