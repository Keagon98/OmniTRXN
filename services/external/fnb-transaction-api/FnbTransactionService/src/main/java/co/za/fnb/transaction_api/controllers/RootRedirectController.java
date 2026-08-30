package co.za.fnb.transaction_api.controllers;

import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;

@Controller
public class RootRedirectController {

    @GetMapping("/")
    public String redirectToWsdl() {
        return "redirect:/ws/CustomerTransactions.wsdl";
    }
}
