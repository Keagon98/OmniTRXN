package co.za.fnb.transaction_api.exceptions;

public class AccountNotFoundException extends RuntimeException {

    public AccountNotFoundException(String accountId) {
        super("The account ID '" + accountId + "' does not exist.");
    }
}
