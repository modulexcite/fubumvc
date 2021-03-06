﻿namespace FubuMVC.Authentication.Membership
{
    public interface IMembershipRepository
    {
        bool MatchesCredentials(LoginRequest request);
        IUserInfo FindByName(string username);
    }

}