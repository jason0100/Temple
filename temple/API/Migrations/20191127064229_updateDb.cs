using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Migrations
{
    public partial class updateDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<string>(
            //    name: "CustomerPhone",
            //    table: "FinancialRecords",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "TEXT");

            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "CreateDate",
            //    table: "FinancialRecords",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "TEXT",
            //    oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "FinancialRecords",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PayType",
                table: "FinancialRecords",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "FinancialRecords");

            migrationBuilder.DropColumn(
                name: "PayType",
                table: "FinancialRecords");

            //migrationBuilder.AlterColumn<string>(
            //    name: "CustomerPhone",
            //    table: "FinancialRecords",
            //    type: "TEXT",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "CreateDate",
            //    table: "FinancialRecords",
            //    type: "TEXT",
            //    nullable: true,
            //    oldClrType: typeof(DateTime));
        }
    }
}
