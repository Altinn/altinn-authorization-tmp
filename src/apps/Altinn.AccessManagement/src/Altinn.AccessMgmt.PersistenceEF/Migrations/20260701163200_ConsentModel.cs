using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <summary>
    /// Brings the <c>consent</c> schema under the EF model so it can be queried through
    /// <c>AppDbContext</c>. The schema is owned by <see cref="ConsentSchema_Baseline"/>, so this
    /// migration changes nothing: <see cref="Up"/> and <see cref="Down"/> are no-ops and only the
    /// model snapshot is updated.
    /// </summary>
    /// <inheritdoc />
    public partial class ConsentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: the consent schema is owned by ConsentSchema_Baseline; only the model snapshot changes.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: adopting an existing schema into the model is not reversible.
        }
    }
}
