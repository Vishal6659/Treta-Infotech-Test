# 🧾 Invoice Management System – ASP.NET Core MVC

This project is a lightweight **Invoice Management System** built with **ASP.NET Core MVC**, showcasing practical implementation of full-stack web development concepts including database interaction, stored procedures, AJAX, and modular design.

---

## 🔧 Features

- **Create & Edit Invoices**
  - Add multiple line items per invoice
  - Auto-generate unique invoice numbers
  - Dynamic calculation of total, paid amount, and balance due

- **Invoice Listing**
  - View a list of all invoices with amount paid and item descriptions
  - Fetch detailed invoice data via **AJAX** in a modal without reloading the page

- **Invoice Details Modal**
  - Displays itemized breakdown using **AJAX** to improve user experience
  - Clean Bootstrap modal design for intuitive interaction

- **Stock Validation**
  - Ensures selected quantity does not exceed available stock
  - Prevents overbooking with real-time quantity alerts

- **Responsive UI**
  - Built with **Bootstrap 5** and **Bootstrap Icons**
  - Fully responsive layout for desktop and mobile screens

---

## 💡 Technical Highlights

- ✅ **MVC Architecture** – Separation of concerns for maintainable, scalable code  
- ✅ **Entity & View Models** – Clean model binding and form validation  
- ✅ **AJAX Integration** – Fetch invoice data without refreshing the page  
- ✅ **Stored Procedures** – All database operations are performed via stored procedures, ensuring better security and performance  
- ✅ **Client-Side Scripting** – JavaScript logic for dynamic row addition, total calculations, and form updates  
- ✅ **Code Readability** – Meaningful comments are added across views and controllers for better understanding

---

## 💻 Technologies Used

| Technology         | Purpose                        |
|--------------------|--------------------------------|
| ASP.NET Core MVC   | Backend framework (cross-platform) |
| SQL Server         | Relational database            |
| ADO.NET + SPs      | Data access via stored procedures |
| Bootstrap 5        | UI styling and layout          |
| JavaScript         | Dynamic form behavior          |
| AJAX / Fetch API   | Asynchronous modal population  |

---

## 🗃️ Database Structure

Includes 3 core tables:
- `Item` – Stores item catalog
- `Invoice` – Invoice header with customer/vendor info
- `InvoiceItem` – Line items per invoice

> All data interactions use **parameterized stored procedures** to prevent SQL injection and improve performance.

## 📚 Additional Notes

- 📝 **Database queries and architecture-related questions** are documented in:

  - **`Queries.txt`**
  - **`databaseArchitectureAndQueries.txt`**

> 📂 **These files are located in the `/wwwroot/SQL Queries/` folder.**

