using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPSLauncher.Clients
{
    public record ClientResult<T>(bool IsSuccess, string Message, T? ResponseData, Exception? Ex);
}
