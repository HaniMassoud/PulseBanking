// Create new file: src/PulseBanking.Application/Features/Accounts/Queries/GetAccount/GetAccountQuery.cs
using MediatR;
using PulseBanking.Application.Features.Accounts.Common;

namespace PulseBanking.Application.Features.Accounts.Queries.GetAccount;

public record GetAccountQuery(Guid Id) : IRequest<AccountDto>;