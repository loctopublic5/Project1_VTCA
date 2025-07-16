Sneaker Shop Console Application
A comprehensive console-based e-commerce management system for a sneaker store, built with .NET and C#. This application provides a feature-rich, role-based interface for both customers and administrators to manage products, orders, and user accounts efficiently.

🌟 Key Features
The application is divided into two main roles: Customer and Admin.

👤 Customer Features
Product Browsing: View all available products with pagination, sorting, and filtering options.

Advanced Search: Search for products by name, filter by category (style, brand), and price range.

Shopping Cart: Add products to the cart, update quantities, change sizes, and remove items.

Secure Checkout: A complete checkout process with address selection and multiple payment options (Cash on Delivery, Wallet Balance).

Account Management:

Manage multiple shipping addresses.

View order history with status tracking.

Manage a personal wallet, including depositing funds.

Promotions: Automatically applies the best available discounts to products.

🛠️ Admin Features
Order Management: View, confirm, or reject customer orders with detailed reporting.

Product Management:

Add new products with detailed descriptions, categories, and gender applicability.

Update stock quantities for specific product sizes.

De-list (soft delete) products from the customer view.

Customer Analytics: View statistics on customer spending and order history.

💻 Technology Stack
Language: C#

Framework: .NET 8

Database: Microsoft SQL Server

ORM: Entity Framework Core 8

UI: Console Application with Spectre.Console for a rich, interactive interface.

IDE: Visual Studio 2022

📦 NuGet Packages Used
This project relies on the following NuGet packages:

Spectre.Console

Microsoft.EntityFrameworkCore

Microsoft.EntityFrameworkCore.Design

Microsoft.EntityFrameworkCore.SqlServer

Microsoft.EntityFrameworkCore.Tools

Microsoft.Extensions.Configuration.Json

Microsoft.Extensions.Hosting

System.Data.SqlClient

🚀 Getting Started
To get a local copy up and running, follow these simple steps.

Prerequisites
.NET 8 SDK

Visual Studio 2022

SQL Server (e.g., Express or Developer edition)

Installation
Clone the repository:

git clone https://your-repository-url.git

Configure the database connection: Open the appsettings.json file and update the DefaultConnection string with your SQL Server credentials.

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=SneakerShopDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  }
}

Apply Migrations:
Open the solution in Visual Studio. In the Package Manager Console, run the following command to create the database and apply the schema:

Update-Database

This command will create the SneakerShopDB database, all necessary tables, and seed it with initial data, including an admin account.

Run the application:
Press F5 or click the "Start" button in Visual Studio to build and run the project.

usage
Upon first launch, you can register a new customer account or log in with the default administrator credentials.

Admin Username: Admin25

Admin Password: Snss.2025 (As defined in the database seeding migration)

📁 Project Structure
The project is organized into logical layers to ensure separation of concerns and maintainability:

Data/: Contains the DbContext, Entity Framework Core models, and migration files.

Services/: Houses the business logic for all application features (e.g., AuthService, OrderService, ProductService).

UI/: Contains all user interface logic, separated into Admin and Customer roles. It uses Spectre.Console for rendering.

DTOs/: Data Transfer Objects used to pass data between layers.

Utils/: Helper classes for common tasks like input validation and password hashing.

Program.cs: The application's entry point, responsible for dependency injection and service configuration.
