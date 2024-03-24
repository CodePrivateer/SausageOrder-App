# Sausage Order App / BratWurst-App

## Overview
This is a simple web application designed for managing products and customers. It allows each customer to place multiple orders, and these orders can be partially or fully paid and picked up. 

## Features
- **Product Management**: Allows creation and management of products.
- **Customer Management**: Enables capturing and managing customer information.
- **Order Management**: Each customer can place multiple orders. These orders are displayed and can be partially or fully paid and picked up.
- **Sponsor Feature**: All orders can be picked up by a sponsor (Paten), who acts as a collective picker.

## Prerequisites
This is an ASP.NET Core application and requires the .NET runtime. You can download it from the official .NET website.

### Configuring the Database
This application uses SQLite for data storage. The connection string for the database is located in the `DataBaseService.cs`. You need to adjust this connection string to match your environment settings. If the connection string is not correctly set, the application may not find the SQLite database.

## Built With
* .NET 8.0

## License
This project is licensed under the MIT License - see the LICENSE.md file for details
