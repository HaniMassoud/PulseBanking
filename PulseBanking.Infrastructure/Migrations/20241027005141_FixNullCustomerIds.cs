using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace PulseBanking.Infrastructure.Migrations
{
    public partial class FixNullCustomerIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First ensure default customers exist for all tenants
            migrationBuilder.Sql(@"
                INSERT INTO Customers (Id, FirstName, LastName, Email, PhoneNumber, TenantId)
                SELECT 
                    NEWID(),
                    'Default',
                    'Customer',
                    CONCAT('default@', TenantId, '.com'),
                    '0000000000',
                    TenantId
                FROM (
                    SELECT DISTINCT TenantId 
                    FROM Accounts a
                    WHERE NOT EXISTS (
                        SELECT 1 FROM Customers c 
                        WHERE c.TenantId = a.TenantId
                        AND c.FirstName = 'Default' 
                        AND c.LastName = 'Customer'
                    )
                ) AS TenantsNeedingCustomers;
            ");

            // Update accounts to use their tenant's default customer
            migrationBuilder.Sql(@"
                UPDATE a
                SET a.CustomerId = c.Id
                FROM Accounts a
                JOIN Customers c ON c.TenantId = a.TenantId
                WHERE a.CustomerId IS NULL
                AND c.FirstName = 'Default' 
                AND c.LastName = 'Customer';
            ");

            // Now make CustomerId non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Accounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Make CustomerId nullable again
            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Accounts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: false,
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}