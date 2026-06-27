# URL Shortener Assignment – Engineering Planning

## 1. Greenfield Scenario

| Section | Details |
| :--- | :--- |
| **Requirement** | Build a URL Shortener service from scratch with core APIs, analytics, and reliability features. |
| **Requirement Understanding** | Identify functional and non-functional requirements. |
| **Task Decomposition** | Break the work into setup, architecture, Project Structure, database, APIs, testing, and documentation. |
| **Architecture** | Define system components, layers, APIs, and data flow. |
| **Implementation** | Build the solution using Clean Architecture and .NET 8. |
| **Validation** | Verify APIs, database operations, analytics, and test coverage. |
| **Risks** | Identify implementation, performance, and security risks. |
| **Deliverables** | Working prototype, architecture, tests, documentation, and AI usage log. |
| **Engineering Decision** | Document technology choices, architecture rationale, trade-offs, and implementation decisions. |

## 2. Brownfield Scenario

| Section | Details |
| :--- | :--- |
| **Existing System** | Existing ASP.NET Core application with authentication, logging, database, and APIs. |
| **New Requirement** | Integrate URL Shortener functionality into the existing application. |
| **Impact Analysis** | Identify affected modules, APIs, database, dependency injection, logging, and monitoring. |
| **Task Decomposition** | Plan feature integration, migration, testing, and deployment. |
| **Validation** | Ensure existing functionality continues to work through regression testing. |
| **Risks** | Route conflicts, database migration issues, performance impact, backward compatibility. |
| **Deliverables** | Updated APIs, database changes, regression tests, and documentation. |
| **Engineering Decision** | Minimize impact on existing modules, maintain backward compatibility, and reuse existing infrastructure wherever possible. |

## 3. Ambiguity & Assumptions

| Category | Details |
| :--- | :--- |
| **Ambiguity** | Short code generation strategy, custom alias, code length, analytics scope, expiration policy, authentication, caching, database choice, expected traffic, rate limiting. |
| **Questions for Stakeholders** | Clarify business rules, analytics requirements, expiration, scalability expectations, and security requirements. |
| **Assumptions** | If clarification is unavailable, document implementation assumptions such as random 6-character codes, HTTP 302 redirects, PostgreSQL, click-count-only analytics, and no authentication for the MVP. |
| **Validation** | Confirm assumptions with stakeholders or update them as requirements become available. |
| **Risks** | Incorrect assumptions may require design changes or rework later. |
| **Engineering Decision** | Clearly separate confirmed requirements from assumptions, document every assumption, and implement only what is explicitly agreed upon for the MVP. |

---

## Requirement Ambiguities

| ID | Ambiguity | Clarification Needed | Assumption (if not clarified) |
| :--- | :--- | :--- | :--- |
| **RA-1** | Short code generation strategy | Should the short code be random, sequential, hashed, or user-defined? | Generate a random 6-character alphanumeric code. |
| **RA-2** | Short code length | Should the length be fixed or configurable? | Use a fixed length of 6 characters. |
| **RA-3** | Duplicate URLs | Should the same original URL return the existing short URL or create a new one? | Return the existing short URL if it already exists. |
| **RA-4** | URL expiration | Should shortened URLs expire? If yes, after how long? | URLs do not expire in the MVP. |
| **RA-5** | Analytics scope | Should analytics include only click count or also IP address, browser, device, location, and timestamps? | Store only the total click count. |
| **RA-6** | Redirect behavior | Should the service use HTTP 301 (Permanent) or HTTP 302 (Temporary) redirects? | Use HTTP 302 redirects. |
| **RA-7** | Authentication | Should the APIs be public or require authentication? | APIs are public for the MVP. |
| **RA-8** | Rate limiting | Should requests be rate-limited to prevent abuse? | Not implemented in the MVP; document as a future enhancement. |
| **RA-9** | Database technology | Which database should be used? | PostgreSQL is used for the prototype. |
| **RA-10** | Caching | Is caching required for faster redirects? | Not implemented in the MVP; architecture allows future Redis integration. |
| **RA-11** | Analytics retention | How long should analytics data be retained? | Retain analytics for the lifetime of the shortened URL. |
| **RA-12** | Scalability requirements | What is the expected traffic volume and concurrency and rate limiting? | Design the application to be scalable, but optimize for a prototype implementation. |

---

## Functional and Non-Functional Requirements

### Functional Requirements

| ID | Requirement |
| :--- | :--- |
| **FR-1** | Users should be able to submit a valid URL and receive a shortened URL. |
| **FR-2** | The system should generate a unique short code for each URL. |
| **FR-3** | The system should redirect users from the shortened URL to the original URL. |
| **FR-4** | The system should store the mapping between the original URL and the short code. |
| **FR-5** | The system should provide analytics for each shortened URL (e.g., click count). |
| **FR-6** | The system should validate that the submitted URL is in a valid format. |
| **FR-7** | The system should return appropriate HTTP status codes for successful and failed requests. |
| **FR-8** | The system should handle requests for non-existent short codes gracefully. |
| **FR-9** | The system should expose REST APIs with Swagger documentation. |
| **FR-10** | (Optional, based on assumptions) The system should support deleting a shortened URL. |

### Non-Functional Requirements

| ID | Requirement |
| :--- | :--- |
| **NFR-1** | The application should follow Clean Architecture principles. |
| **NFR-2** | APIs should be asynchronous and responsive. |
| **NFR-3** | The application should be maintainable and modular using SOLID principles. |
| **NFR-4** | The system should include structured logging for monitoring and debugging. |
| **NFR-5** | The application should include unit and integration tests. |
| **NFR-6** | The system should validate inputs and handle exceptions consistently. |
| **NFR-7** | The database should enforce uniqueness of short codes. |
| **NFR-8** | The application should be scalable to support future enhancements such as caching and distributed deployment. |
| **NFR-9** | API documentation should be available through Swagger/OpenAPI. |
| **NFR-10** | The solution should include setup instructions, architecture documentation, and an AI-assisted engineering log. |
