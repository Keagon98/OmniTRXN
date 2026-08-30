package co.za.fnb.transaction_api;

import co.za.fnb.transaction_api.exceptions.AccountNotFoundException;
import org.springframework.ws.server.endpoint.annotation.Endpoint;
import org.springframework.ws.server.endpoint.annotation.PayloadRoot;
import org.springframework.ws.server.endpoint.annotation.RequestPayload;
import org.springframework.ws.server.endpoint.annotation.ResponsePayload;
import za.co.soapservice.fnb.transactions.*;

import javax.xml.datatype.DatatypeConfigurationException;
import javax.xml.datatype.DatatypeFactory;
import javax.xml.datatype.XMLGregorianCalendar;
import java.io.IOException;
import java.math.BigDecimal;

@Endpoint
public class TransactionEndpoint {

    private final SimpleCsvReader csvReader;

    public TransactionEndpoint(SimpleCsvReader csvReader) {
        this.csvReader = csvReader;
    }

    @PayloadRoot(namespace = "http://transactions.fnb.soapservice.co.za", localPart = "getCustomerTransactionsRequest")
    @ResponsePayload
    public GetCustomerTransactionsResponse getCustomerTransactions(@RequestPayload GetCustomerTransactionsRequest request) throws DatatypeConfigurationException, IOException {
        GetCustomerTransactionsResponse response = new GetCustomerTransactionsResponse();

        var accountId = "cust-acct-908";
        var statementId = "STMT-ACCT-1111-20260825-02";
        var createdDateTimeStr = "2026-08-25T11:30:00Z";
        var maskedBankAccNumber = "****2311";
        var customerName = "Jeremy Clarkson";
        var currency = "ZAR";
        var transactions = csvReader.getRows();
        var transactionsList = new TransactionListType();
        var xmlCalendar = DatatypeFactory.newInstance().newXMLGregorianCalendar(createdDateTimeStr);

        if (!request.getAccountId().equalsIgnoreCase(accountId)) {
            throw new AccountNotFoundException(request.getAccountId());
        }

        response.setStatementId(statementId);
        response.setCreatedDateTime(xmlCalendar);

        var account = new AccountType();

        account.setAccountId(accountId);
        account.setMasked(maskedBankAccNumber);
        account.setName(customerName);
        account.setCurrency(currency);
        account.setAvailableBalance(BigDecimal.valueOf(10250.75));

        response.setAccount(account);

        for (var transaction : transactions) {
            TransactionType trxn = new TransactionType();

            XMLGregorianCalendar bookingDate = DatatypeFactory.newInstance().newXMLGregorianCalendar(transaction[1]);
            XMLGregorianCalendar valueDate = DatatypeFactory.newInstance().newXMLGregorianCalendar(transaction[2]);

            trxn.setTxId(transaction[0]);
            trxn.setBookingDate(bookingDate);
            trxn.setValueDate(valueDate);
            trxn.setAmount(new BigDecimal(transaction[3]));
            trxn.setCurrency(transaction[4]);
            trxn.setCreditDebit(transaction[5].equals("DEBIT") ? CreditDebitType.DEBIT : CreditDebitType.CREDIT);
            trxn.setMerchantName(transaction[6]);
            trxn.setCategory(transaction[7]);
            trxn.setMcc(transaction[8]);
            trxn.setRemittance(transaction[9]);
            trxn.setBankReference(transaction[10]);

            transactionsList.getTransaction().add(trxn);
        }

        response.setTransactions(transactionsList);

        return response;
    }
}
