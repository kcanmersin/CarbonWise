﻿// <auto-generated />
using System;
using CarbonWise.BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CarbonWise.BuildingBlocks.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.AirQuality.AirQuality", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<int>("AQI")
                        .HasColumnType("int");

                    b.Property<double?>("CO")
                        .HasPrecision(10, 2)
                        .HasColumnType("double");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("DominentPollutant")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<double?>("Humidity")
                        .HasPrecision(10, 2)
                        .HasColumnType("double");

                    b.Property<double>("Latitude")
                        .HasPrecision(10, 7)
                        .HasColumnType("double");

                    b.Property<double>("Longitude")
                        .HasPrecision(10, 7)
                        .HasColumnType("double");

                    b.Property<double?>("NO2")
                        .HasPrecision(10, 2)
                        .HasColumnType("double");

                    b.Property<double?>("Ozone")
                        .HasPrecision(10, 2)
                        .HasColumnType("double");

                    b.Property<double?>("PM10")
                        .HasPrecision(10, 2)
                        .HasColumnType("double");

                    b.Property<double?>("PM25")
                        .HasPrecision(10, 2)
                        .HasColumnType("double");

                    b.Property<double?>("Pressure")
                        .HasPrecision(10, 2)
                        .HasColumnType("double");

                    b.Property<DateTime>("RecordDate")
                        .HasColumnType("datetime(6)");

                    b.Property<double?>("SO2")
                        .HasPrecision(10, 2)
                        .HasColumnType("double");

                    b.Property<double?>("Temperature")
                        .HasPrecision(10, 2)
                        .HasColumnType("double");

                    b.Property<double?>("WindSpeed")
                        .HasPrecision(10, 2)
                        .HasColumnType("double");

                    b.HasKey("Id");

                    b.HasIndex("City");

                    b.HasIndex("RecordDate");

                    b.HasIndex("City", "RecordDate");

                    b.ToTable("AirQualities", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.Buildings.Building", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<string>("E_MeterCode")
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<string>("G_MeterCode")
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Buildings", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.CarbonFootprintTest", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<string>("CategoryResults")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CompletedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("TotalFootprint")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("CarbonFootprintTests", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.TestQuestion", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.HasKey("Id");

                    b.ToTable("TestQuestions", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.TestQuestionOption", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<decimal>("FootprintFactor")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid>("QuestionId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(600)
                        .HasColumnType("varchar(600)");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("TestQuestionOptions", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.TestResponse", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("CarbonFootprintTestId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("QuestionId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("SelectedOptionId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("TestId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("CarbonFootprintTestId");

                    b.HasIndex("QuestionId");

                    b.HasIndex("SelectedOptionId");

                    b.ToTable("TestResponses", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.Electrics.Electric", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("BuildingId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("FinalMeterValue")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("InitialMeterValue")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("KWHValue")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Usage")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("BuildingId");

                    b.HasIndex("Date");

                    b.ToTable("Electrics", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.NaturalGases.NaturalGas", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("BuildingId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("FinalMeterValue")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("InitialMeterValue")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("SM3Value")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Usage")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("BuildingId");

                    b.HasIndex("Date");

                    b.ToTable("NaturalGas", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.Papers.Paper", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("BuildingId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("Usage")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("BuildingId");

                    b.HasIndex("Date");

                    b.ToTable("Papers", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.SchoolInfos.CampusVehicleEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<int>("CarsEnteringUniversity")
                        .HasColumnType("int");

                    b.Property<int>("CarsManagedByUniversity")
                        .HasColumnType("int");

                    b.Property<int>("MotorcyclesEnteringUniversity")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("CampusVehicleEntries", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.SchoolInfos.SchoolInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("CampusVehicleEntryId")
                        .HasColumnType("char(36)");

                    b.Property<int>("NumberOfPeople")
                        .HasColumnType("int");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CampusVehicleEntryId");

                    b.HasIndex("Year")
                        .IsUnique();

                    b.ToTable("SchoolInfos", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.Users.User", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<string>("ApiKey")
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Gender")
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<bool>("IsAcademicPersonal")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsAdministrativeStaff")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsInInstitution")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsStudent")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Surname")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int?>("SustainabilityPoint")
                        .HasColumnType("int");

                    b.Property<string>("UniqueId")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.Waters.Water", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("BuildingId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("FinalMeterValue")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("InitialMeterValue")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Usage")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("BuildingId");

                    b.HasIndex("Date");

                    b.ToTable("Waters", (string)null);
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.CarbonFootprintTest", b =>
                {
                    b.HasOne("CarbonWise.BuildingBlocks.Domain.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.TestQuestionOption", b =>
                {
                    b.HasOne("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.TestQuestion", null)
                        .WithMany("Options")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.TestResponse", b =>
                {
                    b.HasOne("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.CarbonFootprintTest", null)
                        .WithMany("Responses")
                        .HasForeignKey("CarbonFootprintTestId");

                    b.HasOne("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.TestQuestion", "Question")
                        .WithMany()
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.TestQuestionOption", "SelectedOption")
                        .WithMany()
                        .HasForeignKey("SelectedOptionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Question");

                    b.Navigation("SelectedOption");
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.Electrics.Electric", b =>
                {
                    b.HasOne("CarbonWise.BuildingBlocks.Domain.Buildings.Building", "Building")
                        .WithMany()
                        .HasForeignKey("BuildingId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Building");
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.NaturalGases.NaturalGas", b =>
                {
                    b.HasOne("CarbonWise.BuildingBlocks.Domain.Buildings.Building", "Building")
                        .WithMany()
                        .HasForeignKey("BuildingId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Building");
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.Papers.Paper", b =>
                {
                    b.HasOne("CarbonWise.BuildingBlocks.Domain.Buildings.Building", "Building")
                        .WithMany()
                        .HasForeignKey("BuildingId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Building");
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.SchoolInfos.SchoolInfo", b =>
                {
                    b.HasOne("CarbonWise.BuildingBlocks.Domain.SchoolInfos.CampusVehicleEntry", "Vehicles")
                        .WithMany()
                        .HasForeignKey("CampusVehicleEntryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Vehicles");
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.Waters.Water", b =>
                {
                    b.HasOne("CarbonWise.BuildingBlocks.Domain.Buildings.Building", "Building")
                        .WithMany()
                        .HasForeignKey("BuildingId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Building");
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.CarbonFootprintTest", b =>
                {
                    b.Navigation("Responses");
                });

            modelBuilder.Entity("CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest.TestQuestion", b =>
                {
                    b.Navigation("Options");
                });
#pragma warning restore 612, 618
        }
    }
}
