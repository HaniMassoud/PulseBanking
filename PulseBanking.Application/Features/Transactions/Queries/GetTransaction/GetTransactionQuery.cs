using MediatR;
using PulseBanking.Application.Common.Exceptions;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Features.Transactions.Queries.GetTransaction;

public record GetTransactionQuery(Guid Id) : IRequest<BankTransaction>;

