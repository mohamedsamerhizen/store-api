# Store API

A production-style e-commerce backend built with **.NET 9** and **ASP.NET Core Web API**.

This project was developed as a backend portfolio project to demonstrate practical API design, authentication and authorization, business rule enforcement, validation, error handling, and maintainable service-based architecture using the ASP.NET Core ecosystem.

---

## Overview

`Store API` is a backend for a simple online store that covers the core workflow of an e-commerce system:

- user registration and login
- JWT-based authentication
- role-based authorization
- category and product management
- cart operations
- checkout and order creation
- order status management
- stock updates during checkout and cancellation

The project focuses on clean structure and realistic backend concerns rather than only basic CRUD operations.

---

## Tech Stack

- **.NET 9**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQLite**
- **ASP.NET Identity**
- **JWT Authentication**
- **FluentValidation**
- **Serilog**
- **Swagger / OpenAPI**
- **Health Checks**
- **Rate Limiting**
- **In-Memory Caching**

---

## Features

### Authentication and Authorization
- User registration
- User login
- JWT token generation
- Role-based authorization
- Admin / User separation

### Categories
- Get categories with pagination
- Search categories
- Optional inactive category filtering
- Create category
- Update category
- Deactivate category
- Prevent deactivation when a category contains active products

### Products
- Public product listing
- Product details endpoint
- Search products
- Category filtering
- Pagination support
- Admin-only create, update, and delete operations
- Cache invalidation after write operations

### Cart
- Get current user cart
- Add item to cart
- Update item quantity
- Remove item from cart
- Clear cart

### Orders
- Checkout from cart
- Save shipping details with each order
- Get current user orders
- Get all orders as admin
- Update order status as admin
- Decrease stock during checkout
- Restore stock when cancelling an order
- Prevent invalid order status transitions

### Cross-Cutting Concerns
- FluentValidation for request validation
- Global exception handling middleware
- Unified API response format
- Structured logging with Serilog
- Health check endpoint
- Fixed-window rate limiting

---

## Project Structure

```text
store/
├── Common/
├── Controllers/
├── Data/
├── Dtos/
├── Middlewares/
├── Models/
├── Services/
├── Validation/
├── Migrations/
├── Program.cs
└── appsettings.json
