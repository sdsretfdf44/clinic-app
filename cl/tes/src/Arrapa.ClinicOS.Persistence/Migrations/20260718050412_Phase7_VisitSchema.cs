using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arrapa.ClinicOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase7_VisitSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Visits",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Symptoms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DoctorNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalSchema: "dbo",
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Visits_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "dbo",
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "dbo",
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssuedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GeneralInstructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "dbo",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitVitals",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BloodPressureSystolic = table.Column<int>(type: "int", nullable: true),
                    BloodPressureDiastolic = table.Column<int>(type: "int", nullable: true),
                    PulseBpm = table.Column<int>(type: "int", nullable: true),
                    TemperatureCelsius = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    HeightCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Bmi = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SpO2Percent = table.Column<int>(type: "int", nullable: true),
                    RespirationRate = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitVitals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitVitals_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "dbo",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Medicine = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Strength = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Duration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "dbo",
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_PrescriptionId",
                schema: "dbo",
                table: "PrescriptionItems",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_TenantId_PrescriptionId_SortOrder",
                schema: "dbo",
                table: "PrescriptionItems",
                columns: new[] { "TenantId", "PrescriptionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_TenantId_VisitId_IssuedOn",
                schema: "dbo",
                table: "Prescriptions",
                columns: new[] { "TenantId", "VisitId", "IssuedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_VisitId",
                schema: "dbo",
                table: "Prescriptions",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_AppointmentId",
                schema: "dbo",
                table: "Visits",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_DoctorId",
                schema: "dbo",
                table: "Visits",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_PatientId",
                schema: "dbo",
                table: "Visits",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_TenantId_DoctorId_VisitDate",
                schema: "dbo",
                table: "Visits",
                columns: new[] { "TenantId", "DoctorId", "VisitDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_TenantId_PatientId_VisitDate",
                schema: "dbo",
                table: "Visits",
                columns: new[] { "TenantId", "PatientId", "VisitDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VisitVitals_TenantId_VisitId",
                schema: "dbo",
                table: "VisitVitals",
                columns: new[] { "TenantId", "VisitId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_VisitVitals_VisitId",
                schema: "dbo",
                table: "VisitVitals",
                column: "VisitId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrescriptionItems",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "VisitVitals",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Prescriptions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Visits",
                schema: "dbo");
        }
    }
}
