using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laundry.Migrations
{
    /// <inheritdoc />
    public partial class addindividualstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Barcodes_BookingClothesId",
                table: "Barcodes");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BookingClothes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Barcodes_BookingClothesId",
                table: "Barcodes",
                column: "BookingClothesId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Barcodes_BookingClothesId",
                table: "Barcodes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BookingClothes");

            migrationBuilder.CreateIndex(
                name: "IX_Barcodes_BookingClothesId",
                table: "Barcodes",
                column: "BookingClothesId");
        }
    }
}
