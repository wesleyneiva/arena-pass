using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArenaPass.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxaPorHoraEExclusaoDeSobreposicao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Agendamentos_QuadraId_Data_HoraInicio",
                table: "Agendamentos");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxaPorHora",
                table: "Quadras",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_QuadraId",
                table: "Agendamentos",
                column: "QuadraId");

            // Bloqueio de conflito real: como uma reserva agora pode durar 1h ou 2h, duas
            // reservas podem se sobrepor sem compartilhar o mesmo HoraInicio (ex: 18h-20h
            // e 19h-20h). Uma constraint de exclusão (EXCLUDE USING gist) impede qualquer
            // sobreposição de intervalo por quadra/data, mesmo sob concorrência — o Fluent
            // API do EF Core não expressa EXCLUDE, por isso SQL puro. btree_gist é a
            // extensão que dá suporte a gist pra tipos comuns (uuid, date) usados aqui.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");
            migrationBuilder.Sql(
                """
                ALTER TABLE "Agendamentos"
                ADD CONSTRAINT "EX_Agendamentos_SemSobreposicao"
                EXCLUDE USING gist (
                    "QuadraId" WITH =,
                    "Data" WITH =,
                    tsrange("Data" + "HoraInicio", "Data" + "HoraFim") WITH &&
                ) WHERE ("Status" <> 'Cancelado');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Agendamentos\" DROP CONSTRAINT IF EXISTS \"EX_Agendamentos_SemSobreposicao\";");

            migrationBuilder.DropIndex(
                name: "IX_Agendamentos_QuadraId",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "TaxaPorHora",
                table: "Quadras");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_QuadraId_Data_HoraInicio",
                table: "Agendamentos",
                columns: new[] { "QuadraId", "Data", "HoraInicio" },
                unique: true,
                filter: "\"Status\" <> 'Cancelado'");
        }
    }
}
