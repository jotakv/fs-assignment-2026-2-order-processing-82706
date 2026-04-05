using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncStoreSchemaForCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[Products]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Products] (
                        [ProductID] bigint NOT NULL IDENTITY(1,1),
                        [Name] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [Price] decimal(8,2) NOT NULL,
                        [Category] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_Products] PRIMARY KEY ([ProductID])
                    );
                END;
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[Orders]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Orders] (
                        [OrderID] int NOT NULL IDENTITY(1,1),
                        [CustomerId] int NULL,
                        [Name] nvarchar(max) NOT NULL,
                        [Line1] nvarchar(max) NOT NULL,
                        [Line2] nvarchar(max) NULL,
                        [Line3] nvarchar(max) NULL,
                        [City] nvarchar(max) NOT NULL,
                        [State] nvarchar(max) NOT NULL,
                        [Zip] nvarchar(max) NULL,
                        [Country] nvarchar(max) NOT NULL,
                        [GiftWrap] bit NOT NULL,
                        [Shipped] bit NOT NULL,
                        [Status] nvarchar(50) NOT NULL,
                        [StripeSessionId] nvarchar(max) NULL,
                        [StripePaymentIntentId] nvarchar(max) NULL,
                        [StripePaymentStatus] nvarchar(max) NULL,
                        [CreatedAtUtc] datetime2 NOT NULL,
                        [UpdatedAtUtc] datetime2 NOT NULL,
                        [PaidAtUtc] datetime2 NULL,
                        [CompletedAtUtc] datetime2 NULL,
                        [FailedAtUtc] datetime2 NULL,
                        CONSTRAINT [PK_Orders] PRIMARY KEY ([OrderID])
                    );
                END
                ELSE
                BEGIN
                    IF COL_LENGTH(N'Orders', N'CustomerId') IS NULL
                    BEGIN
                        ALTER TABLE [Orders] ADD [CustomerId] int NULL;
                    END;

                    IF COL_LENGTH(N'Orders', N'Status') IS NULL
                    BEGIN
                        ALTER TABLE [Orders] ADD [Status] nvarchar(50) NULL;
                    END;

                    IF COL_LENGTH(N'Orders', N'CreatedAtUtc') IS NULL
                    BEGIN
                        ALTER TABLE [Orders] ADD [CreatedAtUtc] datetime2 NULL;
                    END;

                    IF COL_LENGTH(N'Orders', N'UpdatedAtUtc') IS NULL
                    BEGIN
                        ALTER TABLE [Orders] ADD [UpdatedAtUtc] datetime2 NULL;
                    END;

                    IF COL_LENGTH(N'Orders', N'CompletedAtUtc') IS NULL
                    BEGIN
                        ALTER TABLE [Orders] ADD [CompletedAtUtc] datetime2 NULL;
                    END;

                    IF COL_LENGTH(N'Orders', N'FailedAtUtc') IS NULL
                    BEGIN
                        ALTER TABLE [Orders] ADD [FailedAtUtc] datetime2 NULL;
                    END;

                    IF COL_LENGTH(N'Orders', N'PaidAtUtc') IS NULL
                    BEGIN
                        ALTER TABLE [Orders] ADD [PaidAtUtc] datetime2 NULL;
                    END;

                    IF COL_LENGTH(N'Orders', N'StripeSessionId') IS NULL
                    BEGIN
                        ALTER TABLE [Orders] ADD [StripeSessionId] nvarchar(max) NULL;
                    END;

                    IF COL_LENGTH(N'Orders', N'StripePaymentIntentId') IS NULL
                    BEGIN
                        ALTER TABLE [Orders] ADD [StripePaymentIntentId] nvarchar(max) NULL;
                    END;

                    IF COL_LENGTH(N'Orders', N'StripePaymentStatus') IS NULL
                    BEGIN
                        ALTER TABLE [Orders] ADD [StripePaymentStatus] nvarchar(max) NULL;
                    END;
                END;
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
                BEGIN
                    UPDATE [Orders]
                    SET [Status] = CASE
                        WHEN [Shipped] = CAST(1 AS bit) THEN N'Completed'
                        ELSE N'Submitted'
                    END
                    WHERE [Status] IS NULL;

                    UPDATE [Orders]
                    SET [CreatedAtUtc] = COALESCE([CreatedAtUtc], [PaidAtUtc], SYSUTCDATETIME())
                    WHERE [CreatedAtUtc] IS NULL;

                    UPDATE [Orders]
                    SET [UpdatedAtUtc] = COALESCE([UpdatedAtUtc], [PaidAtUtc], [CreatedAtUtc], SYSUTCDATETIME())
                    WHERE [UpdatedAtUtc] IS NULL;

                    UPDATE [Orders]
                    SET [CompletedAtUtc] = COALESCE([CompletedAtUtc], [PaidAtUtc])
                    WHERE [Shipped] = CAST(1 AS bit)
                        AND [CompletedAtUtc] IS NULL;

                    ALTER TABLE [Orders] ALTER COLUMN [Status] nvarchar(50) NOT NULL;
                    ALTER TABLE [Orders] ALTER COLUMN [CreatedAtUtc] datetime2 NOT NULL;
                    ALTER TABLE [Orders] ALTER COLUMN [UpdatedAtUtc] datetime2 NOT NULL;
                END;
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[CartLine]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [CartLine] (
                        [CartLineID] int NOT NULL IDENTITY(1,1),
                        [ProductID] bigint NOT NULL,
                        [Quantity] int NOT NULL,
                        [OrderID] int NULL,
                        CONSTRAINT [PK_CartLine] PRIMARY KEY ([CartLineID]),
                        CONSTRAINT [FK_CartLine_Orders_OrderID] FOREIGN KEY ([OrderID]) REFERENCES [Orders] ([OrderID]) ON DELETE CASCADE,
                        CONSTRAINT [FK_CartLine_Products_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Products] ([ProductID]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_CartLine_OrderID] ON [CartLine] ([OrderID]);
                    CREATE INDEX [IX_CartLine_ProductID] ON [CartLine] ([ProductID]);
                END;
                """);

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "InventoryRecords",
                columns: table => new
                {
                    InventoryRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ReservationReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryRecords", x => x.InventoryRecordId);
                    table.ForeignKey(
                        name: "FK_InventoryRecords_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRecords",
                columns: table => new
                {
                    PaymentRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExternalPaymentId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRecords", x => x.PaymentRecordId);
                    table.ForeignKey(
                        name: "FK_PaymentRecords_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentRecords",
                columns: table => new
                {
                    ShipmentRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ShipmentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShippedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentRecords", x => x.ShipmentRecordId);
                    table.ForeignKey(
                        name: "FK_ShipmentRecords_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryRecords_OrderId",
                table: "InventoryRecords",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_OrderId",
                table: "PaymentRecords",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentRecords_OrderId",
                table: "ShipmentRecords",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Orders_Customers_CustomerId' AND parent_object_id = OBJECT_ID(N'[Orders]'))
                BEGIN
                    ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_Customers_CustomerId];
                END;

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Orders_CustomerId' AND object_id = OBJECT_ID(N'[Orders]'))
                BEGIN
                    DROP INDEX [IX_Orders_CustomerId] ON [Orders];
                END;
                """);

            migrationBuilder.DropTable(
                name: "InventoryRecords");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "PaymentRecords");

            migrationBuilder.DropTable(
                name: "ShipmentRecords");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH(N'Orders', N'CustomerId') IS NOT NULL
                    BEGIN
                        ALTER TABLE [Orders] DROP COLUMN [CustomerId];
                    END;

                    IF COL_LENGTH(N'Orders', N'Status') IS NOT NULL
                    BEGIN
                        ALTER TABLE [Orders] DROP COLUMN [Status];
                    END;

                    IF COL_LENGTH(N'Orders', N'CreatedAtUtc') IS NOT NULL
                    BEGIN
                        ALTER TABLE [Orders] DROP COLUMN [CreatedAtUtc];
                    END;

                    IF COL_LENGTH(N'Orders', N'UpdatedAtUtc') IS NOT NULL
                    BEGIN
                        ALTER TABLE [Orders] DROP COLUMN [UpdatedAtUtc];
                    END;

                    IF COL_LENGTH(N'Orders', N'CompletedAtUtc') IS NOT NULL
                    BEGIN
                        ALTER TABLE [Orders] DROP COLUMN [CompletedAtUtc];
                    END;

                    IF COL_LENGTH(N'Orders', N'FailedAtUtc') IS NOT NULL
                    BEGIN
                        ALTER TABLE [Orders] DROP COLUMN [FailedAtUtc];
                    END;
                END;
                """);
        }
    }
}
