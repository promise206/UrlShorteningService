# UrlShorteningService

Table Schema:
CREATE TABLE IF NOT EXISTS public."UrlMappings"
(
    "Id" uuid NOT NULL,
    "LongUrl" text COLLATE pg_catalog."default" NOT NULL,
    "ShortUrl" text COLLATE pg_catalog."default" NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "AccessCount" integer NOT NULL,
    "ExpiryDate" timestamp with time zone,
    CONSTRAINT "PK_UrlMappings" PRIMARY KEY ("Id")
)


Design Considerations:
1. Scalability
Horizontal Scaling: Add more instances to handle increased traffic and ensure high availability.
Caching: Use Redis to cache frequently accessed data, reducing database load.
Load Balancing: Distribute requests across instances to prevent bottlenecks.
2. High Availability & Fault Tolerance
Redundancy: Deploy across multiple regions for failover.
Database Replication: Ensure data availability with automated failover to secondary replicas.
Microservices: Isolate failures to specific services.
Health Checks & Monitoring: Use tools like Prometheus or Datadog for real-time monitoring and proactive issue resolution.
Retry Logic & Circuit Breakers: Prevent cascading failures by retrying or halting faulty operations.
3. Security
Authentication & Authorization: Use OAuth and JWT for secure access and role-based control.
Data Encryption: Encrypt sensitive data in transit (HTTPS) and at rest (AES).
Input Validation & Sanitization: Protect against injection attacks using parameterized queries.
Rate Limiting: Prevent abuse with API-level rate limiting.
Logging & Monitoring: Track security events and flag suspicious activity.
API Security: Secure endpoints using OAuth tokens or API keys and limit exposed data.
Vulnerability Scanning & Patching: Regularly scan and patch the system for security flaws.
