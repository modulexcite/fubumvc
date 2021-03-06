using FubuCore.Dates;

namespace FubuMVC.Authentication.Tickets
{
    public class TicketAuthenticationSession : IAuthenticationSession
    {
        private readonly ITicketSource _source;
        private readonly ISystemTime _systemTime;
        private readonly AuthenticationSettings _settings;

        public TicketAuthenticationSession(ITicketSource source, ISystemTime systemTime, AuthenticationSettings settings)
        {
            _source = source;
            _systemTime = systemTime;
            _settings = settings;
        }

        public void MarkAccessed()
        {
            var current = _source.CurrentTicket();
            current.LastAccessed = _systemTime.UtcNow();

            _source.Persist(current);
        }

        public string PreviouslyAuthenticatedUser()
        {
            var current = _source.CurrentTicket();
            if (current == null) return null;

            if (IsExpired(current))
            {
                _source.Delete();
                return null;
            }

            return current.UserName;
        }

        public virtual bool IsExpired(AuthenticationTicket ticket)
        {
            return _settings.SlidingExpiration
                       ? _systemTime.UtcNow().Subtract(ticket.LastAccessed).TotalMinutes >= _settings.ExpireInMinutes
                       : _systemTime.UtcNow() >=
                         ticket.Expiration;
        }

        public void MarkAuthenticated(string userName)
        {
            var ticket = new AuthenticationTicket{
                UserName = userName,
                LastAccessed = _systemTime.UtcNow()
            };

            if (_settings.SlidingExpiration)
            {
                ticket.Expiration = _systemTime.UtcNow().AddMinutes(_settings.ExpireInMinutes);
            }

            _source.Persist(ticket);
        }

        public void ClearAuthentication()
        {
            _source.Delete();
        }
    }
}