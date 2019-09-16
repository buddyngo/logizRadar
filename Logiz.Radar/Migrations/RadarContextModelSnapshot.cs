﻿// <auto-generated />
using System;
using Logiz.Radar.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Logiz.Radar.Migrations
{
    [DbContext(typeof(RadarContext))]
    partial class RadarContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Logiz.Radar.Data.Model.Project", b =>
                {
                    b.Property<string>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<bool>("IsActive");

                    b.Property<string>("ProjectName")
                        .IsRequired();

                    b.Property<string>("UpdatedBy");

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("ID");

                    b.ToTable("Project");
                });

            modelBuilder.Entity("Logiz.Radar.Data.Model.TestCase", b =>
                {
                    b.Property<string>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ActualResult");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<string>("ExpectedResult")
                        .IsRequired();

                    b.Property<bool>("IsActive");

                    b.Property<string>("Note");

                    b.Property<DateTime?>("PlannedDate");

                    b.Property<string>("TestCaseName")
                        .IsRequired();

                    b.Property<string>("TestCaseSteps")
                        .IsRequired();

                    b.Property<int>("TestStatus");

                    b.Property<string>("TestVariantID")
                        .IsRequired();

                    b.Property<string>("TesterName");

                    b.Property<string>("UpdatedBy");

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("ID");

                    b.ToTable("TestCase");
                });

            modelBuilder.Entity("Logiz.Radar.Data.Model.TestScenario", b =>
                {
                    b.Property<string>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<bool>("IsActive");

                    b.Property<string>("ProjectID")
                        .IsRequired();

                    b.Property<string>("ScenarioName")
                        .IsRequired();

                    b.Property<string>("UpdatedBy");

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("ID");

                    b.ToTable("TestScenario");
                });

            modelBuilder.Entity("Logiz.Radar.Data.Model.TestVariant", b =>
                {
                    b.Property<string>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<bool>("IsActive");

                    b.Property<string>("ScenarioID")
                        .IsRequired();

                    b.Property<string>("UpdatedBy");

                    b.Property<DateTime>("UpdatedDateTime");

                    b.Property<string>("VariantName")
                        .IsRequired();

                    b.HasKey("ID");

                    b.ToTable("TestVariant");
                });

            modelBuilder.Entity("Logiz.Radar.Data.Model.UserMappingProject", b =>
                {
                    b.Property<string>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<bool>("IsActive");

                    b.Property<string>("ProjectID")
                        .IsRequired();

                    b.Property<string>("UpdatedBy");

                    b.Property<DateTime>("UpdatedDateTime");

                    b.Property<string>("Username")
                        .IsRequired();

                    b.HasKey("ID");

                    b.ToTable("UserMappingProject");
                });
#pragma warning restore 612, 618
        }
    }
}
