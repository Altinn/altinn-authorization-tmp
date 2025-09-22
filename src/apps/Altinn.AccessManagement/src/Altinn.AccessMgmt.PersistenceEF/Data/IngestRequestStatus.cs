using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;

namespace Altinn.AccessMgmt.PersistenceEF.Data;

public partial class StaticDataIngest
{
    /// <summary>
    /// Ingest ProviderTypes
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestRequestStatus(CancellationToken cancellationToken = default)
    {
        /*
0195efb8-7c80-7c6d-aec6-5eafa8154ca1
0195efb8-7c80-761b-b950-e709c703b6b1
0195efb8-7c80-7239-8ee5-7156872b53d1
0195efb8-7c80-7731-82a3-1f6b659ec848
0195efb8-7c80-79c9-8841-b0963234bfcf
0195efb8-7c80-7f3a-9f6e-ee610e978af8
0195efb8-7c80-71d2-bdca-ef5eb69d82b0
0195efb8-7c80-7bf2-87e1-f6068ccf563f
0195efb8-7c80-771c-8585-d61e05324070
0195efb8-7c80-7535-b68a-f56fd74437f2 
        */
        var data = new List<RequestStatus>()
        {
            new RequestStatus() { Id = Guid.Parse("0195efb8-7c80-7c6d-aec6-5eafa8154ca1"), Name = "Akseptert", Description = "Forespørsel er akseptert" },
            new RequestStatus() { Id = Guid.Parse("0195efb8-7c80-761b-b950-e709c703b6b1"), Name = "Avvist", Description = "Forespørsel er avvist" },
            new RequestStatus() { Id = Guid.Parse("0195efb8-7c80-7239-8ee5-7156872b53d1"), Name = "Åpen", Description = "Forespørsel er åpen" },
            new RequestStatus() { Id = Guid.Parse("0195efb8-7c80-7731-82a3-1f6b659ec848"), Name = "Lukket", Description = "Forespørsel er lukket" }
        };

        var translations = new List<TranslationEntry>()
        {
            new TranslationEntry() { Id = Guid.Parse("0195efb8-7c80-7c6d-aec6-5eafa8154ca1"), LanguageCode = "eng", Type = nameof(RequestStatus), FieldName = "Name", Value = "Accepted" },
            new TranslationEntry() { Id = Guid.Parse("0195efb8-7c80-7c6d-aec6-5eafa8154ca1"), LanguageCode = "nno", Type = nameof(RequestStatus), FieldName = "Name", Value = "Akseptera" },

            new TranslationEntry() { Id = Guid.Parse("0195efb8-7c80-761b-b950-e709c703b6b1"), LanguageCode = "eng", Type = nameof(RequestStatus), FieldName = "Name", Value = "Rejected" },
            new TranslationEntry() { Id = Guid.Parse("0195efb8-7c80-761b-b950-e709c703b6b1"), LanguageCode = "nno", Type = nameof(RequestStatus), FieldName = "Name", Value = "Avvist" },

            new TranslationEntry() { Id = Guid.Parse("0195efb8-7c80-7239-8ee5-7156872b53d1"), LanguageCode = "eng", Type = nameof(RequestStatus), FieldName = "Name", Value = "Open" },
            new TranslationEntry() { Id = Guid.Parse("0195efb8-7c80-7239-8ee5-7156872b53d1"), LanguageCode = "nno", Type = nameof(RequestStatus), FieldName = "Name", Value = "Åpen" },

            new TranslationEntry() { Id = Guid.Parse("0195efb8-7c80-7731-82a3-1f6b659ec848"), LanguageCode = "eng", Type = nameof(RequestStatus), FieldName = "Name", Value = "Closed" },
            new TranslationEntry() { Id = Guid.Parse("0195efb8-7c80-7731-82a3-1f6b659ec848"), LanguageCode = "nno", Type = nameof(RequestStatus), FieldName = "Name", Value = "Stengd" },
        };

        foreach (var d in data)
        {
            var obj = db.RequestStatus.FirstOrDefault(t => t.Id == d.Id);
            if (obj == null)
            {
                db.RequestStatus.Add(d);
            }
            else
            {
                obj.Name = d.Name;
            }
        }

        foreach (var translation in translations)
        {
            await translationService.UpsertTranslationAsync(translation);
        }

        var result = await db.SaveChangesAsync(AuditValues, cancellationToken);
    }
}
