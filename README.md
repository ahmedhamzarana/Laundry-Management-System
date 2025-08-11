# 🧺 Online Laundry Management System

## 📋 Project Overview

This is a web-based Laundry Management System designed to streamline the process of customer registration, invoice generation, delivery tracking, and reporting for laundry businesses. The system supports multiple types of billing methods, barcode tagging, delivery statuses, and a powerful admin dashboard.

> **Note:** This project does not include SMS sending functionality.

---

## 🚀 Features

### 1. Customer Registration
- Add new customers with details like name, phone number, and address.
- Admin functionalities: **Create, View, Edit, Delete** customers.
- Search and filter customers using keywords.

### 2. Invoice Generation
- Generate invoices using 3 methods:
  - **Piece-wise**
  - **Weight-wise**
  - **Package-wise**
- View previous invoices during creation.
- Auto-calculate total amounts based on selected method.

### 3. Barcode Generation
- Generate unique barcodes for each laundry item.
- Print barcode labels and tag garments.

### 4. Delivery Management
- Track item status through:
  - `Pending`
  - `Ready`
  - `Delivered`
- Color-coded statuses for quick identification.
- Collect pending payments and finalize invoice upon delivery.

### 5. Reporting Module
- Generate useful reports such as:
  - Customer List
  - Due Payments
  - Daily Collections
  - Garment-wise Collection
  - Cash Book
  - Total Business per Customer

### 6. Admin Dashboard
- Clean and interactive dashboard UI.
- Graphs and charts for:
  - Total business stats
  - Daily/Monthly revenue
  - Active deliveries

### 7. Email Notification API
- Email sending functionality is located in the `HomeController`.
- User must enter:
  - ✅ Their **own Gmail address**
  - ✅ Their **App Password** (not regular password)
- The system uses these credentials to send transactional emails (order confirmation, invoice alerts, etc.).

#### Example Configuration (`appsettings.json`)
```json
"EmailSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSSL": true,
  "Username": "your-email@gmail.com",
  "Password": "your-app-password"
}
⚠️ Note: For Gmail users:

Enable 2-Step Verification in your Google account.

Generate an App Password via https://myaccount.google.com/apppasswords

Use this password in the app, not your real Gmail password.

📌 Backend Code Location: HomeController.cs → method SendEmail(...) or similar

8. Stripe Payment Integration
Secure payment integration using Stripe.

Accept card payments directly from the invoice module.

Stripe's test mode is available for development.

Stripe Configuration (appsettings.json)
json
Always show details

Copy
"Stripe": {
  "SecretKey": "sk_test_xxxxxxxxxxxxxxxxxx",
  "PublishableKey": "pk_test_xxxxxxxxxxxxxxxxxx"
}
🛠️ Technology Stack
Frontend: HTML, CSS, JavaScript (or Razor Pages if using .NET)

Backend: ASP.NET Core

Database: SQL Server / MySQL

Barcode Library: e.g., ZXing, BarcodeLib

Charting: Chart.js / ApexCharts (for graphs)

Authentication: Role-based admin access

Email API: SMTP or SendGrid via HomeController

Payment Gateway: Stripe

🧩 Database Tables (Sample Schema)
Users

Customers

Invoices

InvoiceItems

Packages

Deliveries

Payments

Reports

Barcodes

🔐 Admin Role Setup (Optional Logic)
If admin user is manually inserted into the database, set role to 2 for full access.

sql
Always show details

Copy
UPDATE Users
SET Role = 2
WHERE Email = 'admin@example.com';
Or configure a seeding method in your backend to assign role programmatically on startup.

🧭 Step-by-Step User Guide
Step 1: Clone the Repository
bash
Always show details

Copy
git clone https://github.com/your-repo/laundry-system.git
cd laundry-system
Step 2: Setup Database Connection
Open appsettings.json and update your database connection string:

json
Always show details

Copy
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=LaundryDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
Step 3: Update the Database (EF Core)
bash
Always show details

Copy
Update-Database
⚠️ Run this in Package Manager Console to apply migrations and create all necessary tables.

Step 4: Configure Email API (HomeController)
Enter your Gmail email and app password.

Update appsettings.json or pass dynamically via UI input (as per your logic).

Step 5: Setup Stripe API
Get your test API keys from Stripe dashboard.

Configure them in your settings as shown above.

Step 6: Run the Application
bash
Always show details

Copy
dotnet run
Step 7: Access the Application
Open your browser and visit: http://localhost:5000 (or configured port)

Step 8: Use the Admin Panel
Login with admin credentials.

Register customers, generate invoices, track deliveries, and view reports.

📦 Modules Yet to be Integrated
❌ SMS Notification System (Not implemented)

✅ All other modules complete

📈 Future Improvements (Optional)
Customer login panel

Subscription-based packages

Push/email notifications

Laundry item image upload

