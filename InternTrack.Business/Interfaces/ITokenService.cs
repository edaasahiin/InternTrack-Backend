using InternTrack.Core.Models;

namespace InternTrack.Business.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(User user);

    string CreateRefreshToken();
}