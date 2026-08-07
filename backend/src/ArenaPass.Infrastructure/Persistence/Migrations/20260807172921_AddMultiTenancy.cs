using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArenaPass.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Espaço "clube atual" — representa o clube já existente antes do multi-tenant
            // (HR Tennis, hrtennis.wnlabs.com.br), pra onde todos os dados de hoje são
            // migrados.
            var espacoPadraoId = Guid.NewGuid();

            migrationBuilder.CreateTable(
                name: "Espacos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Subdominio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DominioPersonalizado = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Espacos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Espacos_Subdominio",
                table: "Espacos",
                column: "Subdominio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Espacos_DominioPersonalizado",
                table: "Espacos",
                column: "DominioPersonalizado",
                unique: true);

            migrationBuilder.InsertData(
                table: "Espacos",
                columns: new[] { "Id", "Nome", "Subdominio", "DominioPersonalizado", "Ativo", "CreatedAt" },
                values: new object[] { espacoPadraoId, "HR Tennis", "hrtennis", null!, true, DateTime.UtcNow });

            migrationBuilder.CreateTable(
                name: "ProfessoresEspacos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessorId = table.Column<Guid>(type: "uuid", nullable: false),
                    EspacoId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusAprovacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DataSolicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessoresEspacos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfessoresEspacos_Espacos_EspacoId",
                        column: x => x.EspacoId,
                        principalTable: "Espacos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfessoresEspacos_Professores_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Professores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfessoresEspacos_EspacoId",
                table: "ProfessoresEspacos",
                column: "EspacoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessoresEspacos_ProfessorId_EspacoId",
                table: "ProfessoresEspacos",
                columns: new[] { "ProfessorId", "EspacoId" },
                unique: true);

            // Backfill do vínculo por espaço a partir do StatusAprovacao antigo — precisa
            // rodar ANTES de a coluna ser removida de Professores logo abaixo.
            migrationBuilder.Sql($"""
                INSERT INTO "ProfessoresEspacos" ("Id", "ProfessorId", "EspacoId", "StatusAprovacao", "DataSolicitacao", "CreatedAt")
                SELECT gen_random_uuid(), "Id", '{espacoPadraoId}', "StatusAprovacao", now(), now()
                FROM "Professores";
                """);

            migrationBuilder.DropIndex(
                name: "IX_SolicitacoesRegistroProfessor_Email",
                table: "SolicitacoesRegistroProfessor");

            migrationBuilder.DropIndex(
                name: "IX_Modalidades_Nome",
                table: "Modalidades");

            migrationBuilder.DropColumn(
                name: "StatusAprovacao",
                table: "Professores");

            // EspacoId nas demais tabelas: adiciona nullable, faz backfill pro espaço
            // padrão, só então torna NOT NULL — evita qualquer defaultValue "fantasma"
            // ficando no schema (que não existe no modelo C#, e o EF ia querer desfazer
            // na próxima migration).
            migrationBuilder.AddColumn<Guid>(
                name: "EspacoId",
                table: "Usuarios",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EspacoId",
                table: "SolicitacoesRegistroProfessor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EspacoId",
                table: "Quadras",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EspacoId",
                table: "Modalidades",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EspacoId",
                table: "Agendamentos",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql($"""UPDATE "Quadras" SET "EspacoId" = '{espacoPadraoId}';""");
            migrationBuilder.Sql($"""UPDATE "Modalidades" SET "EspacoId" = '{espacoPadraoId}';""");
            migrationBuilder.Sql($"""UPDATE "Agendamentos" SET "EspacoId" = '{espacoPadraoId}';""");
            migrationBuilder.Sql($"""UPDATE "SolicitacoesRegistroProfessor" SET "EspacoId" = '{espacoPadraoId}';""");
            migrationBuilder.Sql($"""UPDATE "Usuarios" SET "EspacoId" = '{espacoPadraoId}' WHERE "Role" = 'AdminClube';""");

            migrationBuilder.AlterColumn<Guid>(
                name: "EspacoId",
                table: "SolicitacoesRegistroProfessor",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EspacoId",
                table: "Quadras",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EspacoId",
                table: "Modalidades",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EspacoId",
                table: "Agendamentos",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // Usuarios.EspacoId permanece nullable — Professor e Master não têm espaço fixo.

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EspacoId",
                table: "Usuarios",
                column: "EspacoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesRegistroProfessor_EspacoId_Email",
                table: "SolicitacoesRegistroProfessor",
                columns: new[] { "EspacoId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quadras_EspacoId",
                table: "Quadras",
                column: "EspacoId");

            migrationBuilder.CreateIndex(
                name: "IX_Modalidades_EspacoId_Nome",
                table: "Modalidades",
                columns: new[] { "EspacoId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_EspacoId",
                table: "Agendamentos",
                column: "EspacoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Espacos_EspacoId",
                table: "Agendamentos",
                column: "EspacoId",
                principalTable: "Espacos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Modalidades_Espacos_EspacoId",
                table: "Modalidades",
                column: "EspacoId",
                principalTable: "Espacos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quadras_Espacos_EspacoId",
                table: "Quadras",
                column: "EspacoId",
                principalTable: "Espacos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitacoesRegistroProfessor_Espacos_EspacoId",
                table: "SolicitacoesRegistroProfessor",
                column: "EspacoId",
                principalTable: "Espacos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Espacos_EspacoId",
                table: "Usuarios",
                column: "EspacoId",
                principalTable: "Espacos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_Espacos_EspacoId",
                table: "Agendamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Modalidades_Espacos_EspacoId",
                table: "Modalidades");

            migrationBuilder.DropForeignKey(
                name: "FK_Quadras_Espacos_EspacoId",
                table: "Quadras");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitacoesRegistroProfessor_Espacos_EspacoId",
                table: "SolicitacoesRegistroProfessor");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Espacos_EspacoId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "ProfessoresEspacos");

            migrationBuilder.DropTable(
                name: "Espacos");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EspacoId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_SolicitacoesRegistroProfessor_EspacoId_Email",
                table: "SolicitacoesRegistroProfessor");

            migrationBuilder.DropIndex(
                name: "IX_Quadras_EspacoId",
                table: "Quadras");

            migrationBuilder.DropIndex(
                name: "IX_Modalidades_EspacoId_Nome",
                table: "Modalidades");

            migrationBuilder.DropIndex(
                name: "IX_Agendamentos_EspacoId",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "EspacoId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EspacoId",
                table: "SolicitacoesRegistroProfessor");

            migrationBuilder.DropColumn(
                name: "EspacoId",
                table: "Quadras");

            migrationBuilder.DropColumn(
                name: "EspacoId",
                table: "Modalidades");

            migrationBuilder.DropColumn(
                name: "EspacoId",
                table: "Agendamentos");

            migrationBuilder.AddColumn<string>(
                name: "StatusAprovacao",
                table: "Professores",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesRegistroProfessor_Email",
                table: "SolicitacoesRegistroProfessor",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modalidades_Nome",
                table: "Modalidades",
                column: "Nome",
                unique: true);
        }
    }
}
