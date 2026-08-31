# OmniTRXN API

OmniTRXN is a backend service that aggregates customer transaction data from multiple external sources, normalizes it into a standardized format, stores it in a SQL Server database, and exposes it through a secure, documented REST API. The solution follows **Clean Architecture** principles and adheres to **SOLID** and **OOP** best practices.

#### Project Goals
---
#### The why?

To get a damn job at Capitec!!! I'm kidding (well kind of)…

The goal of this project is to demonstrate how production-ready systems integrate with external vendors/merchants, how systems ingest data (sometimes ambiguous) in various formats (JSON/XML in this instance) and normalize data to a single standardized schema, and how they store data in a queryable relational database with a retention policy. It should also demonstrate how systems securely expose data to external users for consumption via a well documented API, and how systems handle retry logic, logging, and metrics.

This project should also be easy to clone and run locally by anybody using a docker compose file.

---
#### The Who?

Who is the target user for this type of project?

- **Data analysts**: to build dashboards and run ad-hoc queries on transaction trends.
- **Business intelligence teams**: to create scheduled reports from aggregated data.
- **Finance and accounting**: to reconcile transactions, run summaries, and support audits.
- **Fraud and risk teams**: to detect anomalies and investigate suspicious activity.

---

This repository includes:
- **OmniTRXN API** (internal service)
- **API Gateway** (YARP) – handles routing, authentication, caching, and more
- **External vendor services** – one REST (Ozow) and one SOAP (FNB) service
- **SQL Server** database
- **Docker Compose** file to run the entire stack locally

---

## Architecture Overview

The solution is divided into four main layers:

1. **Domain** – Core entities (`Customer`, `Transaction`, `Vendor_Customer_Map`), enums, and business rules.
2. **Application** – Use cases (transaction ingestion, querying), interfaces for repositories and external services, DTOs, and validation.
3. **Infrastructure** – Implementations of repositories (EF Core), external service clients (REST/SOAP via gateway), XML‑to‑JSON adapter, background polling, and database context.
4. **API** – ASP.NET Core Web API, controllers, Swagger, exception handling middleware, and dependency injection setup.

All communication with external services goes through the API Gateway. The gateway authenticates incoming requests (JWT) and injects the required Basic Auth credentials for the upstream vendor services.

Below is a high-level view of the System Architecture for this project

<img width="13852" height="6465" alt="OmnTRXN-System-Architecture" src="https://github.com/user-attachments/assets/2dcf4e03-d4ba-4289-b60e-dfa2dd425e9b" />

---

## Key Features

- **Periodic Ingestion**: A background service polls the external APIs at configurable intervals to fetch transaction data.
- **Normalization**: Raw responses are converted to a common JSON format and mapped to the internal `Transaction` entity. XML responses are first converted to JSON.
- **Storage**: Transactions are upserted into SQL Server.
- **Secure Query API**: Endpoints allow filtering by category, debit/credit, vendor, customer number, and date range.
- **Seeding**: The database is automatically seeded with a sample customer and vendor mappings on startup.
- **Observability**: Structured logging, exception handling, and health checks are included.
- **Testing**: Unit and integration tests cover adapters, normalizers, services, and API endpoints.

---

## Tech Stack

- **.NET 10** (Internal REST Service - OmniTRXN, External REST service – Ozow)
- **Entity Framework Core** (SQL Server)
- **SQL Server 2022** (Docker container)
- **YARP** (API Gateway)
- **Spring Boot** (External SOAP service – FNB)
- **AutoMapper** (Object mapping)
- **Testcontainers** and **WireMock** (Integration tests)
- **xUnit**, **Moq**, **FluentAssertions** (Testing)
- **Scalar** (API Documentation/Testing)

---

## Prerequisites

- **Docker Desktop** (to run the entire stack with Docker Compose)
- (Optional) **SQL Server Management Studio** or **Azure Data Studio** for database inspection

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Keagon98/OmniTRXN.git
cd OmniTRXN
```

### 2. Start up Docker Containers

In project root, run the following command:

```
docker-compose up -d
```

To see if all of the containers are running, run this command:

```
docker-compose ps
```

### 3. Navigate to OmniTRXN API documentation

Once the **api-omnitrxn** container is up and running (gi), navigate to this URL to test the API:

```
http://localhost:8083/scalar/v1
```

The ``http://localhost:8082
/api/v1/Transactions`` endpoint uses a JWT to authenticate requests going through the API Gateway.

Run this command in your terminal to retrieve the token:

```bash
curl --request POST \
  --url http://localhost:8082/token \
  --header 'content-type: application/json' \
  --header 'X-Api-Key: batlNzmMSrTWckNfdoBmtYrT4o/SUuUGRyFBXqAZlMA=' \
  --data '{
    "ClientId":"GIglLRAx0RL6RjmN8OjPbVlimKwcnVIc",
    "ClientSecret":"DAdCCsPls--wZrG3s1KmlPPePzqrTj-X-2pjPEBixIglgdWOPFEOJtx4rIepOhFF"
  }'
```

Use the **access token** in the **response** to add to the Authorization header.

Once you've added your Bearer token, you have the option to pass several query parameters to the ``/api/v1/Transactions`` endpoint, they are the following:

- **CustomerNumber**: (cust42158) - <span style="color:red;font-weight: bold">This is required</span>
- **Category**: (Groceries, Fuel, Utilities, Telecoms, Transport, Dining, Entertainment, Retail, Health, Insurance/Financial, Salary/Income, Savings/Investments, Cash/ATM, Uncategorized)
- **DebitCredit**: (Debit, Credit)
- **Vendor**: (Ozow, Fnb)
- **FromDate**: (2026-07-18)
- **ToDate** (2026-08-27)
- **Page**
- **PageSize**

### 4. Project Testing

This project includes unit and integration testing. Run the following command in the **root** folder ``(services/internal/OmniTrxnService)`` of the **OmnTRXN API** solution.

```bash
dotnet test
```

To run Unit tests separately, run the following command

```bash
dotnet test tests/OmniTrxn.Tests.Unit/OmniTrxn.Tests.Unit.csproj
```

To run Integration tests separately, run the following command

```bash
dotnet test tests/OmniTrxn.Tests.Integration/OmniTrxn.Tests.Integration.csproj
```
