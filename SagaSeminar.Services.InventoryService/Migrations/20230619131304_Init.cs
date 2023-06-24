using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaSeminar.Services.InventoryService.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryNote",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryNote", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "InventoryNote",
                columns: new[] { "Id", "CreatedTime", "Note", "Quantity", "Reason", "TransactionId" },
                values: new object[] { new Guid("d60f9c5e-174f-4cd0-a841-20a830f7abb9"), new DateTime(2023, 6, 19, 20, 13, 4, 141, DateTimeKind.Local).AddTicks(5995), null, 1000000, "Initial reception", new Guid("663bf954-3203-4830-a102-e14b750a8cf9") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryNote");
        }
    }
}
