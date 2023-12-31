﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SagaSeminar.Services.InventoryService.Entities;

#nullable disable

namespace SagaSeminar.Services.InventoryService.Migrations
{
    [DbContext(typeof(InventoryDbContext))]
    partial class InventoryDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.18")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("SagaSeminar.Services.InventoryService.Entities.InventoryNoteEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Note")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("InventoryNote");

                    b.HasData(
                        new
                        {
                            Id = new Guid("d60f9c5e-174f-4cd0-a841-20a830f7abb9"),
                            CreatedTime = new DateTime(2023, 6, 19, 20, 13, 4, 141, DateTimeKind.Local).AddTicks(5995),
                            Quantity = 1000000,
                            Reason = "Initial reception",
                            TransactionId = new Guid("663bf954-3203-4830-a102-e14b750a8cf9")
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
