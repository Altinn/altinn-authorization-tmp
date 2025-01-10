using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessMgmt.Worker.ER.Telemetry;

/// <summary>
/// RepoTelemetry
/// </summary>
public static class BrRegTelemetry
{
    /// <summary>
    /// RepoTelemetry DbAccessSource
    /// </summary>
    public static ActivitySource Source = new ActivitySource("Altinn.Authorization.Workers.BrReg", "1.0.0");
}
