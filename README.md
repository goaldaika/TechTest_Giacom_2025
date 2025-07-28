# Project Overview

## Frameworks and Technologies

- **Development:**
  - ASP.NET Web API
  - EF Core
  - MySQL

- **Testing:**
  - NUnit
  - EF Core

- **Container:**
  - Docker

## Application Structure

The application consists of 6 API methods:

1. **Get Orders:**
   - Retrieves all orders along with their corresponding items.

2. **Get Order By Status:**
   - Fetches orders filtered by a specified status.

3. **Get Profit Of All Completed Order:**
   - Returns profit data for orders with status "Completed" for all months of a specified year. If no year is provided, records default to the current year.

4. **Get Total Profit Of Each Month:**
   - Calculates total profit for orders with status "Completed" for all months of a specified year. If no year is provided, records default to the current year.

5. **Update Order Status:**
   - Updates the status of a specified order.

6. **Create Order:**
   - Accepts an `OrderDetail` object as input to create a new order.

## Application Architecture

The changes in the structure of the application compared to the original version:

- **OrderStatusEnum:**
  - Order statuses are implemented as an enumeration (`OrderStatusEnum`), mapped to the database, aligning with standard practices for management systems.

- **Interface:**
  - All interfaces are consolidated into the same files as their implementation methods for streamlined development and maintenance.

- **Api client:**
  - Swagger is configured as the API client for testing. The Swagger setup is ready to use.

- **Docker:**
  - Docker is configured to support Swagger. To test, run `docker-compose up` and access the Swagger UI at `http://localhost:8000/swagger`.

- **Test Units:**
  - Unit tests are implemented for each API method, covering expected standard behaviors.

## Notable Considerations and Future Enhancements

- **Exception Handling:**
  - Implement enhanced exception handling to prevent exposing raw errors to users, improving security and user experience.

- **Pagination Enhancement:**
  - The current dataset includes approximately 100 order records, suitable for retrieving all at once. In production environments with potentially thousands of records, implementing pagination for `GetOrders` and `GetOrderByStatus` is recommended to reduce performance load.

- **Unit Testing:**
  - Expand unit test coverage to include edge cases and additional scenarios for comprehensive validation.

- **MySql and Docker studies:**
  - As MySQL and Docker were newly adopted for this project, further optimization of database queries and container configurations could enhance application performance with additional time investment.
