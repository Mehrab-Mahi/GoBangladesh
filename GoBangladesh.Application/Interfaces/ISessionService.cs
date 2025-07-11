using GoBangladesh.Application.DTOs.Session;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface ISessionService
{
    PayloadResponse StartSession(SessionStartDto sessionStartDto);
    PayloadResponse StopSession(SessionStopDto sessionStopDto);
    PayloadResponse GetSessionStatistics(string sessionId);
    PayloadResponse CheckIfSessionRunning(string sessionId);
    PayloadResponse GetStatistics(string sessionId);
    PayloadResponse CheckIfSessionRunningForLoggedInUser(string userId);
}