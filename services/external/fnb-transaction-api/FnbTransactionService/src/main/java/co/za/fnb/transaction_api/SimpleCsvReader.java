package co.za.fnb.transaction_api;

import com.opencsv.CSVReader;
import com.opencsv.CSVReaderBuilder;
import com.opencsv.exceptions.CsvException;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.io.Resource;
import org.springframework.stereotype.Component;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.List;

@Component
public class SimpleCsvReader {

    @Value("classpath:FNB_Transactions.csv")
    private Resource transactions;

    public List<String[]> getRows() throws IOException {
        try (
                InputStream inputStream = transactions.getInputStream();
                Reader reader = new InputStreamReader(inputStream, StandardCharsets.UTF_8);
                CSVReader csvReader = new CSVReaderBuilder(reader).withSkipLines(1).build()) {

            return csvReader.readAll();

        } catch (IOException | CsvException e) {
            throw new FileNotFoundException(e.getMessage());
        }
    }
}
