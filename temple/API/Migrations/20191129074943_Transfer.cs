using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Migrations
{
    public partial class Transfer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "City",
            //    table: "members");

            //migrationBuilder.DropColumn(
            //    name: "Township",
            //    table: "members");

            //migrationBuilder.DropColumn(
            //    name: "DueYear",
            //    table: "FinancialRecords");

            //migrationBuilder.AddColumn<int>(
            //    name: "CityId",
            //    table: "members",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "TownshipId",
            //    table: "members",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AlterColumn<string>(
            //    name: "DueDate",
            //    table: "FinancialRecords",
            //    nullable: true,
            //    oldClrType: typeof(DateTime),
            //    oldType: "TEXT");

            //migrationBuilder.AddColumn<string>(
            //    name: "ReturnDate",
            //    table: "FinancialRecords",
                //nullable: true);

            migrationBuilder.CreateTable(
                name: "TransferRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    eventName = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    TransferType = table.Column<string>(nullable: false),
                    BankName = table.Column<string>(nullable: false),
                    BankAccount = table.Column<string>(nullable: false),
                    Notes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferRecords", x => x.Id);
                });

            //migrationBuilder.CreateIndex(
            //    name: "IX_members_CityId",
            //    table: "members",
            //    column: "CityId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_members_TownshipId",
            //    table: "members",
            //    column: "TownshipId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_members_Cities_CityId",
            //    table: "members",
            //    column: "CityId",
            //    principalTable: "Cities",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_members_TownShip_TownshipId",
            //    table: "members",
            //    column: "TownshipId",
            //    principalTable: "TownShip",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_members_Cities_CityId",
            //    table: "members");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_members_TownShip_TownshipId",
            //    table: "members");

            //migrationBuilder.DropTable(
            //    name: "TransferRecords");

            //migrationBuilder.DropIndex(
            //    name: "IX_members_CityId",
            //    table: "members");

            //migrationBuilder.DropIndex(
            //    name: "IX_members_TownshipId",
            //    table: "members");

            //migrationBuilder.DropColumn(
            //    name: "CityId",
            //    table: "members");

            //migrationBuilder.DropColumn(
            //    name: "TownshipId",
            //    table: "members");

            //migrationBuilder.DropColumn(
            //    name: "ReturnDate",
            //    table: "FinancialRecords");

            //migrationBuilder.AddColumn<string>(
            //    name: "City",
            //    table: "members",
            //    type: "TEXT",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "Township",
            //    table: "members",
            //    type: "TEXT",
            //    nullable: true);

            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "DueDate",
            //    table: "FinancialRecords",
            //    type: "TEXT",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DueYear",
                table: "FinancialRecords",
                type: "TEXT",
                nullable: true);
        }
    }
}
