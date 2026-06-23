using Altinn.AccessMgmt.FFB.Jobs.Models;
using Altinn.AccessMgmt.PersistenceEF.Constants;

namespace Altinn.AccessMgmt.FFB.Jobs;

public static class AssignmentInstanceCleanupJob
{
    public const string JobName = "AssignmentInstanceCleanup";

    public static async Task RunAsync(DuoRepo repo, JobRun run, AssignmentInstanceCleanupOptions opts, CancellationToken ct)
    {
        run.AddLog($"Assignment Instance Cleanup");

        if (!RoleConstants.TryGetByAll(opts.OldRole, out var oldRole))
        {
            run.AddLog($"Old role '{opts.OldRole}' not found in RoleConstants", true);
            return;
        }

        if (!RoleConstants.TryGetByAll(opts.NewRole, out var newRole))
        {
            run.AddLog($"New role '{opts.NewRole}' not found in RoleConstants", true);
            return;
        }

        if (!Guid.TryParse(opts.ChangedBySystem, out var changedBySystemId))
        {
            run.AddLog($"ChangedBySystem '{opts.ChangedBySystem}' is not a valid GUID", true);
            return;
        }

        int count = 0;

        while (count < opts.Limit)
        {
            var instances = (await repo.GetAccAssignmentInstance(oldRole.Id, changedBySystemId, limit: opts.LoopLimit)).ToList();
            if (instances.Count == 0)
            {
                break;
            }

            var movedThisBatch = 0;

            foreach (var instance in instances)
            {
                if (count >= opts.Limit)
                {
                    break;
                }

                run.AddLog($"Moving AssignmentInstance: {instance.Id} from Assignment: {instance.AssignmentId}");

                // Upsert Assignment
                var newAssignmentId = await repo.UpsertAssignment(instance.AssignmentFromId, instance.AssignmentToId, newRole.Id, instance.ValidFrom, instance.ChangedBy, instance.ChangedBySystem, opts.OperationId, ct);
                if (newAssignmentId is null || !newAssignmentId.HasValue)
                {
                    run.AddLog($"Failed to upsert assignment for AssignmentInstance: {instance.Id}", true);
                    continue;
                }

                // Move AssignmentInstance
                var updated = await repo.UpdateAssignmentInstance(instance.Id, newAssignmentId.Value, DateTimeOffset.UtcNow, SystemEntityConstants.DBA, SystemEntityConstants.DBA, opts.OperationId);
                if (!updated)
                {
                    run.AddLog($"Failed to update AssignmentInstance '{instance.Id}' AssignmentId from '{instance.AssignmentId}' to '{newAssignmentId.Value}'", true);
                    continue;
                }

                // Remove Old Assignment if it is now empty (a normal side effect, not a failure)
                var deleted = await repo.RemoveAssignmentIfEmpty(instance.AssignmentId, SystemEntityConstants.DBA, SystemEntityConstants.DBA, opts.OperationId);
                if (deleted)
                {
                    run.AddLog($"Deleted empty assignment '{instance.AssignmentId}'");
                }

                count++;
                movedThisBatch++;
            }

            // Guard against an endless loop: a full batch was fetched but nothing could be moved,
            // so the same rows would be returned again on the next iteration.
            if (movedThisBatch == 0)
            {
                run.AddLog("No instances in this batch could be moved — stopping to avoid an endless loop.", true);
                break;
            }
        }

        run.AddLog($"Done. Moved {count} assignment instance(s).");
    }
}

public record AssignmentInstanceCleanupOptions(string ChangedBySystem, string OldRole, string NewRole, string OperationId, int Limit, int LoopLimit);
