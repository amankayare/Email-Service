# Email Microservice Implementation Plan

This plan details the architecture and implementation strategy for a standalone, production-ready Email Microservice using .NET 8, Redis, and Hangfire. This service acts as a centralized notification engine.

## Goal Description
Build a highly scalable, containerized email sending service in `d:\Workspace\Email-service`. It will accept HTTP requests, queue them in Redis, and process them asynchronously via background workers using a configurable provider (SendGrid or AWS SES). 

The implementation will strictly adhere to **SOLID principles** and industry-standard Clean Architecture patterns, ensuring the codebase is highly maintainable, testable, and loosely coupled.

## Proposed Architecture

The system will be split into a classic Clean Architecture structure located at `d:\Workspace\Email-service`:

```text
EmailService/
├── docker-compose.yml
├── EmailService.Core/           (Domain entities, Interfaces - No external dependencies)
├── EmailService.Infrastructure/ (SendGrid/SES integrations, Hangfire setup)
├── EmailService.API/            (HTTP endpoints, API Key Auth, Swagger, Hangfire UI)
└── EmailService.Worker/         (Hangfire background server processor)
```

### 1. SOLID & Industry Standards
- **Single Responsibility Principle (SRP):** API only accepts/validates requests and enqueues jobs. Workers strictly process jobs.
- **Open/Closed Principle (OCP):** New email providers can be added simply by creating a new class implementing `IEmailProvider`, without modifying existing code.
- **Liskov Substitution Principle (LSP):** The system can seamlessly swap between `SendGridEmailProvider` and `AwsSesEmailProvider` without altering program correctness.
- **Interface Segregation Principle (ISP):** Interfaces like `IEmailProvider` and `IEmailQueueService` will be small and focused.
- **Dependency Inversion Principle (DIP):** High-level modules (API/Worker) will depend only on Core abstractions (`IEmailProvider`), not concrete Infrastructure implementations.

### 2. EmailService.Core
This project contains no dependencies on external frameworks, just pure C#.
- **Models**: `EmailRequest` (To, Subject, Body, IsHtml)
- **Interfaces**: `IEmailProvider`, `IEmailQueueService`

### 3. EmailService.Infrastructure
This handles all external dependencies.
- **Providers**: 
  - `SendGridEmailProvider` (Uses SendGrid API)
  - `AwsSesEmailProvider` (Uses AWS SDK)
- **Factory**: Resolves the correct provider based on the `EMAIL_PROVIDER` env variable (no fallback logic required for V1).
- **Queueing**: Hangfire implementation using `RedisStorage`.

### 4. EmailService.API
The entry point for client applications.
- **Middleware**: API Key validation (`X-API-Key` header).
- **Endpoints**: 
  - `POST /api/email/send`: Validates request and calls `IEmailQueueService.Enqueue()`.
  - `GET /health`: Docker health check.
- **Hangfire Dashboard**: Exposed at `/hangfire` (protected by API Key auth or local-only policy) for visual queue management.
- **Dockerfile**: Optimized multi-stage build exposing port 8080.

### 5. EmailService.Worker
A headless background service that continuously polls Redis for jobs.
- **Processing**: Picks up jobs from Hangfire and calls `IEmailProvider.SendEmailAsync()`.
- **Resilience**: Configured with automatic retries (3 attempts) on failure.
- **Dockerfile**: Optimized multi-stage build.

### 6. Docker Configuration
- **docker-compose.yml**:
  - `redis`: `redis:alpine` container.
  - `email-api`: The HTTP gateway, depends on Redis.
  - `email-worker`: The processor, depends on Redis.
- Environment variables configured via `.env` file.

## Verification Plan

### Automated Tests
- While formal unit tests are deferred for V1, the architecture guarantees 100% testability through Dependency Injection.

### Manual Verification
1. Navigate to `d:\Workspace\Email-service`.
2. Run `docker-compose up --build`.
3. Send a curl/Postman request to `POST http://localhost:8080/api/email/send` with an API Key.
4. Verify the API returns `202 Accepted`.
5. Open the browser to `http://localhost:8080/hangfire` and observe the job processing successfully.
