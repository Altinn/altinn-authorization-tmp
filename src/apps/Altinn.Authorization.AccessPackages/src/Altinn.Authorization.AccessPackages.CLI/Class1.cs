using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Authorization.AccessPackages.CLI;

public static class Telemetry
{
    public static ActivitySource Source = new ActivitySource("Altinn.Authorization.AccessPackages.CLI", "1.0.0");
}
