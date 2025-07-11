﻿using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IHistoryService
{
    PayloadResponse PassengerHistory(string id, int pageNo, int pageSize);
    PayloadResponse AgentHistory(string id, int pageNo, int pageSize);
    PayloadResponse SessionHistory(string id, int pageNo, int pageSize);
}