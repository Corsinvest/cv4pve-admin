﻿/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */

// <auto-generated />
using System;
using Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Migrations
{
    [DbContext(typeof(ReplicationTrendDbContext))]
    partial class ReplicationTrendDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("NOCASE")
                .HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Models.ReplicationResult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClusterName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Duration")
                        .HasColumnType("REAL");

                    b.Property<DateTime?>("End")
                        .HasColumnType("TEXT");

                    b.Property<string>("Error")
                        .HasColumnType("TEXT");

                    b.Property<string>("JobId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastSync")
                        .HasColumnType("TEXT");

                    b.Property<string>("Log")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Size")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Start")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VmId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("End");

                    b.HasIndex("LastSync");

                    b.HasIndex("Start");

                    b.HasIndex("VmId");

                    b.ToTable("ReplicationResults");
                });
#pragma warning restore 612, 618
        }
    }
}
