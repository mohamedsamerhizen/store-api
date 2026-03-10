# store API

A production-style e-commerce backend built with **.NET 9** and **ASP.NET Core Web API**.

This project was built as a backend portfolio project with a focus on clean structure, authentication and authorization, practical business logic, validation, error handling, and real-world API design.

---

## Overview

`store API` is an e-commerce backend that supports the main workflow of an online store:

- user registration and login
- JWT-based authentication
- role-based authorization
- category and product management
- shopping cart operations
- checkout and order creation
- order status management
- stock updates during checkout and cancellation

The project is designed as a portfolio backend to demonstrate backend engineering skills using the ASP.NET Core ecosystem.

---

## Tech Stack

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- ASP.NET Identity
- JWT Authentication
- FluentValidation
- Serilog
- Swagger / OpenAPI
- Health Checks
- Rate Limiting
- Response Caching

---

## Main Features

### Authentication and Authorization
- User registration
- User login
- JWT token generation
- Role-based authorization
- Admin / User separation

### Products and Categories
- Public product listing
- Product details endpoint
- Product search
- Pagination support
- Category filtering
- Admin-only category management
- Admin-only product management
- Soft delete for products and categories

### Cart
- Get current user cart
- Add product to cart
- Update cart item quantity
- Remove cart item
- Clear cart

### Orders
- Checkout from cart
- Save shipping information with each order
- Get current user orders
- Admin can get all orders
- Admin can update order status
- Stock decreases on checkout
- Stock is restored when an order is cancelled

### API Quality and Stability
- Global exception handling
- Unified API response format
- FluentValidation for request validation
- Structured logging with Serilog
- Health check endpoint
- Response caching
- Rate limiting
- Swagger UI for testing endpoints

---

## Project Structure

```text
store
├── Common
├── Controllers
├── Data
├── Dtos
│   ├── Auth
│   ├── Cart
│   ├── Categories
│   ├── Orders
│   └── Products
├── Middlewares
├── Migrations
├── Models
├── Properties
├── Services
│   ├── Cart
│   ├── Categories
│   ├── Orders
│   └── Products
├── Validation
├── Program.cs
└── store.csproj
