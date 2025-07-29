# Neo Global Warehouse System

A modern warehouse management system built with **Blazor** (.NET 8), featuring user management, product inventory, transaction processing, and PDF report generation.

<p align="center"><img width="512" height="664" alt="754fe25c-c092-4189-92cb-7ba2d6e0807c" src="https://github.com/user-attachments/assets/efcfb78a-f64a-4602-8a85-ba9d10591758" /></p>

## Features

- **User Management**: Admins can create, edit, delete users, and manage roles (Admin, Storeman, Cashier).
- **Product Management**: Add, edit, and track products with stock control.
- **Transaction Processing**: Record sales and employee product take transactions, with detailed product breakdowns.
- **Reporting**: Generate and download monthly PDF reports for customer sales and employee product takes.
- **Receipts**: Download transaction receipts as PDFs.
- **Role-Based Access**: Secure pages and actions based on user roles.
- **Modern UI**: Built with [MudBlazor](https://mudblazor.com/) for a responsive, material design experience.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server or PostgreSQL (see connection string in `appsettings.json`)
- Node.js (for front-end build, if needed)

### Setup

1. **Clone the repository**
   ```sh
   git clone <your-repo-url>
   cd NeoGlobalWarehouseSystem
   ```
2. **Configure the database**
   - Update the connection string in `appsettings.json` as needed.
3. **Apply migrations**
   ```sh
   dotnet ef database update
   ```
4. **Run the application**
   ```sh
   dotnet run
   ```
   The app will be available at `https://localhost:5001` (or as configured).

### Default Admin User

- The first user must be created manually via the database or registration page, then assigned the `Admin` role.

## Usage

- **Account Management**: `/account-management` (Admins only)
- **Product Management**: `/product`
- **Transactions**: `/transaction`
- **Reports**: `/report` (Admins only)

## PDF Reports & Receipts

- **Reports**: Select type, month, and year on `/report` and click "Download PDF".
- **Receipts**: On `/transaction`, use the download icon to get a PDF receipt for any transaction.

## Technologies Used

- **Blazor** (.NET 8)
- **Entity Framework Core** (SQL Server & PostgreSQL support)
- **MudBlazor** (UI components)
- **QuestPDF** (PDF generation)
- **ASP.NET Core Identity** (Authentication & Authorization)

## Custom Scripts

- `wwwroot/script.js` includes helpers for file download and UI effects.

## Development

- Open the solution in Visual Studio 2022 or later.
- Build and run as a standard Blazor Server project.
- Use __Tools > NuGet Package Manager__ to manage dependencies.

## License

This project is licensed under the MIT License.
