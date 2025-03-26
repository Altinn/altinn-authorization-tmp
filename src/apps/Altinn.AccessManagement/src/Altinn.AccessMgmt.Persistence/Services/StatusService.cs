﻿using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using System;

namespace Altinn.AccessMgmt.Persistence.Services;

public class StatusService(IStatusRecordRepository statusRecordRepository) : IStatusService
{
    private readonly IStatusRecordRepository statusRecordRepository = statusRecordRepository;
    
    public async Task<StatusRecord> GetOrCreateRecord(Guid id, string name, int limit = 5)
    {
        var status = await statusRecordRepository.Get(id);
        if (status == null)
        {
            status = new StatusRecord()
            {
                Id = id,
                Name = name,
                Limit = limit,
                Count = 0,
                Message = "Initial",
                Payload = "[]",
                State = "RUNNING",
                Timestamp = DateTimeOffset.Now
            };

            await statusRecordRepository.Upsert(status);
        }

        return status;
    }

    public async Task RunFailed(StatusRecord record, Exception exception)
    {
        record.State = "RETRY";
        record.Message = exception.Message;
        record.Payload = "[]";
        record.Timestamp = DateTimeOffset.Now;
        await statusRecordRepository.Upsert(record);
    }

    public async Task RunSuccess(StatusRecord record)
    {
        if (record.State != "RUNNING" || record.Timestamp.AddMinutes(15) < DateTimeOffset.Now)
        {
            record.State = "RUNNING";
            record.Count = 0;
            record.Message = "Ok";
            record.Payload = "[]";
            record.Timestamp = DateTimeOffset.Now;
            await statusRecordRepository.Upsert(record);
        }
    }

    public async Task<bool> TryToRun(StatusRecord record)
    {
        if (record.State == "STOPPED")
        {
            return false;
        }

        if (record.Count >= record.Limit)
        {
            record.State = "STOPPED";
            record.Timestamp = DateTimeOffset.Now;
            await statusRecordRepository.Upsert(record);
            return false;
        }

        if (record.State == "RETRY")
        {
            record.Count += 1;
        }

        record.Timestamp = DateTimeOffset.Now;
        await statusRecordRepository.Upsert(record);

        return true;
    }
}

public interface IStatusService
{
    Task<StatusRecord> GetOrCreateRecord(Guid id, string name, int limit = 5);

    Task<bool> TryToRun(StatusRecord record);

    Task RunSuccess(StatusRecord record);
    Task RunFailed(StatusRecord record, Exception exception);
}
