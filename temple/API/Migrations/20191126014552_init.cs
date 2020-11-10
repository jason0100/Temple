using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    MemberId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Identity = table.Column<string>(nullable: false),
                    Gender = table.Column<string>(nullable: false),
                    Birth = table.Column<string>(nullable: true),
                    LunarBirth = table.Column<string>(nullable: true),
                    CreateDate = table.Column<string>(nullable: true),
                    TimeOfLunarBirth = table.Column<string>(nullable: true),
                    ZodiacAnimal = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    CellPhone = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    Township = table.Column<string>(nullable: true),
                    Zip = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.MemberId);
                });

            migrationBuilder.CreateTable(
                name: "TownShip",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Zip = table.Column<string>(nullable: true),
                    CityId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TownShip", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TownShip_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerName = table.Column<string>(nullable: false),
                    CustomerPhone = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    CreateDate = table.Column<string>(nullable: true),
                    DueYear = table.Column<string>(nullable: true),
                    Position = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    FinancialItemId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialRecords_FinancialItems_FinancialItemId",
                        column: x => x.FinancialItemId,
                        principalTable: "FinancialItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RitualMoneyRecords",
                columns: table => new
                {
                    RitualMoneyRecordId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BorrowDate = table.Column<string>(nullable: true),
                    BorrowAmount = table.Column<decimal>(nullable: true),
                    ReturnDate = table.Column<string>(nullable: true),
                    ReturnAmount = table.Column<decimal>(nullable: true),
                    IsReturn = table.Column<bool>(nullable: false),
                    MemberId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RitualMoneyRecords", x => x.RitualMoneyRecordId);
                    table.ForeignKey(
                        name: "FK_RitualMoneyRecords_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    MainGod = table.Column<string>(nullable: true),
                    BirthDate = table.Column<string>(nullable: true),
                    ActivityDate = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    ContactName = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    CellPhone = table.Column<string>(nullable: true),
                    CityId = table.Column<int>(nullable: false),
                    TownshipId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Friends_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Friends_TownShip_TownshipId",
                        column: x => x.TownshipId,
                        principalTable: "TownShip",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialRecords_FinancialItemId",
                table: "FinancialRecords",
                column: "FinancialItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_CityId",
                table: "Friends",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_TownshipId",
                table: "Friends",
                column: "TownshipId");

            migrationBuilder.CreateIndex(
                name: "IX_RitualMoneyRecords_MemberId",
                table: "RitualMoneyRecords",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_TownShip_CityId",
                table: "TownShip",
                column: "CityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialRecords");

            migrationBuilder.DropTable(
                name: "Friends");

            migrationBuilder.DropTable(
                name: "RitualMoneyRecords");

            migrationBuilder.DropTable(
                name: "FinancialItems");

            migrationBuilder.DropTable(
                name: "TownShip");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
