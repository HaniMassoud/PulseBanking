namespace PulseBanking.Domain.Enums;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer,
    Interest,
    Fee,
    Refund,
    Adjustment,
    DirectDebit,
    DirectCredit,
    InternationalTransfer,
    ATMWithdrawal,
    POSPayment,
    StandingOrder,
    LoanDisbursement,
    LoanRepayment
}