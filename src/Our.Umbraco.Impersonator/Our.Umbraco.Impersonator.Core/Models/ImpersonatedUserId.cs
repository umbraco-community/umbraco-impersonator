using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.Impersonator.Core.Models;

public class ImpersonatedUserId
{
    public Guid UserId { get; }
    public Guid ImpersonatingUserId { get; }

    public string SessionId { get; }

    public ImpersonatedUserId(Guid userId, Guid impersonatingUserId, string sessionId)
    {
        UserId = userId;
        ImpersonatingUserId = impersonatingUserId;
        SessionId = sessionId;
    }
}
