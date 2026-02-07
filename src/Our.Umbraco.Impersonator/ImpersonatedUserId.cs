using System;

namespace Our.Umbraco.Impersonator
{
    public class ImpersonatedUserId
    {
        public Guid UserId { get; }
        public int ImpersonatingUserId { get; }

        public string SessionId { get; }

        public ImpersonatedUserId(Guid userId, int impersonatingUserId, string sessionId)
        {
            UserId = userId;
            SessionId = sessionId;
            ImpersonatingUserId = impersonatingUserId;
        }
    }
}
