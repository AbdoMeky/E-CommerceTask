using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IF.Data.Configuration
{
    public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasOne(x => x.Order).WithMany(x => x.OrderItems).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(x=>x.Product).WithMany(x=>x.OrderItems).HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
