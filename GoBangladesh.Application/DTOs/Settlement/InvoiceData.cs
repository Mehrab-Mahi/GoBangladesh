using System;
using System.Collections.Generic;
using GoBangladesh.Domain.Entities;

namespace GoBangladesh.Application.DTOs.Settlement;

public class InvoiceData : Invoice
{
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
}