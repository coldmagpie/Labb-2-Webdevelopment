﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebbLabb2.Shared.DTOs;

namespace DataAccess.DataAccess.Models
{
    public class OrderItemsModel
    {
        public int Id { get; set; }
        [ForeignKey("OrderId")]
        public int OrderId { get; set; }
        public ProductModel Product { get; set; }
        public int Quantity { get; set; }
        public double PriceEach { get; set; }
    }
}
