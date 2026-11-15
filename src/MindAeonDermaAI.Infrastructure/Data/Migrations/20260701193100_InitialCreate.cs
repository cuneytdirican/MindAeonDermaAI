using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindAeonDermaAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AestheticProcedures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AestheticProcedures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ProviderType = table.Column<int>(type: "int", nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ApiKeyEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultChatModel = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DefaultVisionModel = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ExtraHeadersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlcoholUses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlcoholUses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BiologicalDomains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiologicalDomains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cohorts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsSynthetic = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cohorts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FitzpatrickTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FitzpatrickTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterventionCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenopauseStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenopauseStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhenotypeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhenotypeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sexes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sexes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SkinTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkinTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SleepQualities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleepQualities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmokingStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmokingStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StressLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StressLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UVExposureLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UVExposureLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    ModelId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Capabilities = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiModels_AiProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiRequestLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    TaskKind = table.Column<int>(type: "int", nullable: false),
                    AssessmentId = table.Column<int>(type: "int", nullable: true),
                    PromptTokens = table.Column<int>(type: "int", nullable: true),
                    CompletionTokens = table.Column<int>(type: "int", nullable: true),
                    LatencyMs = table.Column<int>(type: "int", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiRequestLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiRequestLogs_AiProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Panels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DomainId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Panels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Panels_BiologicalDomains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "BiologicalDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProteinBiomarkers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DomainId = table.Column<int>(type: "int", nullable: false),
                    NeedDirection = table.Column<int>(type: "int", nullable: false),
                    YoungRefMean = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    YoungRefSd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    IsSyntheticReference = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProteinBiomarkers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProteinBiomarkers_BiologicalDomains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "BiologicalDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProteinBiomarkers_MeasurementUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "MeasurementUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CohortSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohortId = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: true),
                    SexId = table.Column<int>(type: "int", nullable: true),
                    ProfileLabel = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CohortSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CohortSubjects_Cohorts_CohortId",
                        column: x => x.CohortId,
                        principalTable: "Cohorts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CohortSubjects_Sexes_SexId",
                        column: x => x.SexId,
                        principalTable: "Sexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    HeightCm = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Bmi = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SexId = table.Column<int>(type: "int", nullable: true),
                    SkinTypeId = table.Column<int>(type: "int", nullable: true),
                    FitzpatrickTypeId = table.Column<int>(type: "int", nullable: true),
                    MenopauseStatusId = table.Column<int>(type: "int", nullable: true),
                    SmokingStatusId = table.Column<int>(type: "int", nullable: true),
                    AlcoholUseId = table.Column<int>(type: "int", nullable: true),
                    SleepQualityId = table.Column<int>(type: "int", nullable: true),
                    StressLevelId = table.Column<int>(type: "int", nullable: true),
                    UVExposureLevelId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_AlcoholUses_AlcoholUseId",
                        column: x => x.AlcoholUseId,
                        principalTable: "AlcoholUses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_FitzpatrickTypes_FitzpatrickTypeId",
                        column: x => x.FitzpatrickTypeId,
                        principalTable: "FitzpatrickTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_MenopauseStatuses_MenopauseStatusId",
                        column: x => x.MenopauseStatusId,
                        principalTable: "MenopauseStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_Sexes_SexId",
                        column: x => x.SexId,
                        principalTable: "Sexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_SkinTypes_SkinTypeId",
                        column: x => x.SkinTypeId,
                        principalTable: "SkinTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_SleepQualities_SleepQualityId",
                        column: x => x.SleepQualityId,
                        principalTable: "SleepQualities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_SmokingStatuses_SmokingStatusId",
                        column: x => x.SmokingStatusId,
                        principalTable: "SmokingStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_StressLevels_StressLevelId",
                        column: x => x.StressLevelId,
                        principalTable: "StressLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subjects_UVExposureLevels_UVExposureLevelId",
                        column: x => x.UVExposureLevelId,
                        principalTable: "UVExposureLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PanelBiomarkers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PanelId = table.Column<int>(type: "int", nullable: false),
                    BiomarkerId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PanelBiomarkers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PanelBiomarkers_Panels_PanelId",
                        column: x => x.PanelId,
                        principalTable: "Panels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PanelBiomarkers_ProteinBiomarkers_BiomarkerId",
                        column: x => x.BiomarkerId,
                        principalTable: "ProteinBiomarkers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CohortMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohortSubjectId = table.Column<int>(type: "int", nullable: false),
                    BiomarkerId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CohortMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CohortMeasurements_CohortSubjects_CohortSubjectId",
                        column: x => x.CohortSubjectId,
                        principalTable: "CohortSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CohortMeasurements_ProteinBiomarkers_BiomarkerId",
                        column: x => x.BiomarkerId,
                        principalTable: "ProteinBiomarkers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    InterventionCategoryId = table.Column<int>(type: "int", nullable: true),
                    TimePoint = table.Column<int>(type: "int", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SelectedPanelId = table.Column<int>(type: "int", nullable: true),
                    PrimaryPhenotypeId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_InterventionCategories_InterventionCategoryId",
                        column: x => x.InterventionCategoryId,
                        principalTable: "InterventionCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assessments_Panels_SelectedPanelId",
                        column: x => x.SelectedPanelId,
                        principalTable: "Panels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assessments_PhenotypeTypes_PrimaryPhenotypeId",
                        column: x => x.PrimaryPhenotypeId,
                        principalTable: "PhenotypeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assessments_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectAestheticProcedures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    AestheticProcedureId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectAestheticProcedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectAestheticProcedures_AestheticProcedures_AestheticProcedureId",
                        column: x => x.AestheticProcedureId,
                        principalTable: "AestheticProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectAestheticProcedures_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProteinMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    BiomarkerId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    ZScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    FoldChange = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PercentChange = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IsMissing = table.Column<bool>(type: "bit", nullable: false),
                    IsOutlier = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProteinMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProteinMeasurements_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProteinMeasurements_MeasurementUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "MeasurementUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProteinMeasurements_ProteinBiomarkers_BiomarkerId",
                        column: x => x.BiomarkerId,
                        principalTable: "ProteinBiomarkers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    SectionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasoningSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutiveSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalizedInterpretation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhenotypeClassificationText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Limitations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsedSyntheticData = table.Column<bool>(type: "bit", nullable: false),
                    GeneratedByProvider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedByModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportDocuments_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScoreResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    CollagenRemodelingNeedScore = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BarrierHydrationNeedScore = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DeepEcmLaxityNeedScore = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IsMixedPhenotype = table.Column<bool>(type: "bit", nullable: false),
                    PanelScore = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ImageQualityScore = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LifestyleScore = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CohortPercentileScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IntegratedSkinQualityScore = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Confidence = table.Column<int>(type: "int", nullable: false),
                    UsedRealCohort = table.Column<bool>(type: "bit", nullable: false),
                    UsedSyntheticCohort = table.Column<bool>(type: "bit", nullable: false),
                    ModelUsed = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ModelRationale = table.Column<string>(type: "nvarchar(1200)", maxLength: 1200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoreResults_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkinImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    HasLightingArtifact = table.Column<bool>(type: "bit", nullable: false),
                    HasMakeup = table.Column<bool>(type: "bit", nullable: false),
                    HasFilter = table.Column<bool>(type: "bit", nullable: false),
                    NonStandardAngle = table.Column<bool>(type: "bit", nullable: false),
                    HydrationScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TextureSmoothnessScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ElasticityScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    WrinkleBurdenScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PigmentationUniformityScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    RednessHealthScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PoreVisibilityScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    BrightnessScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    OverallSkinQualityScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AnalysisNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsAnalyzed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkinImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkinImages_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiModels_ProviderId",
                table: "AiModels",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_AiProviders_Name",
                table: "AiProviders",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiRequestLogs_ProviderId",
                table: "AiRequestLogs",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_InterventionCategoryId",
                table: "Assessments",
                column: "InterventionCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_PrimaryPhenotypeId",
                table: "Assessments",
                column: "PrimaryPhenotypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_SelectedPanelId",
                table: "Assessments",
                column: "SelectedPanelId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_SubjectId",
                table: "Assessments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CohortMeasurements_BiomarkerId",
                table: "CohortMeasurements",
                column: "BiomarkerId");

            migrationBuilder.CreateIndex(
                name: "IX_CohortMeasurements_CohortSubjectId",
                table: "CohortMeasurements",
                column: "CohortSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CohortSubjects_CohortId",
                table: "CohortSubjects",
                column: "CohortId");

            migrationBuilder.CreateIndex(
                name: "IX_CohortSubjects_SexId",
                table: "CohortSubjects",
                column: "SexId");

            migrationBuilder.CreateIndex(
                name: "IX_PanelBiomarkers_BiomarkerId",
                table: "PanelBiomarkers",
                column: "BiomarkerId");

            migrationBuilder.CreateIndex(
                name: "IX_PanelBiomarkers_PanelId_BiomarkerId",
                table: "PanelBiomarkers",
                columns: new[] { "PanelId", "BiomarkerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Panels_DomainId",
                table: "Panels",
                column: "DomainId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProteinBiomarkers_DomainId",
                table: "ProteinBiomarkers",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_ProteinBiomarkers_UnitId",
                table: "ProteinBiomarkers",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProteinMeasurements_AssessmentId",
                table: "ProteinMeasurements",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProteinMeasurements_BiomarkerId",
                table: "ProteinMeasurements",
                column: "BiomarkerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProteinMeasurements_UnitId",
                table: "ProteinMeasurements",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDocuments_AssessmentId",
                table: "ReportDocuments",
                column: "AssessmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScoreResults_AssessmentId",
                table: "ScoreResults",
                column: "AssessmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkinImages_AssessmentId",
                table: "SkinImages",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectAestheticProcedures_AestheticProcedureId",
                table: "SubjectAestheticProcedures",
                column: "AestheticProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectAestheticProcedures_SubjectId",
                table: "SubjectAestheticProcedures",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_AlcoholUseId",
                table: "Subjects",
                column: "AlcoholUseId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Code",
                table: "Subjects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_FitzpatrickTypeId",
                table: "Subjects",
                column: "FitzpatrickTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_MenopauseStatusId",
                table: "Subjects",
                column: "MenopauseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_SexId",
                table: "Subjects",
                column: "SexId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_SkinTypeId",
                table: "Subjects",
                column: "SkinTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_SleepQualityId",
                table: "Subjects",
                column: "SleepQualityId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_SmokingStatusId",
                table: "Subjects",
                column: "SmokingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_StressLevelId",
                table: "Subjects",
                column: "StressLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UVExposureLevelId",
                table: "Subjects",
                column: "UVExposureLevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiModels");

            migrationBuilder.DropTable(
                name: "AiRequestLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CohortMeasurements");

            migrationBuilder.DropTable(
                name: "PanelBiomarkers");

            migrationBuilder.DropTable(
                name: "ProteinMeasurements");

            migrationBuilder.DropTable(
                name: "ReportDocuments");

            migrationBuilder.DropTable(
                name: "ScoreResults");

            migrationBuilder.DropTable(
                name: "SkinImages");

            migrationBuilder.DropTable(
                name: "SubjectAestheticProcedures");

            migrationBuilder.DropTable(
                name: "AiProviders");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CohortSubjects");

            migrationBuilder.DropTable(
                name: "ProteinBiomarkers");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "AestheticProcedures");

            migrationBuilder.DropTable(
                name: "Cohorts");

            migrationBuilder.DropTable(
                name: "MeasurementUnits");

            migrationBuilder.DropTable(
                name: "InterventionCategories");

            migrationBuilder.DropTable(
                name: "Panels");

            migrationBuilder.DropTable(
                name: "PhenotypeTypes");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "BiologicalDomains");

            migrationBuilder.DropTable(
                name: "AlcoholUses");

            migrationBuilder.DropTable(
                name: "FitzpatrickTypes");

            migrationBuilder.DropTable(
                name: "MenopauseStatuses");

            migrationBuilder.DropTable(
                name: "Sexes");

            migrationBuilder.DropTable(
                name: "SkinTypes");

            migrationBuilder.DropTable(
                name: "SleepQualities");

            migrationBuilder.DropTable(
                name: "SmokingStatuses");

            migrationBuilder.DropTable(
                name: "StressLevels");

            migrationBuilder.DropTable(
                name: "UVExposureLevels");
        }
    }
}
