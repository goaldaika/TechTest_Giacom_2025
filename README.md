<<<<<<< HEAD
# Giacom Tech Test

## Background
Giacom Cloud Market is a B2B e-commerce platform which allows IT companies (resellers) to buy services indirectly from major vendors (Microsoft, Symantec, Webroot etc) in high volumes at low cost. IT companies then resell the purchased services on to their customers, making a small margin. Behind Cloud Market are several microservices, one of which is an Order API much like the one we are going to work on for this test.

## Concepts
* Reseller = A customer of Giacom
* Customer = A customer of a Reseller
* Order = An order placed by a Reseller for a specific Customer
* Order Item = A service and product which belongs to an Order
* Order Status = The current state of an Order
* Product = An end-offering which can be purchased e.g. '100GB Mailbox'
* Service = The category the Product belongs to e.g. 'Email'
* Profit = The difference between Cost and Price

## Time
You should allocate approx. 2 hours to complete the tech test though it will likely take less time for more experienced engineers.

## Pre-Reqs
* Visual Studio 2022 (or compatible IDE for working with .net)
* .NET 8.0 SDK
* Git
* Docker (running Linux containers)
* Optional: MySQL Workbench / Heidi (database client)
* Optional: Postman (can also use any other API client)

## Setup
1. Clone this repository locally
2. Using a terminal, cd to the local repository and run 'docker-compose up db', which will start and seed the database
3. Open the solution file in /src
4. Start debugging or run the Order.WebAPI project then query http://localhost:8000/orders in your API client / browser to test that setup is complete. You should see orders being returned from the API
   
## Tasks
Add a new API endpoint for each of the following tasks:
1. Return Orders with a specified Order Status e.g. 'Failed'
2. Allow an Order Status to be updated to a different status e.g. 'InProgress'
3. Allow an Order to be created. This should include validation of any parameters
4. Calculate profit by month for all 'completed' Orders

Finally, once code-complete, close your IDE, run 'docker-compose down --volumes' to stop and remove the database container. Now run 'docker-compose up'. This will run the local database and also build the microservice in Release mode. Test the API is working correctly via this method (as this is the one Giacom will run to test the submission).

## Submission
Please push your code to a new github repository then send the repository link to the email address from which the tech test was issued. If applicable, add notes in the email explaining why you have chosen a particular approach.
Alternatively, zip or git-bundle the repository and email it.

## Help
If you happen to run into any issues when running the Docker container, try deselecting Hyper-V Services in "Windows Features" (Search for Windows Features in Start Menu), selecting again, and then restarting your computer.

To connect to the MySQL database directly the credentials are as follows:
* Hostname: *localhost*
* Username: *order-service*
* Password: *nmCsdkhj20n@Sa*

If you experience further issues getting set up with the tech test please reply to the email address from which the tech test was issued with your query.

Copyright (c) 2025, Giacom.
=======
# TechTest_Giacom_2025
>>>>>>> 611470580a926ed9c0d4273567cafdd2ab2a318c
