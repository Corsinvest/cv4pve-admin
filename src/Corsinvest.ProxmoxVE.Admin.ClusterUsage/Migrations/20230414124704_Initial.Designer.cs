﻿/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */

// <auto-generated />
using System;
using Corsinvest.ProxmoxVE.Admin.ClusterUsage.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage.Migrations
{
    [DbContext(typeof(ClusterUsageDbContext))]
    [Migration("20230414124704_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("NOCASE")
                .HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("Corsinvest.ProxmoxVE.Admin.ClusterUsage.Models.DataVm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClusterName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("CpuSize")
                        .HasColumnType("INTEGER");

                    b.Property<double>("CpuUsagePercentage")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<long>("MemorySize")
                        .HasColumnType("INTEGER");

                    b.Property<long>("MemoryUsage")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Node")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("VmId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VmName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("DataVms");
                });

            modelBuilder.Entity("Corsinvest.ProxmoxVE.Admin.ClusterUsage.Models.DataVmStorage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("DataVmId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Storage")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DataVmId");

                    b.ToTable("DataVmStorages");
                });

            modelBuilder.Entity("Corsinvest.ProxmoxVE.Admin.ClusterUsage.Models.DataVmStorage", b =>
                {
                    b.HasOne("Corsinvest.ProxmoxVE.Admin.ClusterUsage.Models.DataVm", "DataVm")
                        .WithMany("Storages")
                        .HasForeignKey("DataVmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DataVm");
                });

            modelBuilder.Entity("Corsinvest.ProxmoxVE.Admin.ClusterUsage.Models.DataVm", b =>
                {
                    b.Navigation("Storages");
                });
#pragma warning restore 612, 618
        }
    }
}
