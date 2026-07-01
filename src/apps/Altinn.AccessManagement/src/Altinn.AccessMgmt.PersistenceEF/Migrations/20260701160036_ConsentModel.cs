using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <summary>
    /// Adopts the pre-existing <c>consent</c> schema into the EF model. The enum types,
    /// the six consent tables, their keys and indexes already exist, created by the
    /// <see cref="ConsentSchema_Baseline"/> migration (embedded <c>ConsentSchema.sql</c>).
    /// This migration only brings those objects under the EF model snapshot so the
    /// repository can query them through <c>AppDbContext</c>; it makes no schema changes,
    /// so both <see cref="Up"/> and <see cref="Down"/> are intentional no-ops.
    /// EF's generated Up would have created the tables and enum types, which already exist;
    /// running it would fail and, for the enums, would not match the live labels
    /// (<c>status_type</c>/<c>event_type</c> carry values not present on the CLR enums).
    /// The model snapshot in the accompanying Designer file reflects the CLR view and is
    /// what future migrations diff against.
    /// </summary>
    /// <inheritdoc />
    public partial class ConsentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentional no-op: the consent schema and its objects already exist
            // (see ConsentSchema_Baseline). This migration only updates the EF model snapshot.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentional no-op. Adopting the pre-existing consent schema into the model is
            // not reversible; dropping the schema would destroy live consent data.
        }
    }
}
